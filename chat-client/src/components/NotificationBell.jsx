import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { getNotifications, getUnreadCount, markAsRead, markAllAsRead } from "../services/notification.service";

function NotificationBell() {
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef(null);
    const navigate = useNavigate();

    const loadUnreadNotifications = async () => {
        try {
            const count = await getUnreadCount();
            setUnreadCount(count);
        } catch (error) {
            console.error(error);
        }
    };

    const loadNotifications = async () => {
        try {
            const data = await getNotifications();
            setNotifications(data);
        } catch (error) {
            console.error(error);
        }
    };

    useEffect(() => {
        loadUnreadNotifications();
        const interval = setInterval(loadUnreadNotifications, 30000); 
        return () => clearInterval(interval);
    }, []);

    const toggleOpen = async () => {
        const opening = !isOpen;
        setIsOpen(opening);
        if (opening) {
            await loadNotifications();
        }
    };

    const handleNotificationClick = async (notification) => {
        if (!notification.isRead) {
            try {
                await markAsRead(notification.id);
                setNotifications((prev) =>
                    prev.map((n) =>
                        n.id === notification.id ? { ...n, isRead: true } : n
                    )
                );
                setUnreadCount((prev) => Math.max(prev - 1, 0));
            } catch (error) {
                console.error(error);
            }
        }

        setIsOpen(false);

        if (notification.type === "FriendRequest" || notification.type === "FriendRequestAccepted") {
            navigate("/friend-list");
        } else if (notification.type === "Message" && notification.conversationId) {
            navigate(`/chat/${notification.conversationId}`);
        } else if (notification.type === "AddedToGroup" && notification.conversationId) {
            navigate(`/chat/${notification.conversationId}`);
        }
    };

    const handleMarkAllRead = async () => {
        try {
            await markAllAsRead();
            setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
            setUnreadCount(0);
        } catch (err) {
            console.error(err);
        }
    };

    useEffect(() => {
        if (!isOpen) return;
        const handleOutsideClick = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setIsOpen(false);
            }
        };
        document.addEventListener("click", handleOutsideClick);
        return () => document.removeEventListener("click", handleOutsideClick);
    }, [isOpen]);

    return (
        <div className="notification-bell-wrapper" ref={dropdownRef}>
            <button className="notification-bell-btn" onClick={toggleOpen}>
                🔔
                {unreadCount > 0 && <span className="notification-badge">{unreadCount}</span>}
            </button>

            {isOpen && (
                <div className="notification-dropdown">
                    <div className="notification-dropdown-header">
                        <p>Notifications</p>
                        {unreadCount > 0 && (
                            <button className="mark-all-read-btn" onClick={handleMarkAllRead}>
                                Mark all read
                            </button>
                        )}
                    </div>

                    <div className="notification-list">
                        {notifications.length === 0 ? (
                            <p className="notification-empty">No notifications yet</p>
                        ) : (
                            notifications.map((n) => (
                                <div
                                    key={n.id}
                                    className={`notification-item ${n.isRead ? "" : "unread"}`}
                                    onClick={() => handleNotificationClick(n)}
                                >
                                    <p className="notification-text">
                                        <strong>{n.relatedUserDisplayName}</strong> {n.message}
                                    </p>
                                    <span className="notification-time">
                                        {new Date(n.createdAt).toLocaleString()}
                                    </span>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}

export default NotificationBell;
