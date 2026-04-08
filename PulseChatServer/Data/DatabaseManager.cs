using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PulseChatServer.Models;
using PulseChatServer.Utils;

namespace PulseChatServer.Data
{
    public static class DatabaseManager
    {
        private static string _connectionString;
        private static readonly string DbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private static readonly string DbFileName = "PulseChat.mdf";

        // Group color palette — each new group gets the next color
        private static readonly string[] ColorPalette = {
            "#8A60FF", "#4EA0FF", "#FF6B6B", "#4CD964", "#FFD60A",
            "#FF9F43", "#A855F7", "#06B6D4", "#EC4899", "#10B981"
        };

        // ==================== INITIALIZATION ====================

        public static void Initialize()
        {
            try
            {
                if (!Directory.Exists(DbFolder))
                    Directory.CreateDirectory(DbFolder);

                string mdfPath = Path.Combine(DbFolder, DbFileName);
                _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={mdfPath};Integrated Security=True;Connect Timeout=30;";

                if (!File.Exists(mdfPath))
                {
                    CreateDatabase(mdfPath);
                    Logger.Info("Database created: PulseChat.mdf");
                }
                else
                {
                    Logger.Info("Database found: PulseChat.mdf");
                }

                CreateTables();
                Logger.Info("Database tables verified");

                EnsureDefaultGroup();
            }
            catch (Exception ex)
            {
                Logger.Error($"Database init failed: {ex.Message}");
                throw;
            }
        }

        private static void CreateDatabase(string mdfPath)
        {
            string masterConn = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;";
            using (var conn = new SqlConnection(masterConn))
            {
                conn.Open();
                string dbName = "PulseChatDB_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                string sql = $@"CREATE DATABASE [{dbName}] ON PRIMARY (
                    NAME = PulseChatData, FILENAME = '{mdfPath}',
                    SIZE = 10MB, MAXSIZE = 200MB, FILEGROWTH = 5MB)";
                using (var cmd = new SqlCommand(sql, conn)) { cmd.ExecuteNonQuery(); }

                string detach = $"EXEC sp_detach_db @dbname = '{dbName}'";
                using (var cmd = new SqlCommand(detach, conn)) { cmd.ExecuteNonQuery(); }
            }
        }

        private static void CreateTables()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Users')
                    CREATE TABLE Users (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Username NVARCHAR(50) NOT NULL UNIQUE,
                        PasswordHash NVARCHAR(256) NOT NULL,
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )");

                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Groups')
                    CREATE TABLE Groups (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(50) NOT NULL UNIQUE,
                        ColorHex NVARCHAR(10) NOT NULL,
                        CreatedBy NVARCHAR(50) NOT NULL,
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )");

                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='GroupMembers')
                    CREATE TABLE GroupMembers (
                        GroupId INT NOT NULL,
                        Username NVARCHAR(50) NOT NULL,
                        JoinedAt DATETIME DEFAULT GETDATE(),
                        PRIMARY KEY (GroupId, Username)
                    )");

                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Messages')
                    CREATE TABLE Messages (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Sender NVARCHAR(50) NOT NULL,
                        Content NVARCHAR(MAX),
                        MessageType NVARCHAR(10) NOT NULL DEFAULT 'text',
                        ImagePath NVARCHAR(500),
                        ChannelType NVARCHAR(10) NOT NULL DEFAULT 'group',
                        ChannelId NVARCHAR(100) NOT NULL DEFAULT '1',
                        Timestamp DATETIME DEFAULT GETDATE()
                    )");

