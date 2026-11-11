# Body weight & body fat, 1RM calculator, macro calculator

Status: plan, nothing built yet. Written 11 Nov 2026.

Three pieces, built in this order because each leans on the one before:

1. **Body log**: body weight, plus body fat % when you have it. Logged from the dashboard, imported, and shown in Insights.
2. **1RM calculator**: needs only what's logged already (sets, reps, RIR).
3. **Macro calculator**: needs the body weight trend and one new profile field (sex).

---

## 1. Data to gather

### Already there

| Data | Where | Used for |
|---|---|---|
| Height (cm) | `User.Height` | BMR |
| Birth date | `User.BirthDate` | age, which feeds BMR |
| Weight (kg) | `User.Weight` | this becomes the *first* body weight entry, then is derived (see below) |
| Target workouts/week | `User.ActiveDays` | fallback for activity level |
| Every set: weight, reps, RIR, marker | logged workouts | 1RM, activity level from real training |

### New

| Data | Where | Why |
|---|---|---|
| **Body log entries**: date, kg, optional body fat % | new `BodyMeasurement` table | weight and body fat over time, trends, the rate of change for macros, lean mass for Katch-McArdle |
| **Sex** (male / female), optional | `User.Sex`, nullable | Mifflin-St Jeor has a different constant for each. Asked once, in the macro calculator if it's missing, then stored in the profile |
| Body fat %, optional | part of each body log entry (above) | the latest trend value switches BMR to Katch-McArdle, which is more accurate when known; can still be overridden in the calculator |
| Goal + rate | not stored at first; remembered per viewer in `localStorage` | macro target |

Not collected: daily food intake. Bloom is not a food diary. That rules out the "adaptive TDEE" approach (intake vs. trend), so the macro numbers are estimates and the UI says so once, plainly.

---

## 2. Body log (weight + body fat)

### Backend

New aggregate, following the existing domain style (`ImportBatch` is the closest model):

```
BodyMeasurement : AggregateRoot<BodyMeasurementId>
  UserId      UserId
  Date        DateOnly        // one entry per user per day
  WeightKg    decimal(5,2)    // 20 < w < 400
  BodyFatPct  decimal(4,1)?   // optional, 3 ≤ bf ≤ 70
  Source      Manual | Import | Signup
  CreatedAt   DateTime
```

- **Weight is required, body fat is optional.** Body fat is rarely measured alone: a smart scale gives both, and a caliper or DEXA reading comes with a weigh-in anyway. So one entry per day carries both, and most entries have no body fat.
- **Body fat readings are noisy and method-dependent.** A bioimpedance scale can swing 1–2 points with hydration, and switching methods (scale → calipers → DEXA) shifts the level entirely. Bloom doesn't try to correct for this. It trends body fat the same way as weight, and the UI says the number is only comparable to itself from the same method.

- **One per day.** There's a unique index on `(UserId, Date)`. Logging a second time on the same day replaces the first; that's what people mean when they re-weigh.
- **`User.Weight` stays** as the "current weight" that other code already reads. It's updated from the latest entry whenever an entry is added, edited or deleted. This avoids touching the leaderboard, the profile and register flows at once.
- **Signup** creates the first entry (`Source = Signup`, dated the signup day) from the weight given at registration.
- **Migration:** every existing user gets one `Signup` entry with their current `User.Weight`. `User` has no created-at column, so it's dated to their earliest logged workout, or to the migration date if they have none.
- **Profile settings:** the weight field leaves the editable form. In its place goes a read-only line: "78.4 kg · logged 9 Nov", with a link to the log. Height, birth date and sex stay editable there.

Endpoints, following the `Routes.cs` pattern:

| Method | Route | Body / result |
|---|---|---|
| GET | `/body?from&to` | entries, oldest first |
| PUT | `/body/{date}` | `{ weightKg, bodyFatPct?, clearBodyFat? }`: upsert for that day. Leaving `bodyFatPct` out keeps the day's existing value, so re-weighing doesn't wipe a body fat reading; `clearBodyFat` removes it |
| DELETE | `/body/{date}` | |
| PUT | `/users/me/sex` | `{ sex: "Male" | "Female" | null }` |

### Import

