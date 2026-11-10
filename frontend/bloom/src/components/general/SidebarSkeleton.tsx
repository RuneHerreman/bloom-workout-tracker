import Skeleton from "./Skeleton.tsx";

/** Placeholder cards for the feature sidebars (logbook, templates) while data loads. */
function SidebarSkeleton({ count = 6, withHeader = true }: { count?: number; withHeader?: boolean }) {
    return (
        <div className="sidebar-skeleton" role="status" aria-label="Loading">
            {withHeader && <Skeleton width="5.5rem" height="0.6rem" style={{ marginTop: "0.35rem" }} />}
            {Array.from({ length: count }, (_, i) => (
                <div key={i} className="sidebar-skeleton-card" style={{ animationDelay: `${i * 60}ms` }}>
                    <Skeleton width={`${55 + ((i * 17) % 35)}%`} height="0.85rem" />
                    <Skeleton width="70%" height="0.65rem" />
                </div>
            ))}
        </div>
    );
}

export default SidebarSkeleton;
