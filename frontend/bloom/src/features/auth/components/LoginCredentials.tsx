import { NavLink, useNavigate } from "react-router-dom";
import { useState } from "react";
import { login } from "../api.ts";
import { useAuth } from "../../../context/AuthContext.tsx";
import type { ApiError } from "../../../assets/js/data/apiClient.ts";

function LoginCredentials() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const { setToken } = useAuth();
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const token = await login(email, password);
            setToken(token);
            navigate("/dashboard");
        } catch (err) {
            const apiError = err as ApiError;
            setErrorMessage(apiError.error || "Login failed");
        }
    };

    return(
        <section id="login" className="loginsignup-form">
            <h1>Log in</h1>
            <p>Log into your account to start saving your progress and sync on other devices</p>
            <form onSubmit={handleLogin}>
                <div>
                    <label htmlFor="email"></label>
                    <input
                        type="email"
                        id="email"
                        required
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="Email">
                    </input>
                </div>
                <div>
                    <label htmlFor="password"></label>
                    <input
                        type="password"
                        id="password"
                        placeholder="Password"
                        required
                        onChange={(e) => setPassword(e.target.value)}>
                    </input>
                </div>
                <p className="error-message">{errorMessage}</p>
                <div id="login-checks">
                    <div>
                        <label htmlFor="remember">Remember me</label>
                        <input type="checkbox" id="remember"></input>
                    </div>
                    <a id="forgot-password">Forgot password?</a>
                </div>
                <div id="login-action">
                    <button type="submit">Log In</button>
                    <div>
                        <p>Dont have an account yet?</p>
                        <NavLink to="/signup">Sign up!</NavLink>
                    </div>
                </div>
            </form>
        </section>
    );
}

export default LoginCredentials;
