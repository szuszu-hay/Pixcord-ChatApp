import DiscoverNewFriends from "../components/DiscoverNewFriends";
import MyFriends from "../components/MyFriends";
import "../App.css";
import "../index.css";
import { getFriends } from "../services/friendRequest.service";
import { useState, useEffect } from "react";

function FriendList(){
    const [message, setMessage] = useState("");
    const [friends, setFriends] = useState([]);

    const loadFriends = async () => {
        setMessage("");
            try {
                const data = await getFriends();
                setFriends(data);
            } catch(err) {
                console.error(err);
                setMessage("Failed to load friends.");
            }
        };
    
    useEffect(() => {
        loadFriends();
    }, []);

    return(
        <div className="friends-page">
            <header className="page-title">
                <h1>Your Circle</h1>
                <p>Friends, requests, and people to discover</p>
            </header>

            <div className="page-container">
                <div className="left-column">
                    <MyFriends friends={friends} message={message} setFriends={setFriends} setMessage={setMessage} loadFriends={loadFriends}/>
                </div>

                <div className="right-column">
                    <DiscoverNewFriends />
                </div>
            </div>
        </div>
    );
}

export default FriendList;