# Real-Time Messaging System Architecture & Flow

This document outlines the architecture, data flow, and usage of the Real-Time Messaging System built into the TravelTourManagement platform. The system facilitates direct communication between **Travelers (Users)** and **Packagers** using a hybrid REST + WebSocket (SignalR) architecture.

---

## 1. Core Architecture

The messaging system relies on two primary technologies:
1. **PostgreSQL / Entity Framework Core**: For persistent storage of chat histories and threads.
2. **SignalR (WebSockets)**: For real-time, low-latency delivery of messages.

### Database Entities
- **`MessageThread`**: Represents a private "chat room" between exactly one Traveler and one Packager.
  - Generates a unique `ThreadId` (UUID).
  - Tracks `LastMessageAt` to sort active conversations on the dashboard.
- **`Message`**: Represents an individual text message within a thread.
  - Contains `SenderRole` (User or Packager) so the UI can render chat bubbles on the left or right correctly.
  - Contains `IsRead` flag to drive notification badges.

---

## 2. Real-Time Routing Strategy (WhatsApp-Style)

Instead of forcing users to explicitly "subscribe" to specific thread rooms, the system uses **JWT Identity-Based Routing**.

1. When a user connects to `ws://localhost:<port>/hubs/chat`, SignalR automatically extracts their `UserId` from the JWT token.
2. When a message is sent in a thread, the backend looks up the `UserId` of the Traveler and the `UserId` of the Packager involved in that thread.
3. The server pushes the message payload directly to those two `UserId`s globally.
4. **Benefit**: The user receives messages for *any* of their active threads instantly, without needing to explicitly join individual socket rooms.

---

## 3. Step-by-Step Data Flow

### A. Initializing a Chat
When a Traveler wants to contact a Packager (e.g., clicking "Contact Packager" on a tour page):
- **Action**: `POST /api/Messages/threads/init`
- **Payload**: `{ "packagerId": "<uuid>" }`
- **Result**: The backend checks if a thread already exists between these two users. If not, it creates one. It returns the `MessageThreadDto` containing the `ThreadId`.

### B. Sending a Message
When either party types a message and clicks send:
- **Action**: `POST /api/Messages/send`
- **Payload**:
  ```json
  {
    "threadId": "<uuid>",
    "senderRole": 0, // 0 for User, 1 for Packager
    "body": "Hello, I have a question!"
  }
  ```
- **Backend Flow**:
  1. Validates the user is actually a participant in `ThreadId`.
  2. Saves the `Message` to PostgreSQL.
  3. Updates the `MessageThread.LastMessageAt` timestamp.
  4. Triggers `IMessageDispatcher.DispatchMessageAsync(...)`.
  5. SignalR fires the `ReceiveMessage` event to the WebSockets of the Traveler and Packager.

### C. Receiving a Message (Frontend)
The frontend establishes a persistent connection to the SignalR hub:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5081/hubs/chat", { accessTokenFactory: () => token })
    .build();

connection.on("ReceiveMessage", (message) => {
    // 1. If the user is currently looking at this ThreadId, append the chat bubble.
    // 2. If they are looking elsewhere, increment the unread notification badge!
});
```

### D. Fetching Offline History
If a user was offline when a message arrived, the next time they open the thread:
- **Action**: `GET /api/Messages/threads/{threadId}/messages`
- **Result**: Returns an array of all historical messages in chronological order.

### E. Marking Messages as Read
When the user scrolls through the chat window, the UI clears the unread badge:
- **Action**: `PUT /api/Messages/threads/{threadId}/read?readerRole=0`
- **Result**: The database marks all pending messages sent by the *other* party as `IsRead = true`.

---

## 4. Security & Authorization

- **JWT Enforcement**: The `/hubs/chat` endpoint rejects any WebSocket handshake that does not contain a valid JWT token.
- **Thread Isolation**: The REST endpoints (`GetThreadMessages`, `SendMessage`) strictly enforce that the `UserId` derived from the JWT matches either the Traveler or the Packager assigned to the requested `ThreadId`. It is mathematically impossible for a user to query or intercept messages from a thread they do not belong to.
