"""
Convert the "Gym Progress" Google Sheets export (one sheet per month) into the
JSON that DomainDbSeeder uses to seed the development user's workout history.

Usage:
    python tools/gym-history/import_gym_history.py "<path to Gym Progress.xlsx>"

Writes src/Bloom.Infrastructure/Persistence/EntityFramework/Seeders/Data/workout-history.json
and prints a report of everything it mapped, inferred or skipped.

Requires: openpyxl (pip install openpyxl)

Sheet format (per month): blocks of
    <dd/mm/yyyy> | <split>
    Exercise | Sets | Reps | Weight | PR? | Notes
    <name>   | 3    | 8    | 60/80/75 | 80 | free text
separated by blank rows. Columns B–G; everything to the right is statistics.
"""
from __future__ import annotations

import collections
import datetime as dt
import json
import re
import sys
from pathlib import Path

import openpyxl

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "src/Bloom.Infrastructure/Persistence/EntityFramework/Seeders/Data/workout-history.json"

MONTHS_NL = {
    "januari": 1, "februari": 2, "maart": 3, "april": 4, "mei": 5, "juni": 6, "juli": 7,
    "augustus": 8, "september": 9, "oktober": 10, "november": 11, "december": 12,
}

# Sheet spelling (lower-case, whitespace collapsed) → catalog exercise name.
EXERCISE_MAP = {
    # chest
    "bench press": "Bench Press",
    "decline bench press": "Decline Bench Press",
    "incline dumbbell press": "Incline Dumbbell Press",
    "incline dumbbel press": "Incline Dumbbell Press",
    "incline dumbbell pres": "Incline Dumbbell Press",
    "inclne dumbbell press": "Incline Dumbbell Press",
    "inline dumbbell press": "Incline Dumbbell Press",
    "incline dumbbell chest press": "Incline Dumbbell Press",
    "incline chest press": "Chest Press",
    "chest press": "Chest Press",
    "chest fly": "Chest Fly",
    "peck fly": "Chest Fly",
    "cable fly": "Cable Crossover",
    "incline smith press": "Smith Machine Bench Press",
    "smith incline press": "Smith Machine Bench Press",
    "smith incline bench press": "Smith Machine Bench Press",
    "dip": "Dip",
    "dips": "Dip",
    "seated dip": "Dip",
    "push up": "Push Up",
    # shoulders
    "overhead shoulder press": "Overhead Press",
    "shoulder press": "Overhead Press",
    "militairy shoulder press": "Overhead Press",
    "dumbbell shoulder press": "Dumbbell Shoulder Press",
    "dumbell shoulder press": "Dumbbell Shoulder Press",
    "overhead dumbbell shoulder press": "Dumbbell Shoulder Press",
    "overhead dumbbell press": "Dumbbell Shoulder Press",
    "dumbbell overhead shoulder press": "Dumbbell Shoulder Press",
    "machine shoulder press": "Machine Shoulder Press",
    "machines shoulder press": "Machine Shoulder Press",
    "lateral raise": "Lateral Raise",
    "lateral press": "Lateral Raise",
    "cable lateral raise": "Cable Lateral Raise",
    "rear delt": "Reverse Flye",
    "rear delts": "Reverse Flye",
    "reverse fly": "Reverse Flye",
    "rear delt (cable)": "Cable Reverse Fly",
    "face pull": "Face Pull",
    # back
    "lat pulldown": "Lat Pulldown",
    "lat pulldown close": "Close Grip Lat Pulldown",
    "lat pulldown neutral": "Neutral Grip Lat Pulldown",
    "cross body lat pull-around": "Lat Pulldown",
    "seated cable row": "Seated Cable Row",
    "cable row": "Seated Cable Row",
    "seated row": "Seated Cable Row",
    "horizontal row": "Seated Cable Row",
    "horizontal cable row": "Seated Cable Row",
    "machine row": "Machine Row",
    "seated machine row": "Machine Row",
    "low row": "Machine Row",
    "machine low row": "Machine Row",
    "converging high machine row": "Machine Row",
    "chest supported row": "Chest Supported Row",
    "chest suppoprted row": "Chest Supported Row",
    "t-bar row": "T-bar Row",
    "bent over row": "Barbell Row",
    "pull up": "Pull Up",
    "pull ups": "Pull Up",
    "deadlift": "Deadlift",
    "back extension": "Back Extension",
    "lower back extension": "Back Extension",
    # arms
    "preacher curl": "Preacher Curl",
    "machine preacher curl": "Preacher Curl",
    "preacher curl (machine)": "Preacher Curl",
    "cable curl": "Cable Curl",
    "bicep cable curl": "Cable Curl",
    "dumbbell curl": "Dumbbell Curl",
    "dumbbell bicep curl": "Dumbbell Curl",
    "db bicep curl": "Dumbbell Curl",
    "bicep curl": "Dumbbell Curl",
    "hammer curl": "Hammer Curl",
    "cable hammer curl": "Cable Hammer Curl",
    "cable hammer curls": "Cable Hammer Curl",
    "incline dumbbell curl": "Incline Dumbbell Curl",
    "incline dumbell curl": "Incline Dumbbell Curl",
    "incline bicep curl": "Incline Dumbbell Curl",
    "bayesian curl": "Bayesian Curl",
    "forearm curl": "Forearm Curl",
    "forearm curls": "Forearm Curl",
    "forarm curl": "Forearm Curl",
    "forearm cable curl": "Forearm Curl",
    "reverse forearm curl": "Reverse Forearm Curl",
    "tricep pushdown": "Tricep Pushdown",
    "tricep pulldown": "Tricep Pushdown",
    "tricep pulldown (bar)": "Tricep Pushdown",
    "overhead tricep extension": "Overhead Tricep Extension",
    "overhead tricep extenion": "Overhead Tricep Extension",
    "overhead tricep extention": "Overhead Tricep Extension",
    "overhead tricep pushdown": "Overhead Tricep Extension",
    "tricep extension": "Overhead Tricep Extension",
    "tricep extention": "Overhead Tricep Extension",
    "tricep overhead extention": "Overhead Tricep Extension",
    "skullcrusher": "Skull Crusher",
    # legs
    "squat": "Squat",
    "barbell squat": "Squat",
    "hack squat": "Hack Squat",
    "leg press": "Leg Press",
    "leg extension": "Leg Extension",
    "leg extensions": "Leg Extension",
    "leg extention": "Leg Extension",
    "leg extensoin": "Leg Extension",
    "leg curl": "Leg Curl",
    "lying leg curl": "Lying Leg Curl",
    "adductor": "Hip Adductor",
    "adduction": "Hip Adductor",
    "adductor & abductor": "Hip Adductor",
    "adductor / abductor": "Hip Adductor",
    "abductor": "Hip Abductor",
    "abduction": "Hip Abductor",
    "adbuctor": "Hip Abductor",
    "calf raise": "Standing Calf Raise",
    "calf curl": "Standing Calf Raise",
    "calf press": "Calf Press",
    # core
    "sit up": "Sit Up",
    "crunch": "Crunch",
    "crunches": "Crunch",
    "crunch machine": "Machine Crunch",
    "machine crunch": "Machine Crunch",
    "weighted crunch": "Machine Crunch",
    "weighted crunches": "Machine Crunch",
    "leg raise": "Hanging Leg Raise",
    "leg raise (l-sit)": "L-Sit",
    "plank": "Plank",
    "russian twist": "Russian Twist",
    # cardio
    "joggen": "Treadmill Run",
    "run": "Treadmill Run",
    "incline walking": "Walking",
    "tredmill incline walk": "Walking",
    "incline tredmill": "Walking",
    "bouldering": "Bouldering",
    "indoor snowboarding": "Indoor Snowboarding",
}

