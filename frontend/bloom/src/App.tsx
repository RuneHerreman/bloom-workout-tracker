import { lazy, Suspense } from "react";
import { Routes, Route, Outlet } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import AppLayout from "./components/AppLayout.tsx";
import HomePage from "./features/home/HomePage.tsx";
import LoginPage from "./features/auth/LoginPage.tsx";
import SignUpPage from "./features/auth/SignUpPage.tsx";

const DashboardPage  = lazy(() => import("./features/dashboard/DashboardPage.tsx"));
const TemplatePage   = lazy(() => import("./features/templates/TemplatePage.tsx"));
const LogbookPage    = lazy(() => import("./features/logbook/LogbookPage.tsx"));
const MacroPage      = lazy(() => import("./features/tool-pages/macros/MacroPage.tsx"));
const OneRepMaxPage  = lazy(() => import("./features/tool-pages/1-RM/1RmPage.tsx"));
const ProfilePage    = lazy(() => import("./features/profile/ProfilePage.tsx"));

function ProtectedLayout() {
    return (
        <ProtectedRoute>
            <AppLayout>
                <Suspense>
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
                </Route>
            </Routes>
        </AuthProvider>
    );
}

export default App;
