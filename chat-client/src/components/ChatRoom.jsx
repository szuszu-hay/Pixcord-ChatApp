import { useEffect, useRef, useState } from "react";
import { useParams } from "react-router-dom";
import { deleteMessage, editMessage, getConversation, getMessages, sendMessage, toggleReaction, sendMessageWithAttachment } from "../services/conversation.service";
import { toUrl } from "../services/api";
import ChatSettings from "./ChatSettings";
import Profile from "./Profile";

function ChatRoom({ conversationId: propConversationId, onLeft }) {
    const params = useParams();
    const conversationId = propConversationId ?? params.conversationId;

    const sendSoundRef = useRef(new Audio("/sounds/message-sent.wav"));
    const [conversation, setConversation] = useState(null);
    const [messages, setMessages] = useState([]);
    const [message, setMessage] = useState("");
    const [isSending, setIsSending] = useState(false);
    const [draft, setDraft] = useState("");
    const [activeMenuId, setActiveMenuId] = useState(null);
    const [editingId, setEditingId] = useState(null);
    const [editDraft, setEditDraft] = useState(""); 
    const [isNearBottom, setIsNearBottom] = useState(true);
    const [failedSend, setFailedSend] = useState(0);
    const [showSettings, setShowSettings] = useState(false);
    
    const currentUserId = localStorage.getItem("userId");

    const bottomRef = useRef(null);
    const inputRef = useRef(null);
    const messagesContainerRef = useRef(null);

    const loadConversation = async () => {
        try {
            const data = await getConversation(conversationId);
            setConversation(data);
        } catch (err) {
            console.error(err);
            setMessage("Failed to load conversation.");
        }
    }

    const loadMessages = async () => {
        try {
            const data = await getMessages(conversationId);
            setMessages(data);
        } catch (err) {
            console.error(err);
            setMessage("Failed to load messages.");
        }
    };

    useEffect(() => {
        if (!conversationId) return;
        loadConversation();
        loadMessages();
    }, [conversationId]);

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: "smooth"});
    }, [messages]);

    const [pendingFile, setPendingFile] = useState(null);
    const fileInputRef = useRef(null);

    const handleFileSelect = (event) => {
        const file = event.target.files[0];
        if (file) setPendingFile(file);
    };

    const handleSend = async (event) => {
        event.preventDefault();
        const content = draft.trim();
        if ((!content && !pendingFile) || isSending) return;

        setIsSending(true);
        setDraft("");
        const fileToSend = pendingFile;
        setPendingFile(null);

        try {
            const sent = fileToSend
            ? await sendMessageWithAttachment(conversationId, content, fileToSend)
            : await sendMessage(conversationId, content);

            setMessages((prev) => [...prev, sent]);
            if (fileInputRef.current) {
                fileInputRef.current.value = "";
            }

            sendSoundRef.current.currentTime = 0;
            sendSoundRef.current.play().catch(() => {});
        } catch (err) {
            console.error(err);
            setMessage("Failed to send message.");
            setDraft(content);
            setPendingFile(fileToSend);
            setFailedSend({ content, file: fileToSend });
        } finally {
            setIsSending(false);
        }
    };

    const handleBubbleClick = (event, messageId, isOwn) => {
        event.preventDefault();
        if (!isOwn) return;
        setActiveMenuId((prev) => (prev === messageId ? null : messageId));
    };

    const startEdit = (m) => {
        setEditingId(m.id);
        setEditDraft(m.content);
        setActiveMenuId(null);
    };

    const cancelEdit = () => {
        setEditingId(null);
        setEditDraft("");
    };

    const handleEditSubmit = async (event, messageId) => {
        event.preventDefault();
        const content = editDraft.trim();
        if (!content) return;

        try {
            const updated = await editMessage(conversationId, messageId, content);
            setMessages((prev) => 
                prev.map((m) => (m.id === messageId ? updated : m)));
            cancelEdit();
        } catch (err) {
            console.error(err);
            setMessage("Failed to edit message.");
        }
    };

    const handleDelete = async (messageId) => {
        setActiveMenuId(null);
        try {
            await deleteMessage(conversationId, messageId);
            setMessages((prev) => prev.filter((m) => m.id !== messageId));
        } catch (err) {
            console.error(err);
            setMessage("Failed to delete message.");
        }
    }

    const handleScroll = () => {
        const el = messagesContainerRef.current;
        if (!el) return;

        const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
        setIsNearBottom(distanceFromBottom < 100);
    };

    const scrollToBottom = () => {
        bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }

    useEffect(() => {
        if (activeMenuId === null) return;

        const handleOutsideClick = () => setActiveMenuId(null);
        document.addEventListener("click", handleOutsideClick);
        return () => document.removeEventListener("click", handleOutsideClick);
    }, [activeMenuId]);

    useEffect(() => {
        if (!isSending) {
            inputRef.current?.focus();
        }
    }, [isSending]);

    useEffect(() => {
        if (isNearBottom) {
            bottomRef.current?.scrollIntoView({ behavior: "smooth" });
        }
    }, [messages]);

    const QUICK_EMOJIS = ["👍", "❤️", "😂", "😮", "😢"];
    const handleReact = async (messageId, emoji) => {
        try {
            const reactions = await toggleReaction(conversationId, messageId, emoji);
            setMessages((prev) =>
                prev.map((m) => (m.id === messageId ? { ...m, reactions } : m))
            );
        } catch (err) {
            console.error(err);
            setMessage("Failed to react.");
        }
    };

    const placeholder = conversation?.messageTo ? `Message ${conversation.messageTo}`: "Type a message...";

    if (!conversationId) {
        return (
            <div className="chat-room chat-room-empty">
                <p>Select a conversation to start chatting</p>
            </div>
        )
    }

    return(
        <div className="chat-room"> 
            {conversation && (
                <div className="chat-header">
                    <div className="chat-header-avatar-wrapper">
                        {conversation.iconUrl ? (
                            <Profile url={conversation.iconUrl} name={conversation.name} className="chat-avatar" />
                        ) : (
                            <div className="chat-avatar avatar-fallback">
                                {conversation.name?.[0]?.toUpperCase() || "?"}
                            </div>
                        )}
                        {conversation.isOnline && <span className="online-dot" />}
                    </div>
                    <div className="chat-header-info">
                        <div>
                            <p className="chat-header-name">
                                {conversation.type === "Direct" && conversation.messageTo
                                    ? `${conversation.messageTo}`
                                    : conversation.name}
                            </p>
                            <p className="chat-header-status">{conversation.isOnline ? "Online" : "Offline"}</p>
                        </div>

                        <div className="chat-settings-btn" title="Chat settings" onClick={() => setShowSettings(true)}>
                            ☰
                            {showSettings && (
                                <ChatSettings
                                    conversationId={conversationId}
                                    conversationType={conversation?.type}
                                    onClose={() => setShowSettings(false)}
                                    onMembersChanged={loadMessages}
                                    onLeft={onLeft}
                                    onFriendRemoved={loadConversation}
                                />
                            )}
                        </div>
                    </div>
                </div>
            )}

            <div className="chat-messages" ref={messagesContainerRef} onScroll={handleScroll}>
                {messages.map((m, index) => {
                    if (m.type === "System") {
                        return (
                            <div key={m.id} className="system-message">
                                <strong>{m.senderId == currentUserId ? "You" : m.senderDisplayName}</strong>{" "}
                                {m.content}
                            </div>
                        );
                    }

                    const isOwn = m.senderId == currentUserId;
                    const prevMsg = messages[index - 1];
                    const isGrouped = prevMsg && prevMsg.type !== "System" && prevMsg.senderId === m.senderId;

                    const initials = m.senderDisplayName
                        ?.split(" ")
                        .map((w) => w[0])
                        .join("")
                        .slice(0, 2)
                        .toUpperCase();

                    return (
                        <div key={m.id} className={`chat-row ${isOwn ? "own" : "other"} ${isGrouped ? "grouped" : ""}`}>
                            {!isOwn && (
                                <div className="chat-avatar-slot">
                                    {!isGrouped && (
                                        m.senderProfilePictureUrl ? (
                                            <Profile url={m.senderProfilePictureUrl} name={m.senderDisplayName} className="chat-avatar" />
                                        ) : (
                                            <div className="chat-avatar avatar-fallback">{initials}</div>
                                        )
                                    )}
                                </div>
                            )}

                            <div className="chat-bubble-stack">
                                {!isGrouped && !isOwn && (
                                    <p className="chat-sender-name">{m.senderDisplayName}</p>
                                )}

                                {editingId === m.id ? (
                                    <form className="chat-edit-form" onSubmit={(e) => handleEditSubmit(e, m.id)}>
                                        <input type="text" value={editDraft} onChange={(e) => setEditDraft(e.target.value)} autoFocus />
                                        <button type="submit">Save</button>
                                        <button type="button" onClick={cancelEdit}>Cancel</button>
                                    </form>
                                ) : (
                                    <div className="chat-bubble" onContextMenu={(e) => handleBubbleClick(e, m.id, isOwn)}>
                                        <p>{m.content}</p>
                                        <span className="chat-time">
                                            {new Date(m.sentAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                                            {m.editedAt && " (edited)"}
                                        </span>
                                    </div>
                                )}

                                {m.reactions?.length > 0 && (
                                    <div className="reaction-badges">
                                        {m.reactions.map((r) => (
                                            <button key={r.emoji} className={`reaction-badge ${r.reactedByMe ? "mine" : ""} btn-reset`}
                                                onClick={() => handleReact(m.id, r.emoji)}>{r.emoji} {r.count}</button>
                                        ))}
                                    </div>
                                )}

                                <div className="quick-react-bar">
                                    {QUICK_EMOJIS.map((emoji) => (
                                        <button key={emoji} onClick={() => handleReact(m.id, emoji)}>
                                            {emoji}
                                        </button>
                                    ))}
                                </div>

                                {m.attachments?.length > 0 && (
                                    <div className="message-attachments">
                                        {m.attachments.map((a) => (
                                            a.contentType.startsWith("image/") ? (
                                                <img key={a.id} src={toUrl(a.fileUrl)} alt={a.fileName} className="attachment-image" />
                                            ) : (
                                                <a key={a.id} href={toUrl(a.fileUrl)} target="_blank" rel="noopener noreferrer" className="attachment-file">
                                                    📄 {a.fileName}
                                                </a>
                                            )
                                        ))}
                                    </div>
                                )}

                                {activeMenuId === m.id && (
                                    <div className="chat-action-menu">
                                        <button onClick={() => startEdit(m)}>Edit</button>
                                        <button onClick={() => handleDelete(m.id)}>Delete</button>
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })}
                <div ref={bottomRef} />
            </div>

            {!isNearBottom && (
                <button className="scroll-to-bottom-btn" onClick={scrollToBottom}>
                    ﹀
                </button>
            )}

            {conversation?.canMessage === false ? (
                <div className="chat-readonly-notice">
                    You're no longer friends, so you can't send messages in this chat.
                </div>
            ) : (
                <form className="chat-composer" onSubmit={handleSend}>
                    <input type="file" ref={fileInputRef} onChange={handleFileSelect} style={{ display: "none" }}
                        accept="image/*,application/pdf,text/plain" />
                    <button className="attach-btn btn-reset" type="button" onClick={() => fileInputRef.current?.click()}>📎</button>

                    {pendingFile && (
                        <span className="pending-file-chip">
                            <span className="attachment-file-name">📎 {pendingFile.name}</span>
                            <button type="button" onClick={() => setPendingFile(null)}>✕</button>
                        </span>
                    )}

                    {failedSend && (
                        <div className="send-failed-banner">
                            ⚠ Message failed to send — check your file and try again
                        </div>
                    )}

                    <input ref={inputRef} type="text" value={draft} onChange={(e) => setDraft(e.target.value)}
                        placeholder={placeholder} disabled={isSending} />
                    <button type="submit" disabled={isSending || (!draft.trim() && !pendingFile)}>
                        {isSending ? <span className="spinner" /> : "Send"}
                    </button>
                </form>
            )}

            {message && <p className="msg">{message}</p>}
        </div>
    );
}

export default ChatRoom;
