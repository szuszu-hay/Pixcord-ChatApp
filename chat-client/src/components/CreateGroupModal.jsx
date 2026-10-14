import { useState, useEffect } from "react";
import { getFriends } from "../services/friendRequest.service"; 
import { createGroupConversation } from "../services/conversation.service";
import { useNavigate } from "react-router-dom";
import Profile from "./Profile";

function CreateGroupModal({ onClose }) {
    const navigate = useNavigate();
    const [friends, setFriends] = useState([]);
    const [selectedIds, setSelectedIds] = useState(new Set());
    const [groupName, setGroupName] = useState(""); 
    const [isCreating, setIsCreating] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchFriends = async () => {
            try {
                const data = await getFriends();
                setFriends(data);
            } catch (err) {
                console.error(err);
                setError("Failed to load friends.");
            }
        };

        fetchFriends();
    }, []);

    const toggleFriend = (friendId) => {
        setSelectedIds((prev) => {
            const newSet = new Set(prev);
            if (newSet.has(friendId)) {
                newSet.delete(friendId);
            } else {
                newSet.add(friendId);
            }
            return newSet;
        })
    };

    const handleCreateGroup = async (event) => {
        event.preventDefault();
        setError(null);

        if (!groupName.trim()) {
            setError("Give your group a name.");
            return;
        }
        if (selectedIds.size < 2) {
            setError("Select at least 2 friends to start a group.");
            return;
        }

        setIsCreating(true);
        try {
            const data = await createGroupConversation(groupName.trim(), Array.from(selectedIds));
            navigate(`/chat/${data.conversationId}`);
            onClose();
        } catch (err) {
            console.error(err);
            setError(err.message || "Failed to create group.");
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                    <h2>New Group</h2>
                    <button className="modal-close-btn btn-reset" onClick={onClose}>✕</button>
                </div>

                <form onSubmit={handleCreateGroup}>
                    <input
                        type="text"
                        value={groupName}
                        onChange={(e) => setGroupName(e.target.value)}
                        placeholder="Group name"
                        className="group-name-input"
                    />

                    <p className="modal-subtitle">Select friends ({selectedIds.size} selected)</p>

                    <div className="friend-select-list">
                        {friends.map((f) => (
                            <label key={f.id} className="friend-select-row">
                                <input type="checkbox" checked={selectedIds.has(f.id)} onChange={() => toggleFriend(f.id)} />
                                {f.profilePictureUrl ? (
                                    <Profile url={f.profilePictureUrl} name={f.displayName} className="user-avatar" />
                                ) : (
                                    <div className="user-avatar avatar-fallback">
                                        {f.displayName?.[0]?.toUpperCase()}
                                    </div>
                                )}
                                <span>{f.displayName}</span>
                            </label>
                        ))}
                    </div>

                    {error && <p className="msg">{error}</p>}

                    <button type="submit" className="create-group-btn" disabled={isCreating}>
                        {isCreating ? <span className="spinner" /> : "Create Group"}
                    </button>
                </form>
            </div>
        </div>
    );
}

export default CreateGroupModal;