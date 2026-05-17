import { useState, useEffect, useRef } from "react";
import { Save, Trash2, Eye, EyeOff, KeyRound, X } from "lucide-react";
import { getMe, updateMe, changePassword, deleteMe } from "../auth/api.ts";
import type { User } from "../auth/api.ts";
import { useAuth } from "../../context/AuthContext.tsx";
import { useShortcut } from "../../hooks/useShortcut.ts";
import Button from "../../components/general/ButtonComponent.tsx";
import "../../assets/css/profile.css";

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
    const [showSticky, setShowSticky] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [deleteConfirmText, setDeleteConfirmText] = useState("");

    const actionsRef = useRef<HTMLDivElement>(null);
    const panelTopRef = useRef(0);

    useEffect(() => {
        getMe().then(u => {
            setUser(u);
            setFirstName(u.firstName);
            setLastName(u.lastName);
            setUsername(u.username);
            setEmail(u.email);
            setBirthDate(u.birthDate ?? "");
            setWeight(u.weight);
            setHeight(u.height);
            setActiveDays(u.activeDays);
        }).finally(() => setLoading(false));
    }, []);

    useEffect(() => {
        const el = actionsRef.current;
        if (!el || loading) return;
        const panel = el.closest(".profile-page") as HTMLElement | null;
        const header = panel?.querySelector(".page-header") as HTMLElement | null;
        const top = Math.round((header?.getBoundingClientRect().bottom ?? 0)) - 1;
        panelTopRef.current = top;
        const observer = new IntersectionObserver(
            ([entry]) => setShowSticky(!entry.isIntersecting),
            { rootMargin: `-${top}px 0px 0px 0px`, threshold: 0 }
        );
        observer.observe(el);
        return () => observer.disconnect();
    }, [loading]);

    const isDirty = user != null && (
        firstName !== user.firstName ||
        lastName !== user.lastName ||
        username !== user.username ||
        email !== user.email ||
        birthDate !== (user.birthDate ?? "") ||
        weight !== user.weight ||
        height !== user.height ||
        activeDays !== user.activeDays
    );

    async function handleSave() {
        if (!isDirty || saving) return;
        setSaving(true);
        setError(null);
        try {
            await updateMe({ email, username, firstName, lastName, weight, height, activeDays, birthDate });
            setUser(prev => prev ? { ...prev, email, username, firstName, lastName, weight, height, activeDays, birthDate } : prev);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save profile");
        } finally {
            setSaving(false);
        }
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

    useShortcut("s", handleSave, true);

    if (loading) {
        return (
            <div className="profile-page">
                <header className="page-header">
                    <div><p>Settings</p><h1>Profile</h1></div>
                </header>
                <div className="profile-detail">
                    <p className="profile-loading">Loading…</p>
                </div>
            </div>
        );
    }

    return (
        <div className="profile-page">
            <header className="page-header">
                <div>
                    <p>Settings</p>
                    <h1>Profile</h1>
                </div>
            </header>

            <div className="profile-detail">
                {showSticky && isDirty && (
                    <div className="sticky-save-bar" style={{ top: panelTopRef.current }}>
                        <Button text="Save Changes" style="white" icon={<Save size={14} />} disabled={saving} onClick={handleSave} />
                    </div>
                )}

                {error && <div className="error-banner">{error}</div>}

                <section className="profile-section">
                    <h2 className="profile-section-title">Personal Information</h2>

                    <div className="profile-fields">
                        <div className="profile-row-2">
                            <div className="profile-field">
                                <label htmlFor="firstName">First name</label>
                                <input
                                    type="text"
                                    id="firstName"
                                    value={firstName}
                                    maxLength={100}
                                    onChange={e => setFirstName(e.target.value)}
                                />
                            </div>
                            <div className="profile-field">
                                <label htmlFor="lastName">Last name</label>
                                <input
                                    type="text"
                                    id="lastName"
                                    value={lastName}
                                    maxLength={100}
                                    onChange={e => setLastName(e.target.value)}
                                />
                            </div>
                        </div>

                        <div className="profile-field">
                            <label htmlFor="username">Username</label>
                            <input
                                type="text"
                                id="username"
                                value={username}
                                maxLength={128}
                                onChange={e => setUsername(e.target.value)}
                            />
                        </div>

                        <div className="profile-field">
                            <label htmlFor="email">Email</label>
                            <input
                                type="email"
                                id="email"
                                value={email}
                                onChange={e => setEmail(e.target.value)}
                            />
                        </div>

                        <div className="profile-field">
                            <label htmlFor="birthDate">Date of birth</label>
                            <input
                                type="date"
                                id="birthDate"
                                value={birthDate}
                                max={new Date().toISOString().split("T")[0]}
                                onChange={e => setBirthDate(e.target.value)}
                            />
                        </div>

                        <div className="profile-row-2">
                            <div className="profile-field">
                                <label htmlFor="weight">Weight (kg)</label>
                                <input
                                    type="number"
                                    id="weight"
                                    value={weight}
                                    min={0.1}
                                    max={500}
                                    step={0.1}
                                    onChange={e => setWeight(Number(e.target.value))}
                                />
                            </div>
                            <div className="profile-field">
                                <label htmlFor="height">Height (cm)</label>
                                <input
                                    type="number"
                                    id="height"
                                    value={height}
                                    min={1}
                                    max={300}
                                    onChange={e => setHeight(Number(e.target.value))}
                                />
                            </div>
                        </div>

                        <div className="profile-field">
                            <label htmlFor="activeDays">Active days per week</label>
                            <input
                                type="number"
                                id="activeDays"
                                value={activeDays}
                                min={0}
                                max={7}
                                onChange={e => setActiveDays(Number(e.target.value))}
                            />
                        </div>
                    </div>

                    <div className="profile-section-actions" ref={actionsRef}>
                        <Button
                            text="Save Changes"
                            style="white"
                            icon={<Save size={14} />}
                            disabled={!isDirty || saving}
                            onClick={handleSave}
                        />
                    </div>
                </section>

                <section className="profile-section">
                    <h2 className="profile-section-title">Change Password</h2>

                    <form onSubmit={handleSavePassword} className="profile-fields">
                        {pwError && <div className="error-banner">{pwError}</div>}
                        {pwSuccess && <div className="profile-success-banner">Password changed successfully.</div>}

                        <div className="profile-field">
                            <label htmlFor="oldPassword">Current password</label>
                            <div className="profile-pw-wrap">
                                <input
                                    type={showOldPw ? "text" : "password"}
                                    id="oldPassword"
                                    value={oldPassword}
                                    required
                                    onChange={e => setOldPassword(e.target.value)}
                                />
                                <button type="button" className="profile-pw-toggle" onClick={() => setShowOldPw(v => !v)}>
                                    {showOldPw ? <EyeOff size={14} /> : <Eye size={14} />}
                                </button>
                            </div>
                        </div>

                        <div className="profile-row-2">
                            <div className="profile-field">
                                <label htmlFor="newPassword">New password</label>
                                <div className="profile-pw-wrap">
                                    <input
                                        type={showNewPw ? "text" : "password"}
                                        id="newPassword"
                                        value={newPassword}
                                        minLength={8}
                                        required
                                        onChange={e => setNewPassword(e.target.value)}
                                    />
                                    <button type="button" className="profile-pw-toggle" onClick={() => setShowNewPw(v => !v)}>
                                        {showNewPw ? <EyeOff size={14} /> : <Eye size={14} />}
                                    </button>
                                </div>
                            </div>
                            <div className="profile-field">
                                <label htmlFor="confirmPassword">Confirm new password</label>
                                <div className="profile-pw-wrap">
                                    <input
                                        type={showNewPw ? "text" : "password"}
                                        id="confirmPassword"
                                        value={confirmPassword}
                                        required
                                        onChange={e => setConfirmPassword(e.target.value)}
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="profile-section-actions">
                            <button
                                type="submit"
                                className="button-component white"
                                disabled={savingPw || !oldPassword || !newPassword || !confirmPassword}
                            >
                                <KeyRound size={14} />
                                Change Password
                            </button>
                        </div>
                    </form>
                </section>

                <section className="profile-section profile-danger-section">
                    <h2 className="profile-section-title profile-danger-title">Danger Zone</h2>
                    <p className="profile-danger-desc">
                        Permanently delete your account and all associated data. This cannot be undone.
                    </p>
                    <button
                        type="button"
                        className="button-component red"
                        onClick={() => { setShowDeleteConfirm(true); setDeleteConfirmText(""); }}
                    >
                        <Trash2 size={14} />
                        Delete Account
                    </button>
                </section>
            </div>

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
