using System;
using System.Drawing;
using System.Windows.Forms;

namespace PulseChatClient.Forms
{
    public class CreateGroupDialog : Form
    {
        private TextBox txtGroupName;
        private Button btnCreate;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblHint;

        public string GroupName { get; private set; }

        // Colors
        private readonly Color BgDark = Color.FromArgb(22, 22, 32);
        private readonly Color BgInput = Color.FromArgb(35, 35, 48);
        private readonly Color BorderClr = Color.FromArgb(55, 55, 72);
        private readonly Color AccentPurple = Color.FromArgb(138, 96, 255);
        private readonly Color TextWhite = Color.FromArgb(235, 235, 242);
        private readonly Color TextGray = Color.FromArgb(130, 130, 155);

        public CreateGroupDialog()
        {
            this.Text = "Create Group";
            this.ClientSize = new Size(340, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BgDark;
            this.Font = new Font("Segoe UI", 10F);

            lblTitle = new Label
            {
                Text = "Create New Group",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AccentPurple,
                Location = new Point(20, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblHint = new Label
            {
                Text = "Enter a unique group name (2-30 characters)",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                Location = new Point(20, 45),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtGroupName = new TextBox
            {
                Location = new Point(20, 70),
                Size = new Size(300, 30),
                BackColor = BgInput,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 30
            };

            btnCreate = new Button
            {
                Text = "CREATE",
                Location = new Point(20, 115),
                Size = new Size(145, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentPurple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += (s, e) =>
            {
                GroupName = txtGroupName.Text.Trim();
                if (string.IsNullOrWhiteSpace(GroupName) || GroupName.Length < 2)
                {
                    MessageBox.Show("Group name must be at least 2 characters.", "PulseChat",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                }
            };

            btnCancel = new Button
            {
                Text = "CANCEL",
                Location = new Point(175, 115),
                Size = new Size(145, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = BorderClr,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, lblHint, txtGroupName, btnCreate, btnCancel });
            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
        }
    }
}
