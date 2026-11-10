import { useEffect, useReducer } from "react";
import { useNavigate } from "react-router-dom";
import "../../assets/css/dashboard.css";
import { getMe } from "../auth/api.ts";
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
import { getLogs, getVolume } from "../logbook/api.ts";
import { searchExercises } from "../exercises/api.ts";
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
import { formatWorkoutDate } from "../../utils/workoutUtils.ts";

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
    | { type: "LOADED"; payload: Omit<DashboardState, "user" | "loading" | "error"> }
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
    const [state, dispatch] = useReducer(reducer, initial);
    const navigate = useNavigate();

    useEffect(() => {
        getMe().then(user => dispatch({ type: "SET_USER", user })).catch(() => null);

        Promise.all([getVolume(), getLogs(), searchExercises()])
            .then(([volume, workouts, exercises]) => {
                const { series, labels } = transFormVolumeDataForLineGraph(volume);
                dispatch({
                    type: "LOADED",
                    payload: {
                        stats: calculateDashboardStats(workouts),
                        activityData: transformLogsForActivityCalendar(workouts),
                        focusSegments: transformWorkoutsForFocusChart(workouts),
                        muscleFocusSegments: transformWorkoutsForMuscleChart(workouts, exercises),
                        volumeSeries: series,
                        volumeLabels: labels,
                        logEntries: transformWorkoutLogsForLogPanel(workouts).slice(0, 5),
                    },
                });
            })
            .catch(e => dispatch({ type: "ERROR", error: e instanceof Error ? e.message : "Failed to load dashboard" }));
    }, []);

    const { user, stats, activityData, focusSegments, muscleFocusSegments, volumeSeries, volumeLabels, logEntries, error, loading } = state;

    return (
        <div className="dashboard">
            <HeaderComponent
                title={`Welcome back, ${user?.firstName ?? "Stranger"}!`}
                subtitle={`Today  ·  ${formatWorkoutDate(new Date())}`}
                action={<Button text={"Log"} icon={<PlusIcon size={15} />} style={"green"} onClick={() => navigate("/logbook", { state: { openStart: true } })} />}
            />

            {error && <div className="error-banner" style={{ margin: "0 2rem" }}>{error}</div>}

            {loading ? <DashboardSkeleton /> : (
            <div className="dashboard-body">
                <div className="dashboard-stats">
                    <StatWidget label="Workouts (this month)" value={stats?.workoutsThisMonth ?? 0} changePercent={stats?.workoutChange} subtext={`${stats?.workoutsThisYear ?? 0} this year`} unit="sessions" />
                    <StatWidget label="Volume (this month)"  value={stats?.volumeThisMonth ?? "0"} changePercent={stats?.volumeChange} unit={stats?.volumeUnit ?? "kg"} />
                    <StatWidget label="Active streak"        value={`${stats?.currentStreak ?? 0}`} subtext={`best: ${stats?.bestStreak ?? 0} days`} unit="days" />
                    <StatWidget label="Total PRs (this month)" value={stats?.totalPRs ?? 0} unit="PRs" />
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
