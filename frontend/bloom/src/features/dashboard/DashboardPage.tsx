import { useEffect, useState } from "react";
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
    type DashboardStats
} from "./transforms.ts";
import Button from "../../components/general/ButtonComponent.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import TechnicalPointsWidget from "./components/TechnicalPointsWidget.tsx";
import {PlusIcon} from "lucide-react";
function formatDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric",
    });
}

function DashboardPage() {
    const [user, setUser] = useState<User | null>(null);
    const [activityData, setActivityData] = useState<ActivityDay[]>([]);
    const [volumeSeries, setVolumeSeries] = useState<ExerciseSeries[]>([]);
    const [volumeLabels, setVolumeLabels] = useState<string[]>(["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]);
    const [focusSegments, setFocusSegments] = useState<FocusSegment[]>([]);
    const [muscleFocusSegments, setMuscleFocusSegments] = useState<FocusSegment[]>([]);
    const [logEntries, setLogEntries] = useState<LogEntryData[]>([]);
    const [stats, setStats] = useState<DashboardStats | null>(null);

    useEffect(() => {
        getMe().then(setUser).catch(() => null);

        Promise.all([getVolume(), getLogs()])
            .then(([volume, workouts]) => {
                setActivityData(transformLogsForActivityCalendar(workouts));
                const { series, labels } = transFormVolumeDataForLineGraph(volume);
                setVolumeSeries(series);
                setVolumeLabels(labels);
                setFocusSegments(transformWorkoutsForFocusChart(workouts));
                setMuscleFocusSegments(transformWorkoutsForMuscleChart(workouts, volume));
                setLogEntries(transformWorkoutLogsForLogPanel(workouts).slice(0, 5));
                setStats(calculateDashboardStats(workouts));
            })
            .catch(() => null);
    }, []);

    return (
        <div className="dashboard">
            <HeaderComponent
                title={`Welcome back, ${user?.firstName ?? "—"}!`}
                subtitle={`Today  ·  ${formatDate(new Date())}`}
                action={<Button text={"Log Workout"} icon={<PlusIcon size={15} />} style={"green"} target="/logbook" />}
            />

            <div className="dashboard-body">
                <div className="dashboard-stats">
                    <StatWidget
                        label="Workouts (this year)"
                        value={stats?.workoutsThisYear ?? 0}
                        changePercent={stats?.workoutChange}
                        unit="sessions"
                    />
                    <StatWidget
                        label="Volume (this month)"
                        value={stats?.volumeThisMonth ?? "0"}
                        changePercent={stats?.volumeChange}
                        unit="tonnes"
                    />
                    <StatWidget
                        label="Active streak"
                        value={`${stats?.currentStreak ?? 0}`}
                        subtext={`best: ${stats?.bestStreak ?? 0} days`}
                        unit="days"
                    />
                    <StatWidget
                        label="Total PRs (this month)"
                        value={stats?.totalPRs ?? 0}
                        unit="PRs"
                    />
                </div>

                <div className="dash-activity"><ActivityWidget data={activityData}/></div>
                <div className="dash-training"><TrainingFocusWidget segments={focusSegments} muscleSegments={muscleFocusSegments}/></div>
                <div className="dash-notes"><TechnicalPointsWidget
                    initialContent={user?.technicalPoints}
                    onSave={(content) => updateTechnicalPoints(content).catch(() => null)}
                /></div>
                <div className="dash-logs"><LogWidget entries={logEntries}/></div>
                <div className="dash-volume"><VolumeWidget series={volumeSeries} monthLabels={volumeLabels}/></div>
            </div>
        </div>
    );
}

export default DashboardPage;
