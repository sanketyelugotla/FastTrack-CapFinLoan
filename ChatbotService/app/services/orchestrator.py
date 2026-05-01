"""Deterministic chatbot orchestration for current-user loan support."""

from __future__ import annotations

import logging
import math
import re
from typing import Any, Optional

from app.models.chat import ChatAction, ChatProgress, ChatResponse
from app.models.session import SessionContext
from app.services.backend_client import backend_client

logger = logging.getLogger(__name__)

LOAN_TYPES = [
    "Home Loan", "Education Loan", "Personal Loan", "Vehicle Loan",
    "Business Loan", "Medical Loan", "Debt Consolidation",
    "Agriculture Loan", "Gold Loan", "Other",
]

LOAN_TYPE_KEYWORDS = {
    "home": "Home Loan",
    "education": "Education Loan",
    "student": "Education Loan",
    "personal": "Personal Loan",
    "personel": "Personal Loan",
    "vehicle": "Vehicle Loan",
    "car": "Vehicle Loan",
    "bike": "Vehicle Loan",
    "business": "Business Loan",
    "medical": "Medical Loan",
    "debt": "Debt Consolidation",
    "agriculture": "Agriculture Loan",
    "agri": "Agriculture Loan",
    "gold": "Gold Loan",
}

PROFILE_REQUIRED_PERSONAL_FIELDS = [
    "firstName",
    "lastName",
    "email",
    "phone",
    "dateOfBirth",
    "gender",
    "addressLine1",
    "city",
    "state",
    "postalCode",
]

PROFILE_REQUIRED_EMPLOYMENT_FIELDS = [
    "employerName",
    "employmentType",
    "monthlyIncome",
    "annualIncome",
]


def _format_currency(value: float | int | None) -> str:
    if value is None:
        return ""
    return f"INR {float(value):,.0f}"


def _normalize_text(message: str) -> str:
    return re.sub(r"\s+", " ", message.strip().lower())


def _extract_number(message: str) -> Optional[float]:
    match = re.search(r"\b\d[\d,]*\b", message.replace("INR", "").replace("$", ""))
    if not match:
        return None
    return float(match.group(0).replace(",", ""))


def _extract_serial_number(message: str) -> Optional[str]:
    match = re.search(r"\b(\d{1,2})\b", message)
    return match.group(1) if match else None


def _looks_affirmative(message: str) -> bool:
    normalized = _normalize_text(message)
    return normalized in {
        "yes",
        "y",
        "ok",
        "okay",
        "continue",
        "save",
        "save my draft",
        "create draft",
        "confirm",
        "go ahead",
        "submit",
        "use saved details",
        "use these",
    }


def _looks_change_amount(message: str) -> bool:
    normalized = _normalize_text(message)
    return "change amount" in normalized or "edit amount" in normalized


def _looks_change_tenure(message: str) -> bool:
    normalized = _normalize_text(message)
    return "change tenure" in normalized or "edit tenure" in normalized or "change months" in normalized


def _mentions_status(message: str) -> bool:
    normalized = _normalize_text(message)
    return "status" in normalized or "progress" in normalized or "stage" in normalized


def _mentions_remarks(message: str) -> bool:
    normalized = _normalize_text(message)
    return "remark" in normalized or "remarks" in normalized or "comment" in normalized


def _is_application_list_request(message: str) -> bool:
    normalized = _normalize_text(message)
    return "application" in normalized and any(word in normalized for word in ["my", "show", "list", "applications"])


def _is_document_request(message: str) -> bool:
    normalized = _normalize_text(message)
    return "document" in normalized or "documents" in normalized or "doc status" in normalized


def _is_apply_intent(message: str) -> bool:
    normalized = _normalize_text(message)
    return any(phrase in normalized for phrase in [
        "apply for a loan",
        "start application",
        "start my application",
        "apply loan",
        "loan application",
        "apply for loan",
    ])


