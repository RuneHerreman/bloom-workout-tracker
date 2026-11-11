import ButtonComponent from "../../../components/general/ButtonComponent.tsx";
import { useAuth } from "../../../context/AuthContext.tsx";

function NavbarComponent() {
    const { isAuthenticated, isLoading } = useAuth();
    return(
        <>
            <nav id="home-navbar">
                <img src="/media/bloom_logo.png" alt="Bloom"/>
                <div id="home-navbar-links">
                    <a href="#our-mission">Our Mission</a>
                    <a href="#features">Features</a>
                </div>
                <div id="home-navbar-buttons">
                    {isAuthenticated ? (
                        <ButtonComponent text="Open app" target="/dashboard" style="green"/>
                    ) : !isLoading && (
                        <>
                            <ButtonComponent text={"Log In"} target={"login"} style="white"/>
                            <ButtonComponent text={"Sign Up"} target={"signup"} style="green"/>
                        </>
                    )}
                </div>
            </nav>
        </>
    )
}

export default NavbarComponent;
