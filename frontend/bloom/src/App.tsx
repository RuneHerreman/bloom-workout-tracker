import { Routes, Route } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import AppLayout from "./components/AppLayout.tsx";
import HomePage from "./features/home/HomePage.tsx";
import LoginPage from "./features/auth/LoginPage.tsx";
import SignUpPage from "./features/auth/SignUpPage.tsx";
import DashboardPage from "./features/dashboard/DashboardPage.tsx";
import TemplatePage from "./features/templates/TemplatePage.tsx";
import LogbookPage from "./features/logbook/LogbookPage.tsx";
import MacroPage from "./features/tool-pages/macros/MacroPage.tsx";
import OneRepMaxPage from "./features/tool-pages/1-RM/1RmPage.tsx";

function App() {
    return (
        <AuthProvider>
            <Routes>
                <Route path="/" element={<HomePage/>}/>
                <Route path="/login" element={<LoginPage/>}/>
                <Route path="/signup" element={<SignUpPage/>}/>
                <Route path="/dashboard" element={
                    <ProtectedRoute><AppLayout><DashboardPage/></AppLayout></ProtectedRoute>
                }/>
                <Route path="/templates" element={
                    <ProtectedRoute><AppLayout><TemplatePage/></AppLayout></ProtectedRoute>
                }/>
                <Route path="/logbook" element={
                    <ProtectedRoute><AppLayout><LogbookPage/></AppLayout></ProtectedRoute>
                }/>
                <Route path="/tools/one-rep-max" element={
                    <ProtectedRoute><AppLayout><OneRepMaxPage/></AppLayout></ProtectedRoute>
                }/>
                <Route path="/tools/macro-calculator" element={
                    <ProtectedRoute><AppLayout><MacroPage/></AppLayout></ProtectedRoute>
                }/>
            </Routes>
        </AuthProvider>
    );
}

export default App;
