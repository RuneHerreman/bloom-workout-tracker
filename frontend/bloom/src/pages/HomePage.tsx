import '../assets/homePage.css'
import NavbarComponent from "../components/home/NavbarComponent.tsx";
import HeaderComponent from "../components/home/HeaderComponent.tsx";
import MissionComponent from "../components/home/MissionComponent.tsx";
import ExerciseTypeImageComponent from "../components/home/ExerciseTypeImageComponent.tsx";

const HomePage = () => {
    return (
        <>
            <NavbarComponent/>
            <HeaderComponent/>
            <MissionComponent/>
            <ExerciseTypeImageComponent/>
        </>
    );
}

export default HomePage;