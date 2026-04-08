using System;
using System.Windows.Forms;

namespace PulseChatClient.Forms
{
    public partial class CreateGroupDialog : Form
    {
        public string GroupName { get; private set; }

        public CreateGroupDialog()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            GroupName = txtGroupName.Text.Trim();
            if (string.IsNullOrWhiteSpace(GroupName))
            {
                this.DialogResult = DialogResult.None;
                txtGroupName.Focus();
            }
        }
    }
}
