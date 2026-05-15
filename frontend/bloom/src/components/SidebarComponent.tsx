import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";
import { X } from "lucide-react";
import "../assets/css/sidebar.css";
import { getMe } from "../features/auth/api.ts";
import type { User } from "../features/auth/api.ts";

const GridIcon = () => (
    <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor" aria-hidden="true">
        <rect x="0" y="0" width="6" height="6" rx="1"/>
        <rect x="8" y="0" width="6" height="6" rx="1"/>
        <rect x="0" y="8" width="6" height="6" rx="1"/>
        <rect x="8" y="8" width="6" height="6" rx="1"/>
    </svg>
);

function navClass({ isActive }: { isActive: boolean }) {
    return `sidebar-item${isActive ? " active" : ""}`;
}

interface SidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

function SidebarComponent({ isOpen, onClose }: SidebarProps) {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
    }, []);

    return (
        <aside className={`sidebar${isOpen ? " open" : ""}`}>
            <div className="sidebar-header">
                <NavLink to="/dashboard" className="sidebar-logo" onClick={onClose}>
                    <img src="/media/bloom_logo.png" alt="Bloom"/>
                </NavLink>
                <button className="sidebar-close" onClick={onClose} aria-label="Close menu">
                    <X size={18} />
                </button>
            </div>

            <span className="sidebar-section-label">Pages</span>
            <NavLink to="/dashboard" className={navClass} onClick={onClose}>
                <GridIcon/> Dashboard
            </NavLink>
            <NavLink to="/templates" className={navClass} onClick={onClose}>
                <GridIcon/> Templates
            </NavLink>
            <NavLink to="/logbook" className={navClass} onClick={onClose}>
                <GridIcon/> Log Book
            </NavLink>

            <span className="sidebar-section-label">Tools</span>
            <NavLink to="/tools/macro-calculator" className={navClass} onClick={onClose}>
                <GridIcon/> Macro Calculator
            </NavLink>
            <NavLink to="/tools/one-rep-max" className={navClass} onClick={onClose}>
                <GridIcon/> 1RM Calculator
            </NavLink>

            <div className="sidebar-user">
                <div className="sidebar-avatar"/>
                <span>{user?.username ?? "—"}</span>
            </div>
        </aside>
    );
}

export default SidebarComponent;
