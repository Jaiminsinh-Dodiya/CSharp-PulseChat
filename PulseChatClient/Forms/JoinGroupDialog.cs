using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PulseChatClient.Services;

namespace PulseChatClient.Forms
{
    public partial class JoinGroupDialog : Form
    {
        public int SelectedGroupId { get; private set; }

        private readonly List<GroupData> _groups;

        public JoinGroupDialog() : this(new List<GroupData>()) { }

        public JoinGroupDialog(List<GroupData> availableGroups)
        {
            _groups = availableGroups ?? new List<GroupData>();
            InitializeComponent();

            // Populate the listbox
            foreach (var g in _groups)
                lstAvailableGroups.Items.Add(g);

            if (_groups.Count == 0)
            {
                lblInfo.Text = "No groups available to join.";
                btnJoin.Enabled = false;
            }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (lstAvailableGroups.SelectedItem is GroupData selected)
            {
                SelectedGroupId = selected.Id;
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }
        }

        private void lstAvailableGroups_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var item = lstAvailableGroups.Items[e.Index] as GroupData;
            if (item == null) return;

            Color bg = (e.State & DrawItemState.Selected) != 0
                ? Color.FromArgb(50, 40, 75)
                : Color.FromArgb(28, 28, 38);
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            // Colored dot
            Color dotColor = ColorTranslator.FromHtml(item.ColorHex ?? "#8A60FF");
            using (var brush = new SolidBrush(dotColor))
                g.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 12, 12, 12);

            // Group name
            using (var brush = new SolidBrush(Color.FromArgb(235, 235, 242)))
            using (var font = new Font("Segoe UI", 10F, FontStyle.Bold))
                g.DrawString($"# {item.Name}", font, brush, e.Bounds.X + 30, e.Bounds.Y + 4);

            // Member count
            using (var brush = new SolidBrush(Color.FromArgb(130, 130, 155)))
            using (var font = new Font("Segoe UI", 8F))
                g.DrawString($"{item.MemberCount} members", font, brush, e.Bounds.X + 30, e.Bounds.Y + 20);
        }
    }
}
