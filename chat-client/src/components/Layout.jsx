import { useState, useEffect } from "react";
import { Outlet, useNavigate, Link } from "react-router-dom";
import NotificationBell from "./NotificationBell";
import Profile from "./Profile";
import ProfileModal from "./ProfileModal";
import { getMyProfile } from "../services/user.service";
import ConfirmationModal from "./ConfirmationModal";

function Layout() {
    const navigate = useNavigate();
    const [profile, setProfile] = useState(null);
    const [showProfile, setShowProfile] = useState(false);
    const [showSignOutConfirm, setShowSignOutConfirm] = useState(false);

    useEffect(() => {
        const loadProfile = async () => {
            try {
                const data = await getMyProfile();
                setProfile(data);
            } catch (err) {
                console.error(err);
            }
        };
        loadProfile();
    }, []);

    const handleSignOut = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("userId");
        navigate("/login", { replace: true });
    };

    return (
        <div className="layout">
            <nav className="navbar">
                <h1 className="logo">Pixcord</h1>
                <div className="nav-links">
                    <Link to="/chat-list">Chats</Link>
                    <Link to="/friend-list">Friends</Link>
                </div>

                <div className="nav-right">
                    <NotificationBell />
                    <button
                        type="button"
                        className="nav-avatar-btn btn-reset"
                        onClick={() => setShowProfile(true)}
                        title="Your profile"
                    >
                        <Profile url={profile?.profilePictureUrl} name={profile?.displayName} />
                    </button>
                    <button type="button" className="sign-out-btn btn-reset" onClick={() => setShowSignOutConfirm(true)}>
                        Sign Out
                    </button>
                </div>
            </nav>

            <main className="main-content">
                <Outlet />
            </main>

            {showProfile && profile && (
                <ProfileModal profile={profile} onClose={() => setShowProfile(false)} onProfileUpdated={setProfile}/>
            )}

            {showSignOutConfirm && (
                <ConfirmationModal
                    title="Sign out?"
                    message="You'll need to log in again to get back to your chats."
                    confirmLabel="Sign Out"
                    onConfirm={handleSignOut}
                    onCancel={() => setShowSignOutConfirm(false)}
                />
            )}
        </div>
    );
}

export default Layout;