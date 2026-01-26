import './assets/App.css'
import { Routes, Route } from "react-router-dom";
import LogInPage from "./pages/LogInPage.tsx";
import HomePage from "./pages/HomePage.tsx";
import DashboardPage from "./pages/DashboardPage.tsx";
import TemplatePage from "./pages/TemplatePage.tsx";
import LogbookPage from "./pages/LogbookPage.tsx";

function App() {
  return (
    <>
        <Routes>
            <Route path="/" element={<HomePage/>} />
            <Route path="/login" element={<LogInPage/>} />
            <Route path="/dashboard" element={<DashboardPage/>} />
            <Route path="/templates" element={<TemplatePage/>} />
            <Route path="/logbook" element={<LogbookPage/>} />
        </Routes>
    </>
  )
}

export default App
