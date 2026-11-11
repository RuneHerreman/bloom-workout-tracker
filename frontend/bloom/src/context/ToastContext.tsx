import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";

export interface ToastOptions {
    message: string;
    /** Label of the inline action, e.g. "Undo". */
    actionLabel?: string;
    onAction?: () => void | Promise<void>;
    /** Milliseconds before the toast closes on its own. */
    duration?: number;
}

interface ToastState extends ToastOptions {
    id: number;
}

interface ToastContextValue {
    showToast: (options: ToastOptions) => void;
}

const ToastContext = createContext<ToastContextValue>({ showToast: () => {} });

// eslint-disable-next-line react-refresh/only-export-components
export function useToast() {
    return useContext(ToastContext);
}

/**
 * One toast at a time, bottom-centre. Used for undoable actions: the change
 * happens immediately and the toast offers a short window to reverse it.
 */
export function ToastProvider({ children }: { children: ReactNode }) {
    const [toast, setToast] = useState<ToastState | null>(null);
    const [busy, setBusy] = useState(false);
    const timer = useRef<ReturnType<typeof setTimeout> | null>(null);
    const nextId = useRef(1);

    const dismiss = useCallback(() => {
        if (timer.current) clearTimeout(timer.current);
        setToast(null);
    }, []);

    const showToast = useCallback((options: ToastOptions) => {
        if (timer.current) clearTimeout(timer.current);
        const id = nextId.current++;
        setToast({ ...options, id });
        setBusy(false);
        timer.current = setTimeout(() => setToast(t => (t?.id === id ? null : t)), options.duration ?? 6000);
    }, []);

    useEffect(() => () => { if (timer.current) clearTimeout(timer.current); }, []);

    async function runAction() {
        if (!toast?.onAction || busy) return;
        setBusy(true);
        try {
            await toast.onAction();
        } finally {
            dismiss();
            setBusy(false);
        }
    }

    return (
        <ToastContext.Provider value={{ showToast }}>
            {children}
            {toast && createPortal(
                <div className="toast" role="status" aria-live="polite" key={toast.id}>
                    <span className="toast-message">{toast.message}</span>
                    {toast.actionLabel && (
                        <button type="button" className="toast-action" disabled={busy} onClick={runAction}>
                            {busy ? "…" : toast.actionLabel}
                        </button>
                    )}
                    <button type="button" className="toast-close" aria-label="Dismiss" onClick={dismiss}>
                        <X size={14} />
                    </button>
                    <span
                        className="toast-timer"
                        style={{ animationDuration: `${toast.duration ?? 6000}ms` }}
                        aria-hidden
                    />
                </div>,
                document.body
            )}
        </ToastContext.Provider>
    );
}
