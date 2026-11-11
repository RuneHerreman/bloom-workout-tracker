// Route chunks and the data most pages need, loaded ahead of time.
//
// Without this, the first visit to a page waits for its JS chunk, then renders,
// then starts fetching — and because router navigations run in a transition,
// the old page (and the sidebar's active item) stays put until the chunk
// arrives. Warming both up after sign-in makes most navigations instant.

import { getMe } from "./features/auth/api.ts";
import { getLogs } from "./features/logbook/api.ts";
import { getTemplates } from "./features/templates/api.ts";
import { searchExercises } from "./features/exercises/api.ts";

export const routeLoaders = {
    "/dashboard":   () => import("./features/dashboard/DashboardPage.tsx"),
    "/logbook":     () => import("./features/logbook/LogbookPage.tsx"),
    "/templates":   () => import("./features/templates/TemplatePage.tsx"),
    "/insights":    () => import("./features/insights/InsightsPage.tsx"),
    "/leaderboard": () => import("./features/leaderboard/LeaderboardPage.tsx"),
    "/profile":     () => import("./features/profile/ProfilePage.tsx"),
    "/tools/macro-calculator": () => import("./features/tool-pages/macros/MacroPage.tsx"),
    "/tools/one-rep-max":      () => import("./features/tool-pages/1-RM/1RmPage.tsx"),
} as const;

export type RoutePath = keyof typeof routeLoaders;

const started = new Set<string>();

/** Starts downloading a page's chunk (idempotent; the browser caches the module). */
export function prefetchRoute(path: string): void {
    if (started.has(path) || !(path in routeLoaders)) return;
    started.add(path);
    routeLoaders[path as RoutePath]().catch(() => started.delete(path));
}

/** Data shared by most pages; each call fills that module's promise cache. */
export function prefetchCoreData(): void {
    const quiet = (p: Promise<unknown>) => { p.catch(() => {}); };
    quiet(getMe());
    quiet(getLogs());
    quiet(searchExercises());
    quiet(getTemplates());
}

/** Warm everything once the browser is idle so it never competes with the current page. */
export function prefetchAppInBackground(): void {
    const run = () => {
        prefetchCoreData();
        (Object.keys(routeLoaders) as RoutePath[]).forEach(prefetchRoute);
    };
    const idle = (window as Window & { requestIdleCallback?: (cb: () => void, opts?: { timeout: number }) => number }).requestIdleCallback;
    if (idle) idle(run, { timeout: 2000 });
    else setTimeout(run, 300);
}
