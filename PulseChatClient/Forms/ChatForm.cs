using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PulseChatClient.Services;

namespace PulseChatClient.Forms
{
    public class ChatForm : Form
    {
        private ChatService _chatService;
        private string _username;

        // Active conversation state
        private string _activeChatType = "group";   // "group" or "private"
        private string _activeChatId = "1";          // group ID or username
        private string _activeChatName = "General";
        private string _activeChatColor = "#8A60FF";

        // Data caches
        private List<GroupData> _myGroups = new List<GroupData>();
        private List<string> _onlineUsers = new List<string>();
        private HashSet<string> _unreadChats = new HashSet<string>();
        private Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        // (startPos, endPos, serverPath, originalFileName) for clickable download links
        private List<Tuple<int, int, string, string>> _fileLinks = new List<Tuple<int, int, string, string>>();
        private bool _isRefreshing = false;

        // ==================== UI CONTROLS ====================
        private Panel pnlHeader, pnlSidebar, pnlChatHeader, pnlInput;
        private RichTextBox rtbChat;
        private TextBox txtMessage;
        private Button btnSend, btnAttach;
        private ListBox lstGroups, lstUsers;
        private Label lblLogo, lblStatus, lblUser;
        private Label lblGroupsTitle, lblDirectTitle;
        private Label lblChatName, lblChatInfo;
        private Panel pnlStatusDot, pnlChatDot;
        private Button btnCreateGroup, btnBrowseGroups;

        // ==================== COLORS ====================
        private readonly Color BgDarkest = Color.FromArgb(15, 15, 22);
        private readonly Color BgSidebar = Color.FromArgb(20, 20, 30);
        private readonly Color BgChat = Color.FromArgb(28, 28, 38);
        private readonly Color BgHeader = Color.FromArgb(18, 18, 26);
        private readonly Color BgInput = Color.FromArgb(35, 35, 48);
        private readonly Color BgHover = Color.FromArgb(42, 42, 58);
        private readonly Color BgSelected = Color.FromArgb(50, 40, 75);
        private readonly Color BorderColor = Color.FromArgb(48, 48, 65);
        private readonly Color AccentPurple = Color.FromArgb(138, 96, 255);
        private readonly Color AccentBlue = Color.FromArgb(78, 160, 255);
        private readonly Color TextPrimary = Color.FromArgb(235, 235, 242);
        private readonly Color TextSecondary = Color.FromArgb(130, 130, 155);
        private readonly Color TextDim = Color.FromArgb(80, 80, 105);
        private readonly Color StatusGreen = Color.FromArgb(76, 217, 100);
        private readonly Color StatusYellow = Color.FromArgb(255, 204, 0);
        private readonly Color StatusRed = Color.FromArgb(255, 82, 82);
        private readonly Color UnreadColor = Color.FromArgb(255, 120, 80);

        // ==================== CONSTRUCTORS ====================

        /// <summary>Designer-safe constructor</summary>
        public ChatForm()
        {
            _username = "Designer";
            _chatService = null;
            BuildUI();
        }

        public ChatForm(ChatService chatService, string username)
        {
            _chatService = chatService;
            _username = username;
            BuildUI();
            SetupEventHandlers();
            LoadInitialData();
        }

        // ==================== UI CONSTRUCTION ====================

