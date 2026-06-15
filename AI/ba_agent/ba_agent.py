"""
AI Business Analyst Agent — Gmail Auto-Responder
=================================================
Watches a Gmail inbox for emails from a target domain,
runs each email through the BA Agent LLM, and sends
the full requirement analysis as a reply.

Setup
-----
1.  pip install google-auth google-auth-oauthlib google-auth-httplib2
        google-api-python-client openai

2.  Create a Google Cloud project, enable the Gmail API, and download
    credentials.json (OAuth 2.0 Desktop App) into this directory.

3.  Set environment variables:
        OPENROUTER_API_KEY   → your OpenRouter key  (https://openrouter.ai)
        WATCH_DOMAIN         → e.g.  globaleexplorertours.com
        POLL_INTERVAL        → seconds between inbox checks (default 60)

4.  On first run you will be asked to authorise via browser.
    A token.json file is saved so subsequent runs are headless.
"""

import base64
import email as email_lib
import json
import logging
import os
import time
from pathlib import Path

from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from openai import OpenAI

# ── Logging ───────────────────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)
log = logging.getLogger("ba_agent")

# ── Configuration (via env vars or edit defaults here) ────────────────────────
OPENROUTER_API_KEY  = os.getenv("OPENROUTER_API_KEY", "YOUR_OPENROUTER_API_KEY")
OPENROUTER_BASE_URL = "https://openrouter.ai/api/v1"
LLM_MODEL           = "qwen/qwen3-32b"
LLM_MAX_TOKENS      = 4096
LLM_TEMPERATURE     = 0.3
LLM_TOP_P           = 0.9

WATCH_DOMAIN        = os.getenv("WATCH_DOMAIN", "globaleexplorertours.com")
POLL_INTERVAL       = int(os.getenv("POLL_INTERVAL", "60"))   # seconds

CREDENTIALS_FILE    = Path("credentials.json")   # OAuth2 desktop-app credentials
TOKEN_FILE          = Path("token.json")          # saved token (auto-created)
PROCESSED_FILE      = Path("processed_ids.json")  # tracks already-handled message IDs

GMAIL_SCOPES = [
    "https://www.googleapis.com/auth/gmail.readonly",
    "https://www.googleapis.com/auth/gmail.send",
    "https://www.googleapis.com/auth/gmail.modify",
]

# ── Master Prompt ─────────────────────────────────────────────────────────────
MASTER_PROMPT = """
You are an AI Business Analyst Agent operating simultaneously as:
Senior Business Analyst, Solution Architect, Technical Lead, QA Lead,
Product Owner, and Project Manager.

Your PRIMARY OBJECTIVE is to transform the client requirement below into a
COMPLETE requirement analysis package.

════════════════════════════════════════════════════════════════════
CLIENT REQUIREMENT:
════════════════════════════════════════════════════════════════════
{{CLIENT_REQUIREMENT}}
════════════════════════════════════════════════════════════════════

ANALYSIS RULES:
• Do NOT hallucinate requirements.
• Identify missing information and generate clarification questions.
• Mark all assumptions separately.
• Pay special attention to: Authentication & Authorization, Security,
  Performance, Scalability, Reporting, Audit, Data Management,
  Third-party Integrations, User Roles & Permissions, Mobile Compatibility,
  Regulatory/Compliance, Error Handling, and Edge Cases.
• OUTPUT FORMAT: plain text only. Do NOT use any Markdown syntax.
  No #, **, *, __, `, ---, or [text](url). Use plain section titles
  in UPPERCASE, and plain dashes or numbers for lists.

════════════════════════════════════════════════════════════════════
REQUIRED OUTPUT — follow EVERY section in order:
════════════════════════════════════════════════════════════════════

## 1. EXECUTIVE SUMMARY
Concise summary of the project and its business objective.

## 2. FUNCTIONAL REQUIREMENTS
Format: FR-001: Description

## 3. NON-FUNCTIONAL REQUIREMENTS
Categorise under Performance, Security, Scalability, Availability,
Reliability, Usability, Maintainability.

## 4. USER ROLES
For each role — Role Name / Responsibilities / Permissions.

## 5. USER STORIES
Format: As a <role> I want <feature> so that <business value>.

## 6. ACCEPTANCE CRITERIA
Measurable acceptance criteria for each user story.

## 7. ASSUMPTIONS
Numbered list of all assumptions.

## 8. RISKS
Categorise as Business / Technical / Operational / Security.
For each: Description, Impact (High/Medium/Low), Mitigation.

## 9. DEPENDENCIES
External APIs, third-party services, infrastructure, client resources.

## 10. MISSING INFORMATION
Missing requirements, business rules, workflows, integrations.

## 11. CLARIFICATION QUESTIONS FOR CLIENT
Numbered, each tagged [HIGH], [MEDIUM], or [LOW].

## 12. SUGGESTED TECHNOLOGY STACK
Frontend / Backend / Database / Authentication / Cloud / Monitoring / CI-CD
with reasoning for each choice.

## 13. DEVELOPMENT PHASES
Phase 1 Discovery  →  Phase 2 Design  →  Phase 3 Development
→  Phase 4 Testing  →  Phase 5 Deployment  →  Phase 6 Support
Key deliverables per phase.

## 14. HIGH-LEVEL EFFORT ESTIMATE
Complexity / Team Composition / Timeline / Key Assumptions.

## 15. PROJECT COMPLEXITY ASSESSMENT
Complexity drivers.
Confidence Score: X/10
Reasoning: ...

## 16. PROFESSIONAL CLIENT RESPONSE EMAIL
A professional consulting email that thanks the client, summarises
understanding, lists clarification questions, highlights assumptions,
and requests confirmation before estimation.

## 17. FINAL RECOMMENDATION
Next actions before development begins.
""".strip()


