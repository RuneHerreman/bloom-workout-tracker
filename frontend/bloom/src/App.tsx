import { lazy, Suspense } from "react";
import { Routes, Route, Outlet, useLocation } from "react-router-dom";
import { routeLoaders } from "./routePrefetch.ts";
import { AuthProvider } from "./context/AuthContext.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import AppLayout from "./components/AppLayout.tsx";
import HomePage from "./features/home/HomePage.tsx";
import LoginPage from "./features/auth/LoginPage.tsx";
import SignUpPage from "./features/auth/SignUpPage.tsx";
import PageLoader from "./components/general/PageLoader.tsx";

const DashboardPage   = lazy(routeLoaders["/dashboard"]);
const TemplatePage    = lazy(routeLoaders["/templates"]);
const LogbookPage     = lazy(routeLoaders["/logbook"]);
const MacroPage       = lazy(routeLoaders["/tools/macro-calculator"]);
const OneRepMaxPage   = lazy(routeLoaders["/tools/one-rep-max"]);
const ProfilePage     = lazy(routeLoaders["/profile"]);
const InsightsPage    = lazy(routeLoaders["/insights"]);
const LeaderboardPage = lazy(routeLoaders["/leaderboard"]);

function ProtectedLayout() {
    const { pathname } = useLocation();
    return (
        <ProtectedRoute>
            <AppLayout>
                {/* Keyed per page: a new boundary shows its fallback right away instead of
                    the router transition holding the previous page (and nav highlight). */}
                <Suspense key={pathname} fallback={<PageLoader />}>
                    <Outlet />
                </Suspense>
            </AppLayout>
        </ProtectedRoute>
    );
}

function App() {
    return (
        <AuthProvider>
            <Routes>
                <Route path="/" element={<HomePage/>}/>
                <Route path="/login" element={<LoginPage/>}/>
                <Route path="/signup" element={<SignUpPage/>}/>
                <Route element={<ProtectedLayout />}>
                    <Route path="/dashboard" element={<DashboardPage/>}/>
                    <Route path="/templates" element={<TemplatePage/>}/>
                    <Route path="/logbook" element={<LogbookPage/>}/>
                    <Route path="/tools/one-rep-max" element={<OneRepMaxPage/>}/>
                    <Route path="/tools/macro-calculator" element={<MacroPage/>}/>
                    <Route path="/profile" element={<ProfilePage/>}/>
                    <Route path="/insights" element={<InsightsPage/>}/>
                    <Route path="/leaderboard" element={<LeaderboardPage/>}/>
                </Route>
            </Routes>
        </AuthProvider>
    );
}

export default App;
