import ButtonComponent from "../buttons/ButtonComponent.tsx";
import {NavLink} from "react-router-dom";

function DashboardPage() {
    return(
        <>
            <nav id="home-navbar">
                <img src="/media/bloom_logo.png" alt="Bloom"/>
                <div id="home-navbar-links">
                    <NavLink to="#our-mission">Our Mission</NavLink>
                    <NavLink to="#features">Features</NavLink>
                </div>
                <div id="home-navbar-buttons">
                    <ButtonComponent text={"Log In"} target={"login"} style="white"/>
                    <ButtonComponent text={"Sign Up"} target={"login"} style="green"/>
                </div>
            </nav>
        </>
    )
}

export default DashboardPage;