- **Bloom export:** the xlsx gets a third sheet, **"Body"**, with the columns `Date`, `Weight (kg)`, `Body fat (%)`. The JSON backup gets a `body: [{ date, weightKg, bodyFatPct }]` array. Both are read back on import.
- **Body fat column** in any weight file: headers `body fat`, `bodyfat`, `fat %`, `bf`, `bf%`, `fat mass %`. Values given as a fraction (0.18) are read as 18%. Columns for fat *mass* in kg are converted to % with that row's weight.
- **Plain weight files** (CSV, or an xlsx sheet from a scale app or a spreadsheet someone kept): the file is detected as body weight when it has a date column and a weight column but **no** exercise column. Accepted headers include `weight`, `body weight`, `bodyweight`, `kg`, `lbs`, `lb` and `mass`, in any case.
  - **Units:** from the header (`lb`/`lbs` is converted × 0.45359237). With no unit in the header, the unit is guessed from the median value: above 140 is very likely lb, and the import step asks to confirm instead of guessing silently. This reuses the existing day/month-order question, so it isn't a new UI.
  - **Several readings on one day** (some scales log every weigh-in): keep the **first reading of the day**. Morning-fasted is the most consistent, and the first entry is usually it.
- **Duplicates:** a date already in the log is skipped and counted as "already there", the same as workouts. A date whose value differs is still skipped; the summary shows "3 days kept your existing weight".
- **Undo:** `ImportBatch` gets a `BodyMeasurementIds` list, so undoing an import removes those entries too. That needs a migration column like the existing `WorkoutIds`.
- **Sanity:** weights outside 20–400 kg and body fat outside 3–70% are listed as failed rows with the reason, using the same per-row failure reporting as workouts. A bad body fat value drops only the body fat and keeps the weight.

### Trend: the maths

Raw daily weight swings ±1–2 kg on water, salt and glycogen. Showing it raw makes the chart mostly noise. Bloom shows both, with the **trend** as the line people read.

