import { API_BASE_URL } from "./api";

export async function searchUsers(query) {
    const token = localStorage.getItem("token");

    const res = await fetch(`${API_BASE_URL}/user/search?query=${encodeURIComponent(query)}`,
        {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }
    );

    const data = await res.json();

    if (!res.ok) {
        throw new Error(data.message || "Failed to search users.");
    }

    return data;
}

export async function getMyProfile() {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/user/me`, {
        headers: { "Authorization": `Bearer ${token}` }
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.title || "Failed to load profile.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function updateMyUsername(username) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/user/me/username`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ username })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || data?.title || "Failed to update username.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function updateMyBio(bio) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE_URL}/user/me/bio`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ bio })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || data?.title || "Failed to update bio.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function updateMyAvatar(file) {
    const token = localStorage.getItem("token");

    const formData = new FormData();
    formData.append("file", file);

    const res = await fetch(`${API_BASE_URL}/user/me/avatar`, {
        method: "PUT",
        headers: { "Authorization": `Bearer ${token}` }, // no Content-Type: the browser sets the multipart boundary
        body: formData
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || data?.title || "Failed to update photo.");
        error.status = res.status;
        throw error;
    }

    return data;
}