import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../assets/css/insights.css";
import "../../assets/css/dashboard.css";
import { getMe, peekMe, updateTechnicalPoints } from "../auth/api.ts";
import type { User } from "../auth/api.ts";
import { getLogs, peekLogs } from "../logbook/api.ts";
import { searchExercises, peekExercises } from "../exercises/api.ts";
import { getTemplates, peekTemplates } from "../templates/api.ts";
import type { Exercise } from "../exercises/api.ts";
import type { LoggedWorkout, WorkoutTemplate } from "../../assets/js/data/apiTypes.ts";
import Skeleton from "../../components/general/Skeleton.tsx";
import Button from "../../components/general/ButtonComponent.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import { PlusIcon } from "lucide-react";
import { formatWorkoutDate, formatDate } from "../../utils/workoutUtils.ts";
import StatWidget from "./components/StatWidget.tsx";
import ActivityWidget from "./components/ActivityWidget.tsx";
import KeyLiftsWidget from "./components/KeyLiftsWidget.tsx";
import UpNextWidget from "./components/UpNextWidget.tsx";
import MuscleFocusWidget from "./components/MuscleFocusWidget.tsx";
import LogWidget from "./components/LogWidget.tsx";
import TechnicalPointsWidget from "./components/TechnicalPointsWidget.tsx";
import {
    buildActivity, buildKeyLifts, buildMuscleFocus, buildRecentWorkouts, calculateDashboardStats, formatVolume, suggestNext,
} from "./transforms.ts";

const WEEKDAYS = ["M", "T", "W", "T", "F", "S", "S"];

const change = (now: number, before: number) => (now > 0 && before > 0 ? Math.round(((now - before) / before) * 100) : undefined);

/** Mirrors the dashboard grid so the layout doesn't jump when data lands. */
function DashboardSkeleton() {
    const block = (area: string, height: string) => (
        <div className={`${area} dash-skeleton-widget`}>
            <Skeleton width="8rem" height="1rem" />
            <Skeleton height={height} radius="var(--radius-md)" style={{ marginTop: "0.75rem", flex: 1 }} />
        </div>
    );
    return (
        <div className="dashboard-body" role="status" aria-label="Loading dashboard">
            <div className="dashboard-stats">
                {[0, 1, 2, 3, 4].map(i => (
                    <div key={i} className="stat-widget">
                        <Skeleton width="6rem" height="0.6rem" />
                        <Skeleton width="4rem" height="1.6rem" />
                        <Skeleton width="7rem" height="0.6rem" />
                    </div>
                ))}
            </div>
            <div className="dash-main">
                {block("dash-lifts", "11rem")}
                {block("dash-activity", "9rem")}
            </div>
            <div className="dash-side">
                {block("dash-next", "6rem")}
                {block("dash-notes", "10rem")}
            </div>
        </div>
    );
}

