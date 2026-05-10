import { useEffect, useState } from "react";
import "../../assets/css/dashboard.css";
import { getMe } from "../auth/api.ts";
import type { User } from "../auth/api.ts";
import StatWidget from "./components/StatWidget.tsx";
import ActivityWidget from "./components/ActivityWidget.tsx";
import TrainingFocusWidget from "./components/TrainingFocusWidget.tsx";
import VolumeWidget from "./components/VolumeWidget.tsx";
import LogWidget from "./components/LogWidget.tsx";

function formatDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        day: "numeric",
        month: "short",
        year: "numeric",
    });
}

function DashboardPage() {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
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

                <div className="dash-activity"><ActivityWidget/></div>
                <div className="dash-training"><TrainingFocusWidget/></div>
                <div className="dash-logs"><LogWidget/></div>
                <div className="dash-volume"><VolumeWidget/></div>
            </div>
        </div>
    );
}

export default DashboardPage;
