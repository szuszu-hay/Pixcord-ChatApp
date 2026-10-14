import { useState, useEffect, useRef } from "react";
import Profile from "./Profile";
import { updateMyBio, updateMyUsername, updateMyAvatar } from "../services/user.service";

const BIO_MAX = 200;

function ProfileModal({ profile, onClose, onProfileUpdated }) {
    // username editing 
    const [isEditingUsername, setIsEditingUsername] = useState(false);
    const [usernameDraft, setUsernameDraft] = useState("");
    const [isSavingUsername, setIsSavingUsername] = useState(false);
    const [usernameError, setUsernameError] = useState("");

    // bio editing
    const [isEditingBio, setIsEditingBio] = useState(false);
    const [bioDraft, setBioDraft] = useState("");
    const [isSavingBio, setIsSavingBio] = useState(false);
    const [bioError, setBioError] = useState("");

    // picture upload
    const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
    const [avatarError, setAvatarError] = useState("");
    const avatarInputRef = useRef(null);

    useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key !== "Escape") return;
            if (isEditingBio) {
                setIsEditingBio(false); 
            } else if (isEditingUsername){
                setIsEditingUsername(false);
            } else {
                onClose();        
            }
        };
        document.addEventListener("keydown", handleKeyDown);
        return () => document.removeEventListener("keydown", handleKeyDown);
    }, [onClose, isEditingBio, isEditingUsername]);

    const startEditingUsername = () => {
        setUsernameDraft(profile.username || "");
        setUsernameError("");
        setIsEditingUsername(true);
    }

    const handleSaveUsername = async () => {
        const trimmed = usernameDraft.trim();
        if (trimmed === (profile.username || "")) {
            setIsEditingUsername(false); 
            return;
        }

        setIsSavingUsername(true);
        setUsernameError("");

        try {
            const updated = await updateMyUsername(trimmed);
            onProfileUpdated(updated);
            setIsEditingUsername(false);
        } catch (err) {
            console.error(err);
            setUsernameError(err.message || "Failed to update username.");
        } finally {
            setIsSavingUsername(false);
        }
    };

    const startEditingBio = () => {
        setBioDraft(profile.bio || "");
        setBioError("");
        setIsEditingBio(true);
    };

    const handleSaveBio = async () => {
        const trimmed = bioDraft.trim();
        if (trimmed === (profile.bio || "")) {
            setIsEditingBio(false); 
            return;
        }

        setIsSavingBio(true);
        setBioError("");

        try {
            const updated = await updateMyBio(trimmed);
            onProfileUpdated(updated);
            setIsEditingBio(false);
        } catch (err) {
            console.error(err);
            setBioError(err.message || "Failed to update bio.");
        } finally {
            setIsSavingBio(false);
        }
    };

    const handleAvatarSelect = async (event) => {
        const file = event.target.files[0];
        if (!file) return;

        setIsUploadingAvatar(true);
        setAvatarError("");

        try {
            const updated = await updateMyAvatar(file);
            onProfileUpdated(updated);
        } catch (err) {
            console.error(err);
            setAvatarError(err.message || "Failed to update photo.");
        } finally {
            setIsUploadingAvatar(false);
            if (avatarInputRef.current) avatarInputRef.current.value = "";
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                    <h2>Your Profile</h2>
                    <button className="modal-close-btn btn-reset" onClick={onClose}>✕</button>
                </div>

                <div className="profile-modal-body">
                    <div className="profile-avatar-wrapper">
                        <Profile url={profile.profilePictureUrl} name={profile.displayName} className="profile-avatar-lg"/>
                        <input type="file" ref={avatarInputRef} accept="image/*" style={{ display: "none" }} onChange={handleAvatarSelect} />
                        <button type="button" className="avatar-camera-btn btn-reset" onClick={() => avatarInputRef.current?.click()}
                            disabled={isUploadingAvatar} title="Change photo">
                            {isUploadingAvatar ? <span className="spinner" /> : "📷"}
                        </button>
                    </div>

                    {avatarError && <p className="msg">{avatarError}</p>}
                    <p className="profile-display-name">{profile.displayName}</p>
                </div>

                <div className="profile-details">
                    <div className="profile-row">
                        <div className="bio-row">
                            <span className="profile-label">Username</span>
                            {!isEditingUsername && (
                                <button type="button" className="bio-edit-btn btn-reset" onClick={startEditingUsername}>
                                    Edit
                                </button>
                            )}
                        </div>

                        {isEditingUsername ? (
                            <div className="bio-editor">
                                <input value={usernameDraft} onChange={(e) => setUsernameDraft(e.target.value)}
                                    placeholder="Type in a username you want to display" autoFocus
                                    disabled={isSavingUsername}
                                />

                                <div className="action-btns-container">
                                    <div className="bio-editor-actions">
                                        <button type="button" className="bio-cancel-btn btn-reset"
                                            onClick={() => setIsEditingUsername(false)}
                                            disabled={isSavingUsername}
                                        >
                                            Cancel
                                        </button>
                                        <button type="button" onClick={handleSaveUsername} disabled={isSavingUsername}>
                                            {isSavingUsername ? <span className="spinner" /> : "Save"}
                                        </button>
                                    </div>
                                </div>
                                    
                                {usernameError && <p className="msg">{usernameError}</p>}
                            </div>
                        ) : (
                            <p>{profile.username || "Add a username"}</p>
                        )}
                    </div>

                    <div className="profile-row">
                        <span className="profile-label">Email</span>
                        <p>{profile.email}</p>
                    </div>

                    <div className="profile-row">
                        <div className="bio-row">
                            <span className="profile-label">Bio</span>
                            {!isEditingBio && (
                                <button type="button" className="bio-edit-btn btn-reset" onClick={startEditingBio}>
                                    {profile.bio ? "Edit" : "Add"}
                                </button>
                            )}
                        </div>

                        {isEditingBio ? (
                            <div className="bio-editor">
                                <textarea value={bioDraft} onChange={(e) => setBioDraft(e.target.value)}
                                    maxLength={BIO_MAX} rows={3}
                                    placeholder="Tell people a little about yourself" autoFocus
                                    disabled={isSavingBio}
                                />
                                <div className="bio-editor-footer">
                                    <span className="bio-counter">{bioDraft.length}/{BIO_MAX}</span>
                                    <div className="bio-editor-actions">
                                        <button type="button" className="bio-cancel-btn btn-reset"
                                            onClick={() => setIsEditingBio(false)}
                                            disabled={isSavingBio}
                                        >
                                            Cancel
                                        </button>
                                        <button type="button" onClick={handleSaveBio} disabled={isSavingBio}>
                                            {isSavingBio ? <span className="spinner" /> : "Save"}
                                        </button>
                                    </div>
                                </div>
                                {bioError && <p className="msg">{bioError}</p>}
                            </div>
                        ) : (
                            <p>{profile.bio || "No bio yet"}</p>
                        )}
                    </div>

                    <div className="profile-row">
                        <span className="profile-label">Member since</span>
                        <p>{new Date(profile.createdAt).toLocaleDateString()}</p>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default ProfileModal;