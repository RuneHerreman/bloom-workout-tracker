import type { CSSProperties } from "react";

interface SkeletonProps {
    width?: CSSProperties["width"];
    height?: CSSProperties["height"];
    radius?: CSSProperties["borderRadius"];
    className?: string;
    style?: CSSProperties;
}

/** Shimmering placeholder block. Purely decorative — hidden from assistive tech. */
function Skeleton({ width = "100%", height = "0.875rem", radius, className, style }: SkeletonProps) {
    return (
        <span
            aria-hidden
            className={`skeleton${className ? ` ${className}` : ""}`}
            style={{ width, height, borderRadius: radius, ...style }}
        />
    );
}

export default Skeleton;