def _parse_loan_type(message: str) -> Optional[str]:
    normalized = _normalize_text(message)
    for keyword, loan_type in LOAN_TYPE_KEYWORDS.items():
        if keyword in normalized:
            return loan_type
    return None


def _extract_profile_snapshot(profile: dict[str, Any]) -> dict[str, Any]:
    return {
        "personal_details": profile.get("personalDetails") or {},
        "employment_details": profile.get("employmentDetails") or {},
    }


def _extract_documents_snapshot(documents: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "id": document.get("id"),
            "application_id": document.get("applicationId") or document.get("application_id"),
            "document_type": document.get("documentType") or document.get("document_type") or "Document",
            "status": document.get("status") or "Unknown",
            "remarks": document.get("remarks") or "",
            "file_name": document.get("fileName") or document.get("file_name") or "",
        }
        for document in documents
    ]


def _extract_applications_snapshot(applications: list[dict[str, Any]]) -> list[dict[str, Any]]:
    snapshot: list[dict[str, Any]] = []
    for application in applications:
        loan_details = application.get("loanDetails") or {}
        snapshot.append(
            {
                "id": application.get("id"),
                "application_number": application.get("applicationNumber") or "N/A",
                "status": application.get("status") or "Unknown",
                "loan_purpose": loan_details.get("loanPurpose") or "Loan",
                "requested_amount": loan_details.get("requestedAmount"),
                "updated_at": application.get("updatedAtUtc") or application.get("createdAtUtc") or "",
            }
        )
    return snapshot


def _is_profile_complete(profile_snapshot: dict[str, Any]) -> bool:
    personal = profile_snapshot.get("personal_details") or {}
    employment = profile_snapshot.get("employment_details") or {}
    return all(personal.get(field) not in (None, "") for field in PROFILE_REQUIRED_PERSONAL_FIELDS) and all(
        employment.get(field) not in (None, "") for field in PROFILE_REQUIRED_EMPLOYMENT_FIELDS
    )


def _sync_profile_into_session(session: SessionContext) -> None:
    snapshot = session.profile_snapshot or {}
    personal = snapshot.get("personal_details") or {}
    employment = snapshot.get("employment_details") or {}

    combined_name = " ".join(
        part for part in [personal.get("firstName", ""), personal.get("lastName", "")] if part
    ).strip()
    if combined_name:
        session.user_name = combined_name
    session.user_email = personal.get("email") or session.user_email
    session.monthly_income = employment.get("monthlyIncome")
    session.annual_income = employment.get("annualIncome")
    session.existing_emi = employment.get("existingEmiAmount") if employment.get("existingEmiAmount") is not None else 0.0
    session.employer_name = employment.get("employerName")
    session.employment_type = employment.get("employmentType")
    session.onboarding_complete = _is_profile_complete(snapshot)


def _build_application_payload(session: SessionContext) -> dict[str, Any]:
    snapshot = session.profile_snapshot or {}
    personal = snapshot.get("personal_details") or {}
    employment = snapshot.get("employment_details") or {}

    first_name = personal.get("firstName") or (session.user_name.split(" ")[0] if session.user_name else "")
    last_name = personal.get("lastName")
    if not last_name and session.user_name:
        last_name = " ".join(session.user_name.split(" ")[1:])

    return {
        "personalDetails": {
            "firstName": first_name,
            "lastName": last_name or "",
            "dateOfBirth": personal.get("dateOfBirth"),
            "gender": personal.get("gender") or "",
            "email": personal.get("email") or session.user_email,
            "phone": personal.get("phone") or "",
            "addressLine1": personal.get("addressLine1") or "",
            "addressLine2": personal.get("addressLine2") or "",
            "city": personal.get("city") or "",
            "state": personal.get("state") or "",
            "postalCode": personal.get("postalCode") or "",
        },
        "employmentDetails": {
            "employerName": employment.get("employerName") or session.employer_name or "",
            "employmentType": employment.get("employmentType") or session.employment_type or "",
            "monthlyIncome": employment.get("monthlyIncome") or session.monthly_income,
            "annualIncome": employment.get("annualIncome") or session.annual_income,
            "existingEmiAmount": employment.get("existingEmiAmount") if employment.get("existingEmiAmount") is not None else session.existing_emi or 0,
        },
        "loanDetails": {
            "requestedAmount": session.loan_amount or 0,
            "requestedTenureMonths": session.tenure_months or 0,
            "loanPurpose": session.loan_type or "",
            "remarks": session.loan_remarks,
        },
    }


