import '../../assets/css/homePage.css';
import { Navigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext.tsx";
import NavbarComponent from "./components/NavbarComponent.tsx";
import HeaderComponent from "./components/HeaderComponent.tsx";
import MissionComponent from "./components/MissionComponent.tsx";
import ExerciseTypeImageComponent from "./components/ExerciseTypeImageComponent.tsx";
import FeaturesComponent from "./components/FeaturesComponent.tsx";
import FooterComponent from "./components/FooterComponent.tsx";
import TestimonialsComponent from "./components/TestimonialsComponent.tsx";

const HomePage = () => {
    const { isAuthenticated, isLoading } = useAuth();

    // Signed in: skip the landing page and go straight to the app.
    if (isLoading) return null;
    if (isAuthenticated) return <Navigate to="/dashboard" replace />;

    return (
        <>
            <NavbarComponent/>
            <HeaderComponent/>
            <MissionComponent/>
            <ExerciseTypeImageComponent/>
            <FeaturesComponent/>
            <TestimonialsComponent/>
            <FooterComponent/>
        </>
    );
}

export default HomePage;
