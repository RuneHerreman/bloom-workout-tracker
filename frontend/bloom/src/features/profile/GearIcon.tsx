import type { GearKind } from "./gearKinds.ts";

const PATHS: Record<GearKind, string> = {
    shoe: "M3 16.5V13l3.5-1 2.5-4 2 .5-.5 3 3 1.5 4.5 1A3 3 0 0 1 21 17v.5H4a1 1 0 0 1-1-1Z M3 17.5h18 M11.5 11.5l1.5-.5 M13 12.5l1.5-.5",
    bike: "M5.5 17.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z M18.5 17.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z M5.5 14l4-6h6l3 6 M9.5 8 12 14h-6.5 M15 5h2l-1.5 3",
    belt: "M2 9h20v6H2Z M15 8v8 M15 12h4 M19 10.5v3",
    wrap: "M7 3h10v18H7Z M7 8h10 M7 13h10 M10 3v18",
    watch: "M8 7h8v10H8Z M9 7l1-4h4l1 4 M9 17l1 4h4l1-4 M12 10v2.5l1.5 1",
    swim: "M2 18c1.5 0 2-1 3.5-1s2 1 3.5 1 2-1 3.5-1 2 1 3.5 1 2-1 3.5-1 2 1 2 1 M8 13l3-5 4 3 M16 7a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z",
    weights: "M2 12h20 M5 8v8 M8 6v12 M16 6v12 M19 8v8",
    racket: "M15 3a6 6 0 1 1 0 .01Z M10.8 13.2 3 21 M11.5 5.5l7 7 M9.5 9.5l5-5",
    other: "M6 8h12l-1 12H7L6 8Z M9 8V6a3 3 0 0 1 6 0v2",
};

function GearIcon({ kind, size = 16 }: { kind: GearKind; size?: number }) {
    return (
        <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden>
            <path d={PATHS[kind]} />
        </svg>
    );
}

export default GearIcon;
