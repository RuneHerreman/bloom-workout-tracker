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
import { getLogs, getVolume } from "./api.ts";
import { updateTechnicalPoints } from "../auth/api.ts";
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
    }
}

function DashboardPage() {
    const [state, dispatch] = useReducer(reducer, initial);
    const navigate = useNavigate();

    useEffect(() => {
        getMe().then(user => dispatch({ type: "SET_USER", user })).catch(() => null);

        Promise.all([getVolume(), getLogs()])
            .then(([volume, workouts]) => {
                const { series, labels } = transFormVolumeDataForLineGraph(volume);
                dispatch({
                    type: "LOADED",
                    payload: {
                        stats: calculateDashboardStats(workouts),
                        activityData: transformLogsForActivityCalendar(workouts),
                        focusSegments: transformWorkoutsForFocusChart(workouts),
                        muscleFocusSegments: transformWorkoutsForMuscleChart(workouts, volume),
                        volumeSeries: series,
                        volumeLabels: labels,
                        logEntries: transformWorkoutLogsForLogPanel(workouts).slice(0, 5),
                    },
                });
            })
            .catch(e => dispatch({ type: "ERROR", error: e instanceof Error ? e.message : "Failed to load dashboard" }));
    }, []);

    const { user, stats, activityData, focusSegments, muscleFocusSegments, volumeSeries, volumeLabels, logEntries, error } = state;

    return (
        <div className="dashboard">
            <HeaderComponent
                title={`Welcome back, ${user?.firstName ?? "Stranger"}!`}
                subtitle={`Today  ·  ${formatWorkoutDate(new Date())}`}
                action={<Button text={"Log"} icon={<PlusIcon size={15} />} style={"green"} onClick={() => navigate("/logbook", { state: { openStart: true } })} />}
            />

            {error && <div className="error-banner" style={{ margin: "0 2rem" }}>{error}</div>}

            <div className="dashboard-body">
                <div className="dashboard-stats">
                    <StatWidget label="Workouts (this year)" value={stats?.workoutsThisYear ?? 0} changePercent={stats?.workoutChange} unit="sessions" />
                    <StatWidget label="Volume (this month)"  value={stats?.volumeThisMonth ?? "0"} changePercent={stats?.volumeChange} unit="tonnes" />
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
        </div>
    );
}

export default DashboardPage;
