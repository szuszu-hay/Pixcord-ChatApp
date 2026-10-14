import { API_BASE_URL } from "./api";

export async function CreateDirectConversation(friendId){
    const token = localStorage.getItem("token");

    const res = await fetch(`${API_BASE_URL}/conversations`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ friendId })
    });

    const text = await res.text();
    console.log("Status:", res.status);
    console.log("Response:", text);

    let data = null;

    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            console.log("Response was not JSON");
        }
    }
    if (!res.ok){
        const error = new Error(data?.message || "Failed to create new conversation.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function getConversations() {
    const token = localStorage.getItem("token");

    const res = await fetch(`${API_BASE_URL}/conversations`, {
        headers: { "Authorization": `Bearer ${token}` }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load conversations.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function getConversation(conversationId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}`, {
        headers: { "Authorization" : `Bearer ${token}` }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load conversation.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function getConversationDetails(conversationId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/details`, {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load conversation details.");
        error.status = res.status;
        throw error;
    }
    return data;
}

export async function addMember(conversationId, userId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/add-members`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ userId })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || data?.title || "Failed to add member.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function removeMember(conversationId, targetUserId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/members/${targetUserId}`, {
        method: "DELETE",
        headers: { "Authorization": `Bearer ${token}` }
    });
    if (!res.ok) {
        const error = new Error("Failed to remove member.");
        error.status = res.status;
        throw error;
    }
}

export async function leaveConversation(conversationId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/leave`, {
        method: "POST",
        headers: { "Authorization": `Bearer ${token}` }
    });
    if (!res.ok) {
        const error = new Error("Failed to leave conversation.");
        error.status = res.status;
        throw error;
    }
}

export async function updateGroupDetails(conversationId, name, iconFile) {
    const token = localStorage.getItem("token");

    const formData = new FormData();
    if (name) formData.append("name", name);
    if (iconFile) formData.append("iconFile", iconFile);

    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/group`, {
        method: "PUT",
        headers: { "Authorization": `Bearer ${token}` }, // no Content-Type — browser sets the multipart boundary itself
        body: formData
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || data?.message || "Failed to update group.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function createGroupConversation(name, memberIds) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/group`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ name, memberIds })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to create group conversation.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function sendMessage(conversationId, content) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages`, {
        method: "POST",
        headers: { 
            "Content-Type" : "application/json",
            "Authorization" : `Bearer ${token}`
        },
        body: JSON.stringify({ content })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok){
        const error = new Error(data?.title || "Failed to send message.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function getMessages(conversationId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages`, {
        headers: { "Authorization" : `Bearer ${token}`}
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load messages.");
        error.status = res.status;
        throw error;
    }

    return data;         
}

export async function editMessage(conversationId, messageId, content) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages/${messageId}`, {
        method: "PUT",
        headers: {
            "Content-Type" : "application/json",  
            "Authorization" : `Bearer ${token}`
        },
        body: JSON.stringify({ content })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to edit message.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function deleteMessage(conversationId, messageId) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages/${messageId}`, {
        method: "DELETE",
        headers: {
            "Authorization" : `Bearer ${token}`
        },
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to delete message.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function toggleReaction(conversationId, messageId, emoji) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages/${messageId}/reactions`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ emoji })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to react.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function sendMessageWithAttachment(conversationId, content, file) {
    const token = localStorage.getItem("token");

    const formData = new FormData();
    formData.append("content", content ?? "");
    formData.append("file", file);

    const res = await fetch(`${API_BASE_URL}/conversations/${conversationId}/messages/upload`, {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        body: formData
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || data?.message || "Failed to send attachment.");
        error.status = res.status;
        throw error;
    }

    return data;
}