import { useState } from "react";
import { searchFriends } from "../services/friendRequest.service";
import { CreateDirectConversation } from "../services/conversation.service";
import { useNavigate } from "react-router-dom";
import Profile from "./Profile";

function MyFriends({ friends, message, setFriends, setMessage, loadFriends }){
    const navigate = useNavigate();
    const [query, setQuery] = useState("");
    const [opendingId, setOpeningId] = useState(null);
    
    const handleSearch = async (event) => {
        const value = event.target.value;
        setQuery(value);
        setMessage("");

        if (value.trim().length < 2){
            loadFriends();
            return;
        }

        try {
            const data = await searchFriends(value);
            setFriends(data);
            if (data.length === 0) {
                setMessage("No users found.");
            } 
        } catch(err) {
            console.error(err);
            setMessage("Failed to search friends.");
        }
    };

    const handleClick = async (friend) => {
        setOpeningId(friend.id);
        setMessage("");
        
        try {
            const conversation = await CreateDirectConversation(friend.id);
            navigate(`/chat/${conversation.conversationId}`, {
                state : { friendDisplayName: friend.displayName }
            });
        } catch(err) {
            console.error(err);
            setMessage(err.message || "Failed to open conversation.");
        } finally {
            setOpeningId(null);
        }
    };

    return (
        <div className="search-page">
            <div className="panel-header friends-accent">
                <div className="panel-icon">👥</div>
                <div>
                    <h2>My Friends</h2>
                    <p>{friends.length > 0 ? `${friends.length} in your circle` : "Your circle is empty"}</p>
                </div>
            </div>
            
            <div className="search-bar">
                <svg className="search-icon" viewBox="0 0 20 20" fill="none">
                    <circle cx="9" cy="9" r="6.5" stroke="currentColor" strokeWidth="1.5" />
                    <path d="M18 18L14 14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                </svg>
                <input type="text" value={query} onChange={handleSearch} placeholder="Search by name or username..." />
            </div>

            {friends.length === 0 && !message ? (
                <div className="empty-requests">
                    <div className="sleepy-cat">🌱</div>
                    <p>No friends yet</p>
                    <span>Search to find people you know</span>
                </div>
            ) : (
                <div className="user-results">
                    {friends.map((f) => {
                        const initials = f.displayName
                            ?.split(" ")
                            .map((w) => w[0])
                            .join("")
                            .slice(0, 2)
                            .toUpperCase();

                    return (
                            <div className="user-card" key={f.id} onClick={() => handleClick(f)}>
                                {f.profilePictureUrl ? (
                                    <Profile url={f.profilePictureUrl} name={f.displayName} className="user-avatar" />
                                ) : (
                                    <div className="user-avatar avatar-fallback">{initials}</div>
                                )}
                                <div className="user-info">
                                    <p className="user-name">{f.displayName}</p>
                                    <p className="user-handle">@{f.username}</p>
                                    {f.bio && <p className="user-bio">{f.bio}</p>}
                                </div>

                            </div>
                        );
                    })}
                </div>
            )}
            {message && (<p className="msg">{message}</p>)}
        </div>
    );
}

export default MyFriends;