def _calculate_emi(principal: float, tenure_months: int, annual_rate: float = 0.08) -> float:
    monthly_rate = annual_rate / 12
    if tenure_months <= 0:
        return 0.0
    if monthly_rate == 0:
        return principal / tenure_months
    factor = math.pow(1 + monthly_rate, tenure_months)
    return principal * monthly_rate * factor / (factor - 1)


def _build_menu_response(session_id: str) -> ChatResponse:
    return ChatResponse(
        session_id=session_id,
        reply="Hi there! I can help you apply for a loan, show your applications, or summarize your document status. What would you like to do?",
        quick_replies=["Apply for a loan", "Show my applications", "Show my documents"],
    )


def _build_onboarding_required_response(session_id: str, session: SessionContext) -> ChatResponse:
    snapshot = session.profile_snapshot or {}
    personal = snapshot.get("personal_details") or {}
    employment = snapshot.get("employment_details") or {}
    missing_fields = [field for field in PROFILE_REQUIRED_PERSONAL_FIELDS if personal.get(field) in (None, "")]
    missing_fields.extend(field for field in PROFILE_REQUIRED_EMPLOYMENT_FIELDS if employment.get(field) in (None, ""))
    preview = ", ".join(missing_fields[:5])
    if len(missing_fields) > 5:
        preview += "..."
    return ChatResponse(
        session_id=session_id,
        reply=(
            "Before I can start a loan application, please complete your onboarding details in your profile page. "
            f"Missing fields include: {preview}."
        ),
        quick_replies=["Open profile page"],
        action=ChatAction(type="navigate", payload="/applicant/profile"),
    )


def _build_applications_reply(applications: list[dict[str, Any]]) -> str:
    lines = ["Here are your current applications:"]
    for index, application in enumerate(applications, start=1):
        lines.append(f"{index}. {application['application_number']} - {application['loan_purpose']} - {application['status']}")
    lines.append("Reply with a serial number, for example 'status of 1' or 'remarks for 2'.")
    return "\n".join(lines)


def _build_documents_reply(documents: list[dict[str, Any]], app_number_by_id: dict[str, str]) -> str:
    if not documents:
        return "I could not find any uploaded documents for your account right now."
    lines = ["Here is the current document status summary:"]
    for document in documents:
        app_label = app_number_by_id.get(document.get("application_id") or "", "Profile documents")
        remarks = f" Remarks: {document['remarks']}." if document.get("remarks") else ""
        lines.append(f"- {document['document_type']} for {app_label}: {document['status']}.{remarks}")
    return "\n".join(lines)


def _build_status_reply(status_payload: dict[str, Any], application_number: str) -> str:
    timeline = status_payload.get("timeline") or []
    latest_remark = ""
    for item in reversed(timeline):
        if item.get("remarks"):
            latest_remark = item["remarks"]
            break
    reply = f"Application {application_number} is currently in {status_payload.get('currentStatus', 'Unknown')} status."
    if latest_remark:
        reply += f" Latest remarks: {latest_remark}."
    return reply


async def _refresh_profile(session: SessionContext, jwt_token: str) -> None:
    profile = await backend_client.get_profile(jwt_token)
    session.profile_snapshot = _extract_profile_snapshot(profile)
    session.profile_snapshot_loaded = True
    _sync_profile_into_session(session)


