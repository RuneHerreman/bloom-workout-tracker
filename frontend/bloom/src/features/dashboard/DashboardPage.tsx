import { useEffect, useReducer } from "react";
import { useNavigate } from "react-router-dom";
import "../../assets/css/dashboard.css";
import { getMe, peekMe } from "../auth/api.ts";
import type { User } from "../auth/api.ts";
import StatWidget from "./components/StatWidget.tsx";
import ActivityWidget from "./components/ActivityWidget.tsx";
import type { ActivityDay } from "./components/ActivityWidget";
import TrainingFocusWidget from "./components/TrainingFocusWidget.tsx";
import type { FocusSegment } from "./components/TrainingFocusWidget";
import VolumeWidget from "./components/VolumeWidget.tsx";
import type { ExerciseSeries } from "./components/VolumeWidget";
import LogWidget from "./components/LogWidget.tsx";
import type { LogEntryData } from "./components/LogWidget";
import { getLogs, getVolume, peekLogs, peekVolume } from "../logbook/api.ts";
import { searchExercises, peekExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import type { ExerciseVolumeResponse, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import { updateTechnicalPoints } from "../auth/api.ts";
import Skeleton from "../../components/general/Skeleton.tsx";
import {
    transFormVolumeDataForLineGraph,
    transformWorkoutLogsForLogPanel,
    transformWorkoutsForFocusChart,
    transformWorkoutsForMuscleChart,
    transformLogsForActivityCalendar,
    calculateDashboardStats,
    type DashboardStats,
} from "./transforms.ts";
import Button from "../../components/general/ButtonComponent.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import TechnicalPointsWidget from "./components/TechnicalPointsWidget.tsx";
import { PlusIcon } from "lucide-react";
import { formatWorkoutDate, formatDate } from "../../utils/workoutUtils.ts";

interface DashboardState {
    user: User | null;
    stats: DashboardStats | null;
    activityData: ActivityDay[];
    focusSegments: FocusSegment[];
    muscleFocusSegments: FocusSegment[];
    volumeSeries: ExerciseSeries[];
    volumeLabels: string[];
    logEntries: LogEntryData[];
    loading: boolean;
    error: string | null;
}

type DashboardAction =
    | { type: "SET_USER"; user: User }
    | { type: "LOADED"; payload: LoadedPayload }
    | { type: "ERROR"; error: string };

const initial: DashboardState = {
    user: null,
    stats: null,
    activityData: [],
    focusSegments: [],
    muscleFocusSegments: [],
    volumeSeries: [],
    volumeLabels: ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"],
    logEntries: [],
    loading: true,
    error: null,
};

type LoadedPayload = Omit<DashboardState, "user" | "loading" | "error">;

function buildPayload(volume: ExerciseVolumeResponse[], workouts: LoggedWorkout[], exercises: Exercise[]): LoadedPayload {
    const { series, labels } = transFormVolumeDataForLineGraph(volume);
    return {
        stats: calculateDashboardStats(workouts),
        activityData: transformLogsForActivityCalendar(workouts),
        focusSegments: transformWorkoutsForFocusChart(workouts),
        muscleFocusSegments: transformWorkoutsForMuscleChart(workouts, exercises),
        volumeSeries: series,
        volumeLabels: labels,
        logEntries: transformWorkoutLogsForLogPanel(workouts).slice(0, 5),
    };
}

/** If everything is already cached (prefetched or visited before), start fully rendered. */
function initialState(): DashboardState {
    const volume = peekVolume(), workouts = peekLogs(), exercises = peekExercises();
    const user = peekMe() ?? null;
    if (volume && workouts && exercises) {
        return { ...initial, ...buildPayload(volume, workouts, exercises), user, loading: false };
    }
    return { ...initial, user };
}

function reducer(state: DashboardState, action: DashboardAction): DashboardState {
    switch (action.type) {
        case "SET_USER": return { ...state, user: action.user };
        case "LOADED":   return { ...state, ...action.payload, loading: false, error: null };
        case "ERROR":    return { ...state, loading: false, error: action.error };
        default:         return state;
    }
}

/** Mirrors the dashboard grid so the layout doesn't jump when data lands. */
function DashboardSkeleton() {
    const block = (area: string, height: string) => (
        <div className={`${area} dash-skeleton-widget`}>
            <Skeleton width="6rem" height="0.6rem" />
            <Skeleton width="10rem" height="1rem" />
            <Skeleton height={height} radius="var(--radius-md)" style={{ marginTop: "0.75rem", flex: 1 }} />
        </div>
    );
    return (
        <div className="dashboard-body" role="status" aria-label="Loading dashboard">
            <div className="dashboard-stats">
                {[0, 1, 2, 3].map(i => (
                    <div key={i} className="stat-widget dash-skeleton-stat">
                        <Skeleton width="7rem" height="0.6rem" />
                        <Skeleton width="4rem" height="1.9rem" />
                        <Skeleton width="5rem" height="0.6rem" />
                    </div>
                ))}
            </div>
            {block("dash-activity", "9rem")}
            {block("dash-training", "9rem")}
            {block("dash-notes", "12rem")}
            {block("dash-logs", "16rem")}
            {block("dash-volume", "16rem")}
        </div>
    );
}

function DashboardPage() {
    const [state, dispatch] = useReducer(reducer, undefined, initialState);
    const navigate = useNavigate();

    useEffect(() => {
        getMe().then(user => dispatch({ type: "SET_USER", user })).catch(() => null);

        Promise.all([getVolume(), getLogs(), searchExercises()])
            .then(([volume, workouts, exercises]) => {
                dispatch({ type: "LOADED", payload: buildPayload(volume, workouts, exercises) });
            })
            .catch(e => dispatch({ type: "ERROR", error: e instanceof Error ? e.message : "Failed to load dashboard" }));
    }, []);

    const { user, stats, activityData, focusSegments, muscleFocusSegments, volumeSeries, volumeLabels, logEntries, error, loading } = state;
    const startLog = () => navigate("/logbook", { state: { openStart: true } });
    const monthName = new Date().toLocaleDateString("en-US", { month: "long" });
    const lastMonthName = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1).toLocaleDateString("en-US", { month: "long" });
    const lastMonthCaption = (value: string | number, unit = "") => `${value}${unit ? ` ${unit}` : ""} in ${lastMonthName}`;

    return (
        <div className="dashboard">
            <HeaderComponent
                title={`Welcome back, ${user?.firstName ?? "Stranger"}!`}
                subtitle={formatWorkoutDate(new Date())}
                action={<Button text="Log workout" icon={<PlusIcon size={15} />} style="green hide-on-phone" onClick={startLog} />}
            />

            {error && <div className="error-banner" style={{ margin: "0 2rem" }}>{error}</div>}

            {loading ? <DashboardSkeleton /> : (
            <div className="dashboard-body">
                <div className="dash-top">
                    {stats && stats.workoutsThisMonth === 0 && (
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
                                        : "Your stats, streaks and PRs show up here once you log a session."}
                                </p>
                            </div>
                            <Button text="Log workout" icon={<PlusIcon size={15} />} style="green" onClick={startLog} />
                        </div>
                    )}
                    <div className="dashboard-stats">
                        <StatWidget label={`Workouts in ${monthName}`} value={stats?.workoutsThisMonth ?? 0} unit="sessions"
                            caption={lastMonthCaption(stats?.workoutsLastMonth ?? 0)}
                            changePercent={stats?.workoutsThisMonth ? stats.workoutChange : undefined} />
                        <StatWidget label={`Volume in ${monthName}`} value={stats?.volumeThisMonth ?? "0"} unit={stats?.volumeUnit ?? "kg"}
                            caption={lastMonthCaption(stats?.volumeLastMonth ?? "0", stats?.volumeLastMonthUnit)}
                            changePercent={stats?.workoutsThisMonth ? stats.volumeChange : undefined} />
                        <StatWidget label="Current streak" value={stats?.currentStreak ?? 0} unit="days"
                            caption={`Best: ${stats?.bestStreak ?? 0} days`} />
                        <StatWidget label={`New PRs in ${monthName}`} value={stats?.totalPRs ?? 0} unit="PRs"
                            caption={lastMonthCaption(stats?.prsLastMonth ?? 0)} />
                    </div>
                </div>

                <div className="dash-activity"><ActivityWidget data={activityData} /></div>
                <div className="dash-training"><TrainingFocusWidget segments={focusSegments} muscleSegments={muscleFocusSegments} /></div>
                <div className="dash-notes">
                    <TechnicalPointsWidget
                        initialContent={user?.technicalPoints}
                        onSave={content => updateTechnicalPoints(content).catch(() => null)}
                    />
                </div>
                <div className="dash-logs"><LogWidget entries={logEntries} /></div>
                <div className="dash-volume"><VolumeWidget series={volumeSeries} monthLabels={volumeLabels} /></div>
            </div>
            )}
        </div>
    );
}

export default DashboardPage;
