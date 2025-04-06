# 🧾 Discount Code Server (.NET + WebSockets)

This project is a C# WebSocket-based server designed to **generate**, **store**, and **consume discount codes** with support for **parallel requests** and **persistent storage** via a simple `.db` file.

---

## 📦 Features

- ✅ Generate unique discount codes (7–8 chars)
- ✅ Mark codes as used
- ✅ Persist data across restarts
- ✅ Handle multiple clients concurrently
- ✅ Communicate over WebSockets (no REST)
- ✅ React client or CLI tester supported

---

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- Optional: [Node.js](https://nodejs.org/) if you're using the React client

---

### 📁 Folder Structure

```
DiscountSolution/
├── DiscountServer/          # WebSocket server (.NET)
├── DiscountClient/          # React frontend (optional)
├── appsettings.json         # Server config
└── README.md
```

---

### 🔧 Configuration

Your WebSocket server URL is configured in `appsettings.json`:

```json
{
  "WebSocketSettings": {
    "Uri": "http://localhost:5000/ws/"
  }
}
```

> You can also override the URI using environment variables or command-line args if needed.

---

### ▶️ Running the Server

1. **From terminal:**

```bash
dotnet run --project DiscountServer
```

2. Or via Visual Studio → Right-click `DiscountServer` → **Set as Startup Project** → Run

---

## 🧪 WebSocket Interface

The server accepts JSON payloads over WebSocket (`ws://localhost:5000/ws/`):

### 📤 Generate Request

```json
{ "Count": 5, "Length": 8 }
```

Response:
```json
{ "Codes": ["ABC12345", "XYZ67890"] }
```

---

### 📤 Use Code Request

```json
{ "Code": "ABC12345" }
```

Response:
```json
{ "Result": 1 } // success
{ "Result": 0 } // already used or invalid
```

---

### 📤 Get All Codes

```json
{ "GetAll": true }
```

Response:
```json
{
  "AllCodes": [
    { "Code": "ABC12345", "Used": false },
    { "Code": "XYZ67890", "Used": true }
  ]
}
```

---

## 🔐 Data Persistence

Codes are stored in a local `discounts.db` text file:

- Format: `CODE|1` for used, `CODE|0` for unused
- Automatically saved and reloaded on server start
- Safe for multi-client use with locking

---

## 🧪 Load Testing (Optional)

Use the `LoadTest` project to simulate multiple clients generating/using codes concurrently.

```bash
dotnet run --project LoadTest
```

---

## 🌐 React Client (Optional)

The `DiscountClient` is a simple WebSocket UI in React for testing.

```bash
cd DiscountClient
npm install
npm start
```
---

## 🧠 Credits

Built with ❤️ using .NET 8 and WebSockets.

---

## 📜 License

This project is open-source. Feel free to modify and improve it for your needs.
