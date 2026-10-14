import { API_BASE_URL } from "./api";

export async function sendRequest(receiverId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/friend-requests`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ receiverId })
    })

    const data = await res.json();
    if (!res.ok){
        const error = new Error(data.title || "Failed to send friend request.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function getIncomingRequests() {
    const token = localStorage.getItem("token");

    const res = await fetch(`${API_BASE_URL}/friend-requests/incoming`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || `Request failed (${res.status}).`);
        error.status = res.status;
        throw error;
    }

    return data;
}


export async function getOutgoingRequests() {
    const token = localStorage.getItem("token");

    const res = await fetch(`${API_BASE_URL}/friend-requests/outgoing`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const data = await res.json();

    if (!res.ok) {
        const error = new Error(data.title || "Failed to load friend requests.");
        error.status = res.status;
        throw error;
    }

    return data;
}


export async function acceptFriendRequest(requestId) {
    const token = localStorage.getItem("token");

    const res = await fetch(
        `${API_BASE_URL}/friend-requests/${requestId}/accept`,
        {
            method: "PUT",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }
    );

    const data = await res.json();

    if (!res.ok) {
        const error = new Error(data.title || "Failed to accept request.");
        error.status = res.status;
        throw error;
    }

    return data;
}


export async function rejectFriendRequest(requestId) {
    const token = localStorage.getItem("token");

    const res = await fetch(
        `${API_BASE_URL}/friend-requests/${requestId}/reject`,
        {
            method: "PUT",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }
    );

    const data = await res.json();

    if (!res.ok) {
        const error = new Error(data.title || "Failed to reject request.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function cancelFriendRequest(requestId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/friend-requests/${requestId}/cancel`, {
        method: "PUT",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });
    
    const data = res.json();

    if (!res.ok){
        const error = new Error(data.title || "Failed to cancel request.");
        error.status = res.status;
        throw error;
    }
    
    return data;
}

export async function getFriends() {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/friend-requests/friends`, {
        headers: { "Authorization": `Bearer ${token}` }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load friends.");
        error.status = res.status;
        throw error;
    }

    return data.map(f => ({
        id: f.friendId,
        displayName: f.friendDisplayName,
        username: f.friendUsername,
        profilePictureUrl: f.friendProfilePictureUrl,
        bio: f.friendBio,
        isOnline: f.isOnline,
        friendSince: f.friendSince
    }));
}

export async function searchFriends(query) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/friend-requests/search?query=${encodeURIComponent(query)}`, {
        headers: {"Authorization": `Bearer ${token}`}
    });

    const data = await res.json();
    if (!res.ok) {
        throw new Error(data.message || "Failed to search users.");
    }

    return data;
}

export async function unfriend(friendId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/friend-requests/friends/${friendId}`, {
        method: "DELETE",
        headers: { "Authorization": `Bearer ${token}` }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || "Failed to remove friend.");
        error.status = res.status;
        throw error;
    }
}