# Names with free-form suffixes are matched on their prefix.
PREFIX_MAP = {
    "cycle": "Road Cycling",
    "indoor snowboard": "Indoor Snowboarding",
}

CARDIO = {"Treadmill Run", "Walking", "Road Cycling", "Bouldering", "Indoor Snowboarding"}

# Exercises where "/" or an empty weight means "no added weight" (stored as 0 kg).
# For anything else an empty weight means it simply wasn't recorded.
BODYWEIGHT = {"Pull Up", "Dip", "Push Up", "Sit Up", "Crunch", "Hanging Leg Raise", "L-Sit",
              "Plank", "Russian Twist", "Back Extension"}

# Reps to assume when weights were logged without reps and there is no earlier session to copy.
DEFAULT_REPS = 8

# Spreadsheet noise rather than exercises.
IGNORE = {"the language entered is not supported at this time."}

# Notes that identify a variant, kept on the exercise when a name was merged.
MERGE_NOTES = {
    "adductor & abductor": "Adductor and abductor machine",
    "adductor / abductor": "Adductor and abductor machine",
    "incline chest press": "Incline machine",
    "seated dip": "Seated dip machine",
    "lower back extension": "Lower back machine",
    "cross body lat pull-around": "Cross-body lat pull-around",
    "machine preacher curl": "Machine",
    "preacher curl (machine)": "Machine",
    "incline smith press": "Incline",
    "smith incline press": "Incline",
    "smith incline bench press": "Incline",
    "calf curl": "Logged as calf curl",
    "lateral press": "Logged as lateral press",
    "rear delt": "Rear delt machine",
    "rear delts": "Rear delt machine",
    "tricep pulldown (bar)": "Bar attachment",
    "leg raise": "Roman chair",
}

