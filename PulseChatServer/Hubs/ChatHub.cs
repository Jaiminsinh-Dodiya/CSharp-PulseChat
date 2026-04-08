using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using PulseChatServer.Data;
using PulseChatServer.Models;
using PulseChatServer.Utils;

namespace PulseChatServer.Hubs
{
    public class ChatHub : Hub
    {
        // ConnectionId → Username
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers
            = new ConcurrentDictionary<string, string>();

        private const int MaxImageSize = 5 * 1024 * 1024;

        // ==================== AUTHENTICATION ====================

        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;
            if (username.Length < 3 || username.Length > 20) return false;
            if (password.Length < 4) return false;
            return DatabaseManager.CreateUser(username.Trim(), password);
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

            username = username.Trim();

            if (ConnectedUsers.Values.Contains(username))
            {
                Logger.Auth($"Login rejected (already online): {username}");
                return false;
            }

            if (!DatabaseManager.ValidateUser(username, password))
            {
                Logger.Auth($"Login failed: {username}");
                return false;
            }

            ConnectedUsers[Context.ConnectionId] = username;
            Logger.Connection($"User connected: {username}");

            // Auto-join General group (ID=1) if not already a member
            DatabaseManager.JoinGroup(1, username);

            // Join all SignalR groups the user belongs to
            var userGroups = DatabaseManager.GetUserGroups(username);
            foreach (var g in userGroups)
            {
                Groups.Add(Context.ConnectionId, $"group_{g.Id}");
            }

            // Notify everyone
            Clients.All.userJoined(username);
            Clients.Caller.updateUserList(ConnectedUsers.Values.ToList());

            return true;
        }

        // ==================== GROUP MANAGEMENT ====================

        public Group CreateGroup(string groupName)
        {
            string username = GetCaller();
            if (username == null) return null;

            if (string.IsNullOrWhiteSpace(groupName) || groupName.Length < 2 || groupName.Length > 30)
                return null;

            var group = DatabaseManager.CreateGroup(groupName.Trim(), username);
            if (group == null) return null;

            // Join SignalR group
            Groups.Add(Context.ConnectionId, $"group_{group.Id}");

            // Notify all clients about the new group
            Clients.All.groupCreated(group.Id, group.Name, group.ColorHex, group.CreatedBy);

            return group;
        }

        public bool JoinGroup(int groupId)
        {
            string username = GetCaller();
            if (username == null) return false;

            bool result = DatabaseManager.JoinGroup(groupId, username);
            if (!result) return false;

            // Join SignalR group
            Groups.Add(Context.ConnectionId, $"group_{groupId}");

            var group = DatabaseManager.GetGroupById(groupId);
            if (group != null)
            {
                // Notify group members
                Clients.Group($"group_{groupId}").memberJoined(groupId, username);
                Logger.Info($"{username} joined group '{group.Name}'");
            }

            return true;
        }

        public bool LeaveGroup(int groupId)
        {
            string username = GetCaller();
            if (username == null) return false;

            // Prevent leaving General
            if (groupId == 1) return false;

            bool result = DatabaseManager.LeaveGroup(groupId, username);
            if (!result) return false;

            Groups.Remove(Context.ConnectionId, $"group_{groupId}");

            Clients.Group($"group_{groupId}").memberLeft(groupId, username);
            Logger.Info($"{username} left group #{groupId}");

            return true;
        }

        public List<Group> GetMyGroups()
        {
            string username = GetCaller();
            if (username == null) return new List<Group>();
            return DatabaseManager.GetUserGroups(username);
        }

        public List<Group> GetAllGroups()
        {
            string username = GetCaller();
            if (username == null) return new List<Group>();
            return DatabaseManager.GetAllGroups();
        }

        public List<string> GetGroupMembers(int groupId)
        {
            string username = GetCaller();
            if (username == null) return new List<string>();
            return DatabaseManager.GetGroupMembers(groupId);
        }

        // ==================== GROUP MESSAGING ====================

