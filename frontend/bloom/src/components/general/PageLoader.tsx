interface PageLoaderProps {
    label?: string;
    /** Fill the viewport (used before the app shell exists). */
    fullscreen?: boolean;
}

/**
 * Botanical loading state: a sprout whose stem and leaves draw themselves in a loop.
 * Shown while a lazy page chunk or its initial data is on the way.
 */
function PageLoader({ label = "Growing your data…", fullscreen }: PageLoaderProps) {
    return (
        <div className={`page-loader${fullscreen ? " fullscreen" : ""}`} role="status" aria-live="polite">
            <svg className="page-loader-sprout" viewBox="0 0 64 64" width="56" height="56" aria-hidden>
                <path className="sprout-soil" d="M14 54 Q32 49 50 54" />
                <path className="sprout-stem" d="M32 54 C32 44 31 36 32 26" />
                <path className="sprout-leaf sprout-leaf-left" d="M32 36 C24 36 17 31 16 23 C24 23 31 28 32 36 Z" />
                <path className="sprout-leaf sprout-leaf-right" d="M32 28 C33 19 40 13 49 13 C48 22 41 27 32 28 Z" />
            </svg>
            <p className="page-loader-label">{label}</p>
        </div>
    );
}

export default PageLoader;
