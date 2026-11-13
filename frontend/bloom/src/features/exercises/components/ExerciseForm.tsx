import { useMemo, useRef, useState, useEffect } from "react";
import { ArrowLeft, Plus, Trash2, X, Check } from "lucide-react";
import type { ExerciseType } from "../../../types.ts";
import type { Exercise } from "../api.ts";
import { createCustomExercise, updateCustomExercise, deleteCustomExercise } from "../api.ts";

const TYPES: { value: ExerciseType; label: string; hint: string }[] = [
    { value: "Strength",   label: "Strength",   hint: "Reps × weight" },
    { value: "Cardio",     label: "Cardio",     hint: "Distance & time" },
    { value: "Plyometric", label: "Plyometric", hint: "Explosive reps" },
];

interface ExerciseFormProps {
    /** Exercise being edited; omit to create a new one. */
    exercise?: Exercise;
    /** Pre-filled name, e.g. from the library search box. */
    initialName?: string;
    /** All muscles known in the catalog, offered as quick picks. */
    knownMuscles: string[];
    onSaved: (exercise: Exercise) => void;
    onDeleted?: (exerciseId: string) => void;
    onCancel: () => void;
    /** Text of the back link; the library is where the form usually opens from. */
    backLabel?: string;
    /** Button text when creating; in the library a new exercise is also added to the workout. */
    createLabel?: string;
}

function ExerciseForm({ exercise, initialName = "", knownMuscles, onSaved, onDeleted, onCancel, backLabel = "Back to library", createLabel = "Create & add" }: ExerciseFormProps) {
    const editing = !!exercise;
    const [name, setName] = useState(exercise?.name ?? initialName);
    const [type, setType] = useState<ExerciseType>(exercise?.type ?? "Strength");
    const [muscles, setMuscles] = useState<string[]>(exercise?.targetMuscles ?? []);
    const [secondary, setSecondary] = useState<string[]>(exercise?.secondaryMuscles ?? []);
    const [description, setDescription] = useState(exercise?.description ?? "");
    const [saving, setSaving] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [touched, setTouched] = useState(false);
    const nameRef = useRef<HTMLInputElement>(null);

    useEffect(() => { nameRef.current?.focus(); }, []);

    const errors = {
        name: name.trim() === "" ? "Give your exercise a name" : null,
        muscles: muscles.length === 0 ? "Pick at least one target muscle" : null,
        description: description.trim() === "" ? "Add a short description or cue" : null,
    };
    const valid = !errors.name && !errors.muscles && !errors.description;

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setTouched(true);
        if (!valid || saving) return;
        setSaving(true);
        setError(null);
        // A muscle is either a primary mover or a helper, never both.
        const body = { name: name.trim(), type, targetMuscles: muscles, secondaryMuscles: secondary.filter(m => !muscles.includes(m)), description: description.trim() };
        try {
            const saved = editing
                ? await updateCustomExercise(exercise.id, body)
                : await createCustomExercise(body);
            onSaved(saved);
        } catch (err) {
            setError((err as { error?: string })?.error ?? "Could not save exercise");
            setSaving(false);
        }
    }

    async function handleDelete() {
        if (!exercise) return;
        setSaving(true);
        try {
            await deleteCustomExercise(exercise.id);
            onDeleted?.(exercise.id);
        } catch (err) {
            setError((err as { error?: string })?.error ?? "Could not delete exercise");
            setSaving(false);
            setConfirmDelete(false);
        }
    }

    return (
        <form className="exercise-form" onSubmit={handleSubmit} noValidate>
            <button type="button" className="exercise-form-back" onClick={onCancel}>
                <ArrowLeft size={14} /> {backLabel}
            </button>

            <div className="exercise-form-heading">
                <p className="eyebrow-label">{editing ? "Edit custom exercise" : "New custom exercise"}</p>
                <p className="exercise-form-sub">
                    {editing ? "Changes apply everywhere this exercise is used." : "Only you can see exercises you create."}
                </p>
            </div>

            {error && <div className="error-banner">{error}</div>}

            <label className="exercise-form-field">
                <span>Name</span>
                <input
                    ref={nameRef}
                    value={name}
                    maxLength={100}
                    placeholder="e.g. Weighted Pistol Squat"
                    onChange={e => setName(e.target.value)}
                    aria-invalid={touched && !!errors.name}
                />
                {touched && errors.name && <small className="exercise-form-error">{errors.name}</small>}
            </label>

            <fieldset className="exercise-form-field">
                <legend>Type</legend>
                <div className="exercise-form-types" role="radiogroup">
                    {TYPES.map(t => (
                        <button
                            key={t.value}
                            type="button"
                            role="radio"
                            aria-checked={type === t.value}
                            className={`exercise-form-type ${t.value.toLowerCase()}${type === t.value ? " active" : ""}`}
                            onClick={() => setType(t.value)}
                        >
                            <span className={`type-dot ${t.value.toLowerCase()}`} />
                            <span className="exercise-form-type-label">{t.label}</span>
                            <span className="exercise-form-type-hint">{t.hint}</span>
                        </button>
                    ))}
                </div>
            </fieldset>

            <MusclePicker
                id="muscles-label"
                label="Target muscles"
                value={muscles}
                onChange={next => { setMuscles(next); setSecondary(prev => prev.filter(m => !next.includes(m))); }}
                knownMuscles={knownMuscles}
                error={touched ? errors.muscles : null}
            />

            <MusclePicker
                id="secondary-muscles-label"
                label="Secondary muscles"
                hint="Optional: muscles that help. Each set counts as half a set for them."
                value={secondary}
                onChange={setSecondary}
                knownMuscles={knownMuscles.filter(m => !muscles.includes(m))}
                maxPicks={8}
            />

            <label className="exercise-form-field">
                <span>Description</span>
                <textarea
                    value={description}
                    maxLength={2000}
                    rows={3}
                    placeholder="How to perform it, key cues…"
                    onChange={e => setDescription(e.target.value)}
                    aria-invalid={touched && !!errors.description}
                />
                {touched && errors.description && <small className="exercise-form-error">{errors.description}</small>}
            </label>

            <div className="exercise-form-actions">
                {editing && onDeleted && (
                    confirmDelete ? (
                        <div className="exercise-form-confirm">
                            <span>Delete for good? Logs using it will show “Unknown exercise”.</span>
                            <button type="button" className="button-component red" disabled={saving} onClick={handleDelete}>
                                <Trash2 size={13} /> Delete
                            </button>
                            <button type="button" className="unsaved-dialog-discard" onClick={() => setConfirmDelete(false)}>Keep</button>
                        </div>
                    ) : (
                        <button type="button" className="exercise-form-delete" onClick={() => setConfirmDelete(true)}>
                            <Trash2 size={13} /> Delete exercise
                        </button>
                    )
                )}
                {!confirmDelete && (
                    <button type="submit" className="button-component green exercise-form-submit" disabled={saving}>
                        <Check size={14} />
                        {saving ? "Saving…" : editing ? "Save changes" : createLabel}
                    </button>
                )}
            </div>
        </form>
    );
}

