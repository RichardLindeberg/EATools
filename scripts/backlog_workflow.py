#!/usr/bin/env python3
import os
import re
import sys
import shutil
from datetime import datetime

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BACKLOG_DIR = os.path.join(ROOT, "backlog")
OLD_DIR = os.path.join(BACKLOG_DIR, "old")
INDEX_PATH = os.path.join(BACKLOG_DIR, "INDEX.md")

STATUS_READY = "🟢 Ready"
STATUS_IN_PROGRESS = "🟡 In Progress"
STATUS_BLOCKED = "🔴 Blocked"
STATUS_DONE = "✅ Done"

FILENAME_RE = re.compile(r"^Item-(\d{3})-Prio-(P\d)-(.*)\.md$")


def find_item_file(item_id: str) -> str | None:
    # Search in backlog/ only (active files)
    for name in os.listdir(BACKLOG_DIR):
        if not name.startswith("Item-"):
            continue
        m = FILENAME_RE.match(name)
        if not m:
            continue
        if m.group(1) == item_id:
            return os.path.join(BACKLOG_DIR, name)
    return None


def compute_counts():
    def parse_dir(path):
        items = []
        for name in os.listdir(path):
            if not name.startswith("Item-"):
                continue
            m = FILENAME_RE.match(name)
            if not m:
                continue
            item_id, prio, status = m.group(1), m.group(2), m.group(3)
            items.append((item_id, prio, status))
        return items

    active = parse_dir(BACKLOG_DIR)
    completed = parse_dir(OLD_DIR) if os.path.isdir(OLD_DIR) else []

    # Normalize status tokens to known values
    def norm(s):
        if STATUS_READY in s:
            return STATUS_READY
        if STATUS_IN_PROGRESS in s:
            return STATUS_IN_PROGRESS
        if STATUS_BLOCKED in s:
            return STATUS_BLOCKED
        if STATUS_DONE in s:
            return STATUS_DONE
        # fallback: try matching emoji only
        if "🟢" in s:
            return STATUS_READY
        if "🟡" in s:
            return STATUS_IN_PROGRESS
        if "🔴" in s:
            return STATUS_BLOCKED
        if "✅" in s:
            return STATUS_DONE
        return s

    ready = sum(1 for _, _, s in active if norm(s) == STATUS_READY)
    blocked = sum(1 for _, _, s in active if norm(s) == STATUS_BLOCKED)
    inprog = sum(1 for _, _, s in active if norm(s) == STATUS_IN_PROGRESS)
    done = len(completed)

    total = len(active) + len(completed)
    active_count = len(active)
    completed_count = len(completed)
    progress_pct = int(round((completed_count / total) * 100)) if total else 0

    return {
        "ready": ready,
        "blocked": blocked,
        "inprog": inprog,
        "done": done,
        "total": total,
        "active": active_count,
        "completed": completed_count,
        "progress_pct": progress_pct,
    }


def update_index():
    counts = compute_counts()
    if not os.path.exists(INDEX_PATH):
        print(f"INDEX not found: {INDEX_PATH}")
        return
    with open(INDEX_PATH, "r", encoding="utf-8") as f:
        text = f.read()

    # Update header lines
    today = datetime.today().strftime("%Y-%m-%d")
    text = re.sub(r"(> \*\*Last Updated:\*\* )\d{4}-\d{2}-\d{2}", rf"\1{today}", text)
    text = re.sub(
        r"(> \*\*Total Items:\*\* )\d+ \(\d+ Active, \d+ Complete\)",
        rf"\1{counts['total']} ({counts['active']} Active, {counts['completed']} Complete)",
        text,
    )
    text = re.sub(
        r"(> \*\*Progress:\*\* )\d+/\d+ complete \(\d+%\)",
        rf"\1{counts['completed']}/{counts['total']} complete ({counts['progress_pct']}%)",
        text,
    )
    text = re.sub(
        r"\*\*Ready to Start:\*\* \d+ items \| \*\*Blocked:\*\* \d+ items \| \*\*In Progress:\*\* \d+",
        rf"**Ready to Start:** {counts['ready']} items | **Blocked:** {counts['blocked']} items | **In Progress:** {counts['inprog']}",
        text,
    )
    text = re.sub(
        r"## ✅ Completed Items \(\d+ total\)",
        rf"## ✅ Completed Items ({counts['completed']} total)",
        text,
    )

    with open(INDEX_PATH, "w", encoding="utf-8") as f:
        f.write(text)

    print(
        f"Updated INDEX.md: ready={counts['ready']}, blocked={counts['blocked']}, in-progress={counts['inprog']}, completed={counts['completed']}, total={counts['total']}"
    )


def set_status(item_id: str, new_status: str):
    path = find_item_file(item_id)
    if not path:
        print(f"Item {item_id} not found in backlog/")
        sys.exit(1)

    m = FILENAME_RE.match(os.path.basename(path))
    prio = m.group(2)
    new_name = f"Item-{item_id}-Prio-{prio}-{new_status}.md"
    new_path = os.path.join(BACKLOG_DIR, new_name)

    # Rename within backlog
    os.rename(path, new_path)
    print(f"Renamed: {os.path.basename(path)} -> {new_name}")

    update_index()


def start_item(item_id: str):
    set_status(item_id, STATUS_IN_PROGRESS)


def complete_item(item_id: str):
    path = find_item_file(item_id)
    if not path:
        print(f"Item {item_id} not found in backlog/")
        sys.exit(1)
    m = FILENAME_RE.match(os.path.basename(path))
    prio = m.group(2)
    # Rename to Done before moving
    done_name = f"Item-{item_id}-Prio-{prio}-{STATUS_DONE}.md"
    done_path = os.path.join(BACKLOG_DIR, done_name)
    os.rename(path, done_path)
    print(f"Marked Done: {os.path.basename(path)} -> {done_name}")
    # Move to old/
    os.makedirs(OLD_DIR, exist_ok=True)
    target = os.path.join(OLD_DIR, done_name)
    shutil.move(done_path, target)
    print(f"Moved to old/: {done_name}")
    update_index()


def usage():
    print(
        """
Backlog Workflow CLI

Usage:
  python scripts/backlog_workflow.py start <id>      # Set item to 🟡 In Progress
  python scripts/backlog_workflow.py complete <id>   # Set item to ✅ Done and move to backlog/old/
  python scripts/backlog_workflow.py status <id> <STATUS>  # STATUS: "🟢 Ready" | "🟡 In Progress" | "🔴 Blocked"
  python scripts/backlog_workflow.py update-index    # Recalculate counts and update backlog/INDEX.md
"""
    )


def main():
    if len(sys.argv) < 2:
        usage()
        sys.exit(1)
    cmd = sys.argv[1]
    if cmd == "start" and len(sys.argv) == 3:
        start_item(sys.argv[2])
    elif cmd == "complete" and len(sys.argv) == 3:
        complete_item(sys.argv[2])
    elif cmd == "status" and len(sys.argv) == 4:
        status = sys.argv[3]
        if status not in (STATUS_READY, STATUS_IN_PROGRESS, STATUS_BLOCKED):
            print("Invalid STATUS. Use one of: 🟢 Ready | 🟡 In Progress | 🔴 Blocked")
            sys.exit(1)
        set_status(sys.argv[2], status)
    elif cmd == "update-index":
        update_index()
    else:
        usage()
        sys.exit(1)


if __name__ == "__main__":
    main()
