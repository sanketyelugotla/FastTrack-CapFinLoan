"""HTTP client for calling backend .NET microservices."""

from __future__ import annotations
import httpx
import logging
from typing import Any, Optional

from app.config import settings

logger = logging.getLogger(__name__)


class BackendClient:
    """Async HTTP client that forwards the user's JWT to backend services."""

    def __init__(self) -> None:
        self._app_base = settings.application_service_url
        self._doc_base = settings.document_service_url

    async def get_my_applications(self, token: str) -> list[dict[str, Any]]:
        """GET /api/applications/my — list the user's applications."""
        return await self._get(f"{self._app_base}/api/applications/my", token)

    async def get_application(self, app_id: str, token: str) -> dict[str, Any]:
        """GET /api/applications/{id} — get a single application."""
        return await self._get(f"{self._app_base}/api/applications/{app_id}", token)

    async def get_application_status(self, app_id: str, token: str) -> dict[str, Any]:
        """GET /api/applications/{id}/status — get application status + timeline."""
        return await self._get(f"{self._app_base}/api/applications/{app_id}/status", token)

    async def create_application(self, data: dict[str, Any], token: str) -> dict[str, Any]:
        """POST /api/applications — create a new draft application."""
        return await self._post(f"{self._app_base}/api/applications", data, token)

    async def submit_application(self, app_id: str, token: str) -> dict[str, Any]:
        """POST /api/applications/{id}/submit — submit a draft application."""
        return await self._post(f"{self._app_base}/api/applications/{app_id}/submit", {}, token)

    async def get_profile(self, token: str) -> dict[str, Any]:
        """GET /api/applications/profile — get the applicant profile."""
        return await self._get(f"{self._app_base}/api/applications/profile", token)

    async def get_my_documents(self, token: str) -> list[dict[str, Any]]:
        """GET /api/documents/my — get the user's uploaded documents."""
        return await self._get(f"{self._doc_base}/api/documents/my", token)

    async def get_application_documents(self, application_id: str, token: str) -> list[dict[str, Any]]:
        """GET /api/documents/application/{applicationId} — get documents for a specific application."""
        return await self._get(f"{self._doc_base}/api/documents/application/{application_id}", token)

    # ── Admin Methods ───────────────────────────────────────────

    async def get_admin_dashboard(self, token: str) -> dict[str, Any]:
        """GET /api/admin/applications/dashboard — get dashboard metrics."""
        return await self._get(f"{settings.admin_service_url}/api/admin/applications/dashboard", token)

    async def get_admin_applications(self, token: str, status: Optional[str] = None) -> list[dict[str, Any]]:
        """GET /api/admin/applications — get full application queue."""
        url = f"{settings.admin_service_url}/api/admin/applications"
        if status:
            url += f"?status={status}"
        return await self._get(url, token)

    async def get_admin_documents(self, token: str, status: Optional[str] = None) -> list[dict[str, Any]]:
        """GET /api/internal/documents/all — get all documents (admin only)."""
        url = f"{self._doc_base}/api/internal/documents/all"
        if status:
            url += f"?status={status}"
        return await self._get(url, token)

    # ── Internal helpers ─────────────────────────────────────────

    async def _get(self, url: str, token: str) -> Any:
        async with httpx.AsyncClient(timeout=15.0) as client:
            resp = await client.get(url, headers={"Authorization": f"Bearer {token}"})
            resp.raise_for_status()
            return resp.json()

    async def _post(self, url: str, data: dict[str, Any], token: str) -> Any:
        async with httpx.AsyncClient(timeout=15.0) as client:
            resp = await client.post(url, json=data, headers={"Authorization": f"Bearer {token}"})
            resp.raise_for_status()
            return resp.json()


# Singleton instance
backend_client = BackendClient()
