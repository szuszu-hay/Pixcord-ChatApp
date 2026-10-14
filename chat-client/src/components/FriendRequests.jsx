//display incoming friend requests
import { getIncomingRequests, acceptFriendRequest, rejectFriendRequest, cancelFriendRequest } from "../services/friendRequest.service";
import { useState, useEffect } from "react";

function FriendRequests({ onFriendAccepted }){
    const [requests, setRequests] = useState([]);
    const [message, setMessage] = useState("");
    const [processingId, setProcessingId] = useState(null);

    //load friend requests
    const loadFriendRequests = async () => {
        setMessage("");

        try {
            const data = await getIncomingRequests();
            setRequests(data);
        } catch(err){
            console.error(err);
            setMessage("Failed to load friend requests.");
        }
    };
    
    const handleAcceptRequest = async (requestId) => {
        setProcessingId(requestId);
        setMessage("");

        try{
            await acceptFriendRequest(requestId);
            setRequests(prev => 
                prev.filter(request => request.id !== requestId)
            );

            setMessage("Friend request accepted!");
            onFriendAccepted?.();
            setTimeout(() => {
                loadFriendRequests();
            }, 1000);
        } catch (err){
            console.error(err);
            setMessage(err.message || "Failed to accept friend request.");
        } finally {
            setProcessingId(null);
        }
    };

    const handleRejectRequest = async(requestId) => {
        setProcessingId(requestId);
        setMessage("");

        try {
            await rejectFriendRequest(requestId);
            setRequests(prev =>
                prev.filter(request => request.id !== requestId)
            );
        } catch(err){
            console.error(err);
            setMessage("Failed to reject friend request.");
        } finally {
            setProcessingId(null);
        }
    };

    const handleCancelRequest = async(requestId) => {
        setMessage("");

        try {
            await cancelFriendRequest(requestId);
            setRequests(prev =>
                prev.filter(request => request.id !== requestId)
            );
        } catch(err){
            console.error(err);
            setMessage("Failed to cancel friend request.");
        } finally {
            setProcessingId(null);
        }
    }

    useEffect(() => {
        loadFriendRequests();
    }, []);

    return (
        <section className="friend-requests">
            <div className="friend-requests-header">
                <div className="friend-title">
                      <div className="friend-icon">🐾</div>
                    <div>
                        <h2>Friend Requests</h2>
                        <p>
                            {requests.length > 0
                                ? `${requests.length} waiting for you`
                                : "No new requests"}
                        </p>
                    </div>
                </div>

                {requests.length > 0 && (
                    <span className="request-count">{requests.length}</span>
                )}
            </div>

            {requests.length > 0 ? (
                <div className="request-list">
                    {requests.map((request) => {

                        const isProcessing = processingId === request.id;

                        const initials = request.senderDisplayName
                                ?.split(" ")
                                .map(word => word[0])
                                .join("")
                                .slice(0, 2)
                                .toUpperCase();
                    return (
                            <div className="friend-request-card" key={request.id}>
                                <div className="request-avatar-wrapper">
                                    {request.senderProfilePictureUrl ? (
                                        <img className="request-avatar" src={request.senderProfilePictureUrl} alt=""/>
                                    ) : (
                                        <div className="request-avatar avatar-fallback">
                                            {initials}
                                        </div>
                                    )}

                                    <span className="online-dot"></span>
                                </div>

                                <div className="request-info">
                                    <p className="request-name">{request.senderDisplayName}</p>
                                    <p className="request-username">@{request.senderUsername}</p>
                                    <p className="request-time">
                                        Wants to be your friend
                                    </p>
                                </div>

                                <div className="request-actions">
                                    <button className="accept-btn"
                                        disabled={isProcessing}
                                        onClick={() =>
                                            handleAcceptRequest(request.id)
                                        }
                                    >
                                        {isProcessing
                                            ? <span className="spinner" />
                                            : "Accept"
                                        }
                                    </button>

                                    <button className="reject-btn" disabled={isProcessing}
                                        onClick={() =>
                                            handleRejectRequest(request.id)
                                        }
                                    >
                                        Reject
                                    </button>

                                    <button className="reject-btn" disabled={isProcessing}
                                        onClick={() =>
                                            handleCancelRequest(request.id)
                                        }
                                    >
                                        Cancel
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            ) : (
                <div className="empty-requests">
                    <div className="sleepy-cat">🐱</div>
                    <p>No new friend requests</p>

                    <span>
                        Your little social corner is quiet...
                    </span>

                </div>

            )}

            {message && (
                <p className="friend-request-message">
                    {message}
                </p>
            )}

        </section>
    );
}

export default FriendRequests;