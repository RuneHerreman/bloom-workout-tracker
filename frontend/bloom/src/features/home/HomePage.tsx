import '../../assets/css/homePage.css';
import NavbarComponent from "./components/NavbarComponent.tsx";
import HeaderComponent from "./components/HeaderComponent.tsx";
import MissionComponent from "./components/MissionComponent.tsx";
import ExerciseTypeImageComponent from "./components/ExerciseTypeImageComponent.tsx";
import FeaturesComponent from "./components/FeaturesComponent.tsx";
import FooterComponent from "./components/FooterComponent.tsx";
import TestimonialsComponent from "./components/TestimonialsComponent.tsx";

const HomePage = () => {
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
