import { useState } from "react";
import Chats from "../components/Chats";
import ChatRoom from "../components/ChatRoom";
import CreateGroupModal from "../components/CreateGroupModal";

function ChatList() {
    const [selectedId, setSelectedId] = useState(null);
    const [showCreateGroup, setShowCreateGroup] = useState(false);
    const [listVersion, setListVersion] = useState(0);

    const handleLeftConversation = () => {
        setSelectedId(null);
        setListVersion((v) => v + 1); 
    };

    return (
        <div className="friends-page">
            <div className="page-container chat-layout">
                <div className="left-column">
                    <Chats onSelect={setSelectedId} activeId={selectedId} refreshKey={listVersion}/>
                    <button className="new-group-btn" onClick={() => setShowCreateGroup(true)}>
                        + New Group
                    </button>
                </div>
                <div className="right-column">
                    <ChatRoom conversationId={selectedId} onLeft={handleLeftConversation} />
                </div>
            </div>

            {showCreateGroup && (
                <CreateGroupModal onClose={() => setShowCreateGroup(false)} />
            )}
        </div>
    );
}

export default ChatList;