import { NavLink, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { register } from "../api.ts";
import { useAuth } from "../../../context/AuthContext.tsx";
import type { ApiError } from "../../../assets/js/data/apiClient.ts";

function SignupCredentials() {
    const [step, setStep] = useState(1);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [passwordAgain, setPasswordAgain] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [username, setUsername] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [height, setHeight] = useState(0);
    const [weight, setWeight] = useState(0);
    const [activeDays, setActiveDays] = useState(0);
    const { setToken } = useAuth();
    const navigate = useNavigate();

    const checkPasswordMatch = () => {
        if (password !== passwordAgain) {
            setErrorMessage("Passwords do not match");
            return false;
        }
        setErrorMessage("");
        return true;
    };

    useEffect(() => {
        const timeoutId = setTimeout(() => {
            checkPasswordMatch();
        }, 300);
        return () => clearTimeout(timeoutId);
    }, [password, passwordAgain]);

    const handleSignUp = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!checkPasswordMatch()) return;
        try {
            const token = await register(email, password, height, weight, username, firstName, lastName, activeDays);
            setToken(token);
            navigate("/dashboard");
        } catch (error) {
            const apiError = error as ApiError;
            setErrorMessage(apiError.error || "Registration failed");
        }
    };

    const handleNextStep = (e: React.FormEvent) => {
        e.preventDefault();
        if (checkPasswordMatch()) {
            setStep(2);
        }
    };

    return(
        <>
            {step === 1 && (
                <section id="login" className="loginsignup-form">
                    <h1>Sign up</h1>
                    <p>Join thousands tracking strength and cardio gains. <br/> Create account in 30 seconds.</p>
                    <form onSubmit={handleNextStep}>
                        <div>
                            <label htmlFor="email"></label>
                            <input
                                type="email"
                                id="email"
                                placeholder="Email"
                                value={email}
                                required
                                onChange={(e) => setEmail(e.target.value)}>
                            </input>
                        </div>
                        <div>
                            <label htmlFor="password"></label>
                            <input
                                type="password"
                                id="password"
                                placeholder="Password"
                                value={password}
                                required
                                onChange={(e) => setPassword(e.target.value)}>
                            </input>
                        </div>
                        <div>
                            <label htmlFor="password-check"></label>
                            <input
                                type="password"
                                id="password-check"
                                placeholder="Repeat password"
                                value={passwordAgain}
                                required
                                onChange={(e) => setPasswordAgain(e.target.value)}>
                            </input>
                        </div>
                        {errorMessage && (
                            <p className="error-message">{errorMessage}</p>
                        )}
                        <div id="login-action">
                            <button type="submit">Sign up</button>
                            <div>
                                <p>Already have an account?</p>
                                <NavLink to="/login">Log in</NavLink>
                            </div>
                        </div>
                    </form>
                </section>
            )}

            {step === 2 && (
                <section id="onboarding" className="loginsignup-form">
                    <h1>Tell us about yourself</h1>
                    <p>Help us personalize your Bloom experience. See personalised graphs showing your performance</p>
                    <form onSubmit={handleSignUp}>
                        <div>
                            <div>
                                <label htmlFor="firstName">First name</label>
                                <input
                                    type="text"
                                    id="firstName"
                                    placeholder="First name"
                                    maxLength={100}
                                    required
                                    onChange={(e) => setFirstName(e.target.value)}>
                                </input>
                            </div>
                            <div>
                                <label htmlFor="lastName">Last name</label>
                                <input
                                    type="text"
                                    id="lastName"
                                    placeholder="Last name"
                                    maxLength={100}
                                    required
                                    onChange={(e) => setLastName(e.target.value)}>
                                </input>
                            </div>
                        </div>
                        <div>
                            <label htmlFor="username">What do you want to be called?</label>
                            <input
                                type="text"
                                id="username"
                                placeholder="Username"
                                maxLength={50}
                                required
                                onChange={(e) => setUsername(e.target.value)}>
                            </input>
                        </div>
                        <div>
                            <div>
                                <label htmlFor="weight">Your weight? (kg)</label>
                                <input
                                    type="number"
                                    id="weight"
                                    placeholder="Weight (kg)"
                                    min="0"
                                    max="500"
                                    step="0.1"
                                    required
                                    onChange={(e) => setWeight(Number(e.target.value))}>
                                </input>
                            </div>
                            <div>
                                <label htmlFor="height">Your height? (cm)</label>
                                <input
                                    type="number"
                                    id="height"
                                    placeholder="Height (cm)"
                                    min="0"
                                    max="300"
                                    required
                                    onChange={(e) => setHeight(Number(e.target.value))}>
                                </input>
                            </div>
                        </div>
                        <div>
                            <label htmlFor="activeDays">How many days per week do you want to exercise?</label>
                            <input
                                type="number"
                                id="activeDays"
                                placeholder="Days per week"
                                min="0"
                                max="7"
                                required
                                onChange={(e) => setActiveDays(Number(e.target.value))}>
                            </input>
                        </div>
                        <p className="error-message">{errorMessage}</p>
                        <div id="login-action">
                            <button type="submit">Continue</button>
                        </div>
                    </form>
                </section>
            )}
        </>
    );
}

export default SignupCredentials;