async def _refresh_applications(session: SessionContext, jwt_token: str) -> list[dict[str, Any]]:
    applications = await backend_client.get_my_applications(jwt_token)
    session.applications_snapshot = _extract_applications_snapshot(applications)
    session.applications_snapshot_loaded = True
    session.application_lookup = {
        str(index): application["id"] for index, application in enumerate(session.applications_snapshot, start=1)
    }
    return session.applications_snapshot


async def _refresh_documents(session: SessionContext, jwt_token: str) -> list[dict[str, Any]]:
    documents = await backend_client.get_my_documents(jwt_token)
    session.documents_snapshot = _extract_documents_snapshot(documents)
    session.documents_snapshot_loaded = True
    return session.documents_snapshot


async def _hydrate_external_context(session: SessionContext, jwt_token: str) -> None:
    if not session.profile_snapshot_loaded:
        try:
            await _refresh_profile(session, jwt_token)
        except Exception as exc:
            logger.info("Profile preload skipped: %s", exc)
            session.profile_snapshot = {"personal_details": {}, "employment_details": {}}
            session.profile_snapshot_loaded = True
            session.onboarding_complete = False

    if not session.applications_snapshot_loaded:
        try:
            await _refresh_applications(session, jwt_token)
        except Exception as exc:
            logger.info("Application preload skipped: %s", exc)
            session.applications_snapshot = []
            session.applications_snapshot_loaded = True

    if not session.documents_snapshot_loaded:
        try:
            await _refresh_documents(session, jwt_token)
        except Exception as exc:
            logger.info("Document preload skipped: %s", exc)
            session.documents_snapshot = []
            session.documents_snapshot_loaded = True


def _loan_progress(session: SessionContext, label: str) -> ChatProgress:
    step = 1
    if session.loan_type:
        step = 2
    if session.loan_amount:
        step = 3
    if session.tenure_months:
        step = 4
    if session.draft_application_id:
        step = 5
    return ChatProgress(step=step, total=5, label=label)


async def _handle_application_lookup(
    session_id: str,
    session: SessionContext,
    user_message: str,
    jwt_token: str,
) -> Optional[ChatResponse]:
    if _is_application_list_request(user_message):
        applications = await _refresh_applications(session, jwt_token)
        if not applications:
            return ChatResponse(
                session_id=session_id,
                reply="I could not find any loan applications for your account yet.",
                quick_replies=["Apply for a loan", "Show my documents"],
            )

        session.conversation_stage = "application_lookup"
        quick_replies = [f"Status of {index}" for index in range(1, min(4, len(applications) + 1))]
        return ChatResponse(
            session_id=session_id,
            reply=_build_applications_reply(applications),
            quick_replies=quick_replies,
            action=ChatAction(type="navigate", payload="/applicant/applications"),
        )

    if session.application_lookup and (
        _mentions_status(user_message) or _mentions_remarks(user_message) or session.conversation_stage == "application_lookup"
    ):
        serial = _extract_serial_number(user_message)
        if serial and serial in session.application_lookup:
            application_id = session.application_lookup[serial]
            application = next((item for item in session.applications_snapshot if item["id"] == application_id), None)
            status_payload = await backend_client.get_application_status(application_id, jwt_token)
            session.selected_application_id = application_id
            session.selected_application_number = application.get("application_number") if application else None
            quick_replies = ["Show remarks", "Show documents", "Show my applications"] if _mentions_status(user_message) else ["Show status", "Show documents", "Show my applications"]
            return ChatResponse(
                session_id=session_id,
                reply=_build_status_reply(status_payload, session.selected_application_number or application_id),
                quick_replies=quick_replies,
                action=ChatAction(type="show_status", payload=f"/applicant/applications/{application_id}/status"),
            )

    return None


