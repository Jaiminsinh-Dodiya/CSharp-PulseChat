namespace PulseChatClient.Forms
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlStatusDot = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblGroupsTitle = new System.Windows.Forms.Label();
            this.lstGroups = new System.Windows.Forms.ListBox();
            this.btnCreateGroup = new System.Windows.Forms.Button();
            this.btnBrowseGroups = new System.Windows.Forms.Button();
            this.lblDirectTitle = new System.Windows.Forms.Label();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.pnlChatHeader = new System.Windows.Forms.Panel();
            this.pnlChatDot = new System.Windows.Forms.Panel();
            this.lblChatName = new System.Windows.Forms.Label();
            this.lblChatInfo = new System.Windows.Forms.Label();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.btnAttach = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.pnlHeader.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlChatHeader.SuspendLayout();
            this.pnlInput.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 18, 26);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(960, 50);
            this.pnlHeader.Controls.Add(this.lblLogo);
            this.pnlHeader.Controls.Add(this.pnlStatusDot);
            this.pnlHeader.Controls.Add(this.lblStatus);
            this.pnlHeader.Controls.Add(this.lblUser);
            // 
            // lblLogo
            // 
            this.lblLogo.AutoSize = true;
            this.lblLogo.BackColor = System.Drawing.Color.Transparent;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(138, 96, 255);
            this.lblLogo.Location = new System.Drawing.Point(14, 10);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Text = "PulseChat";
            // 
            // pnlStatusDot
            // 
            this.pnlStatusDot.BackColor = System.Drawing.Color.FromArgb(76, 217, 100);
            this.pnlStatusDot.Location = new System.Drawing.Point(720, 21);
            this.pnlStatusDot.Name = "pnlStatusDot";
            this.pnlStatusDot.Size = new System.Drawing.Size(10, 10);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(76, 217, 100);
            this.lblStatus.Location = new System.Drawing.Point(734, 17);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Connected";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.BackColor = System.Drawing.Color.Transparent;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(130, 130, 155);
            this.lblUser.Location = new System.Drawing.Point(860, 16);
            this.lblUser.Name = "lblUser";
            this.lblUser.Text = "User";
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 50);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(230, 610);
            this.pnlSidebar.Controls.Add(this.lblGroupsTitle);
            this.pnlSidebar.Controls.Add(this.lstGroups);
            this.pnlSidebar.Controls.Add(this.btnCreateGroup);
            this.pnlSidebar.Controls.Add(this.btnBrowseGroups);
            this.pnlSidebar.Controls.Add(this.lblDirectTitle);
            this.pnlSidebar.Controls.Add(this.lstUsers);
            // 
            // lblGroupsTitle
            // 
            this.lblGroupsTitle.AutoSize = true;
            this.lblGroupsTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblGroupsTitle.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblGroupsTitle.ForeColor = System.Drawing.Color.FromArgb(130, 130, 155);
            this.lblGroupsTitle.Location = new System.Drawing.Point(14, 8);
            this.lblGroupsTitle.Name = "lblGroupsTitle";
            this.lblGroupsTitle.Text = "GROUPS";
            // 
            // lstGroups
            // 
            this.lstGroups.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.lstGroups.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstGroups.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lstGroups.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.lstGroups.ItemHeight = 36;
            this.lstGroups.Location = new System.Drawing.Point(6, 28);
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.Size = new System.Drawing.Size(218, 180);
            this.lstGroups.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LstGroups_DrawItem);
            this.lstGroups.SelectedIndexChanged += new System.EventHandler(this.LstGroups_Selected);
            // 
            // btnCreateGroup
            // 
            this.btnCreateGroup.BackColor = System.Drawing.Color.FromArgb(138, 96, 255);
            this.btnCreateGroup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateGroup.FlatAppearance.BorderSize = 0;
            this.btnCreateGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateGroup.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnCreateGroup.ForeColor = System.Drawing.Color.White;
            this.btnCreateGroup.Location = new System.Drawing.Point(6, 212);
            this.btnCreateGroup.Name = "btnCreateGroup";
            this.btnCreateGroup.Size = new System.Drawing.Size(106, 28);
            this.btnCreateGroup.Text = "+ Create";
            this.btnCreateGroup.Click += new System.EventHandler(this.btnCreateGroup_Click);
            // 
            // btnBrowseGroups
            // 
            this.btnBrowseGroups.BackColor = System.Drawing.Color.FromArgb(78, 160, 255);
            this.btnBrowseGroups.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseGroups.FlatAppearance.BorderSize = 0;
            this.btnBrowseGroups.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseGroups.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnBrowseGroups.ForeColor = System.Drawing.Color.White;
            this.btnBrowseGroups.Location = new System.Drawing.Point(118, 212);
            this.btnBrowseGroups.Name = "btnBrowseGroups";
            this.btnBrowseGroups.Size = new System.Drawing.Size(106, 28);
            this.btnBrowseGroups.Text = "Browse";
            this.btnBrowseGroups.Click += new System.EventHandler(this.btnBrowseGroups_Click);
            // 
            // lblDirectTitle
            // 
            this.lblDirectTitle.AutoSize = true;
            this.lblDirectTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblDirectTitle.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblDirectTitle.ForeColor = System.Drawing.Color.FromArgb(130, 130, 155);
            this.lblDirectTitle.Location = new System.Drawing.Point(14, 258);
            this.lblDirectTitle.Name = "lblDirectTitle";
            this.lblDirectTitle.Text = "DIRECT MESSAGES";
            // 
            // lstUsers
            // 
            this.lstUsers.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.lstUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstUsers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstUsers.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lstUsers.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.lstUsers.ItemHeight = 34;
            this.lstUsers.Location = new System.Drawing.Point(6, 278);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(218, 300);
            this.lstUsers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LstUsers_DrawItem);
            this.lstUsers.SelectedIndexChanged += new System.EventHandler(this.LstUsers_Selected);
            // 
            // pnlChatHeader
            // 
            this.pnlChatHeader.BackColor = System.Drawing.Color.FromArgb(18, 18, 26);
            this.pnlChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlChatHeader.Location = new System.Drawing.Point(230, 50);
            this.pnlChatHeader.Name = "pnlChatHeader";
            this.pnlChatHeader.Size = new System.Drawing.Size(730, 48);
            this.pnlChatHeader.Controls.Add(this.pnlChatDot);
            this.pnlChatHeader.Controls.Add(this.lblChatName);
            this.pnlChatHeader.Controls.Add(this.lblChatInfo);
            // 
            // pnlChatDot
            // 
            this.pnlChatDot.BackColor = System.Drawing.Color.FromArgb(138, 96, 255);
            this.pnlChatDot.Location = new System.Drawing.Point(12, 12);
            this.pnlChatDot.Name = "pnlChatDot";
            this.pnlChatDot.Size = new System.Drawing.Size(12, 12);
            // 
            // lblChatName
            // 
            this.lblChatName.AutoSize = true;
            this.lblChatName.BackColor = System.Drawing.Color.Transparent;
            this.lblChatName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblChatName.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.lblChatName.Location = new System.Drawing.Point(30, 6);
            this.lblChatName.Name = "lblChatName";
            this.lblChatName.Text = "# General";
            // 
            // lblChatInfo
            // 
            this.lblChatInfo.AutoSize = true;
            this.lblChatInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblChatInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblChatInfo.ForeColor = System.Drawing.Color.FromArgb(130, 130, 155);
            this.lblChatInfo.Location = new System.Drawing.Point(30, 28);
            this.lblChatInfo.Name = "lblChatInfo";
            this.lblChatInfo.Text = "";
            // 
            // pnlInput
            // 
            this.pnlInput.BackColor = System.Drawing.Color.FromArgb(18, 18, 26);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Location = new System.Drawing.Point(230, 602);
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.Padding = new System.Windows.Forms.Padding(8);
            this.pnlInput.Size = new System.Drawing.Size(730, 58);
            this.pnlInput.Controls.Add(this.txtMessage);
            this.pnlInput.Controls.Add(this.btnAttach);
            this.pnlInput.Controls.Add(this.btnSend);
            // 
            // btnAttach
            // 
            this.btnAttach.BackColor = System.Drawing.Color.FromArgb(35, 35, 48);
            this.btnAttach.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAttach.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAttach.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(48, 48, 65);
            this.btnAttach.FlatAppearance.BorderSize = 1;
            this.btnAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttach.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnAttach.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(42, 42);
            this.btnAttach.Text = "+";
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(35, 35, 48);
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(560, 27);
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(138, 96, 255);
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(90, 42);
            this.btnSend.Text = "SEND";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // rtbChat
            // 
            this.rtbChat.BackColor = System.Drawing.Color.FromArgb(28, 28, 38);
            this.rtbChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbChat.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rtbChat.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbChat.Size = new System.Drawing.Size(730, 504);
            this.rtbChat.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RtbChat_MouseClick);
            // 
            // ChatForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(15, 15, 22);
            this.ClientSize = new System.Drawing.Size(960, 660);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.pnlChatHeader);
            this.Controls.Add(this.pnlInput);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(800, 520);
            this.Name = "ChatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PulseChat";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlChatHeader.ResumeLayout(false);
            this.pnlChatHeader.PerformLayout();
            this.pnlInput.ResumeLayout(false);
            this.pnlInput.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Panel pnlStatusDot;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblGroupsTitle;
        private System.Windows.Forms.ListBox lstGroups;
        private System.Windows.Forms.Button btnCreateGroup;
        private System.Windows.Forms.Button btnBrowseGroups;
        private System.Windows.Forms.Label lblDirectTitle;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Panel pnlChatHeader;
        private System.Windows.Forms.Panel pnlChatDot;
        private System.Windows.Forms.Label lblChatName;
        private System.Windows.Forms.Label lblChatInfo;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox rtbChat;
    }
}
