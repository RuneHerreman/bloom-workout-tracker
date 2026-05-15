import "../../assets/css/loginSignup.css";
import { useNavigate } from "react-router-dom";
import LoginCredentials from "./components/LoginCredentials.tsx";

const LoginPage = () => {
    const navigate = useNavigate();
    return(
        <main className="login-page">
            <section>
                <img onClick={() => navigate("/")} className="logo" src="/media/bloom_logo.png" alt="logo"/>
                <LoginCredentials/>
            </section>
            <div className="showcase-image"/>
        </main>
    );
}

export default LoginPage;