async def _handle_document_lookup(
    session_id: str,
    session: SessionContext,
    user_message: str,
    jwt_token: str,
) -> Optional[ChatResponse]:
    if not _is_document_request(user_message):
        return None

    serial = _extract_serial_number(user_message)
    if serial and serial in session.application_lookup:
        application_id = session.application_lookup[serial]
        documents = await backend_client.get_application_documents(application_id, jwt_token)
        normalized_documents = _extract_documents_snapshot(documents)
        application = next((item for item in session.applications_snapshot if item["id"] == application_id), None)
        app_number_by_id = {application_id: application.get("application_number", application_id) if application else application_id}
        session.selected_application_id = application_id
        return ChatResponse(
            session_id=session_id,
            reply=_build_documents_reply(normalized_documents, app_number_by_id),
            quick_replies=["Show status", "Show my applications"],
            action=ChatAction(type="upload_document", payload=f"/applicant/applications/{application_id}/documents"),
        )

    documents = await _refresh_documents(session, jwt_token)
    app_number_by_id = {item["id"]: item["application_number"] for item in session.applications_snapshot}
    return ChatResponse(
        session_id=session_id,
        reply=_build_documents_reply(documents, app_number_by_id),
        quick_replies=["Show my applications", "Apply for a loan"],
        action=ChatAction(type="navigate", payload="/applicant/documents"),
    )


def _loan_start_response(session_id: str, session: SessionContext) -> ChatResponse:
    session.conversation_stage = "loan_purpose"
    session.loan_type = None
    session.loan_amount = None
    session.tenure_months = None
    session.loan_remarks = ""
    session.estimated_emi = None
    snapshot = session.profile_snapshot or {}
    personal = snapshot.get("personal_details") or {}
    first_name = personal.get("firstName") or session.user_name or "there"
    return ChatResponse(
        session_id=session_id,
        reply=f"I will use your saved onboarding details for {first_name}. What type of loan would you like to apply for?",
        quick_replies=["Personal Loan", "Home Loan", "Vehicle Loan", "Education Loan"],
        progress=_loan_progress(session, "Choose Loan Type"),
    )


def _amount_question(session_id: str, session: SessionContext) -> ChatResponse:
    session.conversation_stage = "loan_amount"
    return ChatResponse(
        session_id=session_id,
        reply=f"Noted. You want a {session.loan_type}. How much would you like to borrow in INR?",
        quick_replies=["500000", "1000000", "2000000"],
        progress=_loan_progress(session, "Requested Amount"),
    )


def _tenure_question(session_id: str, session: SessionContext) -> ChatResponse:
    session.conversation_stage = "loan_tenure"
    return ChatResponse(
        session_id=session_id,
        reply=f"Got it. Requested amount is {_format_currency(session.loan_amount)}. How many months do you want for repayment?",
        quick_replies=["12", "24", "36", "60"],
        progress=_loan_progress(session, "Repayment Tenure"),
    )


def _review_response(session_id: str, session: SessionContext) -> ChatResponse:
    session.conversation_stage = "loan_review"
    session.estimated_emi = _calculate_emi(session.loan_amount or 0, session.tenure_months or 0)
    return ChatResponse(
        session_id=session_id,
        reply=(
            f"Here is your estimate for a {session.loan_type}: requested amount {_format_currency(session.loan_amount)}, "
            f"tenure {session.tenure_months} months, and estimated EMI {_format_currency(session.estimated_emi)} per month at an assumed 8% annual interest rate. "
            "Final interest rate and eligibility will be decided by the bank after review. "
            "If you want to add remarks, type them now. Otherwise say 'save my draft'."
        ),
        quick_replies=["Save my draft", "Change amount", "Change tenure"],
        progress=_loan_progress(session, "Review Estimate"),
    )