DATE_RE = re.compile(r"^(\d{1,2})/(\d{1,2})/(\d{4})$")
NUM_RE = re.compile(r"-?\d+(?:[.,]\d+)?")
UNKNOWN_WEIGHT = re.compile(r"idk|clue|random", re.I)


def norm(name: str) -> str:
    return re.sub(r"\s+", " ", name.strip().lower())


def to_float(s: str) -> float:
    return float(s.replace(",", "."))


def catalog_name(raw: str) -> str | None:
    key = norm(raw)
    if key in EXERCISE_MAP:
        return EXERCISE_MAP[key]
    for prefix, name in PREFIX_MAP.items():
        if key.startswith(prefix):
            return name
    return None


def parse_reps(value) -> tuple[int | None, str | None]:
    """Returns (reps, note). Excel turned ranges like "6-8" into dates (6 Aug)."""
    if value is None:
        return None, None
    if isinstance(value, (int, float)):
        return (int(value) if value > 0 else None), None
    if isinstance(value, dt.datetime):
        lo, hi = sorted((value.day, value.month))
        return lo, f"{lo}–{hi} reps"
    s = str(value).strip()
    m = re.match(r"^(\d+)\s*-\s*(\d+)$", s)
    if m:
        lo, hi = sorted((int(m[1]), int(m[2])))
        return lo, f"{lo}–{hi} reps"
    if re.match(r"^\d+(\.0)?$", s):
        return int(float(s)), None
    return None, None


class Weights:
    def __init__(self):
        self.values: list[float] = []
        self.drop_from: int | None = None   # index of the first drop-set value
        self.trailing_drop = False
        self.failure = False
        self.bodyweight = False
        self.unknown = False
        self.notes: list[str] = []


def parse_weights(value) -> Weights:
    w = Weights()
    if value is None or (isinstance(value, str) and not value.strip()):
        w.bodyweight = True
        return w
    if isinstance(value, (int, float)):
        w.values = [float(value)]
        return w
    if isinstance(value, dt.datetime):  # "10/10" became 10 October
        w.values = [float(value.day), float(value.month)]
        return w
    s = str(value).strip()
    if s in ("/", "BW") or s.lower() == "bw":
        w.bodyweight = True
        return w
    if UNKNOWN_WEIGHT.search(s):
        w.unknown = True
        return w
    for raw in s.split("/"):
        t = raw.strip().rstrip(",").strip()
        low = t.lower()
        if not t:
            continue
        if "drop" in low:
            nums = NUM_RE.findall(t)
            w.values.extend(to_float(n) for n in nums)
            if w.drop_from is None:
                w.drop_from = len(w.values)
            continue
        if "failure" in low:
            w.failure = True
            continue
        if t.startswith("(") and t.endswith(")"):
            w.notes.append(f"Attempted {t.strip('()')} kg")
            continue
        nums = NUM_RE.findall(t)
        if not nums:
            continue
        if "->" in t:
            w.notes.append(f"Switched {t.replace('->', ' → ')} kg")
        v = to_float(nums[0])
        if v < 0:
            w.notes.append(f"Assisted ({v:g} kg)")
            v = 0.0
        w.values.append(v)
    if w.drop_from is not None and w.drop_from >= len(w.values):
        w.trailing_drop = True
        w.drop_from = None
    if not w.values and not w.unknown:
        w.bodyweight = True
    return w


