#!/usr/bin/env python3
import os
import re
import sys
from datetime import datetime

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BACKLOG_DIR = os.path.join(ROOT, "backlog")
OLD_DIR = os.path.join(BACKLOG_DIR, "old")
INDEX_PATH = os.path.join(BACKLOG_DIR, "INDEX.md")

FILENAME_RE = re.compile(r"^Item-(\d{3})-Prio-(P\d)-(.*)\.md$")

STATUS_READY = "🟢 Ready"
STATUS_IN_PROGRESS = "🟡 In Progress"
STATUS_BLOCKED = "🔴 Blocked"
STATUS_DONE = "✅ Done"


def parse_items(dirpath):
    items = {}
    for name in os.listdir(dirpath):
        if not name.startswith("Item-"):
            continue
        m = FILENAME_RE.match(name)
        if not m:
            continue
        item_id, prio, status = m.group(1), m.group(2), m.group(3)
        items.setdefault(item_id, []).append((prio, status, name))
    return items


def compute_counts():
    def norm(s):
        if STATUS_READY in s:
            return STATUS_READY
        if STATUS_IN_PROGRESS in s:
            return STATUS_IN_PROGRESS
        if STATUS_BLOCKED in s:
            return STATUS_BLOCKED
        if STATUS_DONE in s:
            return STATUS_DONE
        if "🟢" in s:
            return STATUS_READY
        if "🟡" in s:
            return STATUS_IN_PROGRESS
        if "🔴" in s:
            return STATUS_BLOCKED
        if "✅" in s:
            return STATUS_DONE
        return s

    active_items = parse_items(BACKLOG_DIR)
    old_items = parse_items(OLD_DIR) if os.path.isdir(OLD_DIR) else {}

    ready = sum(1 for vs in active_items.values() for _, s, _ in vs if norm(s) == STATUS_READY)
    blocked = sum(1 for vs in active_items.values() for _, s, _ in vs if norm(s) == STATUS_BLOCKED)
    inprog = sum(1 for vs in active_items.values() for _, s, _ in vs if norm(s) == STATUS_IN_PROGRESS)
    completed = sum(len(vs) for vs in old_items.values())

    total = sum(len(vs) for vs in active_items.values()) + completed
    active_count = sum(len(vs) for vs in active_items.values())

    return {
        "ready": ready,
        "blocked": blocked,
        "inprog": inprog,
        "completed": completed,
        "active": active_count,
        "total": total,
    }


def error(msg):
    print(f"[backlog-policy] ERROR: {msg}")


def main():
    rc = 0

    # Rule 1: No Done files in backlog/
    active_items = parse_items(BACKLOG_DIR)
    for item_id, variants in active_items.items():
        for prio, status, name in variants:
            if STATUS_DONE in status or "✅" in status:
                error(f"Done file in backlog/: {name}. Move it to backlog/old/.")
                rc = 1

    # Rule 2: One file per item in backlog/
    for item_id, variants in active_items.items():
        if len(variants) > 1:
            names = ", ".join(n for _, _, n in variants)
            error(f"Multiple status files for Item-{item_id} in backlog/: {names}. Keep only one.")
            rc = 1

    # Rule 3: INDEX.md counts match filesystem
    counts = compute_counts()
    if not os.path.exists(INDEX_PATH):
        error(f"INDEX.md not found at {INDEX_PATH}")
        rc = 1
    else:
        with open(INDEX_PATH, "r", encoding="utf-8") as f:
            text = f.read()
        # Total Items
        m_total = re.search(r"> \*\*Total Items:\*\* (\d+) \((\d+) Active, (\d+) Complete\)", text)
        if not m_total:
            error("INDEX.md missing Total Items header.")
            rc = 1
        else:
            total, active, complete = map(int, m_total.groups())
            if (total, active, complete) != (counts["total"], counts["active"], counts["completed"]):
                error(
                    f"INDEX.md totals mismatch. Expected Total={counts['total']} Active={counts['active']} Complete={counts['completed']}, found Total={total} Active={active} Complete={complete}."
                )
                rc = 1
        # Ready/Blocked/In Progress snapshot
        m_snap = re.search(
            r"\*\*Ready to Start:\*\* (\d+) items \| \*\*Blocked:\*\* (\d+) items \| \*\*In Progress:\*\* (\d+)",
            text,
        )
        if not m_snap:
            error("INDEX.md missing status snapshot.")
            rc = 1
        else:
            ready, blocked, inprog = map(int, m_snap.groups())
            if (ready, blocked, inprog) != (counts["ready"], counts["blocked"], counts["inprog"]):
                error(
                    f"INDEX.md status snapshot mismatch. Expected Ready={counts['ready']} Blocked={counts['blocked']} InProgress={counts['inprog']}, found Ready={ready} Blocked={blocked} InProgress={inprog}."
                )
                rc = 1
        # Completed Items total section
        m_comp = re.search(r"## ✅ Completed Items \((\d+) total\)", text)
        if not m_comp:
            error("INDEX.md missing Completed Items section heading.")
            rc = 1
        else:
            comp_total = int(m_comp.group(1))
            if comp_total != counts["completed"]:
                error(
                    f"INDEX.md completed total mismatch. Expected {counts['completed']}, found {comp_total}."
                )
                rc = 1

    if rc:
        print("[backlog-policy] Fail. See errors above.")
        sys.exit(1)
    else:
        print("[backlog-policy] OK.")
        sys.exit(0)


if __name__ == "__main__":
    main()
