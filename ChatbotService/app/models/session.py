"""Session context — tracks what the chatbot has collected from the user so far."""

from __future__ import annotations
from datetime import datetime, timezone
from pydantic import BaseModel, Field
from typing import Any, Optional


class ChatMessage(BaseModel):
    """A single message in the conversation history."""
    role: str  # "user" | "assistant"
    content: str


class SessionContext(BaseModel):
    """Per-user session state stored in memory."""
    user_id: str
    user_name: str = ""
    user_email: str = ""
    user_role: str = ""

    # Conversation state
    conversation_stage: str = "idle"
    profile_reuse_decision: Optional[str] = None
    document_reuse_decision: Optional[str] = None

    # Cached profile / document data used for reuse confirmation prompts
    profile_snapshot_loaded: bool = False
    documents_snapshot_loaded: bool = False
    profile_snapshot: dict[str, Any] = Field(default_factory=dict)
    documents_snapshot: list[dict[str, Any]] = Field(default_factory=list)
    applications_snapshot_loaded: bool = False
    applications_snapshot: list[dict[str, Any]] = Field(default_factory=list)
    application_lookup: dict[str, str] = Field(default_factory=dict)
    selected_application_id: Optional[str] = None
    selected_application_number: Optional[str] = None
    onboarding_complete: bool = False

    # Admin State
    admin_dashboard_snapshot: dict[str, Any] = Field(default_factory=dict)
    admin_applications_snapshot: list[dict[str, Any]] = Field(default_factory=list)
    admin_documents_snapshot: list[dict[str, Any]] = Field(default_factory=list)

    # Collected loan application fields
    loan_type: Optional[str] = None
    monthly_income: Optional[float] = None
    annual_income: Optional[float] = None
    loan_amount: Optional[float] = None
    tenure_months: Optional[int] = None
    existing_emi: Optional[float] = None
    employer_name: Optional[str] = None
    employment_type: Optional[str] = None
    loan_remarks: str = ""
    estimated_emi: Optional[float] = None

    # Application lifecycle
    draft_application_id: Optional[str] = None

    # Conversation history (kept trimmed to the last 50 messages)
    history: list[ChatMessage] = Field(default_factory=list)

    created_at: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
    last_active: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))

    def add_message(self, role: str, content: str) -> None:
        self.history.append(ChatMessage(role=role, content=content))
        # Keep only the most recent messages to avoid context overflow
        if len(self.history) > 50:
            self.history = self.history[-50:]
        self.last_active = datetime.now(timezone.utc)

    def get_collected_summary(self) -> str:
        """Returns a summary of fields collected so far for the LLM context."""
        parts: list[str] = []
        if self.loan_type:
            parts.append(f"Loan Type: {self.loan_type}")
        if self.monthly_income is not None:
            parts.append(f"Monthly Income: ₹{self.monthly_income:,.0f}")
        if self.annual_income is not None:
            parts.append(f"Annual Income: ₹{self.annual_income:,.0f}")
        if self.loan_amount is not None:
            parts.append(f"Requested Amount: ₹{self.loan_amount:,.0f}")
        if self.tenure_months is not None:
            parts.append(f"Tenure: {self.tenure_months} months")
        if self.existing_emi is not None:
            parts.append(f"Existing EMI: ₹{self.existing_emi:,.0f}")
        if self.employer_name:
            parts.append(f"Employer: {self.employer_name}")
        if self.employment_type:
            parts.append(f"Employment Type: {self.employment_type}")
        if self.loan_remarks:
            parts.append(f"Remarks: {self.loan_remarks}")
        if self.selected_application_number:
            parts.append(f"Selected Application: {self.selected_application_number}")
        if self.draft_application_id:
            parts.append(f"Draft Application ID: {self.draft_application_id}")

        return "\n".join(parts) if parts else "No fields collected yet."
