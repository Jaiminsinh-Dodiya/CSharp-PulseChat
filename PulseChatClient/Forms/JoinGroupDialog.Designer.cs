namespace PulseChatClient.Forms
{
    partial class JoinGroupDialog
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lstAvailableGroups = new System.Windows.Forms.ListBox();
            this.btnJoin = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(78, 160, 255);
            this.lblTitle.Location = new System.Drawing.Point(20, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(360, 32);
            this.lblTitle.Text = "Browse Groups";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(130, 130, 155);
            this.lblInfo.Location = new System.Drawing.Point(20, 44);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(360, 20);
            this.lblInfo.Text = "Select a group to join:";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstAvailableGroups
            // 
            this.lstAvailableGroups.BackColor = System.Drawing.Color.FromArgb(28, 28, 38);
            this.lstAvailableGroups.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstAvailableGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstAvailableGroups.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstAvailableGroups.ForeColor = System.Drawing.Color.FromArgb(235, 235, 242);
            this.lstAvailableGroups.ItemHeight = 36;
            this.lstAvailableGroups.Location = new System.Drawing.Point(20, 70);
            this.lstAvailableGroups.Name = "lstAvailableGroups";
            this.lstAvailableGroups.Size = new System.Drawing.Size(360, 216);
            this.lstAvailableGroups.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstAvailableGroups_DrawItem);
            // 
            // btnJoin
            // 
            this.btnJoin.BackColor = System.Drawing.Color.FromArgb(78, 160, 255);
            this.btnJoin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnJoin.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnJoin.FlatAppearance.BorderSize = 0;
            this.btnJoin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJoin.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnJoin.ForeColor = System.Drawing.Color.White;
            this.btnJoin.Location = new System.Drawing.Point(20, 300);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(175, 38);
            this.btnJoin.Text = "JOIN GROUP";
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            this.btnClose.Location = new System.Drawing.Point(205, 300);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(175, 38);
            this.btnClose.Text = "CLOSE";
            // 
            // JoinGroupDialog
            // 
            this.AcceptButton = this.btnJoin;
            this.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(400, 355);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lstAvailableGroups);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JoinGroupDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PulseChat \u2014 Browse Groups";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ListBox lstAvailableGroups;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Button btnClose;
    }
}
