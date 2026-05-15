import { NavLink, useNavigate, useLocation } from "react-router-dom";
import { BookOpen, LayoutTemplate, Dumbbell, Calculator } from "lucide-react";
import "../assets/css/bottom-nav.css";

function bottomClass({ isActive }: { isActive: boolean }) {
    return `bottom-nav-item${isActive ? " active" : ""}`;
}

export default function BottomNavComponent() {
    const navigate = useNavigate();
    const location = useLocation();

    function handleClick(e: React.MouseEvent, to: string) {
        if (location.pathname === to || location.pathname.startsWith(to + "/")) {
            e.preventDefault();
            navigate(to);
        }
    }

    return (
        <nav className="bottom-nav">
            <NavLink to="/templates" className={bottomClass} onClick={e => handleClick(e, "/templates")}>
                <LayoutTemplate size={18} />
                <span>Templates</span>
            </NavLink>
            <NavLink to="/logbook" className={bottomClass} onClick={e => handleClick(e, "/logbook")}>
                <BookOpen size={18} />
                <span>Logbook</span>
            </NavLink>

            <NavLink to="/dashboard" className={({ isActive }) => `bottom-nav-center-wrap${isActive ? " active" : ""}`} onClick={e => handleClick(e, "/dashboard")}>
                <img src="/media/bloom_bullet.svg" alt="Dashboard" />
            </NavLink>

            <NavLink to="/tools/macro-calculator" className={bottomClass} onClick={e => handleClick(e, "/tools/macro-calculator")}>
                <Calculator size={18} />
                <span>Macros</span>
            </NavLink>
            <NavLink to="/tools/one-rep-max" className={bottomClass} onClick={e => handleClick(e, "/tools/one-rep-max")}>
                <Dumbbell size={18} />
                <span>1RM</span>
            </NavLink>
        </nav>
    );
}