        public void SendGroupMessage(int groupId, string message)
        {
            string username = GetCaller();
            if (username == null || string.IsNullOrWhiteSpace(message)) return;

            // Verify membership
            var members = DatabaseManager.GetGroupMembers(groupId);
            if (!members.Contains(username)) return;

            var group = DatabaseManager.GetGroupById(groupId);
            if (group == null) return;

            DatabaseManager.SaveMessage(username, message, "text", null, "group", groupId.ToString());

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string groupInfo = $"{group.Name}|{group.ColorHex}";
            Clients.Group($"group_{groupId}").receiveGroupMessage(
                groupInfo, username, message, "text", (string)null, timestamp, groupId);

            Logger.Message($"[{group.Name}] {username}: {(message.Length > 40 ? message.Substring(0, 40) + "..." : message)}");
        }

        public void SendGroupImage(int groupId, byte[] imageData, string extension)
        {
            string username = GetCaller();
            if (username == null || imageData == null || imageData.Length == 0) return;

            var members = DatabaseManager.GetGroupMembers(groupId);
            if (!members.Contains(username)) return;

            if (imageData.Length > MaxImageSize)
            {
                Clients.Caller.receiveError("Image too large. Max 5 MB.");
                return;
            }

            string ext = ValidateImageExtension(extension);
            if (ext == null) return;

            string imagePath = ImageStorage.SaveImage(imageData, ext);
            var group = DatabaseManager.GetGroupById(groupId);
            if (group == null) return;

            DatabaseManager.SaveMessage(username, "[Image]", "image", imagePath, "group", groupId.ToString());

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string groupInfo = $"{group.Name}|{group.ColorHex}";
            Clients.Group($"group_{groupId}").receiveGroupMessage(
                groupInfo, username, "[Image]", "image", imagePath, timestamp, groupId);

            Logger.Image($"[{group.Name}] {username} sent image");
        }

        // ==================== PRIVATE MESSAGING ====================

        public void SendPrivateMessage(string targetUser, string message)
        {
            string sender = GetCaller();
            if (sender == null || string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(targetUser)) return;

            targetUser = targetUser.Trim();
            string channelId = DatabaseManager.GetPrivateChannelId(sender, targetUser);

            DatabaseManager.SaveMessage(sender, message, "text", null, "private", channelId);

            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            // Send to sender (echo)
            Clients.Caller.receivePrivateMessage(sender, targetUser, message, "text", null, timestamp);

            // Send to target (if online)
            var targetEntry = ConnectedUsers.FirstOrDefault(x => x.Value == targetUser);
            if (targetEntry.Key != null)
            {
                Clients.Client(targetEntry.Key).receivePrivateMessage(sender, targetUser, message, "text", null, timestamp);
            }

            Logger.Message($"[DM] {sender} → {targetUser}: {(message.Length > 40 ? message.Substring(0, 40) + "..." : message)}");
        }

        public void SendPrivateImage(string targetUser, byte[] imageData, string extension)
        {
            string sender = GetCaller();
            if (sender == null || imageData == null || imageData.Length == 0) return;

            if (imageData.Length > MaxImageSize)
            {
                Clients.Caller.receiveError("Image too large. Max 5 MB.");
                return;
            }

            string ext = ValidateImageExtension(extension);
            if (ext == null) return;

            targetUser = targetUser.Trim();
            string imagePath = ImageStorage.SaveImage(imageData, ext);
            string channelId = DatabaseManager.GetPrivateChannelId(sender, targetUser);

            DatabaseManager.SaveMessage(sender, "[Image]", "image", imagePath, "private", channelId);

            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            Clients.Caller.receivePrivateMessage(sender, targetUser, "[Image]", "image", imagePath, timestamp);

            var targetEntry = ConnectedUsers.FirstOrDefault(x => x.Value == targetUser);
            if (targetEntry.Key != null)
            {
                Clients.Client(targetEntry.Key).receivePrivateMessage(sender, targetUser, "[Image]", "image", imagePath, timestamp);
            }

            Logger.Image($"[DM] {sender} → {targetUser} sent image");
        }

        // ==================== FILE SHARING ====================

