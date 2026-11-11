import Skeleton from "./Skeleton.tsx";

/** Placeholder for a log/template detail panel: title, meta line and two exercise cards. */
function DetailSkeleton() {
    return (
        <div className="detail-skeleton" role="status" aria-label="Loading">
            <Skeleton width="16rem" height="2rem" />
            <Skeleton width="7rem" height="0.9rem" />
            {[0, 1].map(i => (
                <div key={i} className="detail-skeleton-card">
                    <Skeleton width="10rem" height="1rem" />
                    <Skeleton width="6rem" height="0.75rem" />
                    <Skeleton height="6rem" radius="var(--radius-md)" style={{ marginTop: "0.5rem" }} />
                </div>
            ))}
        </div>
    );
}

export default DetailSkeleton;
