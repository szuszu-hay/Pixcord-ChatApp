import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import Login from './pages/Login';
import Register from "./pages/Register";
import EmailConfirmed from "./pages/EmailConfirmed";
import EmailConfirmedFailed from "./pages/EmailConfirmedFailed";
import FriendList from "./pages/FriendList";
import ChatList from "./pages/ChatList";
import ChatRoom from "./components/ChatRoom";
import Layout from "./components/Layout";
import PageSecurity from "./components/PageSecurity";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Pages */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/email-confirmed" element={<EmailConfirmed />} />
        <Route path="/email-confirmed-failed" element={<EmailConfirmedFailed />} />
        
        <Route element={
          <PageSecurity>
            <Layout />
          </PageSecurity>
        }>
          <Route path="/friend-list" element={<FriendList />} />
          <Route path="/chat-list" element={<ChatList />} />
          <Route path="/chat/:conversationId" element={<ChatRoom />} />
        </Route>
        
      </Routes> 
    </BrowserRouter>
  );
}

export default App;