function DashboardPage() {
    const navigate = useNavigate();
    const [user, setUser] = useState<User | null>(() => peekMe() ?? null);
    const [logs, setLogs] = useState<LoggedWorkout[] | null>(() => peekLogs() ?? null);
    const [exerciseList, setExerciseList] = useState<Exercise[] | null>(() => peekExercises() ?? null);
    const [templates, setTemplates] = useState<WorkoutTemplate[]>(() => peekTemplates() ?? []);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
        getTemplates().then(setTemplates).catch(() => null);
        Promise.all([getLogs(), searchExercises()])
            .then(([l, e]) => { setLogs(l); setExerciseList(e); })
            .catch(e => setError(e instanceof Error ? e.message : "Failed to load dashboard"));
    }, []);

    const loading = !logs || !exerciseList;
    const goal = user?.activeDays || 3;
    const exercises = useMemo(() => Object.fromEntries((exerciseList ?? []).map(e => [e.id, e])), [exerciseList]);
    const stats = useMemo(() => (logs ? calculateDashboardStats(logs, goal) : null), [logs, goal]);
    const activity = useMemo(() => buildActivity(logs ?? []), [logs]);
    const lifts = useMemo(() => buildKeyLifts(logs ?? [], exercises, 3), [logs, exercises]);
    const focus = useMemo(() => buildMuscleFocus(logs ?? [], exercises), [logs, exercises]);
    const recent = useMemo(() => buildRecentWorkouts(logs ?? []), [logs]);
    const next = useMemo(() => suggestNext(templates, logs ?? [], exercises), [templates, logs, exercises]);

    const startLog = () => navigate("/logbook", { state: { openStart: true } });
    const now = new Date();
    const monthName = now.toLocaleDateString("en-GB", { month: "long" });
    const lastMonthName = new Date(now.getFullYear(), now.getMonth() - 1, 1).toLocaleDateString("en-GB", { month: "long" });
    const volume = formatVolume(stats?.volumeThisMonthKg ?? 0);
    const volumeBefore = formatVolume(stats?.volumeLastMonthToDateKg ?? 0);
    const weekLeft = stats ? Math.max(0, stats.goal - stats.workoutsThisWeek) : 0;

    return (
        <div className="dashboard">
            <HeaderComponent
                title={`Welcome back, ${user?.firstName ?? "Stranger"}!`}
                subtitle={formatWorkoutDate(now)}
                action={<Button text="Log workout" icon={<PlusIcon size={15} />} style="green hide-on-phone" onClick={startLog} />}
            />

            {error && <div className="error-banner" style={{ margin: "0 2rem" }}>{error}</div>}

            {loading || !stats ? <DashboardSkeleton /> : (
                <div className="dashboard-body">
                    <div className="dash-top">
                        {stats.workoutsThisMonth === 0 && (
                            <div className="dash-prompt">
                                <div>
                                    <p className="dash-prompt-title">
                                        {stats.lastWorkoutAt ? `No workouts yet in ${monthName}` : "Log your first workout"}
                                    </p>
                                    <p className="dash-prompt-text">
                                        {stats.lastWorkoutAt
                                            ? stats.workoutsLastMonth > 0
                                                ? `You trained ${stats.workoutsLastMonth} time${stats.workoutsLastMonth !== 1 ? "s" : ""} in ${lastMonthName}. Start this month's first session.`
                                                : `Your last workout was on ${formatDate(stats.lastWorkoutAt)}. Pick up where you left off.`
                                            : "Your stats, records and progress show up here once you log a session."}
                                    </p>
                                </div>
                                <Button text="Log workout" icon={<PlusIcon size={15} />} style="green" onClick={startLog} />
                            </div>
                        )}

                        <div className="dashboard-stats">
                            <StatWidget
                                label="This week"
                                value={`${stats.workoutsThisWeek} / ${stats.goal}`}
                                unit="workouts"
                                caption={weekLeft === 0 ? "Weekly goal reached" : `${weekLeft} more to reach your goal`}
                            >
                                <ol className="week-days" aria-label="Days trained this week">
                                    {WEEKDAYS.map((d, i) => (
                                        <li
                                            key={i}
                                            className={`${stats.weekDays[i] ? "done" : ""}${i === stats.todayIndex ? " today" : ""}${i > stats.todayIndex ? " later" : ""}`}
                                            aria-label={`${["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"][i]}${stats.weekDays[i] ? ": trained" : ""}`}
                                        >{d}</li>
                                    ))}
                                </ol>
                            </StatWidget>
                            <StatWidget
                                label="Week streak"
                                value={stats.weekStreak}
                                unit={stats.weekStreak === 1 ? "week" : "weeks"}
                                caption={`Weeks with ${stats.goal}+ workouts · best ${stats.bestWeekStreak}`}
                            />
                            <StatWidget
                                label={`Workouts in ${monthName}`}
                                value={stats.workoutsThisMonth}
                                unit={stats.workoutsThisMonth === 1 ? "session" : "sessions"}
                                changePercent={change(stats.workoutsThisMonth, stats.workoutsLastMonthToDate)}
                                caption={`vs ${stats.workoutsLastMonthToDate} by this point in ${lastMonthName}`}
                            />
                            <StatWidget
                                label={`Volume in ${monthName}`}
                                value={volume.value}
                                unit={volume.unit}
                                changePercent={change(stats.volumeThisMonthKg, stats.volumeLastMonthToDateKg)}
                                caption={`vs ${volumeBefore.value} ${volumeBefore.unit} by this point in ${lastMonthName}`}
                            />
                            <StatWidget
                                label={`Records in ${monthName}`}
                                value={stats.prsThisMonth}
                                unit={stats.prsThisMonth === 1 ? "record" : "records"}
                                caption={`${stats.prsLastMonth} in ${lastMonthName}`}
                            />
                        </div>
                    </div>

                    <div className="dash-main">
                        <div className="dash-lifts"><KeyLiftsWidget lifts={lifts} /></div>
                        <div className="dash-activity"><ActivityWidget data={activity} /></div>
                        <div className="dash-focus"><MuscleFocusWidget focus={focus} /></div>
                    </div>
                    <div className="dash-side">
                        <div className="dash-next"><UpNextWidget next={next} hasTemplates={templates.length > 0} /></div>
                        <div className="dash-notes">
                            <TechnicalPointsWidget initialContent={user?.technicalPoints} onSave={updateTechnicalPoints} />
                        </div>
                        <div className="dash-logs"><LogWidget entries={recent} /></div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default DashboardPage;
