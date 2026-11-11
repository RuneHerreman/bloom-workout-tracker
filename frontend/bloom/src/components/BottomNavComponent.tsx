import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { LayoutDashboard, BookOpen, LayoutTemplate, TrendingUp, Trophy, Plus, Menu, Moon, Sun, LogOut, UserRound, X } from "lucide-react";
import "../assets/css/bottom-nav.css";
import { getMe } from "../features/auth/api.ts";
import type { User } from "../features/auth/api.ts";
import { useDarkModeContext } from "../context/DarkModeContext.tsx";
import { useAuth } from "../context/AuthContext.tsx";
import { prefetchRoute } from "../routePrefetch.ts";

function tabClass({ isActive }: { isActive: boolean }) {
    return `tab-item${isActive ? " active" : ""}`;
}

const MORE_PATHS = ["/templates", "/leaderboard", "/profile"];

/**
 * Phone navigation: a thumb-reachable tab bar with logging in the centre,
 * plus a "More" sheet for the less frequent pages and account actions.
 */
export default function MobileNavComponent() {
    const [moreOpen, setMoreOpen] = useState(false);
    const [user, setUser] = useState<User | null>(null);
    const { dark, toggle } = useDarkModeContext();
    const { logout } = useAuth();
    const navigate = useNavigate();
    const { pathname } = useLocation();

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
    }, []);

    useEffect(() => {
        if (!moreOpen) return;
        const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") setMoreOpen(false); };
        document.addEventListener("keydown", onKey);
        return () => document.removeEventListener("keydown", onKey);
    }, [moreOpen]);

    const close = () => setMoreOpen(false);
    const moreActive = MORE_PATHS.some(p => pathname.startsWith(p));

    return (
        <>
            <nav className="tab-bar" aria-label="Main">
                <NavLink to="/dashboard" className={tabClass} onPointerDown={() => prefetchRoute("/dashboard")} onClick={close}>
                    <LayoutDashboard size={20} /><span>Home</span>
                </NavLink>
                <NavLink to="/logbook" className={tabClass} onPointerDown={() => prefetchRoute("/logbook")} onClick={close}>
                    <BookOpen size={20} /><span>Logbook</span>
                </NavLink>
                <button
                    type="button"
                    className="tab-log"
                    aria-label="Log a workout"
                    onClick={() => { close(); navigate("/logbook", { state: { openStart: true } }); }}
                >
                    <Plus size={24} />
                </button>
                <NavLink to="/insights" className={tabClass} onPointerDown={() => prefetchRoute("/insights")} onClick={close}>
                    <TrendingUp size={20} /><span>Insights</span>
                </NavLink>
                <button
                    type="button"
                    className={`tab-item${moreActive || moreOpen ? " active" : ""}`}
                    aria-expanded={moreOpen}
                    aria-controls="more-sheet"
                    onClick={() => setMoreOpen(o => !o)}
                >
                    {moreOpen ? <X size={20} /> : <Menu size={20} />}<span>More</span>
                </button>
            </nav>

            {moreOpen && <div className="more-backdrop" onClick={close} />}
            <div id="more-sheet" className={`more-sheet${moreOpen ? " open" : ""}`} aria-hidden={!moreOpen}>
                <NavLink to="/templates" className="more-item" onClick={close}><LayoutTemplate size={18} /> Templates</NavLink>
                <NavLink to="/leaderboard" className="more-item" onClick={close}><Trophy size={18} /> Leaderboard</NavLink>
                <NavLink to="/profile" className="more-item" onClick={close}><UserRound size={18} /> {user?.username ?? "Profile"}</NavLink>
                <div className="more-row">
                    <button type="button" className="more-item" onClick={toggle}>
                        {dark ? <Sun size={18} /> : <Moon size={18} />} {dark ? "Light mode" : "Dark mode"}
                    </button>
                    <button type="button" className="more-item danger" onClick={logout}>
                        <LogOut size={18} /> Log out
                    </button>
                </div>
            </div>
        </>
    );
}