**Exponentially weighted moving average** (the Hacker's Diet method), made aware of gaps between weigh-ins:

```
α = 0.1                                  // 10% weight on a new reading, ≈ 20-day lag
first reading:  T₀ = w₀
next reading after Δ days:
    a  = 1 − (1 − α)^Δ                   // a 5-day gap counts as 5 steps, not 1
    Tₙ = Tₙ₋₁ + a · (wₙ − Tₙ₋₁)
```

The plain formula assumes a weigh-in every day. With gaps it would make one reading after a week away move the trend only 10%. The `^Δ` form fixes that.

**Rate of change** (kg/week and %/week): a least-squares slope through the raw entries of the selected period, × 7. It's shown only when the period has **at least 14 days and 6 entries**. Below that it says "Not enough weigh-ins yet". A slope from three points is noise, and this is the number the macro calculator acts on, so it must not be invented.

**Body fat** uses the same trend and rate functions on the entries that have a body fat value. Readings are sparser and noisier than weight, so:
- the trend uses a slower α = 0.05 (≈ 40-day lag), since one hydrated morning on a bioimpedance scale can move the reading 2 points;
- the rate is shown in **points per month**, not per week, and needs at least 4 readings spanning 28 days;
- **lean mass** and **fat mass** in kg are derived per entry (`lean = kg × (1 − bf/100)`), and their trends are shown only when both trends exist. That's the honest answer to "am I losing fat or muscle": a falling weight trend with a steady lean trend.

Reliability tests (Vitest, see §6):
- A constant weight gives a flat trend and a 0 rate.
- A linear series of −0.1 kg/day gives a rate of −0.7 kg/wk, ±0.01.
- A single +3 kg outlier moves the trend by 0.3 kg, not 3.
- A gap of N days equals N daily steps of the same reading.

---

## 3. 1RM calculator

### The maths

No single formula is accurate for everyone. The research is consistent on what is reliable:

- The estimates are good **from 10 reps or fewer**, best around 3–5RM, and get worse fast above that.
- Epley and Brzycki are close up to about 10 reps. Brzycki runs a little lower at higher reps.
- Accuracy varies by lift; deadlift tends to be **under**-estimated.

So:

```
Epley:    1RM = w × (1 + r / 30)
Brzycki:  1RM = w × 36 / (37 − r)
Estimate: the mean of both, for r ≤ 10
Range:    min/max of both, shown as "± x kg"
r = 1  → the weight itself
r > 10 → still computed, labelled "rough: under 10 reps is more reliable"
r > 15 → not computed
```

**RIR matters.** These formulas assume a set **to failure**. Bloom already logs RIR, so it uses **reps to failure = reps + RIR**. 100 kg × 5 @ 2 RIR estimates like 100 kg × 7. Without RIR, the set is treated as taken to failure, which is the conservative choice and slightly underestimates.

**This changes the insights number too.** `analysis.ts` uses Epley only, capped at 12, and ignores RIR. Moving it onto the same shared function keeps the calculator and Insights from disagreeing about the same set.

**Working weights from a 1RM** run the same blended formula in reverse:
`w = 1RM / (1 + (r + RIR) / 30)` for Epley, and the same for Brzycki, averaged. Results are rounded **down** to the plate step (2.5 kg by default, 1.25 kg selectable). Rounding down is the safe side for a working set.

Tests: published Epley/Brzycki reference values; 1 rep → identity; an RIR shift equals an added rep; inverse(estimate(w, r)) ≈ w within rounding.

### UI

There are two inputs on the left and the answer and a table on the right. No hero card.

```
┌ One rep max ─────────────────────────────────────────────────────────┐
│ Exercise  [ Bench press           ▾ ]   Best recent: 80 kg × 6 @1 → │
│ Weight    [ 80   ] kg                                                 │
│ Reps      [ 6    ]     RIR [ 1 ]                                      │
├───────────────────────────────────────┬──────────────────────────────┤
│ Estimated 1RM                         │  Reps  RIR 0   RIR 1  RIR 2   │
│ 97.5 kg   ± 1.5                       │   1    97.5    92.5   90      │
│ Epley 98.7 · Brzycki 96.4             │   3    90      87.5   85      │
│                                       │   5    85      82.5   80      │
│ Your logged best: 101.3 kg (21 Oct)   │   8    77.5    75     72.5    │
│                                       │  10    72.5    70     67.5    │
└───────────────────────────────────────┴──────────────────────────────┘
```

- **Exercise picker (optional):** the same search list as elsewhere. Picking an exercise fills in its best recent set, meaning the highest estimate in the last 8 weeks, and shows the logged best estimate under the result. Without an exercise, it's a plain calculator.
- **The number** is in the serif face, like every other stat. The "±" range sits next to it in muted sans. The two formula values are on one muted line, for people who want to know where it came from.
- **The table** is the useful part: rows are 1, 2, 3, 5, 8, 10, 12 reps; columns are RIR 0–3; values are rounded to the plate step. A plate-step toggle (2.5 / 1.25) goes in the table header.
- **Warnings appear inline** under the number, in muted text, never as a banner: "Above 10 reps: treat this as rough."

---

## 4. Macro calculator

### The maths

**BMR**
- Mifflin-St Jeor is the default. In the main systematic review it was the equation most likely to land within 10% of measured RMR.
  `BMR = 10·kg + 6.25·cm − 5·age + s`, with `s = +5` (male) or `−161` (female).
- Katch-McArdle is used **when there's a recent body fat reading** (trend value, with the latest reading within 60 days) or one is typed into the calculator:
  `BMR = 370 + 21.6 · LBM`, where `LBM = kg × (1 − bf%)`.
- **Weight input = the trend weight**, not the last reading, so one heavy morning doesn't move the target.

**Maintenance (TDEE) = BMR × activity factor**

| Level | Factor | Default when the logged sessions/week over the last 4 weeks are… |
|---|---|---|
| Sedentary | 1.2 | 0 |
| Light | 1.375 | 1–2 |
| Moderate | 1.55 | 3–4 |
| Very active | 1.725 | 5–6 |
| Extra active | 1.9 | 7+, or a physical job (manual pick only) |

The default comes from **actual logged training**. If nothing is logged, it falls back to `ActiveDays`. The user can always override it, and the select says why it picked what it did ("3.5 sessions a week lately").

**Target**
- Rate as **% body weight per week**:
  - Lose: 0.25 / 0.5 / 0.75 / 1.0 %. Default 0.5; nothing above 1%, which is the evidence-based range for keeping muscle.
  - Maintain: 0.
  - Gain: 0.1 / 0.25 / 0.5 %. Default 0.25; lean gaining is slow.
- `daily kcal Δ = rate_kg_per_week × 7700 / 7`.
  The 7,700 kcal/kg rule is fine for a starting point over weeks. It is *not* a long-term prediction, so Bloom never projects a goal date with it.
- There is a floor: never below BMR × 1.1. Below that, the screen shows the floor and says the chosen rate was capped.

**Macros, in this order**
1. **Protein**, in g/kg of body weight:
   - Maintain or gain: 1.6 by default, adjustable 1.4–2.0 (the ISSN range for resistance training).
   - Losing: 2.2 by default. During a deficit, intakes up to about 2.3–3.1 g/kg of *lean* mass help keep muscle. 2.2 g/kg of body weight is the practical version of that without needing body fat %.
2. **Fat:** 25% of kcal, never below 0.6 g/kg (the hormonal floor).
3. **Carbs:** the remainder. If the remainder is under 2 g/kg on a hard deficit, it's shown as is, plus a muted note.
4. Rounding: kcal to the nearest 10 and grams to the nearest 5, so the numbers don't look falsely precise.

**Honest framing:** one line under the kcal number: "Estimate, usually within 10%. Check your weight trend after 2–3 weeks and adjust by ±100–200 kcal." When the weight log has a rate (§2), it shows next to it: "Your trend: −0.3 kg/week". The user can compare those two themselves. Bloom doesn't auto-adjust from them, because it doesn't know their intake.

Tests: Mifflin reference cases for male and female; the Katch path; the floor kicks in; macros sum back to kcal within rounding (4/4/9 kcal per gram).

### UI

Inputs on the left, prefilled, with every prefilled value shown so it can be corrected. The output on the right is one number and three bars.

```
┌ Macros ───────────────────────────────────────────────────────────────┐
│ From your profile          Edit                                       │
│ 78.4 kg trend · 182 cm · 29 y · Male                                   │
│ Body fat  [ 17.8 ] %  from your log, 3 Nov · clear to ignore          │
│ Activity  [ Moderate · 3.5 sessions/wk lately ▾ ]                      │
│ Goal      ( Lose )( Maintain )( Gain )    Rate [ 0.5 % / week ▾ ]     │
├───────────────────────────────────────┬───────────────────────────────┤
│ 2,340 kcal / day                      │ Protein  170 g  ██████░░░ 29% │
│ Maintenance ≈ 2,730 · −390 for        │ Fat       65 g  ████░░░░░ 25% │
│ −0.4 kg/week                          │ Carbs    265 g  ████████░ 46% │
│ Estimate, usually within 10%. …       │                               │
│ Your trend: −0.3 kg/week              │ Protein 2.2 g/kg · why ⓘ      │
└───────────────────────────────────────┴───────────────────────────────┘
```

- **Goal** uses the existing segmented control (same as Strength/Cardio). **Rate** is a select, not a slider, because the steps are deliberate.
- **Macro bars** are plain horizontal bars with flat fills: protein in brand green, fat in the warm accent, carbs in grey. No donut chart.
- The **ⓘ** uses the existing `InfoHint` to explain each rule in one or two sentences, with the source named.
- **If sex is missing,** the only prompt is an inline two-option choice in the "From your profile" line. Answering it saves it to the profile.

---

## 5. Where the body log appears

### Dashboard

- A **"Log weight"** button sits in the page header next to "Log workout", as a secondary (outline) button so the main action stays obvious.
- It opens a **small popover** under the button, not a modal: one number field prefilled with the last weight and focused, an optional **body fat %** field next to it (empty by default; a placeholder shows the last reading), a date that defaults to today and can be changed, and Enter to save. The field gets a toast-free "Saved" state and closes. It also shows the last entry: "Last: 78.6 kg · Mon".
- **Optional and later:** swap the "Records" stat tile for a "Body weight" tile (trend value + weekly rate) for people who log weight. Leave it out of the first version.

### Insights

- The view switch becomes **Strength · Cardio · Body**, using the same segmented control and the same period selector (4w / 3m / 6m / 1y / all).
- The body view has no explorer list. A small segmented control above the chart switches the series: **Weight · Body fat · Lean mass**. The last two are disabled, with a one-line reason, until there are enough body fat readings. The layout is:
  - A **stat row** in the existing `records` style: Trend now · Change this period · Rate /week (+ % bw) · Lowest/highest trend. For body fat, the rate is in points per month, and a muted note under the row says readings are only comparable from the same method.
  - **Chart:** raw entries as small dots in `--viz-axis` grey, and the trend as a 1.5px `--viz-1` line. The tooltip shows the date, the raw value and the trend value. The y-axis is padded to about ±1.5 kg around the data, so a flat month doesn't look like a cliff.
  - **Entries table** (newest first), with date, weight, body fat (blank when not measured) and Δ vs. trend. Rows can be edited and deleted inline, which is where mistakes get fixed. It's capped at 20 rows with "Show all".
- **Empty state:** one sentence and the same "Log weight" popover button. No illustration.

### Profile

Weight becomes the read-only line from §2. A new **Sex** field goes next to birth date (Male / Female / Not set).

---

## 6. Making the numbers reliable

- All maths lives in **one pure module**, `src/utils/bodyMath.ts`: `e1rm`, `weightForReps`, `bmr`, `tdee`, `macroTargets`, `ewmaTrend`, `weeklyRate`. It has no React and no dates beyond plain numbers. Insights, the dashboard and both calculators import it; nothing re-implements a formula.
- **Add Vitest** to the frontend (there's no test runner yet) and give that module table-driven tests with the cases listed above. Add `npm test` to CI next to the build.
- **Backend tests** (existing `tests/UnitTests`): `BodyMeasurement` invariants (weight and body fat ranges, one per day), upsert replaces the weight but keeps body fat when it's left out, `User.Weight` follows the latest entry, import dedupe and undo.
- **Units are stored in kg only.** lb exists only at the import boundary.

---

## 7. Design rules

These keep it consistent with the rest of Bloom and away from the usual generated-UI look:

- **Avoid:**
  - gradient or glass cards, glows, and coloured shadows
  - emoji, sparkles, and icons in tinted circles
  - a big centred hero number on its own card
  - donut and radial gauges
  - "insight" sentences that just repeat the number in words
  - badges on everything
  - "Your journey" copy
  - three equal feature cards in a row
- **Use:**
  - hairline borders and the existing widget shell
  - left-aligned content
  - serif for the one important number, sans for everything else
  - tabular numbers and real tables where the data is a table
  - muted text for caveats, inline where they apply
  - green only where it means something (the trend line, protein, primary action)
- **Copy:**
  - short and specific, with units always attached
  - estimates say they're estimates once, not in every line
  - no exclamation marks

---

## 8. Build order

1. `bodyMath.ts` + Vitest + tests. Move Insights onto the shared `e1rm` (it gains RIR support).
2. **1RM calculator** page. Frontend only, so it ships first.
3. **Backend body log (weight + body fat):** domain, migration (backfill + `User.Sex` + `ImportBatch.BodyMeasurementIds`), endpoints, tests.
4. **Dashboard** "Log weight" popover; **Profile** changes; **signup** writes the first entry.
5. **Insights** body weight view.
6. **Import/export:** the Bloom sheet/JSON, plain weight files with unit detection, undo.
7. **Macro calculator** last, so it can use the trend and the logged activity level.

## Sources

- Frankenfield et al. 2005, a systematic review of RMR equations. Mifflin-St Jeor was the most likely to fall within 10% of measured RMR, with real individual error. https://pure.psu.edu/en/publications/comparison-of-predictive-equations-for-resting-metabolic-rate-in-/
- ISSN position stand on protein and exercise (2017): 1.4–2.0 g/kg/day, and higher in a deficit. https://pmc.ncbi.nlm.nih.gov/articles/PMC5477153/
- Helms, Aragon & Fitschen 2014, natural bodybuilding prep: 0.5–1 %/week loss, 2.3–3.1 g/kg lean mass protein. https://jissn.biomedcentral.com/articles/10.1186/1550-2783-11-20
- 1RM prediction accuracy: best under about 10 reps, error varies by lift, deadlift is underestimated. https://pmc.ncbi.nlm.nih.gov/articles/PMC9465738 and https://arxiv.org/pdf/2603.17495
- Hacker's Diet trend smoothing (EWMA, 10% weight on a new reading). https://fourmilab.ch/hackdiet/www/subsubsection1_2_4_0_4_4.html
