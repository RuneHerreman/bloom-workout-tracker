import ButtonComponent from "../general/ButtonComponent.tsx";

function DashboardPage() {
    return(
        <>
            <nav id="home-navbar">
                <img src="/media/bloom_logo.png" alt="Bloom"/>
                <div id="home-navbar-links">
                    <a href="#our-mission">Our Mission</a>
                    <a href="#features">Features</a>
                </div>
                <div id="home-navbar-buttons">
                    <ButtonComponent text={"Log In"} target={"login"} style="white"/>
                    <ButtonComponent text={"Sign Up"} target={"signup"} style="green"/>
                </div>
            </nav>
        </>
    )
}

export default DashboardPage;