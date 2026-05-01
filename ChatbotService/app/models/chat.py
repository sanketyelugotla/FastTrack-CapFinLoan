"""Pydantic models for chat request / response payloads."""

from __future__ import annotations
from pydantic import BaseModel, Field
from typing import Optional


class ChatRequest(BaseModel):
    """Incoming chat message from the Angular frontend."""
    session_id: Optional[str] = Field(None, description="Existing session ID to continue a conversation")
    message: str = Field(..., min_length=1, max_length=2000, description="User message text")


class ChatAction(BaseModel):
    """An optional UI action the frontend should perform."""
    type: str = Field(..., description="Action type: navigate | upload_document | pre_fill_form | show_status | escalate")
    payload: Optional[str] = Field(None, description="Action-specific data, e.g. route path")


class ChatProgress(BaseModel):
    """Progress tracker for guided flows."""
    step: int
    total: int
    label: str


class ChatResponse(BaseModel):
    """Bot reply sent back to the Angular frontend."""
    session_id: str
    reply: str
    quick_replies: list[str] = Field(default_factory=list)
    action: Optional[ChatAction] = None
    progress: Optional[ChatProgress] = None
