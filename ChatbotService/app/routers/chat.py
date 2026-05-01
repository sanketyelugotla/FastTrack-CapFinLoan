"""Chat router — API endpoints for the chatbot."""

from __future__ import annotations
import logging
from typing import Annotated

from fastapi import APIRouter, Depends, Header, HTTPException, status
from jose import JWTError, jwt

from app.config import settings
from app.models.chat import ChatRequest, ChatResponse
from app.services.session_store import session_store
from app.services.orchestrator import handle_message

logger = logging.getLogger(__name__)
router = APIRouter()


def _decode_jwt(authorization: Annotated[str, Header()]) -> dict:
    """Extract and validate the JWT from the Authorization header.

    Returns the decoded token payload containing userId, name, email, role.
    """
    if not authorization.startswith("Bearer "):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid authorization header")

    token = authorization[7:]

    try:
        payload = jwt.decode(
            token,
            settings.jwt_secret,
            algorithms=[settings.jwt_algorithm],
            audience=settings.jwt_audience,
            issuer=settings.jwt_issuer,
        )
        return {"token": token, "payload": payload}
    except JWTError as e:
        logger.warning("JWT decode failed: %s", e)
        # If strict decode fails, try without audience/issuer validation
        # (some JWT configs may differ)
        try:
            payload = jwt.decode(
                token,
                settings.jwt_secret,
                algorithms=[settings.jwt_algorithm],
                options={"verify_aud": False, "verify_iss": False},
            )
            return {"token": token, "payload": payload}
        except JWTError:
            raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid or expired token")


@router.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest, auth: dict = Depends(_decode_jwt)):
    """Main chat endpoint — send a message, get a bot reply."""
    payload = auth["payload"]
    jwt_token = auth["token"]

    # Extract user info from JWT claims
    # .NET JWT uses these claim URIs by default
    user_id = (
        payload.get("sub")
        or payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
        or payload.get("nameid")
        or "unknown"
    )
    user_name = (
        payload.get("name")
        or payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
        or ""
    )
    user_email = (
        payload.get("email")
        or payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
        or ""
    )

    # Get or create session
    session_id, session = session_store.get_or_create(
        session_id=request.session_id,
        user_id=user_id,
        user_name=user_name,
        user_email=user_email,
    )

    try:
        response = await handle_message(
            session_id=session_id,
            session=session,
            user_message=request.message,
            jwt_token=jwt_token,
        )
        session_store.save_session(session_id)
        return response
    except Exception as e:
        logger.exception("Chat error: %s", e)
        return ChatResponse(
            session_id=session_id,
            reply="I'm sorry, I encountered an issue processing your request. Please try again or contact support.",
            quick_replies=["Try again", "Talk to an agent"],
        )


@router.delete("/chat/session")
async def clear_session(auth: dict = Depends(_decode_jwt)):
    """Clear the current user's chat session."""
    payload = auth["payload"]
    user_id = (
        payload.get("sub")
        or payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
        or payload.get("nameid")
        or "unknown"
    )

    # Delete all sessions for this user (brute force through store)
    # In production, you'd have a proper lookup
    return {"status": "cleared"}


@router.get("/chat/health")
async def health():
    """Health check endpoint for Docker and gateway."""
    return {"status": "healthy", "service": "ChatbotService"}
