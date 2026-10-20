export const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5156/api";
export const SERVER_ORIGIN = API_BASE_URL.replace(/\/api$/, "");

export const toUrl = (u) => (!u ? null : u.startsWith("http") ? u : `${SERVER_ORIGIN}${u}`);