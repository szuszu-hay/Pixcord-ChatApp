import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { getConversationDetails, addMember, removeMember, leaveConversation, updateGroupDetails } from "../services/conversation.service";
import { getFriends, unfriend } from "../services/friendRequest.service";
import { SERVER_ORIGIN } from "../services/api";
import Profile from "./Profile";
import ConfirmationModal from "./ConfirmationModal";

function ChatSettings({ conversationId, conversationType, onClose, onMembersChanged, onLeft, onFriendRemoved }) {
    const navigate = useNavigate();
    const currentUserId = localStorage.getItem("userId");

    const [details, setDetails] = useState(null);
    const [error, setError] = useState("");
    
    const [isEditingName, setIsEditingName] = useState(false);
    const [editName, setEditName] = useState("");
    const [isSavingName, setIsSavingName] = useState(false);
    
    const [showAddMember, setShowAddMember] = useState(false);
    const [friends, setFriends] = useState([]);
    const [addingId, setAddingId] = useState(null);

    const [memberToRemove, setMemberToRemove] = useState(null);
    const [showLeaveConfirm, setShowLeaveConfirm] = useState(false);
    const [isConfirming, setIsConfirming] = useState(false);

    const [isUploadingIcon, setIsUploadingIcon] = useState(false);
    const groupIconInputRef = useRef(null);

    const [showUnfriendConfirm, setShowUnfriendConfirm] = useState(false);

    const canEditGroup = conversationType === "Group";

    const startEditingName = () => {
        if (!canEditGroup) return;
        setEditName(details.name || "");
        setIsEditingName(true);
    };

    const handleSaveName = async (event) => {
        event.preventDefault();
        const trimmed = editName.trim();
        if (!trimmed || trimmed === details.name) {
            setIsEditingName(false);
            return;
        }

        setIsSavingName(true);
        setError("");

        try {
            const updated = await updateGroupDetails(conversationId, trimmed, null);
            setDetails((prev) => ({ ...prev, name: updated.name }));
            setIsEditingName(false);
        } catch (err) {
            console.error(err);
            setError(err.message || "Failed to update name.");
        } finally {
            setIsSavingName(false);
        }
    };

    const handleIconSelect = async (event) => {
        const file = event.target.files[0];
        if (!file) return;

        setIsUploadingIcon(true);
        setError("");

        try {
            const updated = await updateGroupDetails(conversationId, null, file);
            setDetails((prev) => ({ ...prev, iconUrl: updated.iconUrl }));
        } catch (err) {
            console.error(err);
            setError(err.message || "Failed to update icon.");
        } finally {
            setIsUploadingIcon(false);
            if (groupIconInputRef.current) groupIconInputRef.current.value = "";
        }
    };

    useEffect(() => {
        const loadDetails = async () => {
            try {
                const data = await getConversationDetails(conversationId);
                setDetails(data);
            } catch (err) {
                console.error(err);
                setError("Failed to load details.");
            }
        };
        loadDetails();
    }, [conversationId]);

    const openAddMember = async () => {
        setShowAddMember(true);
        setError("");
        try {
            const data = await getFriends();
            setFriends(data);
        } catch (err) {
            console.error(err);
            setError("Failed to load friends.");
        }
    };

    const handleAddMember = async (friend) => {
        setAddingId(friend.id);
        setError("");
        try {
            const newMember = await addMember(conversationId, friend.id);
            setDetails((prev) => ({ ...prev, members: [...prev.members, newMember] }));
            onMembersChanged();
        } catch (err) {
            console.error(err);
            setError(err.message || "Failed to add member.");
        } finally {
            setAddingId(null);
        }
    };

    const handleKick = async (targetUserId) => {
        try {
            await removeMember(conversationId, targetUserId);
            setDetails((prev) => ({
                ...prev,
                members: prev.members.filter((m) => m.userId !== targetUserId)
            }));
            onMembersChanged();
        } catch (err) {
            console.error(err);
            setError("Failed to remove member.");
        }
    };

    const handleLeave = async () => {
        try {
            await leaveConversation(conversationId);
            onClose();
            if (onLeft) onLeft();
            else navigate("/chatlist");
        } catch (err) {
            console.error(err);
            setError("Failed to leave conversation.");
        }
    };

    const confirmRemoveMember = async () => {
        setIsConfirming(true);
        await handleKick(memberToRemove.userId);
        setIsConfirming(false);
        setMemberToRemove(null);
    };

    const confirmLeave = async () => {
        setIsConfirming(true);
        await handleLeave();
        setIsConfirming(false);
        setShowLeaveConfirm(false);
    };

    const confirmUnfriend = async () => {
        setIsConfirming(true);
        setError("");
        try {
            await unfriend(details.otherUserId);
            onFriendRemoved();  
            onClose();
        } catch (err) {
            console.error(err);
            setError(err.message || "Failed to remove friend.");
            setShowUnfriendConfirm(false);
        } finally {
            setIsConfirming(false);
        }
    };

    if (!details) {
        return (
            <div className="settings-overlay" onClick={onClose}>
                <div className="settings-panel" onClick={(e) => e.stopPropagation()}>
                    <p className="msg">Loading...</p>
                </div>
            </div>
        );
    }

    const memberIds = new Set(details.members.map((m) => m.userId));
    const addableFriends = friends.filter((f) => !memberIds.has(f.id));

    const handleNameKeyDown = (event) => {
        if (event.key === "Escape") {
            setIsEditingName(false);
        }
    };

    return (
        <div className="settings-overlay" onClick={onClose}>
            <div className="settings-panel" onClick={(e) => e.stopPropagation()}>
                <div className="settings-header">
                    <h2>{conversationType === "Group" ? "Group Info" : "Contact Info"}</h2>
                    <button className="modal-close-btn btn-reset" onClick={onClose}>✕</button>
                </div>

                <div className="settings-profile">
                    <div className="settings-avatar-wrapper">
                        {details.iconUrl ? (
                            <img src={`${SERVER_ORIGIN}${details.iconUrl}`} alt="" className="settings-avatar" />
                        ) : (
                            <div className="settings-avatar avatar-fallback">
                                {details.name?.[0]?.toUpperCase() || "?"}
                            </div>
                        )}

                        {canEditGroup && (
                            <>
                                <input
                                    type="file"
                                    ref={groupIconInputRef}
                                    accept="image/*"
                                    style={{ display: "none" }}
                                    onChange={handleIconSelect}
                                />
                                <button
                                    type="button"
                                    className="avatar-camera-btn btn-reset"
                                    onClick={() => groupIconInputRef.current?.click()}
                                    disabled={isUploadingIcon}
                                >
                                    {isUploadingIcon ? <span className="spinner" /> : "📷"}
                                </button>
                            </>
                        )}
                    </div>

                    {isEditingName ? (
                        <form className="name-edit-form" onSubmit={handleSaveName}>
                            <input
                                type="text"
                                value={editName}
                                onChange={(e) => setEditName(e.target.value)}
                                onKeyDown={handleNameKeyDown}
                                autoFocus
                                disabled={isSavingName}
                            />
                        </form>
                    ) : (
                        <p
                            className={`settings-name ${canEditGroup ? "editable" : ""}`}
                            onClick={startEditingName}
                        >
                            {details.name}
                            {canEditGroup && <span className="edit-pencil">✎</span>}
                        </p>
                    )}
                </div>

                {conversationType === "Direct" ? (
                    <div className="settings-section">
                        {details.username && (
                            <p className="profile-username">@{details.username}</p>
                        )}

                        {details.otherUserBio && (
                            <p className="settings-bio">{details.otherUserBio}</p>
                        )}
                        
                        {details.friendSince && (
                            <p className="settings-meta">
                                Friends since {new Date(details.friendSince).toLocaleDateString()}
                            </p>
                        )}
                        {details.otherUserJoinedAt && (
                            <p className="settings-meta">
                                Joined ChatApp {new Date(details.otherUserJoinedAt).toLocaleDateString()}
                            </p>
                        )}

                        <button type="button" className="unfriend-btn" onClick={() => setShowUnfriendConfirm(true)}>
                            Unfriend
                        </button>
                    </div>
                ) : (
                    <div className="settings-section">
                        <p className="settings-section-title">
                            {details.members.length} members
                        </p>
    
                        <div className="settings-member-list">
                            {details.members.map((m) => (
                                <div className="settings-member-row" key={m.userId}>
                                    <Profile url={m.profilePictureUrl} name={m.displayName} />

                                    <div className="member-info">
                                        <span>{m.displayName}</span>
                                        {m.username && (
                                            <p className="profile-username">@{m.username}</p>
                                        )}
                                    </div>

                                    {m.userId !== currentUserId && (
                                        <button className="kick-btn btn-reset" onClick={() => setMemberToRemove(m)}>
                                            Remove
                                        </button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {conversationType === "Group" && (
                    <div className="add-member-section">
                        {!showAddMember ? (
                            <button type="button" className="add-member-btn" onClick={openAddMember}>
                                + Add Member
                            </button>
                        ) : (
                            <div className="add-member-panel">
                                <div className="add-member-header">
                                    <p className="settings-section-title">Add a friend</p>
                                    <button
                                        type="button"
                                        className="add-member-close btn-reset"
                                        onClick={() => setShowAddMember(false)}
                                    >
                                        Done
                                    </button>
                                </div>

                                <div className="add-member-list">
                                    {addableFriends.length === 0 ? (
                                        <p className="settings-meta">No friends left to add.</p>
                                    ) : (
                                        addableFriends.map((f) => (
                                            <div className="settings-member-row" key={f.id}>
                                                <Profile url={f.profilePictureUrl} name={f.displayName} />
                                                <span>{f.displayName}</span>
                                                <button
                                                    type="button"
                                                    className="add-btn btn-reset"
                                                    onClick={() => handleAddMember(f)}
                                                    disabled={addingId === f.id}
                                                >
                                                    {addingId === f.id ? "Adding..." : "Add"}
                                                </button>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                )}

                {error && <p className="msg">{error}</p>}

                {conversationType === "Group" && ( 
                    <div>
                        <button className="leave-group-btn" onClick={() => setShowLeaveConfirm(true)}>
                            Leave Group
                        </button>
                    </div>
                )}

            </div>

            {memberToRemove && (
                <ConfirmationModal
                    title="Remove member?"
                    message={`${memberToRemove.displayName} will lose access to this group's messages.`}
                    confirmLabel="Remove"
                    danger
                    isLoading={isConfirming}
                    onConfirm={confirmRemoveMember}
                    onCancel={() => setMemberToRemove(null)}
                />
            )}

            {showLeaveConfirm && (
                <ConfirmationModal
                    title="Leave group?"
                    message={
                        details.members.length <= 1
                            ? "You're the last member, so leaving will delete this group."
                            : "You'll stop getting messages from this group unless someone adds you back."
                    }
                    confirmLabel="Leave"
                    danger
                    isLoading={isConfirming}
                    onConfirm={confirmLeave}
                    onCancel={() => setShowLeaveConfirm(false)}
                />
            )}
            
            {showUnfriendConfirm && (
                <ConfirmationModal
                    title="Unfriend?"
                    message={`You and ${details.name} won't be friends anymore. You'll keep your chat history, but you won't be able to message each other unless you become friends again.`}
                    confirmLabel="Unfriend"
                    danger
                    isLoading={isConfirming}
                    onConfirm={confirmUnfriend}
                    onCancel={() => setShowUnfriendConfirm(false)}
                />
            )}
        </div>

    );
}

export default ChatSettings;