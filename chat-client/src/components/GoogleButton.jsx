import { GoogleLogin } from "@react-oauth/google";
import { useNavigate } from "react-router-dom";
import { googleLogin } from "../services/auth.service";

function GoogleButton({ onError }) {
    const navigate = useNavigate();

    const handleSuccess = async (credentialResponse) => {
        try {
            await googleLogin(credentialResponse.credential);
            navigate("/chat-list"); 
        } catch (err) {
            console.error(err);
            onError(err.message || "Google sign-in failed.");
        }
    };

    return (
        <GoogleLogin onSuccess={handleSuccess}
            onError={() => onError("Google sign-in was cancelled or failed.")}
            shape="pill" text="continue_with"
        />
    );
}

export default GoogleButton;