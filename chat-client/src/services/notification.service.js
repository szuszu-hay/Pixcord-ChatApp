import { API_BASE_URL } from "./api";

export async function getNotifications() {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/notifications`, {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load notifications.");
        error.status = res.status;
        throw error;
    }
    return data;
}

export async function getUnreadCount() {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/notifications/unread-count`, {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load unread count.");
        error.status = res.status;
        throw error;
    }
    return data.count;
}

export async function markAsRead(notificationId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/notifications/${notificationId}/read`, {
        method: "PUT",
        headers: { "Authorization": `Bearer ${token}` }
    });
    if (!res.ok) {
        const error = new Error("Failed to mark notification as read.");
        error.status = res.status;
        throw error;
    }
}

export async function markAllAsRead() {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/notifications/read-all`, {
        method: "PUT",
        headers: { "Authorization": `Bearer ${token}` }
    });
    if (!res.ok) {
        const error = new Error("Failed to mark all as read.");
        error.status = res.status;
        throw error;
    }
}