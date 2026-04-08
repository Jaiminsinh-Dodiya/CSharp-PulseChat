using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace PulseChatClient.Services
{
    public class ChatService
    {
        private HubConnection _connection;
        private IHubProxy _hub;

        // ==================== EVENTS ====================

        // Group messages: groupId, groupName, groupColor, sender, content, msgType, imagePath, timestamp
        public event Action<int, string, string, string, string, string, string, string> OnGroupMessage;

        // Private messages: sender, targetUser, content, msgType, imagePath, timestamp
        public event Action<string, string, string, string, string, string> OnPrivateMessage;

        // User events
        public event Action<string> OnUserJoined;
        public event Action<string> OnUserLeft;
        public event Action<List<string>> OnUserListUpdated;

        // Group events: groupId, name, color, createdBy
        public event Action<int, string, string, string> OnGroupCreated;
        public event Action<int, string> OnMemberJoined;  // groupId, username
        public event Action<int, string> OnMemberLeft;    // groupId, username

        // System
        public event Action<string> OnError;
        public event Action<string> OnConnectionChanged;

        public bool IsConnected { get; private set; }

        // ==================== CONNECTION ====================

        public async Task ConnectAsync(string serverUrl)
        {
            _connection = new HubConnection(serverUrl.TrimEnd('/'));
            _hub = _connection.CreateHubProxy("ChatHub");

            // --- Group messages (7 params max for SignalR 2 Client) ---
            // groupInfo = "name|color", sender, content, msgType, imagePath, timestamp, groupId
            _hub.On<string, string, string, string, string, string, int>(
                "receiveGroupMessage",
                (groupInfo, sender, content, msgType, imgPath, ts, gId) =>
                {
                    string[] parts = groupInfo.Split('|');
                    string gName = parts.Length > 0 ? parts[0] : "";
                    string gColor = parts.Length > 1 ? parts[1] : "#8A60FF";
                    OnGroupMessage?.Invoke(gId, gName, gColor, sender, content, msgType, imgPath, ts);
                });

            // --- Private messages ---
            _hub.On<string, string, string, string, string, string>(
                "receivePrivateMessage",
                (sender, target, content, msgType, imgPath, ts) =>
                    OnPrivateMessage?.Invoke(sender, target, content, msgType, imgPath, ts));

            // --- User presence ---
            _hub.On<string>("userJoined", u => OnUserJoined?.Invoke(u));
            _hub.On<string>("userLeft", u => OnUserLeft?.Invoke(u));
            _hub.On<List<string>>("updateUserList", l => OnUserListUpdated?.Invoke(l));

            // --- Group lifecycle ---
            _hub.On<int, string, string, string>("groupCreated",
                (id, name, color, by) => OnGroupCreated?.Invoke(id, name, color, by));
            _hub.On<int, string>("memberJoined", (gId, u) => OnMemberJoined?.Invoke(gId, u));
            _hub.On<int, string>("memberLeft", (gId, u) => OnMemberLeft?.Invoke(gId, u));

            // --- Errors ---
            _hub.On<string>("receiveError", msg => OnError?.Invoke(msg));

            // --- Connection lifecycle ---
            _connection.Reconnecting += () => { IsConnected = false; OnConnectionChanged?.Invoke("Reconnecting"); };
            _connection.Reconnected += () => { IsConnected = true; OnConnectionChanged?.Invoke("Connected"); };
            _connection.Closed += () => { IsConnected = false; OnConnectionChanged?.Invoke("Disconnected"); };
            _connection.Error += ex => OnError?.Invoke($"Connection error: {ex.Message}");

            await _connection.Start(new Microsoft.AspNet.SignalR.Client.Transports.ServerSentEventsTransport());
            IsConnected = true;
            OnConnectionChanged?.Invoke("Connected");
        }

        // ==================== AUTH ====================

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try { return await _hub.Invoke<bool>("Register", username, password); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); return false; }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try { return await _hub.Invoke<bool>("Login", username, password); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); return false; }
        }

        // ==================== GROUP MANAGEMENT ====================

        public async Task<GroupData> CreateGroupAsync(string name)
        {
            try { return await _hub.Invoke<GroupData>("CreateGroup", name); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); return null; }
        }

        public async Task<bool> JoinGroupAsync(int groupId)
        {
            try { return await _hub.Invoke<bool>("JoinGroup", groupId); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); return false; }
        }

        public async Task<bool> LeaveGroupAsync(int groupId)
        {
            try { return await _hub.Invoke<bool>("LeaveGroup", groupId); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); return false; }
        }

        public async Task<List<GroupData>> GetMyGroupsAsync()
        {
            try { return await _hub.Invoke<List<GroupData>>("GetMyGroups"); }
            catch { return new List<GroupData>(); }
        }

        public async Task<List<GroupData>> GetAllGroupsAsync()
        {
            try { return await _hub.Invoke<List<GroupData>>("GetAllGroups"); }
            catch { return new List<GroupData>(); }
        }

        public async Task<List<string>> GetGroupMembersAsync(int groupId)
        {
            try { return await _hub.Invoke<List<string>>("GetGroupMembers", groupId); }
            catch { return new List<string>(); }
        }

        // ==================== GROUP MESSAGING ====================

        public async Task SendGroupMessageAsync(int groupId, string message)
        {
            try { await _hub.Invoke("SendGroupMessage", groupId, message); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        public async Task SendGroupImageAsync(int groupId, byte[] data, string ext)
        {
            try { await _hub.Invoke("SendGroupImage", groupId, data, ext); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        // ==================== PRIVATE MESSAGING ====================

        public async Task SendPrivateMessageAsync(string targetUser, string message)
        {
            try { await _hub.Invoke("SendPrivateMessage", targetUser, message); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        public async Task SendPrivateImageAsync(string targetUser, byte[] data, string ext)
        {
            try { await _hub.Invoke("SendPrivateImage", targetUser, data, ext); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        // ==================== FILE SHARING ====================

        public async Task SendGroupFileAsync(int groupId, byte[] data, string fileName)
        {
            try { await _hub.Invoke("SendGroupFile", groupId, data, fileName); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        public async Task SendPrivateFileAsync(string targetUser, byte[] data, string fileName)
        {
            try { await _hub.Invoke("SendPrivateFile", targetUser, data, fileName); }
            catch (Exception ex) { OnError?.Invoke(ex.Message); }
        }

        // ==================== HISTORY ====================

        public async Task<List<MessageData>> GetGroupHistoryAsync(int groupId)
        {
            try { return await _hub.Invoke<List<MessageData>>("GetGroupHistory", groupId); }
            catch { return new List<MessageData>(); }
        }

        public async Task<List<MessageData>> GetPrivateHistoryAsync(string otherUser)
        {
            try { return await _hub.Invoke<List<MessageData>>("GetPrivateHistory", otherUser); }
            catch { return new List<MessageData>(); }
        }

        // ==================== IMAGE ====================

        public async Task<byte[]> GetImageAsync(string imagePath)
        {
            try { return await _hub.Invoke<byte[]>("GetImage", imagePath); }
            catch { return null; }
        }

        // ==================== ONLINE USERS ====================

        public async Task<List<string>> GetOnlineUsersAsync()
        {
            try { return await _hub.Invoke<List<string>>("GetOnlineUsers"); }
            catch { return new List<string>(); }
        }

        // ==================== DISCONNECT ====================

        public void Disconnect()
        {
            if (_connection != null)
            {
                _connection.Stop();
                _connection.Dispose();
                IsConnected = false;
            }
        }
    }

    // ==================== DTOs ====================

    public class GroupData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
        public string CreatedBy { get; set; }
        public int MemberCount { get; set; }
    }

    public class MessageData
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string ImagePath { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
