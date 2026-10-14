import { Link } from "react-router-dom";

function EmailConfirmed(){
    return(
        <div className="login-page">
            <div className="login-container">
                <div className="login-header">
                    <h1>Email Verified!</h1>

                    <p>Your ChatApp email address has been successfully verified.</p>
                </div>

                <Link to="/login"><button>Go to Login</button></Link>
            </div>
        </div>
    );
}

export default EmailConfirmed;