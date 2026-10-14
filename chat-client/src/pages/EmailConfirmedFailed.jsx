import { Link } from "react-router-dom";

function EmailConfirmedFailed(){
    return(
        <div className="login-page">
            <div className="login-container">
                <div className="login-header">
                    <h1>Verification failed</h1>

                    <p>This verification link is invalid or has expired.</p>
                </div>

                <Link to="/register"><button>Sign up again</button></Link>
            </div>
        </div>
    );
}

export default EmailConfirmedFailed;