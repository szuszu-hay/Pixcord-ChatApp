import { useState } from "react";
import { SERVER_ORIGIN } from "../services/api";

function Avatar({ url, name, className = "" }) {
    const [failedUrl, setFailedUrl] = useState(null);

    const initials =
        name?.split(" ").map((w) => w[0]).join("").slice(0, 2).toUpperCase() || "?";

    if (url && url !== failedUrl) {
        return (
            <img src={`${SERVER_ORIGIN}${url}`} alt=""
                className={`user-avatar ${className}`}
                onError={() => setFailedUrl(url)}
            />
        );
    }

    return <div className={`user-avatar avatar-fallback ${className}`}>{initials}</div>;
}

export default Avatar;