def parse_minutes(text: str) -> float | None:
    m = re.search(r"(\d{1,2}):(\d{2})\s*-\s*(\d{1,2}):(\d{2})", text)  # 17:00 - 21:00
    if m:
        start = int(m[1]) * 60 + int(m[2])
        end = int(m[3]) * 60 + int(m[4])
        return float(end - start)
    m = re.search(r"(\d+)\s*min", text)
    if m:
        return float(m[1])
    m = re.search(r"\b(\d{1,2}):(\d{2})\b", text)  # mm:ss
    if m:
        return int(m[1]) + int(m[2]) / 60
    return None


def parse_cardio(raw_name: str, notes: str) -> dict:
    text = f"{raw_name} {notes}"
    km = None
    m = re.search(r"(\d+(?:[.,]\d+)?)\s*km", text, re.I)
    if m:
        km = to_float(m[1])
    minutes = None
    pace = re.search(r"avg\s*(\d+):(\d{2})", text)
    if pace and km:
        minutes = km * (int(pace[1]) + int(pace[2]) / 60)
    if minutes is None:
        minutes = parse_minutes(notes) if notes else parse_minutes(raw_name)
    speed = re.search(r"(\d+[.,]\d+)\s*(?:speed|km/h)", text) or re.search(r"°\s*(\d+[.,]\d+)", text)
    if km is None and speed and minutes:
        km = to_float(speed[1]) * minutes / 60
    note_parts = [notes] if notes else []
    if "->" in raw_name or "|" in raw_name:
        note_parts.insert(0, raw_name.split("|", 1)[-1].strip())
    if minutes is None and km:
        minutes = km / 25 * 60
        note_parts.append("Duration not recorded — estimated at 25 km/h")
    if minutes is None:
        minutes = 1.0
        note_parts.append("Duration not recorded")
    if km is None:
        km = 0.01
        note_parts.append("Distance not recorded")
    return {"minutes": round(minutes, 1), "km": round(km, 2), "note": "; ".join(p for p in note_parts if p) or None}


def sheet_month(title: str) -> tuple[int, int]:
    name, year = title.rsplit(" ", 1)
    return MONTHS_NL[name.strip().lower()], int(year)


