import "../../assets/css/loginSignup.css";
import { useNavigate } from "react-router-dom";
import SignupCredentials from "./components/SignupCredentials.tsx";

const SignUpPage = () => {
    const navigate = useNavigate();
    return(
        <main className="login-page">
            <section>
                <img onClick={() => navigate("/")} className="logo" src="/media/bloom_logo.png" alt="logo"/>
                <SignupCredentials/>
            </section>
            <div className="showcase-image"/>
        </main>
    );
}

export default SignUpPage;
