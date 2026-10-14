import { API_BASE_URL } from "./api";

export async function login(email, password){
    const res = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: {"Content-Type" : "application/json"},
        body: JSON.stringify({ email, password })
    });

    const data = await res.json();

    if (!res.ok){
        const error = new Error(data.message || "Failed to login.");
        error.status = res.status;
        throw error;
    }

    localStorage.setItem("token", data.token);
    localStorage.setItem("userId", data.userId);
    localStorage.setItem("email", data.email);
    localStorage.setItem("displayName", data.displayName);

    return data;
}

export async function register(displayName, email, password, confirmPassword){
    const res = await fetch(`${API_BASE_URL}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ 
            displayName, 
            email,
            password,
            confirmPassword
        })        
    });

    const data = await res.json();
    if (!res.ok) {
        const error = new Error(data.message || "Failed to sign up.");
        error.status = res.status;
        throw error;
    }

    return data;
}

export async function googleLogin(idToken) {
    const res = await fetch(`${API_BASE_URL}/auth/google`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ idToken })
    });

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const error = new Error(data?.message || data?.title || "Google sign-in failed.");
        error.status = res.status;
        throw error;
    }

    localStorage.setItem("token", data.token);
    localStorage.setItem("userId", data.userId);
    localStorage.setItem("email", data.email);
    localStorage.setItem("displayName", data.displayName);

    return data;
}