# ── Gmail Auth ────────────────────────────────────────────────────────────────
def get_gmail_service():
    """Authenticate with Gmail and return an authorised service object."""
    creds = None

    if TOKEN_FILE.exists():
        creds = Credentials.from_authorized_user_file(str(TOKEN_FILE), GMAIL_SCOPES)

    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            log.info("Refreshing Gmail token …")
            creds.refresh(Request())
        else:
            if not CREDENTIALS_FILE.exists():
                raise FileNotFoundError(
                    f"'{CREDENTIALS_FILE}' not found. Download it from "
                    "Google Cloud Console (OAuth 2.0 Desktop App)."
                )
            flow = InstalledAppFlow.from_client_secrets_file(
                str(CREDENTIALS_FILE), GMAIL_SCOPES
            )
            creds = flow.run_local_server(port=0)

        TOKEN_FILE.write_text(creds.to_json())
        log.info("Gmail token saved to %s", TOKEN_FILE)

    return build("gmail", "v1", credentials=creds)


# ── Processed-IDs Store ───────────────────────────────────────────────────────
def load_processed_ids() -> set:
    if PROCESSED_FILE.exists():
        return set(json.loads(PROCESSED_FILE.read_text()))
    return set()


def save_processed_id(msg_id: str, processed: set) -> None:
    processed.add(msg_id)
    PROCESSED_FILE.write_text(json.dumps(list(processed)))


# ── Email Helpers ─────────────────────────────────────────────────────────────
def fetch_unread_from_domain(service, domain: str) -> list:
    """Return unread messages whose sender matches *domain*."""
    query  = f"is:unread from:@{domain}"
    result = service.users().messages().list(userId="me", q=query).execute()
    return result.get("messages", [])


def get_message_detail(service, msg_id: str) -> dict:
    return service.users().messages().get(userId="me", id=msg_id, format="full").execute()


def extract_email_parts(msg: dict) -> tuple:
    """Return (from_addr, to_addr, subject, plain_text_body)."""
    headers   = {h["name"]: h["value"] for h in msg["payload"]["headers"]}
    from_addr = headers.get("From", "")
    to_addr   = headers.get("To", "")
    subject   = headers.get("Subject", "(no subject)")
    body      = _extract_body(msg["payload"])
    return from_addr, to_addr, subject, body


def _extract_body(payload: dict) -> str:
    """Recursively extract plain-text body from a Gmail message payload."""
    if payload.get("mimeType") == "text/plain":
        data = payload.get("body", {}).get("data", "")
        if data:
            return base64.urlsafe_b64decode(data).decode("utf-8", errors="replace")

    for part in payload.get("parts", []):
        result = _extract_body(part)
        if result:
            return result

    data = payload.get("body", {}).get("data", "")
    if data:
        return base64.urlsafe_b64decode(data).decode("utf-8", errors="replace")

    return ""


def mark_as_read(service, msg_id: str) -> None:
    service.users().messages().modify(
        userId="me",
        id=msg_id,
        body={"removeLabelIds": ["UNREAD"]},
    ).execute()


def send_reply(service, original_msg: dict, reply_body: str) -> None:
    """Compose and send a reply to the original message."""
    headers    = {h["name"]: h["value"] for h in original_msg["payload"]["headers"]}
    to_addr    = headers.get("From", "")
    subject    = headers.get("Subject", "")
    message_id = headers.get("Message-ID", "")
    thread_id  = original_msg.get("threadId", "")

    reply_subject = subject if subject.startswith("Re:") else f"Re: {subject}"

    raw_message = (
        f"To: {to_addr}\r\n"
        f"Subject: {reply_subject}\r\n"
        f"In-Reply-To: {message_id}\r\n"
        f"References: {message_id}\r\n"
        f"Content-Type: text/plain; charset=utf-8\r\n\r\n"
        f"{reply_body}"
    )

    encoded = base64.urlsafe_b64encode(raw_message.encode("utf-8")).decode("utf-8")
    service.users().messages().send(
        userId="me",
        body={"raw": encoded, "threadId": thread_id},
    ).execute()

    log.info("Reply sent to %s", to_addr)


