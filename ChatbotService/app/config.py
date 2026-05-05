"""Application configuration loaded from environment variables."""

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    # ── Groq / LLM ──────────────────────────────────────────────
    groq_api_key: str = ""
    groq_model: str = "llama-3.1-8b-instant"

    # ── JWT (must match AuthService) ────────────────────────────
    jwt_secret: str = "CapFinLoan.Auth.Jwt.Signing.Key.ChangeThisInProduction.2026"
    jwt_issuer: str = "CapFinLoan.AuthService"
    jwt_audience: str = "CapFinLoan.Gateway"
    jwt_algorithm: str = "HS256"

    # ── Backend service URLs (local dev defaults) ───────────────
    application_service_url: str = "http://localhost:5022"
    document_service_url: str = "http://localhost:5023"
    admin_service_url: str = "http://localhost:5024"

    # ── Session ─────────────────────────────────────────────────
    session_ttl_minutes: int = 30

    # ── CORS ────────────────────────────────────────────────────
    cors_origins: str = "http://localhost:4200,http://localhost:5020"

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}


settings = Settings()