interface MusclePickerProps {
    id: string;
    label: string;
    hint?: string;
    value: string[];
    onChange: (muscles: string[]) => void;
    knownMuscles: string[];
    error?: string | null;
    /** Quick picks shown before you type. */
    maxPicks?: number;
}

/** Chips you can remove, a free-text box, and the catalog's muscles as quick picks. */
function MusclePicker({ id, label, hint, value, onChange, knownMuscles, error, maxPicks = 14 }: MusclePickerProps) {
    const [input, setInput] = useState("");

    const suggestions = useMemo(() => {
        const q = input.trim().toLowerCase();
        return knownMuscles
            .filter(m => !value.includes(m))
            .filter(m => !q || m.toLowerCase().includes(q))
            .slice(0, q ? 8 : maxPicks);
    }, [knownMuscles, value, input, maxPicks]);

    function add(raw: string) {
        const m = raw.trim().replace(/\s+/g, " ");
        if (!m) return;
        const canonical = knownMuscles.find(k => k.toLowerCase() === m.toLowerCase()) ?? m.charAt(0).toUpperCase() + m.slice(1);
        if (!value.includes(canonical)) onChange([...value, canonical]);
        setInput("");
    }

    return (
        <div className="exercise-form-field">
            <span id={id}>{label}</span>
            {hint && <small className="exercise-form-hint">{hint}</small>}
            <div className="exercise-form-muscles" aria-labelledby={id}>
                {value.map(m => (
                    <span key={m} className="exercise-form-muscle selected">
                        {m}
                        <button type="button" aria-label={`Remove ${m}`} onClick={() => onChange(value.filter(x => x !== m))}>
                            <X size={10} />
                        </button>
                    </span>
                ))}
                <input
                    className="exercise-form-muscle-input"
                    value={input}
                    placeholder={value.length ? "Add another…" : "Type or pick below…"}
                    onChange={e => setInput(e.target.value)}
                    onKeyDown={e => {
                        if (e.key === "Enter" || e.key === ",") { e.preventDefault(); add(input); }
                        if (e.key === "Backspace" && input === "" && value.length) onChange(value.slice(0, -1));
                    }}
                />
            </div>
            {suggestions.length > 0 && (
                <div className="exercise-form-suggestions">
                    {suggestions.map(m => (
                        <button key={m} type="button" className="exercise-library-chip" onClick={() => add(m)}>
                            <Plus size={10} /> {m}
                        </button>
                    ))}
                </div>
            )}
            {error && <small className="exercise-form-error">{error}</small>}
        </div>
    );
}

export default ExerciseForm;
