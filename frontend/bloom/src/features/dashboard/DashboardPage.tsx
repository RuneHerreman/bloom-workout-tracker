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
import { getLogs, getPRs, getVolume } from "./api.ts";
import { transformVolumeDataToSeries, transformWorkoutLogsToEntries, transformPrDataToFocus, transformLogsToActivityData } from "./transforms.ts";

function formatDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        day: "numeric",
        month: "short",
        year: "numeric",
    });
}

function DashboardPage() {
    const [user, setUser] = useState<User | null>(null);
    const [activityData, setActivityData] = useState<ActivityDay[]>([]);
    const [volumeSeries, setVolumeSeries] = useState<ExerciseSeries[]>([]);
    const [focusSegments, setFocusSegments] = useState<FocusSegment[]>([]);
    const [logEntries, setLogEntries] = useState<LogEntryData[]>([]);

    useEffect(() => {
        getMe().then(setUser).catch(() => null);

        Promise.all([getPRs(), getVolume(), getLogs()])
            .then(([prs, volume, workouts]) => {
                setActivityData(transformLogsToActivityData(workouts));
                setVolumeSeries(transformVolumeDataToSeries(volume));
                setFocusSegments(transformPrDataToFocus(prs));
                setLogEntries(transformWorkoutLogsToEntries(workouts).slice(0, 9));
            })
            .catch(() => null);
    }, []);

    return (
        <div className="dashboard">
            <header>
                <p className="dashboard-date">{formatDate(new Date())}</p>
                <h1 className="dashboard-title">Welcome back, {user?.username ?? "—"}!</h1>
            </header>

            <div className="dashboard-body">
                <div className="dashboard-stats">
                    <StatWidget label="Workouts (this year)" value="38" changePercent={20}/>
                    <StatWidget label="Volume (this month)" value="12.5k" changePercent={5}/>
                    <StatWidget label="Active streak" value="3 days" subtext="best = 5 days"/>
                    <StatWidget label="PRs this month" value="0" changePercent={-60}/>
                </div>

                <div className="dash-activity"><ActivityWidget data={activityData}/></div>
                <div className="dash-training"><TrainingFocusWidget segments={focusSegments}/></div>
                <div className="dash-logs"><LogWidget entries={logEntries}/></div>
                <div className="dash-volume"><VolumeWidget series={volumeSeries} monthLabels={["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]}/></div>
            </div>
        </div>
    );
}

export default DashboardPage;