async def _save_draft(session_id: str, session: SessionContext, jwt_token: str) -> ChatResponse:
    response = await backend_client.create_application(_build_application_payload(session), jwt_token)
    session.draft_application_id = response.get("id")
    session.conversation_stage = "loan_saved"
    return ChatResponse(
        session_id=session_id,
        reply=(
            f"Your draft application has been saved with loan purpose {session.loan_type}, amount {_format_currency(session.loan_amount)}, "
            f"tenure {session.tenure_months} months, and estimated EMI {_format_currency(session.estimated_emi)}. "
            "You can review it and continue with documents from your applications page."
        ),
        quick_replies=["Show my applications", "Show my documents"],
        action=ChatAction(type="navigate", payload="/applicant/applications"),
        progress=ChatProgress(step=5, total=5, label="Draft Saved"),
    )


async def handle_message(
    session_id: str,
    session: SessionContext,
    user_message: str,
    jwt_token: str,
) -> ChatResponse:
    session.add_message("user", user_message)
    await _hydrate_external_context(session, jwt_token)

    normalized = _normalize_text(user_message)
    if not normalized:
        response = _build_menu_response(session_id)
        session.add_message("assistant", response.reply)
        return response

    application_response = await _handle_application_lookup(session_id, session, user_message, jwt_token)
    if application_response:
        session.add_message("assistant", application_response.reply)
        return application_response

    document_response = await _handle_document_lookup(session_id, session, user_message, jwt_token)
    if document_response:
        session.add_message("assistant", document_response.reply)
        return document_response

    if _is_apply_intent(user_message):
        response = _build_onboarding_required_response(session_id, session) if not session.onboarding_complete else _loan_start_response(session_id, session)
        session.add_message("assistant", response.reply)
        return response

    if session.conversation_stage == "loan_purpose":
        loan_type = _parse_loan_type(user_message)
        if loan_type is None:
            response = ChatResponse(
                session_id=session_id,
                reply="Please choose a loan type so I can continue.",
                quick_replies=["Personal Loan", "Home Loan", "Vehicle Loan", "Education Loan"],
                progress=_loan_progress(session, "Choose Loan Type"),
            )
        else:
            session.loan_type = loan_type
            response = _amount_question(session_id, session)
        session.add_message("assistant", response.reply)
        return response

    if session.conversation_stage == "loan_amount":
        amount = _extract_number(user_message)
        if amount is None or amount <= 0:
            response = ChatResponse(
                session_id=session_id,
                reply="Please enter a valid loan amount in INR.",
                quick_replies=["500000", "1000000", "2000000"],
                progress=_loan_progress(session, "Requested Amount"),
            )
        else:
            session.loan_amount = amount
            response = _tenure_question(session_id, session)
        session.add_message("assistant", response.reply)
        return response

    if session.conversation_stage == "loan_tenure":
        tenure = _extract_number(user_message)
        if tenure is None or tenure < 6:
            response = ChatResponse(
                session_id=session_id,
                reply="Please enter a tenure in months. It should be at least 6 months.",
                quick_replies=["12", "24", "36", "60"],
                progress=_loan_progress(session, "Repayment Tenure"),
            )
        else:
            session.tenure_months = int(tenure)
            response = _review_response(session_id, session)
        session.add_message("assistant", response.reply)
        return response

    if session.conversation_stage == "loan_review":
        if _looks_change_amount(user_message):
            response = _amount_question(session_id, session)
            session.add_message("assistant", response.reply)
            return response
        if _looks_change_tenure(user_message):
            response = _tenure_question(session_id, session)
            session.add_message("assistant", response.reply)
            return response
        if _looks_affirmative(user_message):
            response = await _save_draft(session_id, session, jwt_token)
            session.add_message("assistant", response.reply)
            return response

        session.loan_remarks = user_message.strip()
        response = ChatResponse(
            session_id=session_id,
            reply="Remarks noted. Say 'save my draft' when you want me to create the draft application.",
            quick_replies=["Save my draft", "Change amount", "Change tenure"],
            progress=_loan_progress(session, "Review Estimate"),
        )
        session.add_message("assistant", response.reply)
        return response

    response = _build_menu_response(session_id)
    session.add_message("assistant", response.reply)
    return response
