"""FastAPI application entry point for CapFinLoan ChatbotService."""

import logging
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.config import settings
from app.routers.chat import router as chat_router

# ── Logging ─────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s │ %(levelname)-7s │ %(name)s │ %(message)s",
)

# ── FastAPI App ─────────────────────────────────────────────────
app = FastAPI(
    title="CapFinLoan ChatbotService",
    description="AI-powered loan assistant using Groq (LLaMA 3.3)",
    version="1.0.0",
)

# ── CORS ────────────────────────────────────────────────────────
origins = [o.strip() for o in settings.cors_origins.split(",") if o.strip()]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── Routers ─────────────────────────────────────────────────────
app.include_router(chat_router)


@app.get("/")
async def root():
    return {"service": "CapFinLoan ChatbotService", "status": "running"}
