"""Persistent session store with TTL-based expiry using local JSON file."""

from __future__ import annotations
import uuid
import json
import os
import logging
from datetime import datetime, timezone, timedelta
from typing import Optional

from app.config import settings
from app.models.session import SessionContext

logger = logging.getLogger(__name__)

SESSION_FILE = "sessions.json"

class SessionStore:
    """Persistent store backed by a local JSON file."""

    def __init__(self) -> None:
        self._sessions: dict[str, SessionContext] = {}
        self._load()

    def _load(self) -> None:
        if not os.path.exists(SESSION_FILE):
            return
        try:
            with open(SESSION_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
                for sid, sdata in data.items():
                    # Parse using Pydantic
                    self._sessions[sid] = SessionContext.model_validate(sdata)
        except Exception as e:
            logger.error("Failed to load sessions: %s", e)

    def _save(self) -> None:
        try:
            with open(SESSION_FILE, "w", encoding="utf-8") as f:
                data = {sid: ctx.model_dump(mode="json") for sid, ctx in self._sessions.items()}
                json.dump(data, f)
        except Exception as e:
            logger.error("Failed to save sessions: %s", e)

    def get_or_create(self, session_id: Optional[str], user_id: str, user_name: str = "", user_email: str = "") -> tuple[str, SessionContext]:
        """Return an existing session or create a new one."""
        self._cleanup_expired()

        if session_id and session_id in self._sessions:
            ctx = self._sessions[session_id]
            if ctx.user_id == user_id:
                ctx.last_active = datetime.now(timezone.utc)
                self._save()
                return session_id, ctx
            # Session belongs to a different user — ignore it, create new
            del self._sessions[session_id]

        # Check if user already has an active session
        for existing_id, ctx in self._sessions.items():
            if ctx.user_id == user_id:
                ctx.last_active = datetime.now(timezone.utc)
                self._save()
                return existing_id, ctx

        # Create a new session
        new_id = str(uuid.uuid4())
        ctx = SessionContext(user_id=user_id, user_name=user_name, user_email=user_email)
        self._sessions[new_id] = ctx
        self._save()
        return new_id, ctx

    def delete(self, session_id: str, user_id: str) -> bool:
        """Delete a session, only if owned by user_id."""
        if session_id in self._sessions and self._sessions[session_id].user_id == user_id:
            del self._sessions[session_id]
            self._save()
            return True
        return False
        
    def save_session(self, session_id: str) -> None:
        """Call to force save after modifying a session."""
        if session_id in self._sessions:
            self._save()

    def _cleanup_expired(self) -> None:
        """Remove sessions older than TTL."""
        cutoff = datetime.now(timezone.utc) - timedelta(minutes=settings.session_ttl_minutes)
        expired = [sid for sid, ctx in self._sessions.items() if ctx.last_active < cutoff]
        for sid in expired:
            del self._sessions[sid]
        if expired:
            self._save()


# Singleton instance
session_store = SessionStore()
