import "../assets/css/loginSignup.css";
import SignupCredentials from "../components/login-signup-components/SignupCredentials.tsx";
import {useNavigate} from "react-router-dom";

const SignUpPage = () => {
    const navigate = useNavigate();
    return(
        <main>
            <section>
                <img onClick={() => navigate("/")} className="logo" src="/media/bloom_logo.png" alt="logo"/>
                <SignupCredentials/>
            </section>
            <div className="showcase-image"/>
        </main>
    );
}

export default SignUpPage;