# ── Markdown Stripper ────────────────────────────────────────────────────────
def strip_markdown(text: str) -> str:
    """Convert LLM markdown output to clean plain text."""
    import re

    # Remove bold / italic markers  (**text**, *text*, __text__, _text_)
    text = re.sub(r"\*{1,3}(.+?)\*{1,3}", r"\1", text)
    text = re.sub(r"_{1,3}(.+?)_{1,3}", r"\1", text)

    # Remove inline code  `code`
    text = re.sub(r"`(.+?)`", r"\1", text)

    # Remove fenced code blocks  ```lang\n...\n```
    text = re.sub(r"```[\w]*\n?(.*?)```", r"\1", text, flags=re.DOTALL)
    text = re.sub(r"```[\w]*\n?", "", text)   # catch any unclosed fences

    # Convert ATX headings  ## Heading  →  HEADING (uppercased, no #)
    text = re.sub(r"^#{1,6}\s+(.+)$", lambda m: m.group(1).upper(), text, flags=re.MULTILINE)

    # Remove horizontal rules  ---, ***, ___
    text = re.sub(r"^[-*_]{3,}\s*$", "", text, flags=re.MULTILINE)

    # Remove blockquote markers  > text
    text = re.sub(r"^\s*>\s?", "", text, flags=re.MULTILINE)

    # Convert unordered list bullets  - item / * item  →  • item
    text = re.sub(r"^\s*[-*+]\s+", "  * ", text, flags=re.MULTILINE)

    # Remove hyperlink syntax  [text](url)  →  text
    text = re.sub(r"\[(.+?)\]\(.+?\)", r"\1", text)

    # Collapse more than two consecutive blank lines
    text = re.sub(r"\n{3,}", "\n\n", text)

    return text.strip()


# ── LLM Analysis ──────────────────────────────────────────────────────────────
def run_ba_analysis(requirement: str) -> str:
    """Send the requirement to the LLM and return the analysis text."""
    llm    = OpenAI(api_key=OPENROUTER_API_KEY, base_url=OPENROUTER_BASE_URL)
    prompt = MASTER_PROMPT.replace("{{CLIENT_REQUIREMENT}}", requirement)

    completion = llm.chat.completions.create(
        model=LLM_MODEL,
        messages=[
            {"role": "system", "content": "You are an expert AI Business Analyst Agent."},
            {"role": "user",   "content": prompt},
        ],
        temperature=LLM_TEMPERATURE,
        max_completion_tokens=LLM_MAX_TOKENS,
        top_p=LLM_TOP_P,
    )

    return completion.choices[0].message.content


# ── Core Processing ───────────────────────────────────────────────────────────
def process_email(service, msg_stub: dict, processed: set) -> None:
    """Fetch, analyse, reply to, and mark one email as processed."""
    msg_id = msg_stub["id"]

    if msg_id in processed:
        return

    msg                        = get_message_detail(service, msg_id)
    from_addr, _, subject, body = extract_email_parts(msg)

    if not body.strip():
        log.warning("Empty body in message %s — skipping.", msg_id)
        save_processed_id(msg_id, processed)
        return

    log.info("Processing email from %s | Subject: %s", from_addr, subject)

    analysis = strip_markdown(run_ba_analysis(body))

    reply = (
        "Dear Client,\n\n"
        "Thank you for reaching out. "
        "Please find the full Business Analyst requirement analysis below.\n\n"
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n"
        f"{analysis}\n\n"
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n"
        "Please review and revert with your clarifications so we can "
        "proceed to the estimation phase.\n\n"
        "Best regards,\n"
        "AI Business Analyst Agent\n"
        "Tech Team"
    )

    send_reply(service, msg, reply)
    mark_as_read(service, msg_id)
    save_processed_id(msg_id, processed)
    log.info("Email %s handled successfully.", msg_id)


def watch_inbox(service) -> None:
    """Poll Gmail every POLL_INTERVAL seconds for emails from WATCH_DOMAIN."""
    log.info(
        "Watching inbox for emails from @%s  (poll every %ds) …",
        WATCH_DOMAIN, POLL_INTERVAL,
    )
    processed = load_processed_ids()

    while True:
        try:
            messages = fetch_unread_from_domain(service, WATCH_DOMAIN)
            if messages:
                log.info("Found %d unread message(s) from @%s.", len(messages), WATCH_DOMAIN)
                for stub in messages:
                    process_email(service, stub, processed)
            else:
                log.debug("No new messages from @%s.", WATCH_DOMAIN)
        except Exception as exc:
            log.error("Error during inbox poll: %s", exc, exc_info=True)

        time.sleep(POLL_INTERVAL)


# ── Entry Point ───────────────────────────────────────────────────────────────
if __name__ == "__main__":
    gmail = get_gmail_service()
    watch_inbox(gmail)