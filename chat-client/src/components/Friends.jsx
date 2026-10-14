import { getFriends } from "../services/friendRequest.service";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

function Friends(){
    const navigate = useNavigate();
    const [friends, setFriends] = useState([]);
    const [message, setMessage] = useState("");

    useEffect(() => {
        loadFriends();
    }, []);

    const loadFriends = async () => {
        setMessage("");

        try {
            const data = await getFriends();
            setFriends(data);

            if (data.length === 0){
                setMessage("Add friends to chat with.");
            }
        } catch(err) {
            console.error(err);
            setMessage("Failed to load friends");
        }
    };

    const handleClick = () => {
        navigate("/friend-list");
    };

    return (
        <div>
            {friends.map((f) => (
                <div key={f.friendId}>
                    <img src={f.friendProfilePictureUrl} alt={f.friendUserName} width="40" height="40" />
                    <div>
                        <p>{f.friendDisplayName}</p>
                        <p>{f.friendUsername}</p>
                    </div>
                </div>

            ))}
            <button onClick={handleClick}>Discover more friends</button>
            {message && (<p className="msg">{message}</p>)}
        </div>
    );
}

export default Friends;