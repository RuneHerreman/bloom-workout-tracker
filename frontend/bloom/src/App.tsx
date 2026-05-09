import { Routes, Route } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import HomePage from "./features/home/HomePage.tsx";
import LoginPage from "./features/auth/LoginPage.tsx";
import SignUpPage from "./features/auth/SignUpPage.tsx";
import DashboardPage from "./features/dashboard/DashboardPage.tsx";
import TemplatePage from "./features/templates/TemplatePage.tsx";
import LogbookPage from "./features/logbook/LogbookPage.tsx";

function App() {
    return (
        <AuthProvider>
            <Routes>
                <Route path="/" element={<HomePage/>} />
                <Route path="/login" element={<LoginPage/>} />
                <Route path="/signup" element={<SignUpPage/>} />
                <Route path="/dashboard" element={<ProtectedRoute><DashboardPage/></ProtectedRoute>} />
                <Route path="/templates" element={<ProtectedRoute><TemplatePage/></ProtectedRoute>} />
                <Route path="/logbook" element={<ProtectedRoute><LogbookPage/></ProtectedRoute>} />
            </Routes>
        </AuthProvider>
    );
}

export default App;
