import {NavLink} from "react-router-dom";

function signUp(){
    // TODO
}

function SignupCredentials() {
    return(
        <section id="login">
            <h1>Sign up</h1>
            <p>Join thousands tracking strength and cardio gains. <br/> Create account in 30 seconds.</p>
            <form>
                <div>
                    <label htmlFor="email"></label>
                    <input type="email" id="email" placeholder="Email"></input>
                </div>
                <div>
                    <label htmlFor="password"></label>
                    <input type="password" id="password" placeholder="Password"></input>
                </div>
                <div>
                    <label htmlFor="password-check"></label>
                    <input type="password" id="password-check" placeholder="Repeat password"></input>
                </div>
                <div id="login-action">
                    <button type="submit" onClick={signUp}>Sign up</button>
                    <div>
                        <p>Already have an account?</p>
                        <NavLink to="/login">Log in</NavLink>
                    </div>
                </div>
            </form>
        </section>
    );
}

export default SignupCredentials;