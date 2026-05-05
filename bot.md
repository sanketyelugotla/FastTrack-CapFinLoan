# CapFinLoan ChatbotService Detailed Documentation

## 1) Purpose of ChatbotService

ChatbotService is a Python FastAPI microservice that provides conversational help for two personas:

- Applicant users: apply for loans, check application status, check document status
- Admin users: ask dashboard-level questions and get summarized answers

It works as an orchestration layer between:

- Frontend chat UI
- JWT authentication context
- Backend microservices (Application, Document, Admin)
- LLM provider (Groq)

## 2) High-level architecture

Request flow:

1. Frontend sends POST /chat with message and optional session_id
2. Router validates JWT and extracts user claims
3. SessionStore loads existing session or creates a new one
4. Orchestrator handles the message using role-aware logic
5. If needed, BackendClient fetches profile/applications/documents/admin data
6. For admin queries, Groq LLM is called with current admin context
7. Response is returned and session is saved to sessions.json

Core design choice:

- Applicant flow is deterministic and code-driven
- Admin flow uses LLM summarization over preloaded admin context

## 3) Which model is used?

Configured model:

- Provider: Groq
- Default model in config: llama-3.1-8b-instant

Where configured:

- ChatbotService/app/config.py

Where used:

- ChatbotService/app/services/groq_client.py

Important note:

- app/main.py description text says LLaMA 3.3 in API metadata, but actual configured model default is llama-3.1-8b-instant. Runtime behavior follows config.py and environment variables.

## 4) Did we fine-tune the model?

Short answer: No.

Why:

- There is no training pipeline, no dataset loading, no LoRA adapters, no model checkpoint artifacts
- The service calls hosted inference via Groq SDK
- Behavior customization is done by:
  - Prompting (system prompt for admin mode)
  - Deterministic orchestration logic in Python
  - Backend context hydration

So this is prompt + orchestration engineering, not model fine-tuning.

## 5) What changes were done to the base LLM behavior?

Since there is no fine-tuning, behavior changes are implemented in app code:

1. Role-based control:

- If role is Admin, use LLM for analytics-style replies
- If role is Applicant, use deterministic state machine

2. Context injection:

- Admin prompt includes JSON snapshot of dashboard, applications, and documents
- Context is truncated to safe length before prompt insertion

3. Low temperature for admin analysis:

- temperature=0.1 for more stable, less random summaries

4. Strict orchestration for applicant journey:

- Loan type -> amount -> tenure -> review -> draft save
- Validation and transitions handled by code, not free-form LLM

5. Tool schemas are defined (OpenAI-style), but currently not actively wired into the applicant flow path in orchestrator.

## 6) How context is saved

Session context model:

- Defined in ChatbotService/app/models/session.py as SessionContext

Stored data includes:

- User identity: user_id, user_name, user_email, user_role
- Conversation state: conversation_stage, decisions, selected application
- Profile snapshot: personal and employment details
- Application snapshot: list and lookup mapping by serial number
- Document snapshot
- Admin snapshots for dashboard/application/document lists
- Loan form fields collected via chat
- Draft application id
- Message history (trimmed to last 50)
- created_at and last_active timestamps

## 7) How conversations are stored

Storage mechanism:

- File-based JSON persistence
- File name: sessions.json
- Managed by SessionStore in ChatbotService/app/services/session_store.py

Behavior:

- Loads all sessions at startup
- Saves on session changes and explicit save_session calls
- TTL cleanup removes stale sessions (default 30 minutes)
- If a session_id belongs to a different user, it is discarded and replaced

Trade-off:

- Good for local/dev and small scale
- Not ideal for distributed multi-instance production without shared store (Redis/DB)

## 8) How authentication works

Implemented in ChatbotService/app/routers/chat.py:

- Reads Authorization header with Bearer token
- Tries strict JWT verification using:
  - secret
  - algorithm
  - audience
  - issuer
- If strict verification fails, tries relaxed decode without audience/issuer verification
- Extracts claims for user_id, user_name, user_email using .NET-friendly claim keys

## 9) Detailed file structure and meaning

Repository-level location used:

- ChatbotService under project root

Structure:

- ChatbotService/.dockerignore
- ChatbotService/.env
- ChatbotService/.env.example
- ChatbotService/Dockerfile
- ChatbotService/requirements.txt
- ChatbotService/app/**init**.py
- ChatbotService/app/main.py
- ChatbotService/app/config.py
- ChatbotService/app/models/**init**.py
- ChatbotService/app/models/chat.py
- ChatbotService/app/models/session.py
- ChatbotService/app/routers/**init**.py
- ChatbotService/app/routers/chat.py
- ChatbotService/app/services/**init**.py
- ChatbotService/app/services/backend_client.py
- ChatbotService/app/services/groq_client.py
- ChatbotService/app/services/orchestrator.py
- ChatbotService/app/services/session_store.py
- ChatbotService/app/tools/**init**.py
- ChatbotService/app/tools/definitions.py
- ChatbotService/tests/test_orchestrator.py

Per-file meaning:

1. .dockerignore

- Excludes pycache, virtualenv, secrets, editor files, and markdown from image build context
- Keeps image clean and avoids secret leakage

2. .env

- Local runtime secrets and endpoint config (not committed ideally)

3. .env.example

- Template of required environment variables
- Helps onboarding and deployment setup

4. Dockerfile

- Multi-stage build using python:3.12-slim
- Installs dependencies in builder stage
- Copies only app code and installed packages to final image
- Runs as non-root user capbot
- Starts uvicorn with 2 workers

5. requirements.txt

- Declares runtime dependencies: FastAPI, Uvicorn, HTTPX, Pydantic, jose, groq, dotenv

6. app/**init**.py

- Package marker for Python module import structure

7. app/main.py

- FastAPI app bootstrap
- Logging setup
- CORS configuration
- Registers chat router
- Exposes root health-like endpoint

8. app/config.py

- Central settings class using pydantic-settings
- Reads values from environment and .env
- Contains LLM model name, JWT settings, backend URLs, CORS, session TTL

9. app/models/**init**.py

- Package marker for model namespace

10. app/models/chat.py

- Request/response contracts for API:
  - ChatRequest
  - ChatResponse
  - ChatAction
  - ChatProgress

11. app/models/session.py

- SessionContext data model that holds full conversational memory for a user
- add_message method appends history and trims to 50 entries
- get_collected_summary builds a human-readable summary of collected loan details

12. app/routers/**init**.py

- Package marker for router namespace

13. app/routers/chat.py

- API endpoints:
  - POST /chat
  - DELETE /chat/session
  - GET /chat/health
- JWT decode and claim extraction
- Session load/create + orchestration call + error fallback response
- Important gap: DELETE endpoint currently returns status but does not actually delete sessions

14. app/services/**init**.py

- Package marker for services namespace

15. app/services/backend_client.py

- Async HTTP wrapper around backend microservice endpoints
- Forwards caller JWT to downstream services
- Supports applicant and admin endpoints
- Uses 15-second timeout and raises HTTP errors on failure

16. app/services/groq_client.py

- Wrapper around AsyncGroq chat completions API
- Supports optional tools and auto tool choice
- Returns normalized dict: role/content/tool_calls

17. app/services/orchestrator.py

- Core intelligence layer and state machine
- Responsibilities:
  - Normalize and parse user messages
  - Recognize intents (apply/status/documents)
  - Hydrate session with backend profile/application/document data
  - Build applicant responses and progress
  - Save draft application through backend
  - Route admin questions to LLM with admin context prompt
- Applicant mode is deterministic and staged
- Admin mode uses LLM generation

18. app/services/session_store.py

- Persistent local JSON-backed session repository
- Creates/reuses sessions per user
- Enforces session ownership
- TTL cleanup via last_active

19. app/tools/**init**.py

- Package marker for tools namespace

20. app/tools/definitions.py

- Contains JSON schema definitions for tool/function calling
- Includes tools such as:
  - check_eligibility
  - get_application_status
  - update_loan_details
  - confirm_profile_reuse
  - confirm_document_reuse
  - save_draft_application
  - request_document_upload
  - escalate_to_human
- Current code defines them, but applicant orchestration path is primarily deterministic and does not rely on these tools in current flow

21. tests/test_orchestrator.py

- Unit tests for orchestrator behaviors:
  - onboarding gating
  - loan flow progression
  - application listing by serial
  - document summary replies
  - EMI estimate generation
  - draft save payload correctness

## 10) Applicant conversation state machine

Main stages used:

- idle
- application_lookup
- loan_purpose
- loan_amount
- loan_tenure
- loan_review
- loan_saved

Typical path:

1. User says apply for loan
2. Check onboarding completeness from profile snapshot
3. Ask loan type
4. Ask requested amount
5. Ask tenure
6. Show EMI estimate and ask confirmation/remarks
7. Save draft in backend and return navigation action

## 11) Admin conversation behavior

When role is Admin:

- Preload admin dashboard, applications, documents
- Build system prompt with this context
- Send prompt + user question to Groq client
- Return summarized answer with suggested quick replies

This provides flexibility for free-form admin queries while still grounding on actual current data.

## 12) Data persistence and scaling implications

Current persistence:

- Local file sessions.json

Pros:

- Very simple
- Easy to debug
- No external dependency

Cons:

- Not shared across multiple instances/pods
- Risk of race conditions with concurrent writes
- Data loss risk on container restart unless mounted volume

Recommended production upgrade:

- Redis or database-backed session store
- Session locking or optimistic concurrency
- Optional encryption at rest for sensitive fields

## 13) Security notes

What is good:

- JWT validation is present
- Non-root Docker user
- Env-based configuration

What needs attention:

- Relaxed JWT fallback (without aud/iss) can weaken strict validation if misused
- sessions.json may contain PII; secure file path, permissions, and retention policy needed
- DELETE /chat/session endpoint should actually delete current user sessions

## 14) What is currently not implemented or partially implemented

1. Session clear endpoint logic:

- Endpoint exists, but currently returns cleared status without deleting sessions

2. Tool-calling integration depth:

- Tool schemas exist, but applicant route mostly uses deterministic orchestration

3. Shared, production-grade memory store:

- Local JSON works for dev/small scale, not ideal for horizontal scale

## 15) Final summary

This chatbot is a hybrid design:

- Deterministic state machine for applicant workflows
- LLM-assisted reasoning for admin analytics questions
- Context memory persisted in SessionContext and sessions.json
- No model fine-tuning; customization is done in orchestration and prompts

Overall this is a solid architecture for controlled business workflows with AI augmentation, especially for early-stage or internal deployments. For production scale, session storage and strict auth hardening are the top next improvements.