def main(path: str) -> None:
    wb = openpyxl.load_workbook(path, read_only=True, data_only=True)
    workouts: list[dict] = []
    used = collections.Counter()
    unmapped = collections.Counter()
    skipped: list[str] = []
    inferred: list[str] = []
    recent_reps: dict[str, list[int]] = collections.defaultdict(list)  # sheets are chronological
    today = dt.date.today()

    for ws in wb.worksheets:
        parts = ws.title.rsplit(" ", 1)
        if len(parts) != 2 or not parts[1].isdigit() or parts[0].strip().lower() not in MONTHS_NL:
            continue
        month, year = sheet_month(ws.title)
        current: dict | None = None
        for row in ws.iter_rows(min_col=2, max_col=7, values_only=True):
            name, sets, reps, weight, _pr, notes = row
            text = str(name).strip() if name is not None else ""
            m = DATE_RE.match(text)
            if m:
                d, mo, y = int(m[1]), int(m[2]), int(m[3])
                if not 1 <= mo <= 12:
                    inferred.append(f"{text}: invalid month, used the sheet's ({ws.title})")
                    mo = month
                if mo == month and y != year:
                    inferred.append(f"{text}: year typo, used the sheet's ({ws.title})")
                    y = year
                if dt.date(y, mo, d) > today:
                    current = None  # future placeholder block
                    continue
                current = {
                    "date": dt.date(y, mo, d).isoformat(),
                    "name": str(sets).strip() if sets else "",
                    "note": None,
                    "exercises": [],
                    "_missing": [],
                }
                workouts.append(current)
                continue
            if current is None or not text or text == "Exercise":
                continue
            if norm(text) in IGNORE:
                continue
            exercise = catalog_name(text)
            if exercise is None:
                unmapped[text] += 1
                current["_missing"].append(text)
                continue
            used[(text, exercise)] += 1
            # Durations typed as "1:05" (m:ss) come back from Excel as 01:05:00 (h:mm).
            note_text = f"{notes.hour}:{notes.minute:02d}" if isinstance(notes, dt.time) else (str(notes).strip() if notes else "")
            if exercise in CARDIO:
                c = parse_cardio(text, note_text)
                current["exercises"].append({"exercise": exercise, "note": c["note"], "cardio": {"minutes": c["minutes"], "km": c["km"]}})
                continue

            rep_count, rep_note = parse_reps(reps)
            w = parse_weights(weight)
            if w.bodyweight and exercise not in BODYWEIGHT:
                current["_missing"].append(f"{text} (weight not recorded)")
                skipped.append(f"{current['date']} {text}: no weight")
                continue
            notes_out = [MERGE_NOTES.get(norm(text))] + ([note_text] if note_text else []) + ([rep_note] if rep_note else []) + w.notes
            if exercise == "Plank" and note_text:
                rep_count = rep_count or 1
                notes_out = [f"Held {note_text}"]
            if w.unknown:
                current["_missing"].append(f"{text} (weight not recorded)")
                skipped.append(f"{current['date']} {text}: weight {weight!r}")
                continue
            if rep_count is None:
                if w.bodyweight:
                    current["_missing"].append(f"{text} (reps not recorded)")
                    skipped.append(f"{current['date']} {text}: bodyweight without reps")
                    continue
                # Weights were logged but reps weren't: keep the weights, copy reps from last time.
                history = recent_reps[exercise][-5:]
                if history:
                    counts = collections.Counter(history)
                    rep_count = max(counts, key=lambda r: (counts[r], r))  # most common, ties → higher
                    source = "your usual reps"
                else:
                    rep_count, source = DEFAULT_REPS, "default"
                notes_out.append(f"Reps not recorded — assumed {rep_count} ({source})")
                inferred.append(f"{current['date']} {text}: reps assumed {rep_count} ({source})")
            else:
                recent_reps[exercise].append(rep_count)

            set_count = int(sets) if isinstance(sets, (int, float)) and sets > 0 else max(1, len(w.values))
            if w.bodyweight:
                values = [0.0] * set_count
                notes_out.append("Bodyweight")
            elif len(w.values) == 1 and set_count > 1:
                values = w.values * set_count
            else:
                values = w.values
            out_sets = []
            for i, v in enumerate(values):
                marker = "DropSet" if w.drop_from is not None and i >= w.drop_from else None
                out_sets.append({"reps": rep_count, "weight": round(v, 2), "marker": marker})
            if w.failure and out_sets:
                out_sets[-1]["marker"] = "Failure"
            if w.trailing_drop:
                notes_out.append("Finished with a drop set")
            current["exercises"].append({
                "exercise": exercise,
                "note": "; ".join(dict.fromkeys(n for n in notes_out if n)) or None,
                "sets": out_sets,
            })

    # Finalise: names, notes, drop empty days and exact duplicates.
    result, seen = [], set()
    for wkt in workouts:
        missing = wkt.pop("_missing")
        if missing:
            wkt["note"] = "Also logged: " + "; ".join(missing)
        if not wkt["exercises"]:
            skipped.append(f"{wkt['date']} {wkt['name']}: nothing recorded")
            continue
        if not wkt["name"]:
            types = {e["exercise"] for e in wkt["exercises"]}
            wkt["name"] = next(iter(types)) if len(types) == 1 else "Workout"
        key = json.dumps({k: wkt[k] for k in ("date", "name", "exercises")}, sort_keys=True)
        if key in seen:
            skipped.append(f"{wkt['date']} {wkt['name']}: exact duplicate of another block")
            continue
        seen.add(key)
        result.append(wkt)

    result.sort(key=lambda w: w["date"])
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(result, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")

    sets = sum(len(e.get("sets", [])) for w in result for e in w["exercises"])
    print(f"Wrote {len(result)} workouts, {sum(len(w['exercises']) for w in result)} exercises, {sets} sets → {OUT.relative_to(REPO)}")
    print(f"Range: {result[0]['date']} … {result[-1]['date']}")
    print("\nMapping (sheet name → catalog):")
    for (raw, ex), n in sorted(used.items(), key=lambda x: (x[0][1], x[0][0])):
        print(f"  {n:4}  {raw!r:42} → {ex}")
    if unmapped:
        print("\nUNMAPPED (left out, listed in the workout note):")
        for raw, n in unmapped.most_common():
            print(f"  {n:4}  {raw!r}")
    if inferred:
        print("\nInferred:")
        for line in inferred:
            print("  " + line)
    if skipped:
        print("\nSkipped:")
        for line in skipped:
            print("  " + line)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # report uses arrows/dashes; Windows consoles default to cp1252
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    main(sys.argv[1])