        private const int MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public void SendGroupFile(int groupId, byte[] fileData, string fileName)
        {
            string username = GetCaller();
            if (username == null || fileData == null || fileData.Length == 0 || string.IsNullOrWhiteSpace(fileName)) return;

            var members = DatabaseManager.GetGroupMembers(groupId);
            if (!members.Contains(username)) return;

            if (fileData.Length > MaxFileSize)
            {
                Clients.Caller.receiveError("File too large. Max 10 MB.");
                return;
            }

            string filePath = ImageStorage.SaveFile(fileData, fileName);
            var group = DatabaseManager.GetGroupById(groupId);
            if (group == null) return;

            DatabaseManager.SaveMessage(username, fileName, "file", filePath, "group", groupId.ToString());

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string groupInfo = $"{group.Name}|{group.ColorHex}";
            Clients.Group($"group_{groupId}").receiveGroupMessage(
                groupInfo, username, fileName, "file", filePath, timestamp, groupId);

            Logger.Info($"[{group.Name}] {username} sent file: {fileName}");
        }

        public void SendPrivateFile(string targetUser, byte[] fileData, string fileName)
        {
            string sender = GetCaller();
            if (sender == null || fileData == null || fileData.Length == 0 || string.IsNullOrWhiteSpace(fileName)) return;

            if (fileData.Length > MaxFileSize)
            {
                Clients.Caller.receiveError("File too large. Max 10 MB.");
                return;
            }

            targetUser = targetUser.Trim();
            string filePath = ImageStorage.SaveFile(fileData, fileName);
            string channelId = DatabaseManager.GetPrivateChannelId(sender, targetUser);

            DatabaseManager.SaveMessage(sender, fileName, "file", filePath, "private", channelId);

            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            Clients.Caller.receivePrivateMessage(sender, targetUser, fileName, "file", filePath, timestamp);

            var targetEntry = ConnectedUsers.FirstOrDefault(x => x.Value == targetUser);
            if (targetEntry.Key != null)
            {
                Clients.Client(targetEntry.Key).receivePrivateMessage(sender, targetUser, fileName, "file", filePath, timestamp);
            }

            Logger.Info($"[DM] {sender} → {targetUser} sent file: {fileName}");
        }

        // ==================== HISTORY ====================

        public List<ChatMessage> GetGroupHistory(int groupId)
        {
            string username = GetCaller();
            if (username == null) return new List<ChatMessage>();

            var messages = DatabaseManager.GetGroupMessages(groupId, 50);
            Logger.Info($"Group #{groupId} history sent to {username} ({messages.Count} msgs)");
            return messages;
        }

        public List<ChatMessage> GetPrivateHistory(string otherUser)
        {
            string username = GetCaller();
            if (username == null) return new List<ChatMessage>();

            var messages = DatabaseManager.GetPrivateMessages(username, otherUser, 50);
            Logger.Info($"DM history ({username} ↔ {otherUser}) sent ({messages.Count} msgs)");
            return messages;
        }

        // ==================== IMAGE DOWNLOAD ====================

        public byte[] GetImage(string imagePath)
        {
            if (GetCaller() == null) return null;
            return ImageStorage.GetImageBytes(imagePath);
        }

        // ==================== ONLINE USERS ====================

        public List<string> GetOnlineUsers()
        {
            return ConnectedUsers.Values.ToList();
        }

        // ==================== CONNECTION LIFECYCLE ====================

        public override Task OnDisconnected(bool stopCalled)
        {
            string username;
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out username))
            {
                Logger.Disconnect($"User disconnected: {username}");
                Clients.All.userLeft(username);
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string username;
            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out username))
            {
                Logger.Connection($"User reconnected: {username}");

                // Re-join SignalR groups
                var userGroups = DatabaseManager.GetUserGroups(username);
                foreach (var g in userGroups)
                {
                    Groups.Add(Context.ConnectionId, $"group_{g.Id}");
                }
            }
            return base.OnReconnected();
        }

        // ==================== HELPERS ====================

        private string GetCaller()
        {
            string username;
            ConnectedUsers.TryGetValue(Context.ConnectionId, out username);
            return username;
        }

        private string ValidateImageExtension(string extension)
        {
            string ext = (extension ?? ".png").ToLower();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp")
            {
                Clients.Caller.receiveError("Invalid image format.");
                return null;
            }
            return ext;
        }
    }
}
