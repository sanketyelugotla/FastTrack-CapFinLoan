"""Async wrapper around the Groq SDK with tool-calling support."""

from __future__ import annotations
import json
import logging
from typing import Any, Optional

from groq import AsyncGroq

from app.config import settings

logger = logging.getLogger(__name__)


class GroqClient:
    """Calls the Groq API using the official SDK (OpenAI-compatible)."""

    def __init__(self) -> None:
        self._client = AsyncGroq(api_key=settings.groq_api_key)
        self._model = settings.groq_model

    async def chat(
        self,
        messages: list[dict[str, Any]],
        tools: Optional[list[dict[str, Any]]] = None,
        temperature: float = 0.7,
    ) -> dict[str, Any]:
        """Send a chat completion request and return the first choice's message.

        Returns a dict with keys: role, content, tool_calls (if any).
        """
        kwargs: dict[str, Any] = {
            "model": self._model,
            "messages": messages,
            "temperature": temperature,
            "max_tokens": 1024,
        }
        if tools:
            kwargs["tools"] = tools
            kwargs["tool_choice"] = "auto"

        logger.debug("Groq request: model=%s, messages=%d, tools=%s",
                      self._model, len(messages), bool(tools))

        response = await self._client.chat.completions.create(**kwargs)
        choice = response.choices[0]
        message = choice.message

        result: dict[str, Any] = {
            "role": message.role,
            "content": message.content or "",
        }

        if message.tool_calls:
            result["tool_calls"] = [
                {
                    "id": tc.id,
                    "function": {
                        "name": tc.function.name,
                        "arguments": tc.function.arguments,
                    },
                }
                for tc in message.tool_calls
            ]

        return result


# Singleton instance
groq_client = GroqClient()
