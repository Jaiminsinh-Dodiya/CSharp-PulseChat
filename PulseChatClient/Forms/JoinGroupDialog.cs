using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PulseChatClient.Services;

namespace PulseChatClient.Forms
{
    public class JoinGroupDialog : Form
    {
        private ListBox lstGroups;
        private Button btnJoin;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblHint;

        public int SelectedGroupId { get; private set; }

        private List<GroupData> _groups;

        // Colors
        private readonly Color BgDark = Color.FromArgb(22, 22, 32);
        private readonly Color BgList = Color.FromArgb(28, 28, 40);
        private readonly Color BorderClr = Color.FromArgb(55, 55, 72);
        private readonly Color AccentBlue = Color.FromArgb(78, 160, 255);
        private readonly Color TextWhite = Color.FromArgb(235, 235, 242);
        private readonly Color TextGray = Color.FromArgb(130, 130, 155);
        private readonly Color TextDim = Color.FromArgb(80, 80, 105);
        private readonly Color BgHover = Color.FromArgb(42, 42, 58);

        public JoinGroupDialog(List<GroupData> availableGroups)
        {
            _groups = availableGroups;

            this.Text = "Browse Groups";
            this.ClientSize = new Size(360, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BgDark;
            this.Font = new Font("Segoe UI", 10F);

            lblTitle = new Label
            {
                Text = "Available Groups",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AccentBlue,
                Location = new Point(20, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblHint = new Label
            {
                Text = availableGroups.Count == 0
                    ? "No groups available to join — create one!"
                    : "Select a group and click Join",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                Location = new Point(20, 40),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lstGroups = new ListBox
            {
                Location = new Point(20, 62),
                Size = new Size(320, 190),
                BackColor = BgList,
                ForeColor = TextWhite,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 38,
                Font = new Font("Segoe UI", 10F)
            };
            lstGroups.DrawItem += LstGroups_DrawItem;
            lstGroups.DoubleClick += (s, e) =>
            {
                if (lstGroups.SelectedIndex >= 0)
                {
                    SelectedGroupId = _groups[lstGroups.SelectedIndex].Id;
                    this.DialogResult = DialogResult.OK;
                }
            };

            foreach (var g in _groups)
                lstGroups.Items.Add(g.Name);

            btnJoin = new Button
            {
                Text = "JOIN",
                Location = new Point(20, 264),
                Size = new Size(155, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = availableGroups.Count > 0
            };
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.Click += (s, e) =>
            {
                if (lstGroups.SelectedIndex >= 0)
                {
                    SelectedGroupId = _groups[lstGroups.SelectedIndex].Id;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Select a group first.", "PulseChat",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnCancel = new Button
            {
                Text = "CANCEL",
                Location = new Point(185, 264),
                Size = new Size(155, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = BorderClr,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, lblHint, lstGroups, btnJoin, btnCancel });
            this.CancelButton = btnCancel;
        }

        private void LstGroups_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _groups.Count) return;
            e.DrawBackground();

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var group = _groups[e.Index];

            // Background
            Color bg = (e.State & DrawItemState.Selected) != 0 ? BgHover : BgList;
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            // Color dot
            Color dotColor = HexToColor(group.ColorHex);
            using (var brush = new SolidBrush(dotColor))
                g.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 12, 12);

            // Name
            using (var brush = new SolidBrush(TextWhite))
            using (var font = new Font("Segoe UI", 10F, FontStyle.Bold))
                g.DrawString($"# {group.Name}", font, brush, e.Bounds.X + 30, e.Bounds.Y + 4);

            // Info
            using (var brush = new SolidBrush(TextDim))
            using (var font = new Font("Segoe UI", 7.5F))
                g.DrawString($"{group.MemberCount} members • by {group.CreatedBy}", font, brush,
                    e.Bounds.X + 30, e.Bounds.Y + 22);
        }

        private Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7) return AccentBlue;
            try
            {
                int r = Convert.ToInt32(hex.Substring(1, 2), 16);
                int gr = Convert.ToInt32(hex.Substring(3, 2), 16);
                int b = Convert.ToInt32(hex.Substring(5, 2), 16);
                return Color.FromArgb(r, gr, b);
            }
            catch { return AccentBlue; }
        }
    }
}
