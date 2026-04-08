using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using PulseChatClient.Services;

namespace PulseChatClient.Forms
{
    public partial class LoginForm : Form
    {
        private ChatService _chatService;

        public LoginForm()
        {
            _chatService = new ChatService();
            InitializeComponent();
            WireUpPaintEvents();
        }

        // ==================== COSMETIC PAINT EVENTS ====================

        private void WireUpPaintEvents()
        {
            pnlMain.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(55, 55, 72), 1))
                {
                    var rect = new Rectangle(0, 0, pnlMain.Width - 1, pnlMain.Height - 1);
                    g.DrawPath(pen, RoundedRect(rect, 12));
                }
            };
        }

        // ==================== BUTTON EVENT HANDLERS ====================

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await HandleLogin();
        }

        private async void btnSignUp_Click(object sender, EventArgs e)
        {
            await HandleSignUp();
        }

        // ==================== LOGIN / SIGNUP LOGIC ====================

        private async System.Threading.Tasks.Task HandleLogin()
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string server = txtServerUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                SetStatus("Enter username and password.", Color.FromArgb(255, 82, 82));
                return;
            }

            SetButtons(false);
            SetStatus("Connecting...", Color.FromArgb(255, 204, 0));

            try
            {
                await _chatService.ConnectAsync(server);

                bool ok = await _chatService.LoginAsync(user, pass);
                if (ok)
                {
                    SetStatus("Login successful!", Color.FromArgb(76, 217, 100));
                    var chatForm = new ChatForm(_chatService, user);
                    chatForm.FormClosed += (s, args) => this.Close(); // ← when chat closes, login closes too
                    chatForm.Show();
                    this.Hide();   // still hide it (don't show login behind chat).
                    // When ChatForm closes → triggers LoginForm.Close() → triggers Application.Exit()
                }
                else
                {
                    SetStatus("Invalid credentials.", Color.FromArgb(255, 82, 82));
                    SetButtons(true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", Color.FromArgb(255, 82, 82));
                SetButtons(true);
            }
        }

        private async System.Threading.Tasks.Task HandleSignUp()
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string server = txtServerUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                SetStatus("Enter a username and password.", Color.FromArgb(255, 82, 82));
                return;
            }

            if (user.Length < 3 || pass.Length < 4)
            {
                SetStatus("Username ≥ 3 chars, Password ≥ 4 chars.", Color.FromArgb(255, 82, 82));
                return;
            }

            SetButtons(false);
            SetStatus("Creating account...", Color.FromArgb(255, 204, 0));

            try
            {
                using (var connection = new Microsoft.AspNet.SignalR.Client.HubConnection(server))
                {
                    var hub = connection.CreateHubProxy("ChatHub");
                    await connection.Start(new Microsoft.AspNet.SignalR.Client.Transports.ServerSentEventsTransport());

                    bool result = await hub.Invoke<bool>("Register", user, pass);

                    connection.Stop();

                    if (result)
                    {
                        SetStatus("Account created! Now login.", Color.FromArgb(76, 217, 100));
                    }
                    else
                    {
                        SetStatus("Username already taken.", Color.FromArgb(255, 82, 82));
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"SignUp error: {ex.Message}", Color.FromArgb(255, 82, 82));
            }

            SetButtons(true);
        }

        // ==================== HELPERS ====================

        private void SetStatus(string text, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = text;
        }

        private void SetButtons(bool enabled)
        {
            btnLogin.Enabled = enabled;
            btnSignUp.Enabled = enabled;
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _chatService?.Disconnect();  // ← in case user closes login before chatting
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}
