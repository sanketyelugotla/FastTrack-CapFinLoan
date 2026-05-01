"""Tool definitions exposed to the LLM for function-calling.

Each tool is a JSON schema in the OpenAI tool-calling format that Groq accepts.
"""

TOOL_DEFINITIONS: list[dict] = [
    {
        "type": "function",
        "function": {
            "name": "check_eligibility",
            "description": "Estimates loan eligibility based on income, existing EMI, and requested amount. Returns an estimated eligible amount range.",
            "parameters": {
                "type": "object",
                "properties": {
                    "monthly_income": {
                        "type": "number",
                        "description": "Applicant's monthly income in INR",
                    },
                    "existing_emi": {
                        "type": "number",
                        "description": "Total existing monthly EMI obligations in INR (0 if none)",
                    },
                    "requested_amount": {
                        "type": "number",
                        "description": "Loan amount the applicant wants in INR",
                    },
                    "loan_type": {
                        "type": "string",
                        "description": "Type of loan (e.g. Personal Loan, Home Loan)",
                    },
                },
                "required": ["monthly_income"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_application_status",
            "description": "Gets the current status and timeline of the user's loan application.",
            "parameters": {
                "type": "object",
                "properties": {
                    "application_id": {
                        "type": "string",
                        "description": "The application ID to check. If not provided, checks the most recent one.",
                    },
                },
                "required": [],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "update_loan_details",
            "description": "Extracts and saves loan application details from the user's natural language input. Call this whenever the user provides one or more fields.",
            "parameters": {
                "type": "object",
                "properties": {
                    "loan_type": {"type": "string", "description": "Type of loan requested"},
                    "monthly_income": {"type": "number", "description": "Monthly income in INR"},
                    "loan_amount": {"type": "number", "description": "Requested loan amount in INR"},
                    "tenure_months": {"type": "integer", "description": "Loan tenure in months"},
                    "existing_emi": {"type": "number", "description": "Existing monthly EMI in INR"},
                    "employer_name": {"type": "string", "description": "Name of employer/company"},
                    "employment_type": {"type": "string", "description": "Type of employment (Salaried, Self-employed, etc.)"},
                },
                "required": [],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "confirm_profile_reuse",
            "description": "Call this to lock in whether the user wants to use their existing prefilled profile details or enter new ones.",
            "parameters": {
                "type": "object",
                "properties": {
                    "use_existing": {
                        "type": "boolean",
                        "description": "True if user wants to use existing profile details, False if they want to provide new ones.",
                    },
                },
                "required": ["use_existing"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "confirm_document_reuse",
            "description": "Call this to lock in whether the user wants to use their existing prefilled documents or upload new ones.",
            "parameters": {
                "type": "object",
                "properties": {
                    "use_existing": {
                        "type": "boolean",
                        "description": "True if user wants to use existing documents, False to upload new ones.",
                    },
                },
                "required": ["use_existing"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "save_draft_application",
            "description": "Saves the currently collected loan details to the backend as a Draft application. Call this when all required fields are collected before asking for final submission.",
            "parameters": {
                "type": "object",
                "properties": {},
                "required": [],
            },
        },
    },

    {
        "type": "function",
        "function": {
            "name": "request_document_upload",
            "description": "Guides the user to upload required documents for their application. Call when documents need to be uploaded.",
            "parameters": {
                "type": "object",
                "properties": {
                    "document_type": {
                        "type": "string",
                        "description": "Type of document needed: IdProof, AddressProof, IncomeProof, BankStatement",
                        "enum": ["IdProof", "AddressProof", "IncomeProof", "BankStatement"],
                    },
                },
                "required": ["document_type"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "escalate_to_human",
            "description": "Escalates the conversation to a human agent.",
            "parameters": {
                "type": "object",
                "properties": {
                    "reason": {
                        "type": "string",
                        "description": "Brief reason for escalation",
                    },
                },
                "required": [],
            },
        },
    },
]
