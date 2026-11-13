import { useState, useEffect, useRef } from "react";
import { Save, Trash2, Eye, EyeOff, KeyRound, X, Download, Upload, UserRound, Scale, Link2, ShieldCheck, Footprints, Dumbbell, Database } from "lucide-react";
import { getMe, updateMe, updateSex, changePassword, deleteMe, updateLeaderboardVisibility } from "../auth/api.ts";
import type { Sex } from "../../assets/js/data/apiTypes.ts";
import { getBody, formatEntryDate, localToday, type BodyEntry } from "../body/api.ts";
import LogWeightButton from "../body/LogWeightButton.tsx";
import { clearLeaderboardCache } from "../leaderboard/api.ts";
import type { User } from "../auth/api.ts";
import { useAuth } from "../../context/AuthContext.tsx";
import { useShortcut } from "../../hooks/useShortcut.ts";
import Button from "../../components/general/ButtonComponent.tsx";
import { getStravaStatus, getStravaAuthUrl, disconnectStrava, importStravaHistory } from "../strava/api.ts";
import type { StravaStatus } from "../strava/api.ts";
import GearSection from "./GearSection.tsx";
import CustomExercisesSection from "./CustomExercisesSection.tsx";
import ExportOverlay from "../export/ExportOverlay.tsx";
import Skeleton from "../../components/general/Skeleton.tsx";
import "../../assets/css/profile.css";

/** The page's sections, in order: the rail's nav and the scroll-spy both read this. */
const SECTIONS = [
    { id: "account", label: "Account", icon: UserRound },
    { id: "body", label: "Body", icon: Scale },
    { id: "password", label: "Password", icon: KeyRound },
    { id: "apps", label: "Connected apps", icon: Link2 },
    { id: "privacy", label: "Privacy", icon: ShieldCheck },
    { id: "gear", label: "Gear", icon: Footprints },
    { id: "exercises", label: "My exercises", icon: Dumbbell },
    { id: "data", label: "Your data", icon: Database },
    { id: "danger", label: "Delete account", icon: Trash2 },
] as const;
type SectionId = (typeof SECTIONS)[number]["id"];

const SEXES: { value: Sex | ""; label: string }[] = [
    { value: "Male", label: "Male" },
    { value: "Female", label: "Female" },
    { value: "", label: "Not set" },
];
const ACTIVE_DAYS = [0, 1, 2, 3, 4, 5, 6, 7];

/**
 * The section nearest the top of the screen, for the rail's nav. A section you
 * jump to wins while the page scrolls there, and keeps winning if it sits too
 * low to reach the top (the last few cards on a tall screen).
 */
function useActiveSection(ready: boolean): [SectionId, (id: SectionId) => void] {
    const [active, setActive] = useState<SectionId>("account");
    const pinnedUntil = useRef(0);
    useEffect(() => {
        if (!ready) return;
        // The current section is the last one whose top has passed a line 30% down
        // the screen, read from positions once a frame while anything scrolls, so
        // the tail of the section above never wins over the one you're reading.
        let frame = 0;
        const pick = () => {
            frame = 0;
            if (Date.now() < pinnedUntil.current) return;
            const line = window.innerHeight * 0.3;
            let current: SectionId = SECTIONS[0].id;
            for (const s of SECTIONS) {
                const el = document.getElementById(s.id);
                if (el && el.getBoundingClientRect().top <= line) current = s.id;
            }
            setActive(current);
        };
        const onScroll = () => { if (!frame) frame = requestAnimationFrame(pick); };
        // Capture: the page scrolls inside the app's main column, not the window.
        document.addEventListener("scroll", onScroll, { capture: true, passive: true });
        pick();
        return () => {
            document.removeEventListener("scroll", onScroll, { capture: true });
            if (frame) cancelAnimationFrame(frame);
        };
    }, [ready]);
    function jumpTo(id: SectionId) {
        pinnedUntil.current = Date.now() + 1000;
        setActive(id);
    }
    return [active, jumpTo];
}

