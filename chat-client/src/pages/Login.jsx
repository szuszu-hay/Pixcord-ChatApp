import { useState, useRef } from "react";
import { login } from "../services/auth.service";
import { Link, useNavigate } from "react-router-dom";
import "../App.css";
import "../index.css";
import GoogleButton from "../components/GoogleButton";

function Login() {
    const navigate = useNavigate();

    const [user, setUser] = useState({
        email: "",
        password: ""
    });

    const [message, setMessage] = useState("");

    const handleSubmit = async (event) => {
        event.preventDefault();
        setMessage("");

        try {
            const data = await login(user.email, user.password);
            setMessage("Login successful...");

            setTimeout(() => {
                navigate("/chat-list");
            }, 500);
        } catch (err){
            console.error(err);
            
            if (err.status === 401){
                setMessage("Invalid email or password.");
            }
            else if (err.status === 403){
                setMessage("Please verify your email before logging in.");
            }
            else if (err.status === 423) {
                setMessage("Your account is temporarily locked. Please try again later.");
            }
            else {
                setMessage("Something went wrong. Please try again.");
            }
        }
    };
    
    const handleChange = (event) => {
        const {name, value} = event.target;
        setUser((prevUser) => ({
            ...prevUser,
            [name]: value
        }));
    }

    const bgRef = useRef(null);

    const handleMouseMove = (event) => {
        const { innerWidth, innerHeight } = window;
        const x = (event.clientX / innerWidth - 0.5) * 2;  // -1 (left) to 1 (right)
        const y = (event.clientY / innerHeight - 0.5) * 2;  // -1 (top) to 1 (bottom)

        if (bgRef.current) {
            bgRef.current.style.transform = `translate(${x * -50}px, ${y * -50}px) scale(1.05)`;
        }
    };

    return (
        <div className="login-page" onMouseMove={handleMouseMove}>
            <div className="cosmic-bg" ref={bgRef}></div>
            <div className="stars"></div>
            <div className="login-container">
                <div className = "login-header">
                    <h1>Pixcord</h1>
                    <p>Connect with friends and family anytime, anywhere</p>
                </div>

                <div className="login-methods">
                    <div className="login-form">
                        <form onSubmit={handleSubmit}>
                            <div className="form-group floating">
                                <input type="email" value={user.email} onChange={handleChange}
                                name="email" placeholder=" " required />
                                <label>Email</label>
                            </div>

                            <div className="form-group floating">
                                <input type="password" value={user.password} onChange={handleChange}
                                name="password" placeholder=" " required />
                                <label>Password</label>
                            </div>

                            <div className="btn-container">
                                <button type="submit">Log in</button>
                            </div>
                        </form>
                    </div>

                    <div className="google-signin">
                        {/* Google sign in */}
                        <h3>- or -</h3>
                        <GoogleButton onError={setMessage} />
                    </div>
                </div>

                <div className="msg">{message}</div>
                <div className="login-footer">
                    <p>Don't have an account? <Link to="/register">Register</Link></p>
                </div>
            </div>
        </div>
    );
}

export default Login;