                // Migration: add new columns if upgrading from old schema (v1)
                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('Messages') AND name='ChannelType')
                    ALTER TABLE Messages ADD ChannelType NVARCHAR(10) NOT NULL DEFAULT 'group'");
                Execute(conn, @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('Messages') AND name='ChannelId')
                    ALTER TABLE Messages ADD ChannelId NVARCHAR(100) NOT NULL DEFAULT '1'");
            }
        }

        private static void EnsureDefaultGroup()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string check = "SELECT COUNT(*) FROM Groups WHERE Name = 'General'";
                using (var cmd = new SqlCommand(check, conn))
                {
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string insert = "INSERT INTO Groups (Name, ColorHex, CreatedBy) VALUES ('General', '#8A60FF', 'System')";
                        using (var cmd2 = new SqlCommand(insert, conn))
                        {
                            cmd2.ExecuteNonQuery();
                        }
                        Logger.Info("Default 'General' group created");
                    }
                }
            }
        }

        // ==================== USER METHODS ====================

        public static bool CreateUser(string username, string password)
        {
            try
            {
                string hash = HashPassword(password);
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    Execute(conn, "INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)",
                        new SqlParameter("@u", username), new SqlParameter("@p", hash));
                }
                Logger.Auth($"New user registered: {username}");
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                Logger.Auth($"Registration failed (duplicate): {username}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Registration error: {ex.Message}");
                return false;
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            try
            {
                string hash = HashPassword(password);
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Users WHERE Username=@u AND PasswordHash=@p";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", hash);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Login validation error: {ex.Message}");
                return false;
            }
        }

        // ==================== GROUP METHODS ====================

        public static Group CreateGroup(string name, string createdBy)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Pick color
                    string countSql = "SELECT COUNT(*) FROM Groups";
                    int groupCount;
                    using (var cmd = new SqlCommand(countSql, conn))
                        groupCount = (int)cmd.ExecuteScalar();

                    string color = ColorPalette[groupCount % ColorPalette.Length];

                    string sql = "INSERT INTO Groups (Name, ColorHex, CreatedBy) OUTPUT INSERTED.Id VALUES (@n, @c, @cb)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@c", color);
                        cmd.Parameters.AddWithValue("@cb", createdBy);
                        int id = (int)cmd.ExecuteScalar();

                        // Auto-add creator
                        AddGroupMember(conn, id, createdBy);

                        Logger.Info($"Group created: '{name}' (#{color}) by {createdBy}");
                        return new Group { Id = id, Name = name, ColorHex = color, CreatedBy = createdBy, MemberCount = 1 };
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                Logger.Error($"Group name already exists: {name}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Create group error: {ex.Message}");
                return null;
            }
        }

        public static List<Group> GetAllGroups()
        {
            var groups = new List<Group>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT g.Id, g.Name, g.ColorHex, g.CreatedBy, g.CreatedAt,
                               (SELECT COUNT(*) FROM GroupMembers WHERE GroupId=g.Id) AS MemberCount
                               FROM Groups g ORDER BY g.Id";
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        groups.Add(new Group
                        {
                            Id = r.GetInt32(0), Name = r.GetString(1), ColorHex = r.GetString(2),
                            CreatedBy = r.GetString(3), CreatedAt = r.GetDateTime(4), MemberCount = r.GetInt32(5)
                        });
                    }
                }
            }
            return groups;
        }

        public static List<Group> GetUserGroups(string username)
        {
            var groups = new List<Group>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT g.Id, g.Name, g.ColorHex, g.CreatedBy, g.CreatedAt,
                               (SELECT COUNT(*) FROM GroupMembers WHERE GroupId=g.Id) AS MemberCount
                               FROM Groups g INNER JOIN GroupMembers gm ON g.Id=gm.GroupId
                               WHERE gm.Username=@u ORDER BY g.Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            groups.Add(new Group
                            {
                                Id = r.GetInt32(0), Name = r.GetString(1), ColorHex = r.GetString(2),
                                CreatedBy = r.GetString(3), CreatedAt = r.GetDateTime(4), MemberCount = r.GetInt32(5)
                            });
                        }
                    }
                }
            }
            return groups;
        }

        public static bool JoinGroup(int groupId, string username)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    AddGroupMember(conn, groupId, username);
                }
                Logger.Info($"User {username} joined group #{groupId}");
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                return false; // Already a member
            }
            catch (Exception ex)
            {
                Logger.Error($"Join group error: {ex.Message}");
                return false;
            }
        }

        public static bool LeaveGroup(int groupId, string username)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM GroupMembers WHERE GroupId=@g AND Username=@u";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@g", groupId);
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.ExecuteNonQuery();
                    }
                }
                Logger.Info($"User {username} left group #{groupId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Leave group error: {ex.Message}");
                return false;
            }
        }

        public static List<string> GetGroupMembers(int groupId)
        {
            var members = new List<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT Username FROM GroupMembers WHERE GroupId=@g ORDER BY JoinedAt";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@g", groupId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) members.Add(r.GetString(0));
                    }
                }
            }
            return members;
        }

        public static Group GetGroupById(int groupId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT g.Id, g.Name, g.ColorHex, g.CreatedBy, g.CreatedAt,
                               (SELECT COUNT(*) FROM GroupMembers WHERE GroupId=g.Id) AS MemberCount
                               FROM Groups g WHERE g.Id=@id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", groupId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new Group
                            {
                                Id = r.GetInt32(0), Name = r.GetString(1), ColorHex = r.GetString(2),
                                CreatedBy = r.GetString(3), CreatedAt = r.GetDateTime(4), MemberCount = r.GetInt32(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        private static void AddGroupMember(SqlConnection conn, int groupId, string username)
        {
            string sql = "INSERT INTO GroupMembers (GroupId, Username) VALUES (@g, @u)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@g", groupId);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.ExecuteNonQuery();
            }
        }

        // ==================== MESSAGE METHODS ====================

        public static void SaveMessage(string sender, string content, string messageType, string imagePath,
                                       string channelType, string channelId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO Messages (Sender, Content, MessageType, ImagePath, ChannelType, ChannelId) 
                                   VALUES (@s, @c, @mt, @ip, @ct, @ci)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", sender);
                        cmd.Parameters.AddWithValue("@c", (object)content ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@mt", messageType);
                        cmd.Parameters.AddWithValue("@ip", (object)imagePath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ct", channelType);
                        cmd.Parameters.AddWithValue("@ci", channelId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Save message error: {ex.Message}");
            }
        }

        public static List<ChatMessage> GetGroupMessages(int groupId, int count = 50)
        {
            return GetMessages("group", groupId.ToString(), count);
        }

        public static List<ChatMessage> GetPrivateMessages(string user1, string user2, int count = 50)
        {
            string channelId = GetPrivateChannelId(user1, user2);
            return GetMessages("private", channelId, count);
        }

        private static List<ChatMessage> GetMessages(string channelType, string channelId, int count)
        {
            var messages = new List<ChatMessage>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = $@"SELECT TOP {count} Id, Sender, Content, MessageType, ImagePath, Timestamp
                                FROM Messages WHERE ChannelType=@ct AND ChannelId=@ci
                                ORDER BY Timestamp DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ct", channelType);
                    cmd.Parameters.AddWithValue("@ci", channelId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            messages.Add(new ChatMessage
                            {
                                Id = r.GetInt32(0),
                                Sender = r.GetString(1),
                                Content = r.IsDBNull(2) ? null : r.GetString(2),
                                MessageType = r.GetString(3),
                                ImagePath = r.IsDBNull(4) ? null : r.GetString(4),
                                Timestamp = r.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            messages.Reverse();
            return messages;
        }

        /// <summary>
        /// Gets a consistent private channel ID for two users (alphabetically sorted).
        /// </summary>
        public static string GetPrivateChannelId(string user1, string user2)
        {
            return string.Compare(user1, user2, StringComparison.OrdinalIgnoreCase) < 0
                ? $"{user1}:{user2}"
                : $"{user2}:{user1}";
        }

        // ==================== HELPERS ====================

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static void Execute(SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