function ProfilePage() {
    const { logout } = useAuth();
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);

    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [birthDate, setBirthDate] = useState("");
    const [weight, setWeight] = useState(0);
    const [height, setHeight] = useState(0);
    const [activeDays, setActiveDays] = useState(0);
    const [sex, setSex] = useState<Sex | "">("");
    const [lastWeighIn, setLastWeighIn] = useState<BodyEntry | null>(null);

    const [oldPassword, setOldPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [showOldPw, setShowOldPw] = useState(false);
    const [showNewPw, setShowNewPw] = useState(false);

    const [saving, setSaving] = useState(false);
    const [savingPw, setSavingPw] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [pwError, setPwError] = useState<string | null>(null);
    const [pwSuccess, setPwSuccess] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [deleteConfirmText, setDeleteConfirmText] = useState("");

    const [strava, setStrava] = useState<StravaStatus | null>(null);
    const [stravaImporting, setStravaImporting] = useState(false);
    const [stravaImportResult, setStravaImportResult] = useState<number | null>(null);
    const [stravaError, setStravaError] = useState<string | null>(null);
    const [exportOpen, setExportOpen] = useState(false);
    const [savingVisibility, setSavingVisibility] = useState(false);
    const [visibilityError, setVisibilityError] = useState<string | null>(null);

    const [activeSection, markActive] = useActiveSection(!loading);
    const navRef = useRef<HTMLElement>(null);

    // On phones the nav is a sideways row of chips: keep the current one in view.
    useEffect(() => {
        const nav = navRef.current;
        const item = nav?.querySelector<HTMLElement>(".profile-nav-item.active");
        if (!nav || !item || nav.scrollWidth <= nav.clientWidth) return;
        const left = item.offsetLeft - (nav.clientWidth - item.offsetWidth) / 2;
        nav.scrollTo({ left: Math.max(0, left) });
    }, [activeSection]);

    function fillForm(u: User) {
        setFirstName(u.firstName);
        setLastName(u.lastName);
        setUsername(u.username);
        setEmail(u.email);
        setBirthDate(u.birthDate ?? "");
        setWeight(u.weight);
        setHeight(u.height);
        setActiveDays(u.activeDays);
        setSex(u.sex ?? "");
    }

    useEffect(() => {
        Promise.all([
            getMe(),
            getStravaStatus().catch(() => null),
            getBody().catch(() => [] as BodyEntry[]),
        ]).then(([u, s, body]) => {
            setUser(u);
            fillForm(u);
            setLastWeighIn(body.at(-1) ?? null);
            if (s) setStrava(s);
            // Drop the ?strava=connected the OAuth redirect leaves behind
            const params = new URLSearchParams(window.location.search);
            if (params.get("strava") === "connected") {
                window.history.replaceState({}, "", window.location.pathname);
            }
        }).finally(() => setLoading(false));
    }, []);

    const isDirty = user != null && (
        firstName !== user.firstName ||
        lastName !== user.lastName ||
        username !== user.username ||
        email !== user.email ||
        birthDate !== (user.birthDate ?? "") ||
        height !== user.height ||
        activeDays !== user.activeDays ||
        sex !== (user.sex ?? "")
    );

    /** Weight lives in the body log now; a weigh-in updates it here without making the form dirty. */
    function handleLogged(entry: BodyEntry) {
        setLastWeighIn(prev => (!prev || entry.date >= prev.date ? entry : prev));
        if (!lastWeighIn || entry.date >= lastWeighIn.date) {
            setWeight(entry.weightKg);
            setUser(prev => prev ? { ...prev, weight: entry.weightKg } : prev);
        }
    }

    async function handleSave() {
        if (!isDirty || saving) return;
        setSaving(true);
        setError(null);
        try {
            const sexChanged = sex !== (user?.sex ?? "");
            await updateMe({ email, username, firstName, lastName, weight, height, activeDays, birthDate });
            if (sexChanged) await updateSex(sex || null);
            setUser(prev => prev ? { ...prev, email, username, firstName, lastName, weight, height, activeDays, birthDate, sex: sex || null } : prev);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save profile");
        } finally {
            setSaving(false);
        }
    }

    function handleDiscard() {
        if (user) fillForm(user);
        setError(null);
    }

    async function handleSavePassword(e: React.FormEvent) {
        e.preventDefault();
        if (newPassword !== confirmPassword) {
            setPwError("New passwords do not match");
            return;
        }
        setSavingPw(true);
        setPwError(null);
        setPwSuccess(false);
        try {
            await changePassword(oldPassword, newPassword);
            setPwSuccess(true);
            setOldPassword("");
            setNewPassword("");
            setConfirmPassword("");
        } catch (e) {
            setPwError(e instanceof Error ? e.message : "Failed to change password");
        } finally {
            setSavingPw(false);
        }
    }

    async function handleDeleteAccount() {
        try {
            await deleteMe();
            logout();
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to delete account");
            setShowDeleteConfirm(false);
        }
    }

    async function handleToggleLeaderboard() {
        if (!user || savingVisibility) return;
        const next = !user.showOnLeaderboard;
        setSavingVisibility(true);
        setVisibilityError(null);
        setUser(prev => prev ? { ...prev, showOnLeaderboard: next } : prev);
        try {
            await updateLeaderboardVisibility(next);
            clearLeaderboardCache();
        } catch {
            setUser(prev => prev ? { ...prev, showOnLeaderboard: !next } : prev);
            setVisibilityError("Couldn't update your leaderboard visibility");
        } finally {
            setSavingVisibility(false);
        }
    }

    async function handleStravaConnect() {
        setStravaError(null);
        try {
            const url = await getStravaAuthUrl();
            window.location.href = url;
        } catch {
            setStravaError("Failed to connect to Strava");
        }
    }

    async function handleStravaDisconnect() {
        setStravaError(null);
        try {
            await disconnectStrava();
            setStrava({ connected: false, athleteName: null, connectedAt: null });
            setStravaImportResult(null);
        } catch {
            setStravaError("Failed to disconnect Strava");
        }
    }

    async function handleStravaImport() {
        setStravaImporting(true);
        setStravaError(null);
        setStravaImportResult(null);
        try {
            const result = await importStravaHistory();
            setStravaImportResult(result.imported);
        } catch {
            setStravaError("Import failed — check your connection and try again");
        } finally {
            setStravaImporting(false);
        }
    }

    function goTo(id: SectionId) {
        markActive(id);
        document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    useShortcut("s", handleSave, true);

    if (loading) {
        return (
            <div className="profile-page">
                <header className="page-header">
                    <div className="page-header-titles"><h1>Profile</h1></div>
                </header>
                <div className="profile-body" role="status" aria-label="Loading profile">
                    <div className="profile-rail">
                        <div className="profile-card profile-skeleton">
                            <Skeleton width="3rem" height="3rem" radius="50%" />
                            <Skeleton width="9rem" height="1.1rem" />
                            <Skeleton width="12rem" height="0.9rem" />
                        </div>
                    </div>
                    <div className="profile-main">
                        {[0, 1].map(i => (
                            <div key={i} className="profile-section profile-skeleton">
                                <Skeleton width="11rem" height="1.35rem" />
                                <div className="profile-grid">
                                    <Skeleton height="2.5rem" radius="var(--radius-md)" />
                                    <Skeleton height="2.5rem" radius="var(--radius-md)" />
                                </div>
                                <Skeleton height="2.5rem" radius="var(--radius-md)" />
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        );
    }

    const fullName = `${user?.firstName ?? ""} ${user?.lastName ?? ""}`.trim();

    return (
        <div className="profile-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>Profile</h1>
                    <p className="header-subtitle">Your account, body numbers, connections and data</p>
                </div>
            </header>

            <div className="profile-body">
                <aside className="profile-rail">
                    <div className="profile-card profile-id">
                        <span className="profile-avatar" aria-hidden><UserRound size={22} /></span>
                        <div className="profile-id-text">
                            <strong>{fullName || user?.username}</strong>
                            <span>@{user?.username}</span>
                            <span>{user?.email}</span>
                        </div>
                    </div>
                    <nav className="profile-nav" aria-label="Profile sections" ref={navRef}>
                        {SECTIONS.map(s => (
                            <a
                                key={s.id}
                                href={`#${s.id}`}
                                className={`profile-nav-item${activeSection === s.id ? " active" : ""}${s.id === "danger" ? " danger" : ""}`}
                                aria-current={activeSection === s.id ? "location" : undefined}
                                onClick={e => { e.preventDefault(); goTo(s.id); }}
                            >
                                <s.icon size={18} aria-hidden />
                                {s.label}
                            </a>
                        ))}
                    </nav>
                </aside>

                <div className="profile-main">
                    {error && <div className="error-banner">{error}</div>}

                    <section className="profile-section" id="account">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Account</h2>
                            <p className="profile-section-desc">Your username is what others see on the leaderboard. Email is for signing in.</p>
                        </div>
                        <div className="profile-grid">
                            <div className="profile-field">
                                <label htmlFor="firstName">First name</label>
                                <input type="text" id="firstName" value={firstName} maxLength={100} onChange={e => setFirstName(e.target.value)} />
                            </div>
                            <div className="profile-field">
                                <label htmlFor="lastName">Last name</label>
                                <input type="text" id="lastName" value={lastName} maxLength={100} onChange={e => setLastName(e.target.value)} />
                            </div>
                            <div className="profile-field">
                                <label htmlFor="username">Username</label>
                                <input type="text" id="username" value={username} maxLength={128} onChange={e => setUsername(e.target.value)} />
                            </div>
                            <div className="profile-field">
                                <label htmlFor="email">Email</label>
                                <input type="email" id="email" value={email} onChange={e => setEmail(e.target.value)} />
                            </div>
                        </div>
                    </section>

                    <section className="profile-section" id="body">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Body</h2>
                            <p className="profile-section-desc">Used for calorie and macro targets. Weight comes from your body log.</p>
                        </div>
                        <div className="profile-grid">
                            <div className="profile-field">
                                <span className="profile-field-label">Weight</span>
                                <div className="profile-weight">
                                    <p className="profile-weight-value">
                                        <b>{weight.toLocaleString("en-GB", { maximumFractionDigits: 2 })} kg</b>
                                        {lastWeighIn && (
                                            <span>{lastWeighIn.date === localToday() ? "logged today" : `logged ${formatEntryDate(lastWeighIn.date)}`}</span>
                                        )}
                                    </p>
                                    <LogWeightButton onLogged={handleLogged} />
                                </div>
                            </div>
                            <div className="profile-field">
                                <label htmlFor="height">Height</label>
                                <span className="profile-unit">
                                    <input type="number" id="height" value={height} min={1} max={300} onChange={e => setHeight(Number(e.target.value))} />
                                    <span aria-hidden>cm</span>
                                </span>
                            </div>
                            <div className="profile-field">
                                <label htmlFor="birthDate">Date of birth</label>
                                <input type="date" id="birthDate" value={birthDate} max={new Date().toISOString().split("T")[0]} onChange={e => setBirthDate(e.target.value)} />
                            </div>
                            <div className="profile-field">
                                <span className="profile-field-label" id="sexLabel">Sex</span>
                                <div className="profile-segmented" role="radiogroup" aria-labelledby="sexLabel">
                                    {SEXES.map(s => (
                                        <button key={s.label} type="button" role="radio" aria-checked={sex === s.value}
                                            className={sex === s.value ? "active" : undefined} onClick={() => setSex(s.value)}>
                                            {s.label}
                                        </button>
                                    ))}
                                </div>
                            </div>
                            <div className="profile-field profile-field-wide">
                                <span className="profile-field-label" id="activeDaysLabel">Active days per week <span className="profile-label-hint">your training goal</span></span>
                                <div className="profile-segmented" role="radiogroup" aria-labelledby="activeDaysLabel">
                                    {ACTIVE_DAYS.map(d => (
                                        <button key={d} type="button" role="radio" aria-checked={activeDays === d}
                                            className={activeDays === d ? "active" : undefined} onClick={() => setActiveDays(d)}>
                                            {d}
                                        </button>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </section>

                    <section className="profile-section" id="password">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Password</h2>
                            <p className="profile-section-desc">At least 8 characters.</p>
                        </div>

                        <form onSubmit={handleSavePassword} className="profile-fields">
                            {pwError && <div className="error-banner">{pwError}</div>}
                            {pwSuccess && <div className="profile-success-banner">Password changed successfully.</div>}

                            <div className="profile-grid profile-grid-3">
                                <div className="profile-field">
                                    <label htmlFor="oldPassword">Current password</label>
                                    <div className="profile-pw-wrap">
                                        <input type={showOldPw ? "text" : "password"} id="oldPassword" value={oldPassword} required autoComplete="current-password" onChange={e => setOldPassword(e.target.value)} />
                                        <button type="button" className="profile-pw-toggle" aria-label={showOldPw ? "Hide password" : "Show password"} onClick={() => setShowOldPw(v => !v)}>
                                            {showOldPw ? <EyeOff size={14} /> : <Eye size={14} />}
                                        </button>
                                    </div>
                                </div>
                                <div className="profile-field">
                                    <label htmlFor="newPassword">New password</label>
                                    <div className="profile-pw-wrap">
                                        <input type={showNewPw ? "text" : "password"} id="newPassword" value={newPassword} minLength={8} required autoComplete="new-password" onChange={e => setNewPassword(e.target.value)} />
                                        <button type="button" className="profile-pw-toggle" aria-label={showNewPw ? "Hide password" : "Show password"} onClick={() => setShowNewPw(v => !v)}>
                                            {showNewPw ? <EyeOff size={14} /> : <Eye size={14} />}
                                        </button>
                                    </div>
                                </div>
                                <div className="profile-field">
                                    <label htmlFor="confirmPassword">Confirm new password</label>
                                    <div className="profile-pw-wrap">
                                        <input type={showNewPw ? "text" : "password"} id="confirmPassword" value={confirmPassword} required autoComplete="new-password" onChange={e => setConfirmPassword(e.target.value)} />
                                    </div>
                                </div>
                            </div>

                            <div className="profile-section-actions">
                                <button type="submit" className="button-component white" disabled={savingPw || !oldPassword || !newPassword || !confirmPassword}>
                                    <KeyRound size={14} />
                                    Change password
                                </button>
                            </div>
                        </form>
                    </section>

                    <section className="profile-section" id="apps">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Connected apps</h2>
                            <p className="profile-section-desc">Bring runs and rides in from Strava as logged workouts.</p>
                        </div>

                        {stravaError && <div className="error-banner">{stravaError}</div>}

                        <div className="strava-connection">
                            <div className="strava-connection-info">
                                <div className="strava-logo">STR</div>
                                <div className="strava-connection-text">
                                    {strava?.connected ? (
                                        <>
                                            <span className="strava-connection-name">{strava.athleteName ?? "Strava"}</span>
                                            <span className="strava-connection-date">
                                                Connected {strava.connectedAt ? new Date(strava.connectedAt).toLocaleDateString() : ""}
                                            </span>
                                        </>
                                    ) : (
                                        <>
                                            <span className="strava-connection-name">Strava</span>
                                            <span className="strava-connection-date">Not connected</span>
                                        </>
                                    )}
                                </div>
                            </div>

                            <div className="strava-connection-actions">
                                {strava?.connected ? (
                                    <>
                                        <Button text={stravaImporting ? "Importing…" : "Import history"} style="white" icon={<Download size={14} />} disabled={stravaImporting} onClick={handleStravaImport} />
                                        <button type="button" className="button-component red" onClick={handleStravaDisconnect}>Disconnect</button>
                                    </>
                                ) : (
                                    <Button text="Connect Strava" style="white" onClick={handleStravaConnect} />
                                )}
                            </div>
                        </div>

                        {stravaImportResult !== null && (
                            <p className="strava-import-success">
                                {stravaImportResult === 0
                                    ? "No new activities to import."
                                    : `Imported ${stravaImportResult} activit${stravaImportResult === 1 ? "y" : "ies"} from Strava.`}
                            </p>
                        )}
                    </section>

                    <section className="profile-section" id="privacy">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Privacy</h2>
                            <p className="profile-section-desc">
                                The monthly leaderboard shows your username and monthly totals to other Bloom users.
                                Workouts, names and email are never shared.
                            </p>
                        </div>

                        {visibilityError && <div className="error-banner">{visibilityError}</div>}

                        <div className="profile-toggle-row">
                            <div className="profile-toggle-text">
                                <span id="leaderboardToggleLabel" className="profile-toggle-label">Show me on the leaderboard</span>
                                <span className="profile-toggle-hint">
                                    {user?.showOnLeaderboard
                                        ? "Other users can see your standing."
                                        : "You're hidden. You still see your own standing, but nobody else does."}
                                </span>
                            </div>
                            <button type="button" role="switch" className="profile-switch" aria-checked={user?.showOnLeaderboard ?? true}
                                aria-labelledby="leaderboardToggleLabel" disabled={!user || savingVisibility} onClick={handleToggleLeaderboard}>
                                <span className="profile-switch-thumb" aria-hidden />
                            </button>
                        </div>
                    </section>

                    <GearSection initialGear={user?.gear ?? []} initialIcons={user?.gearIcons ?? {}} />
                    <CustomExercisesSection />

                    <section className="profile-section" id="data">
                        <div className="profile-section-head">
                            <h2 className="profile-section-title">Your data</h2>
                            <p className="profile-section-desc">
                                Download your workouts as an Excel workbook, CSV, or a full JSON backup, or import a file you exported before.
                            </p>
                        </div>
                        <div className="profile-section-actions">
                            <Button text="Export data" style="white" icon={<Download size={14} />} onClick={() => setExportOpen(true)} />
                            <Button text="Import data" style="white" icon={<Upload size={14} />} target="/import" />
                        </div>
                    </section>

                    <section className="profile-section profile-danger-section" id="danger">
                        <div className="profile-danger-row">
                            <div className="profile-section-head">
                                <h2 className="profile-section-title profile-danger-title">Delete account</h2>
                                <p className="profile-danger-desc">Permanently delete your account and all its data. This cannot be undone.</p>
                            </div>
                            <button type="button" className="button-component red" onClick={() => { setShowDeleteConfirm(true); setDeleteConfirmText(""); }}>
                                <Trash2 size={14} />
                                Delete account
                            </button>
                        </div>
                    </section>

                    {isDirty && (
                        <div className="profile-save-bar" role="region" aria-label="Unsaved changes">
                            <span>You have unsaved changes</span>
                            <div>
                                <button type="button" className="button-component grey" onClick={handleDiscard} disabled={saving}>Discard</button>
                                <Button text={saving ? "Saving…" : "Save changes"} style="green" icon={<Save size={14} />} disabled={saving} onClick={handleSave} />
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {exportOpen && <ExportOverlay onClose={() => setExportOpen(false)} />}

            {showDeleteConfirm && (
                <div className="overlay-backdrop" onClick={() => setShowDeleteConfirm(false)}>
                    <div className="overlay-panel" onClick={e => e.stopPropagation()}>
                        <div className="overlay-header">
                            <div>
                                <strong>Delete Account</strong>
                                <span>This action is permanent and cannot be undone.</span>
                            </div>
                            <button className="overlay-close" onClick={() => setShowDeleteConfirm(false)}>
                                <X size={16} />
                            </button>
                        </div>
                        <div className="overlay-content">
                            <div className="unsaved-dialog">
                                <p className="unsaved-dialog-body">
                                    All your workouts, templates, and personal data will be permanently deleted.
                                    Type <strong>delete</strong> to confirm.
                                </p>
                                <input
                                    type="text"
                                    className="profile-confirm-input"
                                    placeholder={`Type "delete" to confirm`}
                                    value={deleteConfirmText}
                                    onChange={e => setDeleteConfirmText(e.target.value)}
                                    autoFocus
                                />
                                <div className="unsaved-dialog-actions">
                                    <button className="unsaved-dialog-discard" onClick={() => setShowDeleteConfirm(false)}>
                                        Cancel
                                    </button>
                                    <button
                                        type="button"
                                        className="button-component red"
                                        disabled={deleteConfirmText.toLowerCase() !== "delete"}
                                        onClick={handleDeleteAccount}
                                    >
                                        <Trash2 size={14} />
                                        Delete Forever
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ProfilePage;
