import { lazy, Suspense } from "react";
import { Routes, Route } from "react-router-dom";
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

function App() {
    return (
        <AuthProvider>
            <Routes>
                <Route path="/" element={<HomePage/>}/>
                <Route path="/login" element={<LoginPage/>}/>
                <Route path="/signup" element={<SignUpPage/>}/>
                <Route path="/dashboard" element={
                    <ProtectedRoute><AppLayout><Suspense><DashboardPage/></Suspense></AppLayout></ProtectedRoute>
                }/>
                <Route path="/templates" element={
                    <ProtectedRoute><AppLayout><Suspense><TemplatePage/></Suspense></AppLayout></ProtectedRoute>
                }/>
                <Route path="/logbook" element={
                    <ProtectedRoute><AppLayout><Suspense><LogbookPage/></Suspense></AppLayout></ProtectedRoute>
                }/>
                <Route path="/tools/one-rep-max" element={
                    <ProtectedRoute><AppLayout><Suspense><OneRepMaxPage/></Suspense></AppLayout></ProtectedRoute>
                }/>
                <Route path="/tools/macro-calculator" element={
                    <ProtectedRoute><AppLayout><Suspense><MacroPage/></Suspense></AppLayout></ProtectedRoute>
                }/>
            </Routes>
        </AuthProvider>
    );
}

export default App;
