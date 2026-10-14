import { useState, useRef } from "react";
import { Link } from "react-router-dom";
import { register } from "../services/auth.service";
import "../App.css";
import GoogleButton from "../components/GoogleButton";

function Register() {
    const [newUser, setNewUser]  = useState({
        displayName: "",
        email: "",
        password: "",
        confirmPassword: ""
    });

    const [message, setMessage] = useState("");
    const [ isSubmitting, setIsSubmitting ] = useState(false);

    const handleChange = (event) => {
        const { name, value } = event.target;
        setNewUser((user) => ({
            ...user,
            [name]: value
        }));
    }

    const validateForm = () => {
        if (newUser.displayName.trim().length < 2){
            return "Display name must be at lease 2 characters.";
        }

        if (newUser.displayName.trim().length > 30) {
            return "Display name cannot exceed 30 characters.";
        }

        const failedCheck = passwordChecks.find(c => !c.passed);
        if (failedCheck) {
            return `Password requirement not met: ${failedCheck.label}.`;
        }

        if (newUser.password !== newUser.confirmPassword) {
            return "Passwords do not match.";
        }

        return null;
    }

    const handleSubmit = async (event) => {
        event.preventDefault();
        setMessage("");

        const validationError = validateForm();
        if (validationError){
            setMessage(validationError);
            return;
        }

        setIsSubmitting(true);
        try {
            const data = await register(
                newUser.displayName,
                newUser.email,
                newUser.password,
                newUser.confirmPassword
            );

            setMessage("Account created! A verification email has been sent.");
            setNewUser({
                displayName: "",
                email: "",
                password: "",
                confirmPassword: ""
            });

            setTimeout(() => {
                navigate("/login");
            }, 2000);
        } catch(err){
            console.error(err);
            setMessage(err.message || "An error occured. Try again!");

            if (err.status === 409)
                setMessage("An account with this email already exists.");

            else if (err.status === 400)
                setMessage(err.message || "Please check the information you entered.");

            else {
                setMessage("Something went wrong. Please try again.");
            }
        } finally{
            setIsSubmitting(false);
        }
    };

    const passwordChecks = [
        { label: "12+ characters", passed: newUser.password.length >= 12},
        { label: "Uppercase letter", passed: /[A-Z]/.test(newUser.password) },
        { label: "Lowercase letter", passed: /[a-z]/.test(newUser.password) },
        { label: "Number", passed: /[0-9]/.test(newUser.password) },
        { label: "Special character", passed: /[^A-Za-z0-9]/.test(newUser.password) }
    ];

    const strength = passwordChecks.filter(c => c.passed).length;
    const strengthColor = strength <= 2 ? "#e07856" : strength <= 4 ? "#e0c756" : "#7fdb8a";

    const bgRef = useRef(null);
    
        const handleMouseMove = (event) => {
            const { innerWidth, innerHeight } = window;
            const x = (event.clientX / innerWidth - 0.5) * 2;  // -1 (left) to 1 (right)
            const y = (event.clientY / innerHeight - 0.5) * 2;  // -1 (top) to 1 (bottom)
    
            if (bgRef.current) {
                bgRef.current.style.transform = `translate(${x * -50}px, ${y * -50}px) scale(1.05)`;
            }
        };

    return(
        <div className="login-page" onMouseMove={handleMouseMove}>
            <div className="cosmic-bg" ref={bgRef}></div>
            <div className="stars"></div>
            <div className="login-container">
                <div className="login-header">
                    <h1>ChatApp</h1>
                    <p>Register your account & starting chatting</p>
                </div>

                <div className="login-methods">
                    <div className="login-form">
                        <form onSubmit={handleSubmit}>
                            <div className="form-group floating">
                                <input type="text" name="displayName" value={newUser.displayName}
                                placeholder=" " onChange={handleChange} required />
                                <label>Display Name</label>
                            </div>

                            <div className="form-group floating">
                                <input type="email" name="email" value={newUser.email}
                                placeholder=" " onChange={handleChange} required />
                                <label>Email</label>
                            </div>

                            <div className="form-group floating" id="password-input">
                                <input type="password" name="password" value={newUser.password}
                                placeholder=" " onChange={handleChange} required />
                                <label>Password</label>
                            </div>

                            <div className="strength-meter">
                                <ul className="strength-checklist">
                                    {passwordChecks.map(check => (
                                        <li key={check.label} className={check.passed ? "met" : ""}>{check.label}</li>
                                    ))}
                                </ul>
                                    <div className="strength-bars">
                                        {[0, 1, 2, 3, 4].map(i => (
                                            <div key={i} className={`bar ${i < strength ? "filled" : ""}`}
                                                style={i < strength ? { backgroundColor: strengthColor } : undefined} />
                                        ))}
                                    </div>
                            </div>

                            <div className="form-group floating">
                                <input type="password" name="confirmPassword" value={newUser.confirmPassword}
                                placeholder=" " onChange={handleChange} required />
                                <label>Confirm Password</label>
                            </div>

                            <button type="submit" disabled={isSubmitting}>
                                {isSubmitting ? <span className="spinner" /> : "Create Account"}</button>
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
                    <p>Already have an account? <Link to="/login">Login</Link></p>
                </div>    
            </div>       
        </div>
    );
}

export default Register;