import '../assets/css/homePage.css'
import NavbarComponent from "../components/home/NavbarComponent.tsx";
import HeaderComponent from "../components/home/HeaderComponent.tsx";
import MissionComponent from "../components/home/MissionComponent.tsx";
import ExerciseTypeImageComponent from "../components/home/ExerciseTypeImageComponent.tsx";
import FeaturesComponent from "../components/home/FeaturesComponent.tsx";
import FooterComponent from "../components/home/FooterComponent.tsx";

const HomePage = () => {
    return (
        <>
            <NavbarComponent/>
            <HeaderComponent/>
            <MissionComponent/>
            <ExerciseTypeImageComponent/>
            <FeaturesComponent/>
            <FooterComponent/>
        </>
    );
}

export default HomePage;