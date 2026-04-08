# ⚡ PulseChat — Real-Time Desktop Messaging System

A **real-time desktop messaging system** built with C#, WinForms, SignalR, and SQL Server LocalDB.

## 🏗️ Architecture

```
[Client 1 (WinForms)]  ───┐
                           ├──► [SERVER (Console App + SignalR OWIN)]  ──► SQL Server LocalDB (.mdf)
[Client 2 (WinForms)]  ───┘                                               ├── Users
                                                                           ├── Messages
                                                                           ├── Groups
                                                                           └── GroupMembers
```

- **Server**: Console application using OWIN Self-Host + SignalR for real-time communication
- **Client**: WinForms application with dark-themed UI and SignalR Client
- **Database**: SQL Server LocalDB (`.mdf` auto-created on first run)
- **File Storage**: Images → `C:\PulseChat\Images\`, Documents → `C:\PulseChat\Files\`

## ✨ Features

| Feature | Description |
|---------|-------------|
| 💬 Private Chat | 1-on-1 private messaging between users |
| 👥 Group Chat | Create, join, browse, and leave groups |
| 🎨 Group Colors | Each group has a unique accent color |
| 📷 Inline Images | WhatsApp-style image thumbnails in chat |
| 📎 File Sharing | Send any file type (PDF, ZIP, DOCX, etc.) with download |
| 🔔 Unread Indicators | Orange highlight on conversations with new messages |
| 🔒 Auth | Signup/login with SHA-256 password hashing |
| 📜 Message History | Persistent message history per channel |
| 🟢 Online Status | Real-time online/offline user presence |

## 📋 Prerequisites

- **Windows 10/11**
- **Visual Studio 2022** (Community or higher) with:
  - ✅ `.NET desktop development` workload
  - ✅ `SQL Server Express LocalDB` (included in VS installer)
- **.NET Framework 4.8.1**

## 🚀 Quick Setup

### Option 1: Batch Script (Recommended)
```bash
git clone https://github.com/Jaiminsinh-Dodiya/CSharp-PulseChat.git
cd CSharp-PulseChat
setup.bat
```
This will automatically:
1. Download NuGet CLI (if not found)
2. Restore all NuGet packages
3. Build both Server and Client projects

### Option 2: Visual Studio
1. Open `PulseChat.sln` in Visual Studio 2022
2. Right-click Solution → **Restore NuGet Packages**
3. Build Solution (`Ctrl+Shift+B`)

## ▶️ How to Run

### Start Server
```
run-server.bat
```
Or set `PulseChatServer` as startup project → `F5`

Server shows:
```
╔══════════════════════════════════════╗
║          P U L S E C H A T          ║
║       Real-Time Messaging Server     ║
╚══════════════════════════════════════╝
[SERVER STARTED] http://localhost:5000
```

### Start Client(s)
```
run-client.bat          ← run multiple times for multiple users
```
Or double-click `PulseChatClient\bin\Debug\PulseChatClient.exe`

### Demo Flow
1. **Sign Up** → Create User1 and User2
2. **Login** → Login as User1 in Client 1, User2 in Client 2
3. **Group Chat** → Both auto-join `#General`, messages appear instantly
4. **Private Chat** → Click a user in "Direct Messages" sidebar
5. **Create Group** → `+ Create` button → new group with unique color
6. **Send Image** → `📎` button → select image → appears inline
7. **Send File** → `📎` button → select any document → click `⬇ Download` to save

## 📁 Project Structure

```
PulseChat/
├── PulseChat.sln                    # Visual Studio Solution
├── setup.bat                        # Auto-setup script
├── run-server.bat                   # Server launcher
├── run-client.bat                   # Client launcher
├── .gitignore
│
├── PulseChatServer/                 # Console App (SignalR OWIN Server)
│   ├── Program.cs                   # Entry point + ASCII banner
│   ├── Startup.cs                   # OWIN/SignalR configuration
│   ├── Hubs/ChatHub.cs              # SignalR Hub (all messaging logic)
│   ├── Data/DatabaseManager.cs      # SQL Server LocalDB operations
│   ├── Models/
│   │   ├── User.cs
│   │   ├── ChatMessage.cs
│   │   └── Group.cs
│   └── Utils/
│       ├── Logger.cs                # Color-coded console logging
│       └── ImageStorage.cs          # Image + File storage
│
└── PulseChatClient/                 # WinForms App (SignalR Client)
    ├── Program.cs                   # Entry point
    ├── Forms/
    │   ├── LoginForm.cs             # Login/Signup screen
    │   ├── ChatForm.cs              # Main chat UI (sidebar + messages)
    │   ├── CreateGroupDialog.cs     # Create group popup
    │   └── JoinGroupDialog.cs       # Browse & join groups popup
    └── Services/
        └── ChatService.cs           # SignalR client wrapper + events
```

## 🛠️ Tech Stack

| Component | Technology |
|-----------|-----------|
| Server | C# Console App, OWIN Self-Host |
| Real-Time | ASP.NET SignalR 2.4.3 |
| Client UI | WinForms (.NET Framework 4.8.1) |
| Database | SQL Server LocalDB (.mdf) |
| Auth | SHA-256 password hashing |

## 📄 License

This project is for educational purposes.

## 👤 Author

**Jaiminsinh Dodiya**
- GitHub: [@Jaiminsinh-Dodiya](https://github.com/Jaiminsinh-Dodiya)
