using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using PulseChatClient.Services;

namespace PulseChatClient.Forms
{
    public class LoginForm : Form
    {
        private ChatService _chatService;

        // UI Controls
        private Panel pnlMain;
        private Label lblTitle, lblSubtitle;
        private TextBox txtUsername, txtPassword, txtServerUrl;
        private Button btnLogin, btnSignUp;
        private Label lblStatus;
        private Label lblServerLabel, lblUsernameLabel, lblPasswordLabel;

        // Colors
        private readonly Color BgDark = Color.FromArgb(15, 15, 22);
        private readonly Color BgCard = Color.FromArgb(24, 24, 34);
        private readonly Color AccentPurple = Color.FromArgb(138, 96, 255);
        private readonly Color AccentBlue = Color.FromArgb(78, 160, 255);
        private readonly Color TextWhite = Color.FromArgb(235, 235, 242);
        private readonly Color TextGray = Color.FromArgb(130, 130, 155);
        private readonly Color InputBg = Color.FromArgb(35, 35, 48);
        private readonly Color InputBorder = Color.FromArgb(55, 55, 72);
        private readonly Color ErrorRed = Color.FromArgb(255, 82, 82);
        private readonly Color SuccessGreen = Color.FromArgb(76, 217, 100);

        public LoginForm()
        {
            _chatService = new ChatService();
            BuildUI();
        }

        private void BuildUI()
        {
            this.SuspendLayout();
            this.Text = "PulseChat — Login";
            this.ClientSize = new Size(420, 580);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.Font = new Font("Segoe UI", 10F);

            // Main card
            pnlMain = new Panel { Size = new Size(360, 500), Location = new Point(30, 40), BackColor = BgCard };
            pnlMain.Paint += (s, e) =>
            {
                using (var pen = new Pen(InputBorder))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = new Rectangle(0, 0, pnlMain.Width - 1, pnlMain.Height - 1);
                    using (var path = RoundedRect(r, 12))
                        e.Graphics.DrawPath(pen, path);
                }
            };

            lblTitle = MakeLabel("⚡ PulseChat", 22, FontStyle.Bold, AccentPurple, 20, 25, 320, 45, ContentAlignment.MiddleCenter);
            lblSubtitle = MakeLabel("Real-Time Messaging System", 10, FontStyle.Regular, TextGray, 20, 70, 320, 25, ContentAlignment.MiddleCenter);

            lblServerLabel = MakeLabel("SERVER URL", 8, FontStyle.Bold, TextGray, 30, 118);
            txtServerUrl = MakeTextBox(30, 138, 300);
            txtServerUrl.Text = "http://localhost:5000";

            lblUsernameLabel = MakeLabel("USERNAME", 8, FontStyle.Bold, TextGray, 30, 186);
            txtUsername = MakeTextBox(30, 206, 300);

            lblPasswordLabel = MakeLabel("PASSWORD", 8, FontStyle.Bold, TextGray, 30, 254);
            txtPassword = MakeTextBox(30, 274, 300);
            txtPassword.UseSystemPasswordChar = true;

            btnLogin = MakeButton("LOGIN", 30, 332, 300, 42, AccentPurple);
            btnLogin.Click += async (s, e) => await HandleLogin();

            btnSignUp = MakeButton("SIGN UP", 30, 384, 300, 42, AccentBlue);
            btnSignUp.Click += async (s, e) => await HandleSignUp();

            lblStatus = MakeLabel("", 9, FontStyle.Regular, TextGray, 30, 440, 300, 40, ContentAlignment.MiddleCenter);

            pnlMain.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle,
                lblServerLabel, txtServerUrl,
                lblUsernameLabel, txtUsername,
                lblPasswordLabel, txtPassword,
                btnLogin, btnSignUp, lblStatus
            });

            this.Controls.Add(pnlMain);
            this.AcceptButton = btnLogin;
            this.ResumeLayout(false);
        }

        private async Task HandleLogin()
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;
            string url = txtServerUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(user)) { SetStatus("Enter your username", ErrorRed); return; }
            if (string.IsNullOrWhiteSpace(pass)) { SetStatus("Enter your password", ErrorRed); return; }

            SetButtons(false);
            SetStatus("Connecting...", TextGray);

            try
            {
                if (!_chatService.IsConnected)
                    await _chatService.ConnectAsync(url);

                SetStatus("Authenticating...", TextGray);
                bool ok = await _chatService.LoginAsync(user, pass);

                if (ok)
                {
                    SetStatus("Login successful!", SuccessGreen);
                    await Task.Delay(400);

                    var chatForm = new ChatForm(_chatService, user);
                    chatForm.FormClosed += (s2, e2) =>
                    {
                        _chatService.Disconnect();
                        _chatService = new ChatService();
                        this.Show();
                        SetButtons(true);
                        SetStatus("", TextGray);
                    };
                    this.Hide();
                    chatForm.Show();
                }
                else
                {
                    SetStatus("Invalid credentials or user already online", ErrorRed);
                    SetButtons(true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Connection failed: {ex.Message}", ErrorRed);
                SetButtons(true);
            }
        }

        private async Task HandleSignUp()
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;
            string url = txtServerUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || user.Length < 3) { SetStatus("Username: at least 3 chars", ErrorRed); return; }
            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 4) { SetStatus("Password: at least 4 chars", ErrorRed); return; }

            SetButtons(false);
            SetStatus("Connecting...", TextGray);

            try
            {
                if (!_chatService.IsConnected)
                    await _chatService.ConnectAsync(url);

                SetStatus("Creating account...", TextGray);
                bool ok = await _chatService.RegisterAsync(user, pass);

                SetStatus(ok ? "Account created! You can now login." : "Username already exists", ok ? SuccessGreen : ErrorRed);
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", ErrorRed);
            }
            SetButtons(true);
        }

        private void SetStatus(string msg, Color c)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetStatus(msg, c))); return; }
            lblStatus.Text = msg;
            lblStatus.ForeColor = c;
        }

        private void SetButtons(bool enabled)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetButtons(enabled))); return; }
            btnLogin.Enabled = enabled;
            btnSignUp.Enabled = enabled;
        }

        // ==================== HELPERS ====================

        private Label MakeLabel(string text, float size, FontStyle style, Color color, int x, int y,
                                int w = 0, int h = 0, ContentAlignment align = ContentAlignment.TopLeft)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
                TextAlign = align
            };
            if (w > 0) { lbl.AutoSize = false; lbl.Size = new Size(w, h > 0 ? h : 20); }
            else lbl.AutoSize = true;
            return lbl;
        }

        private TextBox MakeTextBox(int x, int y, int w)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 34),
                BackColor = InputBg,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button MakeButton(string text, int x, int y, int w, int h, Color bg)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _chatService?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}