        private void BuildUI()
        {
            this.SuspendLayout();
            this.Text = $"PulseChat — {_username}";
            this.ClientSize = new Size(960, 660);
            this.MinimumSize = new Size(800, 520);
            this.BackColor = BgDarkest;
            this.Font = new Font("Segoe UI", 10F);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            BuildHeader();
            BuildSidebar();
            BuildChatHeader();
            BuildInputArea();
            BuildChatArea();

            // Dock order matters: add Fill last
            this.Controls.Add(rtbChat);
            this.Controls.Add(pnlChatHeader);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = BgHeader };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderColor))
                    e.Graphics.DrawLine(pen, 0, 49, pnlHeader.Width, 49);
            };

            lblLogo = new Label
            {
                Text = "⚡ PulseChat",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = AccentPurple,
                Location = new Point(14, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            pnlStatusDot = new Panel
            {
                Size = new Size(10, 10),
                BackColor = StatusGreen
            };
            pnlStatusDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(pnlStatusDot.BackColor))
                    e.Graphics.FillEllipse(b, 0, 0, 9, 9);
            };

            lblStatus = new Label
            {
                Text = "Connected",
                Font = new Font("Segoe UI", 8F),
                ForeColor = StatusGreen,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblUser = new Label
            {
                Text = _username,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.AddRange(new Control[] { lblLogo, pnlStatusDot, lblStatus, lblUser });

            // Position on resize
            pnlHeader.Resize += (s, e) =>
            {
                lblUser.Location = new Point(pnlHeader.Width - lblUser.Width - 18, 16);
                pnlStatusDot.Location = new Point(pnlHeader.Width - lblUser.Width - lblStatus.Width - 48, 21);
                lblStatus.Location = new Point(pnlHeader.Width - lblUser.Width - lblStatus.Width - 34, 17);
            };
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = BgSidebar };
            pnlSidebar.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderColor))
                    e.Graphics.DrawLine(pen, 229, 0, 229, pnlSidebar.Height);
            };

            // --- GROUPS SECTION ---
            lblGroupsTitle = new Label
            {
                Text = "GROUPS",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(14, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lstGroups = new ListBox
            {
                Location = new Point(6, 28),
                Size = new Size(218, 180),
                BackColor = BgSidebar,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 36,
                Font = new Font("Segoe UI", 9.5F)
            };
            lstGroups.DrawItem += LstGroups_DrawItem;
            lstGroups.SelectedIndexChanged += LstGroups_Selected;

            // Group buttons
            btnCreateGroup = MakeSmallButton("+ Create", 6, 212, 106, 28, AccentPurple);
            btnCreateGroup.Click += async (s, e) => await HandleCreateGroup();

            btnBrowseGroups = MakeSmallButton("Browse", 118, 212, 106, 28, AccentBlue);
            btnBrowseGroups.Click += async (s, e) => await HandleBrowseGroups();

            // --- Separator ---
            var sep = new Panel
            {
                Location = new Point(14, 248),
                Size = new Size(202, 1),
                BackColor = BorderColor
            };

            // --- DIRECT MESSAGES SECTION ---
            lblDirectTitle = new Label
            {
                Text = "DIRECT MESSAGES",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(14, 258),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lstUsers = new ListBox
            {
                Location = new Point(6, 278),
                BackColor = BgSidebar,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 34,
                Font = new Font("Segoe UI", 9.5F)
            };
            lstUsers.DrawItem += LstUsers_DrawItem;
            lstUsers.SelectedIndexChanged += LstUsers_Selected;

            pnlSidebar.Controls.AddRange(new Control[] {
                lblGroupsTitle, lstGroups, btnCreateGroup, btnBrowseGroups,
                sep, lblDirectTitle, lstUsers
            });

            // Resize users list to fill remaining space
            pnlSidebar.Resize += (s, e) =>
            {
                lstUsers.Size = new Size(218, pnlSidebar.Height - 288);
            };
        }

        private void BuildChatHeader()
        {
            pnlChatHeader = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = BgHeader };
            pnlChatHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderColor))
                    e.Graphics.DrawLine(pen, 0, 47, pnlChatHeader.Width, 47);
            };

            pnlChatDot = new Panel { Size = new Size(12, 12), Location = new Point(12, 12), BackColor = HexToColor(_activeChatColor) };
            pnlChatDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(pnlChatDot.BackColor))
                    e.Graphics.FillEllipse(b, 0, 0, 11, 11);
            };

            lblChatName = new Label
            {
                Text = "# General",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(30, 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblChatInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextSecondary,
                Location = new Point(30, 28),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            pnlChatHeader.Controls.AddRange(new Control[] { pnlChatDot, lblChatName, lblChatInfo });
        }

        private void BuildInputArea()
        {
            pnlInput = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = BgHeader, Padding = new Padding(8) };
            pnlInput.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderColor))
                    e.Graphics.DrawLine(pen, 0, 0, pnlInput.Width, 0);
            };

            btnAttach = new Button
            {
                Text = "📎",
                Size = new Size(42, 42),
                Dock = DockStyle.Left,
                FlatStyle = FlatStyle.Flat,
                BackColor = BgInput,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 14F),
                Cursor = Cursors.Hand
            };
            btnAttach.FlatAppearance.BorderColor = BorderColor;
            btnAttach.FlatAppearance.BorderSize = 1;
            btnAttach.Click += async (s, e) => await HandleAttachment();

            txtMessage = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgInput,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtMessage.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await HandleSendMessage();
                }
            };

            btnSend = new Button
            {
                Text = "SEND ➤",
                Size = new Size(90, 42),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentPurple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += async (s, e) => await HandleSendMessage();

            var spacerL = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = Color.Transparent };
            var spacerR = new Panel { Dock = DockStyle.Right, Width = 6, BackColor = Color.Transparent };

            pnlInput.Controls.Add(txtMessage);
            pnlInput.Controls.Add(spacerL);
            pnlInput.Controls.Add(btnAttach);
            pnlInput.Controls.Add(spacerR);
            pnlInput.Controls.Add(btnSend);
        }

        private void BuildChatArea()
        {
            rtbChat = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgChat,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            rtbChat.MouseClick += RtbChat_MouseClick;
        }

        // ==================== EVENT HANDLERS ====================

        private void SetupEventHandlers()
        {
            if (_chatService == null) return;

            _chatService.OnGroupMessage += (gId, gName, gColor, sender, content, msgType, imgPath, ts) =>
            {
                RunOnUI(async () =>
                {
                    if (_activeChatType == "group" && _activeChatId == gId.ToString())
                    {
                        await AppendMessageAsync(sender, content, msgType, imgPath, ts, HexToColor(gColor));
                    }
                    else
                    {
                        _unreadChats.Add($"group:{gId}");
                        RefreshGroupsList();
                    }
                });
            };

            _chatService.OnPrivateMessage += (sender, target, content, msgType, imgPath, ts) =>
            {
                RunOnUI(async () =>
                {
                    string otherUser = sender == _username ? target : sender;
                    if (_activeChatType == "private" && _activeChatId == otherUser)
                    {
                        await AppendMessageAsync(sender, content, msgType, imgPath, ts, AccentBlue);
                    }
                    else
                    {
                        _unreadChats.Add($"private:{otherUser}");
                        RefreshUsersList();
                    }
                });
            };

            _chatService.OnUserJoined += username =>
            {
                RunOnUI(() =>
                {
                    if (!_onlineUsers.Contains(username))
                        _onlineUsers.Add(username);
                    RefreshUsersList();
                });
            };

            _chatService.OnUserLeft += username =>
            {
                RunOnUI(() =>
                {
                    _onlineUsers.Remove(username);
                    RefreshUsersList();
                });
            };

            _chatService.OnUserListUpdated += users =>
            {
                RunOnUI(() =>
                {
                    _onlineUsers = users;
                    RefreshUsersList();
                });
            };

            _chatService.OnGroupCreated += (id, name, color, createdBy) =>
            {
                RunOnUI(async () =>
                {
                    _myGroups = await _chatService.GetMyGroupsAsync();
                    RefreshGroupsList();
                });
            };

            _chatService.OnMemberJoined += (gId, username) =>
            {
                RunOnUI(async () =>
                {
                    if (_activeChatType == "group" && _activeChatId == gId.ToString())
                    {
                        AppendSystemMessage($"🟢 {username} joined this group");
                        await UpdateChatInfo();
                    }
                });
            };

            _chatService.OnMemberLeft += (gId, username) =>
            {
                RunOnUI(async () =>
                {
                    if (_activeChatType == "group" && _activeChatId == gId.ToString())
                    {
                        AppendSystemMessage($"🔴 {username} left this group");
                        await UpdateChatInfo();
                    }
                });
            };

            _chatService.OnConnectionChanged += status =>
            {
                RunOnUI(() => UpdateConnectionStatus(status));
            };

            _chatService.OnError += msg =>
            {
                RunOnUI(() => AppendSystemMessage($"⚠️ {msg}"));
            };
        }

        // ==================== INITIAL DATA LOAD ====================

        private async void LoadInitialData()
        {
            if (_chatService == null) return;
            try
            {
                _myGroups = await _chatService.GetMyGroupsAsync();
                _onlineUsers = await _chatService.GetOnlineUsersAsync();

                RefreshGroupsList();
                RefreshUsersList();

                // Default: open General group
                await SwitchToGroup(1, "General", "#8A60FF");
            }
            catch (Exception ex)
            {
                AppendSystemMessage($"Failed to load: {ex.Message}");
            }
        }

        // ==================== CHAT SWITCHING ====================

        private async Task SwitchToGroup(int groupId, string name, string colorHex)
        {
            _activeChatType = "group";
            _activeChatId = groupId.ToString();
            _activeChatName = name;
            _activeChatColor = colorHex;

            Color c = HexToColor(colorHex);
            lblChatName.Text = $"# {name}";
            lblChatName.ForeColor = c;
            pnlChatDot.BackColor = c;
            pnlChatDot.Invalidate();

            _unreadChats.Remove($"group:{groupId}");
            RefreshGroupsList();

            ClearChat();
            await UpdateChatInfo();

            try
            {
                var history = await _chatService.GetGroupHistoryAsync(groupId);
                if (history.Count > 0)
                {
                    foreach (var m in history)
                        await AppendMessageAsync(m.Sender, m.Content, m.MessageType, m.ImagePath,
                            m.Timestamp.ToString("HH:mm"), c);
                }
            }
            catch { }

            txtMessage.Focus();
        }

        private async Task SwitchToPrivateChat(string otherUser)
        {
            _activeChatType = "private";
            _activeChatId = otherUser;
            _activeChatName = otherUser;
            _activeChatColor = "#4EA0FF";

            lblChatName.Text = $"💬 {otherUser}";
            lblChatName.ForeColor = AccentBlue;
            pnlChatDot.BackColor = _onlineUsers.Contains(otherUser) ? StatusGreen : TextDim;
            pnlChatDot.Invalidate();

            _unreadChats.Remove($"private:{otherUser}");
            RefreshUsersList();

            ClearChat();
            lblChatInfo.Text = _onlineUsers.Contains(otherUser) ? "Online" : "Offline";

            try
            {
                var history = await _chatService.GetPrivateHistoryAsync(otherUser);
                if (history.Count > 0)
                {
                    foreach (var m in history)
                        await AppendMessageAsync(m.Sender, m.Content, m.MessageType, m.ImagePath,
                            m.Timestamp.ToString("HH:mm"), AccentBlue);
                }
            }
            catch { }

            txtMessage.Focus();
        }

        private async Task UpdateChatInfo()
        {
            if (_activeChatType == "group")
            {
                try
                {
                    var members = await _chatService.GetGroupMembersAsync(int.Parse(_activeChatId));
                    lblChatInfo.Text = $"{members.Count} member{(members.Count != 1 ? "s" : "")}";
                }
                catch { lblChatInfo.Text = ""; }
            }
        }

        // ==================== MESSAGE DISPLAY ====================

        private static readonly HashSet<string> ImageExtensions = new HashSet<string>
            { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

        private async Task AppendMessageAsync(string sender, string content, string msgType, string imgPath,
                                              string timestamp, Color accentColor)
        {
            bool isSelf = sender == _username;

            // Timestamp
            rtbChat.SelectionColor = TextDim;
            rtbChat.SelectionFont = new Font("Segoe UI", 7.5F);
            rtbChat.AppendText($" [{timestamp}]  ");

            // Sender name
            rtbChat.SelectionColor = isSelf ? AccentPurple : accentColor;
            rtbChat.SelectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rtbChat.AppendText(sender);

            if (msgType == "image")
            {
                // WhatsApp-style: embed image inline
                rtbChat.AppendText(Environment.NewLine);
                await EmbedImageInline(imgPath);
            }
            else if (msgType == "file")
            {
                // Document: show file icon + name + download link
                string fileName = content ?? "unknown";
                string icon = GetFileIcon(fileName);

                rtbChat.SelectionColor = TextSecondary;
                rtbChat.SelectionFont = new Font("Segoe UI", 9.5F);
                rtbChat.AppendText($"  {icon} ");

                rtbChat.SelectionColor = TextPrimary;
                rtbChat.SelectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                rtbChat.AppendText(fileName);
                rtbChat.AppendText("  ");

                rtbChat.SelectionColor = AccentBlue;
                rtbChat.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Underline);
                int start = rtbChat.TextLength;
                rtbChat.AppendText("⬇ Download");
                int end = rtbChat.TextLength;
                _fileLinks.Add(Tuple.Create(start, end, imgPath ?? "", fileName));
            }
            else
            {
                rtbChat.SelectionColor = TextPrimary;
                rtbChat.SelectionFont = new Font("Segoe UI", 10F);
                rtbChat.AppendText($":  {content}");
            }

            rtbChat.AppendText(Environment.NewLine);
            rtbChat.ScrollToCaret();
        }

        private async Task EmbedImageInline(string imgPath)
        {
            if (string.IsNullOrEmpty(imgPath) || _chatService == null) return;

            try
            {
                Image img;
                if (_imageCache.ContainsKey(imgPath))
                {
                    img = _imageCache[imgPath];
                }
                else
                {
                    byte[] data = await _chatService.GetImageAsync(imgPath);
                    if (data == null || data.Length == 0)
                    {
                        rtbChat.SelectionColor = TextDim;
                        rtbChat.AppendText("   [Image unavailable]");
                        return;
                    }
                    using (var ms = new MemoryStream(data))
                    {
                        img = new Bitmap(Image.FromStream(ms));
                    }
                    _imageCache[imgPath] = img;
                }

                // Resize to thumbnail (max 280x200)
                var thumb = ResizeImage(img, 280, 200);

                // Insert 3-space indent
                rtbChat.AppendText("   ");

                // Embed via Clipboard
                Clipboard.SetImage(thumb);
                rtbChat.ReadOnly = false;
                rtbChat.Select(rtbChat.TextLength, 0);
                rtbChat.Paste();
                rtbChat.ReadOnly = true;

                if (thumb != img) thumb.Dispose();
            }
            catch
            {
                rtbChat.SelectionColor = TextDim;
                rtbChat.AppendText("   [Failed to load image]");
            }
        }

        private Image ResizeImage(Image img, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / img.Width;
            double ratioY = (double)maxHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            if (ratio >= 1.0) return new Bitmap(img);

            int newW = (int)(img.Width * ratio);
            int newH = (int)(img.Height * ratio);

            var bmp = new Bitmap(newW, newH);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(img, 0, 0, newW, newH);
            }
            return bmp;
        }

        private string GetFileIcon(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".pdf": return "📕";
                case ".doc": case ".docx": return "📘";
                case ".xls": case ".xlsx": return "📗";
                case ".ppt": case ".pptx": return "📙";
                case ".zip": case ".rar": case ".7z": return "📦";
                case ".txt": return "📄";
                case ".mp3": case ".wav": case ".flac": return "🎵";
                case ".mp4": case ".avi": case ".mkv": return "🎬";
                default: return "📎";
            }
        }

        private void AppendSystemMessage(string text)
        {
            rtbChat.SelectionColor = TextDim;
            rtbChat.SelectionFont = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            rtbChat.AppendText($"     {text}{Environment.NewLine}");
            rtbChat.ScrollToCaret();
        }

        private void ClearChat()
        {
            rtbChat.Clear();
            _fileLinks.Clear();
        }

        // ==================== SIDEBAR REFRESH ====================

        private void RefreshGroupsList()
        {
            _isRefreshing = true;
            try
            {
                lstGroups.Items.Clear();

                foreach (var g in _myGroups)
                {
                    lstGroups.Items.Add(new GroupListItem
                    {
                        Id = g.Id,
                        Name = g.Name,
                        ColorHex = g.ColorHex,
                        MemberCount = g.MemberCount,
                        HasUnread = _unreadChats.Contains($"group:{g.Id}"),
                        IsActive = _activeChatType == "group" && _activeChatId == g.Id.ToString()
                    });
                }
            }
            finally { _isRefreshing = false; }
        }

        private void RefreshUsersList()
        {
            _isRefreshing = true;
            try
            {
                lstUsers.Items.Clear();

                foreach (var user in _onlineUsers.Where(u => u != _username).OrderBy(u => u))
                {
                    lstUsers.Items.Add(new UserListItem
                    {
                        Username = user,
                        IsOnline = true,
                        HasUnread = _unreadChats.Contains($"private:{user}"),
                        IsActive = _activeChatType == "private" && _activeChatId == user
                    });
                }
            }
            finally { _isRefreshing = false; }
        }

        // ==================== SIDEBAR SELECTION ====================

        private async void LstGroups_Selected(object sender, EventArgs e)
        {
            if (_isRefreshing) return;
            if (lstGroups.SelectedItem is GroupListItem item)
            {
                lstUsers.ClearSelected();
                await SwitchToGroup(item.Id, item.Name, item.ColorHex);
            }
        }

        private async void LstUsers_Selected(object sender, EventArgs e)
        {
            if (_isRefreshing) return;
            if (lstUsers.SelectedItem is UserListItem item)
            {
                lstGroups.ClearSelected();
                await SwitchToPrivateChat(item.Username);
            }
        }

        // ==================== SIDEBAR DRAWING ====================

        private void LstGroups_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var item = lstGroups.Items[e.Index] as GroupListItem;
            if (item == null) return;

            // Background
            Color bg = item.IsActive ? BgSelected :
                       (e.State & DrawItemState.Selected) != 0 ? BgHover : BgSidebar;
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            // Left color bar for active item
            if (item.IsActive)
            {
                using (var brush = new SolidBrush(HexToColor(item.ColorHex)))
                    g.FillRectangle(brush, e.Bounds.X, e.Bounds.Y + 4, 3, e.Bounds.Height - 8);
            }

            // Colored dot
            Color dotColor = HexToColor(item.ColorHex);
            using (var brush = new SolidBrush(dotColor))
                g.FillEllipse(brush, e.Bounds.X + 12, e.Bounds.Y + 12, 10, 10);

            // Group name
            Color nameColor = item.HasUnread ? UnreadColor : TextPrimary;
            FontStyle nameStyle = item.HasUnread ? FontStyle.Bold : FontStyle.Regular;
            using (var brush = new SolidBrush(nameColor))
            using (var font = new Font("Segoe UI", 9.5F, nameStyle))
                g.DrawString($"# {item.Name}", font, brush, e.Bounds.X + 28, e.Bounds.Y + 4);

            // Member count
            using (var brush = new SolidBrush(TextDim))
            using (var font = new Font("Segoe UI", 7.5F))
                g.DrawString($"{item.MemberCount}", font, brush, e.Bounds.X + 28, e.Bounds.Y + 20);
        }

        private void LstUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var item = lstUsers.Items[e.Index] as UserListItem;
            if (item == null) return;

            Color bg = item.IsActive ? BgSelected :
                       (e.State & DrawItemState.Selected) != 0 ? BgHover : BgSidebar;
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            if (item.IsActive)
            {
                using (var brush = new SolidBrush(AccentBlue))
                    g.FillRectangle(brush, e.Bounds.X, e.Bounds.Y + 4, 3, e.Bounds.Height - 8);
            }

            // Online dot
            Color dotColor = item.IsOnline ? StatusGreen : TextDim;
            using (var brush = new SolidBrush(dotColor))
                g.FillEllipse(brush, e.Bounds.X + 12, e.Bounds.Y + 11, 8, 8);

            // Username
            Color nameColor = item.HasUnread ? UnreadColor : TextPrimary;
            FontStyle nameStyle = item.HasUnread ? FontStyle.Bold : FontStyle.Regular;
            using (var brush = new SolidBrush(nameColor))
            using (var font = new Font("Segoe UI", 9.5F, nameStyle))
                g.DrawString(item.Username, font, brush, e.Bounds.X + 28, e.Bounds.Y + 7);
        }

        // ==================== SEND HANDLERS ====================

        private async Task HandleSendMessage()
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg) || _chatService == null) return;

            txtMessage.Clear();
            txtMessage.Focus();

            if (_activeChatType == "group")
                await _chatService.SendGroupMessageAsync(int.Parse(_activeChatId), msg);
            else
                await _chatService.SendPrivateMessageAsync(_activeChatId, msg);
        }

        private async Task HandleAttachment()
        {
            if (_chatService == null) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All Files|*.*|Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Documents|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx|Archives|*.zip;*.rar;*.7z";
                ofd.Title = "Select a file to send";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(ofd.FileName);
                        if (data.Length > 10 * 1024 * 1024)
                        {
                            MessageBox.Show("File too large (max 10 MB).", "PulseChat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string ext = Path.GetExtension(ofd.FileName).ToLower();
                        string fileName = Path.GetFileName(ofd.FileName);
                        bool isImage = ImageExtensions.Contains(ext);

                        if (isImage)
                        {
                            if (_activeChatType == "group")
                                await _chatService.SendGroupImageAsync(int.Parse(_activeChatId), data, ext);
                            else
                                await _chatService.SendPrivateImageAsync(_activeChatId, data, ext);
                        }
                        else
                        {
                            if (_activeChatType == "group")
                                await _chatService.SendGroupFileAsync(int.Parse(_activeChatId), data, fileName);
                            else
                                await _chatService.SendPrivateFileAsync(_activeChatId, data, fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ==================== GROUP MANAGEMENT ====================

        private async Task HandleCreateGroup()
        {
            if (_chatService == null) return;

            using (var dlg = new CreateGroupDialog())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.GroupName))
                {
                    var group = await _chatService.CreateGroupAsync(dlg.GroupName);
                    if (group != null)
                    {
                        _myGroups = await _chatService.GetMyGroupsAsync();
                        RefreshGroupsList();
                        await SwitchToGroup(group.Id, group.Name, group.ColorHex);
                    }
                    else
                    {
                        MessageBox.Show("Could not create group. Name may already exist.", "PulseChat",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private async Task HandleBrowseGroups()
        {
            if (_chatService == null) return;

            var allGroups = await _chatService.GetAllGroupsAsync();
            var myGroupIds = new HashSet<int>(_myGroups.Select(g => g.Id));
            var available = allGroups.Where(g => !myGroupIds.Contains(g.Id)).ToList();

            using (var dlg = new JoinGroupDialog(available))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedGroupId > 0)
                {
                    bool ok = await _chatService.JoinGroupAsync(dlg.SelectedGroupId);
                    if (ok)
                    {
                        _myGroups = await _chatService.GetMyGroupsAsync();
                        RefreshGroupsList();

                        var joined = allGroups.FirstOrDefault(g => g.Id == dlg.SelectedGroupId);
                        if (joined != null)
                            await SwitchToGroup(joined.Id, joined.Name, joined.ColorHex);
                    }
                }
            }
        }

        // ==================== FILE DOWNLOAD + IMAGE CLICK ====================

        private async void RtbChat_MouseClick(object sender, MouseEventArgs e)
        {
            int idx = rtbChat.GetCharIndexFromPosition(e.Location);

            // Check file download links
            foreach (var link in _fileLinks)
            {
                if (idx >= link.Item1 && idx <= link.Item2)
                {
                    await DownloadFile(link.Item3, link.Item4);
                    return;
                }
            }
        }

        private async Task DownloadFile(string serverPath, string fileName)
        {
            if (_chatService == null || string.IsNullOrEmpty(serverPath)) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = fileName;
                sfd.Filter = "All Files|*.*";
                sfd.Title = "Save file as...";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = await _chatService.GetImageAsync(serverPath);
                        if (data != null && data.Length > 0)
                        {
                            File.WriteAllBytes(sfd.FileName, data);
                            AppendSystemMessage($"✅ Saved: {Path.GetFileName(sfd.FileName)}");
                        }
                        else
                        {
                            MessageBox.Show("File not found on server.", "PulseChat",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Download failed: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ==================== CONNECTION STATUS ====================

        private void UpdateConnectionStatus(string status)
        {
            switch (status)
            {
                case "Connected":
                    lblStatus.Text = "Connected"; lblStatus.ForeColor = StatusGreen;
                    pnlStatusDot.BackColor = StatusGreen; break;
                case "Reconnecting":
                    lblStatus.Text = "Reconnecting..."; lblStatus.ForeColor = StatusYellow;
                    pnlStatusDot.BackColor = StatusYellow; break;
                case "Disconnected":
                    lblStatus.Text = "Disconnected"; lblStatus.ForeColor = StatusRed;
                    pnlStatusDot.BackColor = StatusRed; break;
            }
            pnlStatusDot.Invalidate();
        }

        // ==================== HELPERS ====================

        private void RunOnUI(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        private Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7) return AccentPurple;
            try
            {
                int r = Convert.ToInt32(hex.Substring(1, 2), 16);
                int g = Convert.ToInt32(hex.Substring(3, 2), 16);
                int b = Convert.ToInt32(hex.Substring(5, 2), 16);
                return Color.FromArgb(r, g, b);
            }
            catch { return AccentPurple; }
        }

        private Button MakeSmallButton(string text, int x, int y, int w, int h, Color bgColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            foreach (var img in _imageCache.Values) img?.Dispose();
            _imageCache.Clear();
            base.OnFormClosing(e);
        }

        // ==================== SIDEBAR ITEM MODELS ====================

        private class GroupListItem
        {
            public int Id;
            public string Name;
            public string ColorHex;
            public int MemberCount;
            public bool HasUnread;
            public bool IsActive;
            public override string ToString() => Name;
        }

        private class UserListItem
        {
            public string Username;
            public bool IsOnline;
            public bool HasUnread;
            public bool IsActive;
            public override string ToString() => Username;
        }
    }
}
