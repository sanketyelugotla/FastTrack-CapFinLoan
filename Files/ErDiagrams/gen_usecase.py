"""
Generate a clean, square-layout Use Case Diagram using matplotlib.
"""
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch
import os

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "usecase_diagram.png")

fig, ax = plt.subplots(figsize=(16, 10))
ax.set_xlim(0, 16)
ax.set_ylim(0, 10)
ax.axis("off")
fig.patch.set_facecolor("#FAFBFF")

# ── Colour palette ──────────────────────────────────────────────────
NAVY        = "#1A234E"
BLUE        = "#2E4BAA"
UC_FILL     = "#EEF2FF"
UC_EDGE     = "#6B7FD4"
ACTOR_FILL  = "#2E4BAA"
ACTOR_TEXT  = "white"
BOX_EDGE    = "#1A234E"
GROUP_FILL  = "#F0F4FF"
GROUP_EDGE  = "#C5CADF"

# ── Helper: rounded rectangle ───────────────────────────────────────
def rounded_rect(ax, x, y, w, h, fc, ec, lw=1.2, radius=0.2, zorder=2):
    box = FancyBboxPatch((x, y), w, h,
                         boxstyle=f"round,pad=0,rounding_size={radius}",
                         facecolor=fc, edgecolor=ec, linewidth=lw, zorder=zorder)
    ax.add_patch(box)

def label(ax, x, y, text, size=9, color="#1C1C2E", weight="normal", zorder=5):
    ax.text(x, y, text, ha="center", va="center", fontsize=size,
            color=color, weight=weight, zorder=zorder,
            wrap=True, multialignment="center")

def actor_circle(ax, cx, cy, name, role_icon=""):
    circle = plt.Circle((cx, cy + 0.5), 0.42, facecolor=ACTOR_FILL,
                        edgecolor="white", linewidth=2, zorder=5)
    ax.add_patch(circle)
    ax.text(cx, cy + 0.5, role_icon, ha="center", va="center",
            fontsize=15, color="white", zorder=6)
    ax.text(cx, cy - 0.1, name, ha="center", va="top",
            fontsize=10, color=NAVY, weight="bold", zorder=6)

def use_case_box(ax, x, y, w, h, text):
    rounded_rect(ax, x, y, w, h, UC_FILL, UC_EDGE, lw=1.0)
    ax.text(x + w/2, y + h/2, text, ha="center", va="center",
            fontsize=8.5, color="#1C1C2E", zorder=5, multialignment="center")

def group_box(ax, x, y, w, h, title):
    rounded_rect(ax, x, y, w, h, GROUP_FILL, GROUP_EDGE, lw=1.4, radius=0.25, zorder=1)
    ax.text(x + w/2, y + h - 0.28, title, ha="center", va="center",
            fontsize=9, color=BLUE, weight="bold", zorder=3)

def arrow(ax, x1, y1, x2, y2):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="-|>", color="#4A6CF7",
                               lw=1.3, mutation_scale=12),
                zorder=4)

# ═══════════════════════════════════════════════════════════════════
# Title
ax.text(8, 9.6, "CapFinLoan — System Use Case Diagram", ha="center", va="center",
        fontsize=15, weight="bold", color=NAVY)

# ── ACTORS ──────────────────────────────────────────────────────────
actor_circle(ax, 1.1, 4.5, "Applicant", "A")
actor_circle(ax, 14.9, 4.5, "Admin", "Ad")

# ── GROUP BOXES ─────────────────────────────────────────────────────
#  Authentication  (top-left)
group_box(ax, 2.8, 6.9, 4.8, 2.4, "Authentication")
#  Loan Management  (bottom-left)
group_box(ax, 2.8, 3.4, 4.8, 3.2, "Loan Management")
#  Document Management (middle)
group_box(ax, 2.8, 0.4, 4.8, 2.7, "Document Management")
#  Admin Operations (right)
group_box(ax, 8.5, 2.8, 4.8, 6.5, "Admin Operations")
#  AI Assistant (bottom right small)
group_box(ax, 8.5, 0.4, 4.8, 2.1, "AI Assistant")

