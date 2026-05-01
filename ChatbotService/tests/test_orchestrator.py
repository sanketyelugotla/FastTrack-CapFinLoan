from __future__ import annotations

import unittest
from unittest.mock import AsyncMock

from app.models.session import SessionContext
from app.services import orchestrator


class OrchestratorContextTests(unittest.IsolatedAsyncioTestCase):
    def setUp(self) -> None:
        self._original_get_profile = orchestrator.backend_client.get_profile
        self._original_get_my_applications = orchestrator.backend_client.get_my_applications
        self._original_get_application_status = orchestrator.backend_client.get_application_status
        self._original_get_application_documents = orchestrator.backend_client.get_application_documents
        self._original_get_my_documents = orchestrator.backend_client.get_my_documents
        self._original_create_application = orchestrator.backend_client.create_application

    def tearDown(self) -> None:
        orchestrator.backend_client.get_profile = self._original_get_profile
        orchestrator.backend_client.get_my_applications = self._original_get_my_applications
        orchestrator.backend_client.get_application_status = self._original_get_application_status
        orchestrator.backend_client.get_application_documents = self._original_get_application_documents
        orchestrator.backend_client.get_my_documents = self._original_get_my_documents
        orchestrator.backend_client.create_application = self._original_create_application

    async def test_apply_requires_onboarding_and_navigates_to_profile(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(
            return_value={
                "personalDetails": {"firstName": "Sam", "email": "sam@example.com"},
                "employmentDetails": {},
            }
        )
        orchestrator.backend_client.get_my_applications = AsyncMock(return_value=[])
        orchestrator.backend_client.get_my_documents = AsyncMock(return_value=[])

        session = SessionContext(user_id="u1")
        response = await orchestrator.handle_message("sid-1", session, "Apply for a loan", "token")

        self.assertIn("complete your onboarding details", response.reply)
        self.assertIsNotNone(response.action)
        self.assertEqual(response.action.type, "navigate")
        self.assertEqual(response.action.payload, "/applicant/profile")
        self.assertFalse(session.onboarding_complete)

    async def test_apply_starts_loan_flow_when_onboarding_complete(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(
            return_value={
                "personalDetails": {
                    "firstName": "Sam",
                    "lastName": "K",
                    "email": "sam@example.com",
                    "phone": "9999999999",
                    "dateOfBirth": "1995-01-01",
                    "gender": "Male",
                    "addressLine1": "Street 1",
                    "city": "Chennai",
                    "state": "TN",
                    "postalCode": "600001",
                },
                "employmentDetails": {
                    "employerName": "ABC Pvt Ltd",
                    "employmentType": "Salaried",
                    "monthlyIncome": 50000,
                    "annualIncome": 600000,
                    "existingEmiAmount": 0,
                },
            }
        )
        orchestrator.backend_client.get_my_applications = AsyncMock(return_value=[])
        orchestrator.backend_client.get_my_documents = AsyncMock(return_value=[])

        session = SessionContext(user_id="u1")

        response = await orchestrator.handle_message("sid-2", session, "apply for a loan", "token")

        self.assertTrue(session.onboarding_complete)
        self.assertEqual(session.monthly_income, 50000)
        self.assertEqual(session.conversation_stage, "loan_purpose")
        self.assertIn("What type of loan", response.reply)

    async def test_application_listing_and_status_by_serial(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(return_value={"personalDetails": {}, "employmentDetails": {}})
        orchestrator.backend_client.get_my_documents = AsyncMock(return_value=[])
        orchestrator.backend_client.get_my_applications = AsyncMock(
            return_value=[
                {
                    "id": "app-1",
                    "applicationNumber": "APP-001",
                    "status": "UnderReview",
                    "loanDetails": {"loanPurpose": "Home Loan", "requestedAmount": 1500000},
                },
                {
                    "id": "app-2",
                    "applicationNumber": "APP-002",
                    "status": "PendingDocuments",
                    "loanDetails": {"loanPurpose": "Personal Loan", "requestedAmount": 300000},
                },
            ]
        )
        orchestrator.backend_client.get_application_status = AsyncMock(
            return_value={
                "currentStatus": "UnderReview",
                "timeline": [{"status": "UnderReview", "remarks": "Income verification in progress"}],
            }
        )

        session = SessionContext(user_id="u1")

        response1 = await orchestrator.handle_message("sid-3", session, "show my applications", "token")
        response2 = await orchestrator.handle_message("sid-3", session, "status of 1", "token")

        self.assertIn("1. APP-001", response1.reply)
        self.assertEqual(session.application_lookup.get("1"), "app-1")
        self.assertIn("currently in UnderReview", response2.reply)
        self.assertIn("Income verification in progress", response2.reply)
        self.assertEqual(session.selected_application_id, "app-1")

    async def test_documents_summary_for_all_documents(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(return_value={"personalDetails": {}, "employmentDetails": {}})
        orchestrator.backend_client.get_my_applications = AsyncMock(
            return_value=[
                {
                    "id": "app-1",
                    "applicationNumber": "APP-001",
                    "status": "UnderReview",
                    "loanDetails": {"loanPurpose": "Home Loan"},
                }
            ]
        )
        orchestrator.backend_client.get_my_documents = AsyncMock(
            return_value=[
                {
                    "id": "doc-1",
                    "applicationId": "app-1",
                    "documentType": "IdProof",
                    "status": "Verified",
                    "remarks": "All good",
                }
            ]
        )

        session = SessionContext(user_id="u1")
        response = await orchestrator.handle_message("sid-4", session, "show my documents", "token")

        self.assertIn("document status summary", response.reply.lower())
        self.assertIn("IdProof", response.reply)
        self.assertIn("APP-001", response.reply)
        self.assertIn("All good", response.reply)

    async def test_emi_estimate_is_returned_after_amount_and_tenure(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(return_value={"personalDetails": {}, "employmentDetails": {}})
        orchestrator.backend_client.get_my_applications = AsyncMock(return_value=[])
        orchestrator.backend_client.get_my_documents = AsyncMock(return_value=[])

        session = SessionContext(
            user_id="u1",
            onboarding_complete=True,
            profile_snapshot_loaded=True,
            profile_snapshot={
                "personal_details": {"firstName": "Sam", "email": "sam@example.com"},
                "employment_details": {"monthlyIncome": 50000, "annualIncome": 600000},
            },
        )

        response1 = await orchestrator.handle_message("sid-5", session, "apply for a loan", "token")
        response2 = await orchestrator.handle_message("sid-5", session, "home loan", "token")
        response3 = await orchestrator.handle_message("sid-5", session, "1000000", "token")
        response4 = await orchestrator.handle_message("sid-5", session, "24", "token")

        self.assertIn("What type of loan", response1.reply)
        self.assertIn("How much would you like to borrow", response2.reply)
        self.assertIn("How many months", response3.reply)
        self.assertIn("estimated EMI", response4.reply)
        self.assertIn("assumed 8% annual interest rate", response4.reply)
        self.assertEqual(session.conversation_stage, "loan_review")
        self.assertIsNotNone(session.estimated_emi)

    async def test_save_draft_uses_profile_payload(self) -> None:
        orchestrator.backend_client.get_profile = AsyncMock(side_effect=AssertionError("Profile should not be reloaded"))
        orchestrator.backend_client.get_my_applications = AsyncMock(return_value=[])
        orchestrator.backend_client.get_my_documents = AsyncMock(return_value=[])
        orchestrator.backend_client.create_application = AsyncMock(return_value={"id": "draft-123"})

        session = SessionContext(
            user_id="u1",
            onboarding_complete=True,
            profile_snapshot_loaded=True,
            profile_snapshot={
                "personal_details": {
                    "firstName": "Sam",
                    "lastName": "K",
                    "dateOfBirth": "1995-01-01",
                    "gender": "Male",
                    "email": "sam@example.com",
                    "phone": "9999999999",
                    "addressLine1": "Street 1",
                    "addressLine2": "",
                    "city": "Chennai",
                    "state": "TN",
                    "postalCode": "600001",
                },
                "employment_details": {
                    "employerName": "ABC Pvt Ltd",
                    "employmentType": "Salaried",
                    "monthlyIncome": 50000,
                    "annualIncome": 600000,
                    "existingEmiAmount": 0,
                },
            },
            loan_type="Home Loan",
            loan_amount=1000000,
            tenure_months=24,
            conversation_stage="loan_review",
        )

        response = await orchestrator.handle_message("sid-6", session, "save my draft", "token")

        self.assertEqual(session.draft_application_id, "draft-123")
        self.assertEqual(session.conversation_stage, "loan_saved")
        self.assertIn("draft application has been saved", response.reply)
        orchestrator.backend_client.create_application.assert_awaited_once()
        called_payload = orchestrator.backend_client.create_application.await_args.args[0]
        self.assertEqual(called_payload["personalDetails"]["firstName"], "Sam")
        self.assertEqual(called_payload["employmentDetails"]["monthlyIncome"], 50000)
        self.assertEqual(called_payload["loanDetails"]["requestedAmount"], 1000000)


if __name__ == "__main__":
    unittest.main()
