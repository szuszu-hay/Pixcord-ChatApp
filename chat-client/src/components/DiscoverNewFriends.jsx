import { useState } from "react";
import { searchUsers } from "../services/user.service";
import { sendRequest } from "../services/friendRequest.service";
import FriendRequests from "./FriendRequests";

function DiscoverNewFriends({ onFriendAccepted }) {
    const [query, setQuery] = useState("");
    const [users, setUsers] = useState([]);
    const [message, setMessage] = useState("");
    const [sendingIds, setSendingIds] = useState(new Set());
    const [sentIds, setSentIds] = useState(new Set());

    const handleSearch = async (event) => {
        const value = event.target.value;
        setQuery(value);
        setMessage("");

        if (value.trim().length < 2){
            setUsers([]);
            return;
        }

        try{
            const data = await searchUsers(value);
            setUsers(data);

            if (data.length === 0) {
                setMessage("No users found.");
            }
        } catch(err){
            console.error(err);
            setMessage("Failed to search users.");
        }
    };

    const handleAddFriend = async (userId) => {
        setSendingIds((prev) => new Set(prev).add(userId));

        try {
            await sendRequest(userId);
            setSentIds((prev) => new Set(prev).add(userId));
        } catch (err){
            console.error(err);
            setMessage(err.message || "Failed to send friend request.");
        } finally {
            setSendingIds((prev) => {
                const next = new Set(prev);
                next.delete(userId);
                return next;
            })
        }
    };

    return (
        <div className="search-page">
            <FriendRequests onFriendAccepted={onFriendAccepted} />
            
            <div className="panel-header discover-accent">
                <div className="panel-icon">🔭</div>
                <div>
                    <h2>Discover New Friends</h2>
                    <p>Search by name or username</p>
                </div>
            </div>          
            <div className="search-bar">
                <svg className="search-icon" viewBox="0 0 20 20" fill="none">
                    <circle cx="9" cy="9" r="6.5" stroke="currentColor" strokeWidth="1.5" />
                    <path d="M18 18L14 14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                </svg>
                <input type="text" value={query} onChange={handleSearch} placeholder="★ᯓ Jump into your search bar ᯓ★" />
            </div>

            <div className="user-results">
                {users.map((user) => {
                    const isSending = sendingIds.has(user.id);
                    const isSent = sentIds.has(user.id);
                    const initials = user.displayName
                        ?.split(" ")
                        .map((w) => w[0])
                        .join("")
                        .slice(0, 2)
                        .toUpperCase();
                        
                    return (
                        <div className="user-card" key={user.id}>
                            {user.profilePictureUrl ? (
                                <img src={user.profilePictureUrl} alt="" className="user-avatar"/>
                            ) : (
                                <div className="user-avatar avatar-fallback">{initials}</div>
                            )}
                            <div className="user-info">
                                <p className="user-name">{user.displayName}</p>
                                <p className="user-handle">@{user.username}</p>
                                {user.bio && <p className="user-bio">{user.bio}</p>}
                            </div>
    
                            <button className={`add-friend-btn ${isSent ? "sent" : ""}`} 
                                onClick={() => handleAddFriend(user.id)} 
                                disabled={isSending || isSent}>
                                {isSent ? "Request Sent" : isSending ? <span className="spinner" /> : "Add Friend"}
                            </button>
                        </div>
                    );
                })}
            </div>
    
            {message && (<p className="msg">{message}</p>)}

        </div>
    );
}

export default DiscoverNewFriends;