# ── USE CASE BOXES ───────────────────────────────────────────────────
BW, BH = 4.0, 0.6

# Authentication
use_case_box(ax, 3.1, 8.65, BW, BH, "Register + OTP Verification")
use_case_box(ax, 3.1, 7.95, BW, BH, "Login / Google SSO")
use_case_box(ax, 3.1, 7.25, BW, BH, "Reset Password")

# Loan Management
use_case_box(ax, 3.1, 6.30, BW, BH, "Calculate EMI")
use_case_box(ax, 3.1, 5.55, BW, BH, "Create / Save Draft Application")
use_case_box(ax, 3.1, 4.80, BW, BH, "Submit Application")
use_case_box(ax, 3.1, 4.05, BW, BH, "Track Application Status")

# Document Management
use_case_box(ax, 3.1, 2.70, BW, BH, "Upload Documents")
use_case_box(ax, 3.1, 1.95, BW, BH, "Replace Rejected Document")
use_case_box(ax, 3.1, 1.15, BW, BH, "View Document Status")

# Admin Operations
use_case_box(ax, 8.8, 8.65, BW, BH, "View Application Queue")
use_case_box(ax, 8.8, 7.95, BW, BH, "Review Applications")
use_case_box(ax, 8.8, 7.25, BW, BH, "Verify Documents")
use_case_box(ax, 8.8, 6.55, BW, BH, "Mark Document: Reupload Required")
use_case_box(ax, 8.8, 5.85, BW, BH, "Approve Loan Application")
use_case_box(ax, 8.8, 5.15, BW, BH, "Reject Loan Application")
use_case_box(ax, 8.8, 4.45, BW, BH, "Generate PDF Reports")
use_case_box(ax, 8.8, 3.75, BW, BH, "View Dashboard Metrics")
use_case_box(ax, 8.8, 3.05, BW, BH, "View Audit Logs")

# AI Assistant
use_case_box(ax, 8.8, 1.65, BW, BH, "Chat with AI Loan Bot")
use_case_box(ax, 8.8, 0.90, BW, BH, "Get Loan Guidance & Assistance")

# ── APPLICANT ARROWS ─────────────────────────────────────────────────
left_box_cx = 3.1   # left edge of left boxes
ap_x = 1.55

ap_targets_left = [8.95, 8.25, 7.55, 6.60, 5.85, 5.10, 4.35, 3.00, 2.25, 1.45]
ap_targets_right_y = [1.95, 1.20]   # AI bot

for y_t in ap_targets_left:
    arrow(ax, ap_x, 4.9, left_box_cx, y_t + 0.3)

for y_t in [1.95, 1.20]:
    arrow(ax, ap_x, 4.9, 8.8, y_t + 0.3)

# Login shared
arrow(ax, ap_x, 4.9, 8.8, 8.25 + 0.3)

# ── ADMIN ARROWS ─────────────────────────────────────────────────────
ad_x = 14.45

admin_targets = [8.95, 8.25, 7.55, 6.85, 6.15, 5.45, 4.75, 4.05, 3.35]
for y_t in admin_targets:
    arrow(ax, ad_x, 4.9, 13.6, y_t + 0.3)

# Admin also logs in (left box)
arrow(ax, ad_x, 4.9, 8.0, 8.25 + 0.3)

# ── SYSTEM BOUNDARY ──────────────────────────────────────────────────
sys_box = FancyBboxPatch((2.6, 0.2), 11.2, 9.2,
                          boxstyle="round,pad=0,rounding_size=0.3",
                          facecolor="none", edgecolor="#8890C0",
                          linewidth=2.0, linestyle="--", zorder=0)
ax.add_patch(sys_box)
ax.text(8.2, 9.52, "« System Boundary »", ha="center", va="center",
        fontsize=8, color="#8890C0", style="italic")

plt.tight_layout(pad=0.3)
plt.savefig(OUT, dpi=180, bbox_inches="tight", facecolor=fig.get_facecolor())
print(f"Saved: {OUT}")
