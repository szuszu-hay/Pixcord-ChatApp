import { useEffect, useState } from "react";
import { getConversations } from "../services/conversation.service";
import Profile from "./Profile";

function Chats({ onSelect, activeId, refreshKey }) {
    const [conversations, setConversations] = useState([]);
    const [message, setMessage] = useState("");

    const loadConversations = async () => {
        setMessage("");

        try {
            const data = await getConversations();
            setConversations(data);
        } catch (err) {
            console.error(err);
            setMessage("Failed to load conversations");
        }
    };

    useEffect (() => {
        loadConversations();
    }, [refreshKey]);

    return (
        <div className="chats-list">
             {conversations.map((c) => {
                 const title = c.type === "Direct" && c.username ? `${c.username}` : c.name;
     
                 return (
                     <div
                         key={c.conversationId}
                         className={`user-card ${activeId === c.conversationId ? "active" : ""}`}
                         onClick={() => onSelect(c.conversationId)}
                     >
                         <Profile url={c.iconUrl} name={c.name} />
                         <div className="user-info">
                             <p className="user-name">{title}</p>
                             <p className="user-handle">{c.lastMessagePreview || "No messages yet"}</p>
                         </div>
                     </div>
                 );
             })}
     
             {message && <p className="msg">{message}</p>}
         </div>       
    );
}

export default Chats;