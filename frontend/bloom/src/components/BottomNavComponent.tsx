import { NavLink } from "react-router-dom";
import { useState, useEffect } from "react";
import { Menu, X, LayoutDashboard, BookOpen, LayoutTemplate, Dumbbell, Calculator, Moon, Sun, LogOut } from "lucide-react";
import "../assets/css/bottom-nav.css";
import { getMe } from "../features/auth/api.ts";
import type { User } from "../features/auth/api.ts";
import { useDarkModeContext } from "../context/DarkModeContext.tsx";
import { useAuth } from "../context/AuthContext.tsx";


function navClass({ isActive }: { isActive: boolean }) {
    return `mobile-nav-item${isActive ? " active" : ""}`;
}

export default function MobileNavComponent() {
    const [open, setOpen] = useState(false);
    const [user, setUser] = useState<User | null>(null);
    const { dark, toggle } = useDarkModeContext();
    const { logout } = useAuth();

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
    }, []);

    function close() { setOpen(false); }

    return (
        <>
            <header className="mobile-nav-bar">
                <NavLink to="/dashboard" className="mobile-nav-logo" onClick={close}>
                    <img src="/media/bloom_logo.png" alt="Bloom" />
                </NavLink>
                <button
                    className="mobile-nav-toggle"
                    onClick={() => setOpen(o => !o)}
                    aria-label={open ? "Close menu" : "Open menu"}
                >
                    {open ? <X size={20} /> : <Menu size={20} />}
                </button>
            </header>

            {open && <div className="mobile-nav-backdrop" onClick={close} />}

            <nav className={`mobile-nav-dropdown${open ? " open" : ""}`}>
                <span className="mobile-nav-section">Pages</span>
                <NavLink to="/dashboard" className={navClass} onClick={close}><LayoutDashboard size={14} /> Dashboard</NavLink>
                <NavLink to="/templates" className={navClass} onClick={close}><LayoutTemplate size={14} /> Templates</NavLink>
                <NavLink to="/logbook" className={navClass} onClick={close}><BookOpen size={14} /> Log Book</NavLink>

                <span className="mobile-nav-section">Tools</span>
                <NavLink to="/tools/macro-calculator" className={navClass} onClick={close}><Calculator size={14} /> Macro Calculator</NavLink>
                <NavLink to="/tools/one-rep-max" className={navClass} onClick={close}><Dumbbell size={14} /> 1RM Calculator</NavLink>

                <div className="mobile-nav-footer">
                    <NavLink to="/profile" className="mobile-nav-username" onClick={close}>{user?.username ?? "—"}</NavLink>
                    <button className="mobile-nav-theme" onClick={toggle} aria-label="Toggle dark mode">
                        {dark ? <Sun size={14} /> : <Moon size={14} />}
                    </button>
                    <button className="mobile-nav-logout" onClick={logout} aria-label="Log out">
                        <LogOut size={14} />
                    </button>
                </div>
            </nav>
        </>
    );
}
