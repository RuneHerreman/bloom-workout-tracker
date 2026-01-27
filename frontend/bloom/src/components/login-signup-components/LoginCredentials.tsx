import {NavLink} from "react-router-dom";

function logIn(){
    // TODO
}

function LoginCredentials() {
    return(
        <section id="login">
            <h1>Log in</h1>
            <p>Log into your account to start saving your progress and sync on other devices</p>
            <form>
                <div>
                    <label htmlFor="email"></label>
                    <input type="email" id="email" placeholder="Email"></input>
                </div>
                <div>
                    <label htmlFor="password"></label>
                    <input type="password" id="password" placeholder="Password"></input>
                </div>
                <div id="login-checks">
                    <div>
                        <label htmlFor="remember">Remember me</label>
                        <input type="checkbox" id="remember"></input>
                    </div>
                    <a id="forgot-password">Forgot password?</a>
                </div>
                <div id="login-action">
                    <button type="submit" onClick={logIn}>Log In</button>
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