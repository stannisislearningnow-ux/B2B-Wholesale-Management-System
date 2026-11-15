using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using B2BWholesale.Data;
using B2BWholesale.Security;

namespace B2BWholesale
{
    public class InitialAdminPasswordForm : Form
    {
        private readonly Database db = new Database();
        private ComboBox cmbAdmin;
        private TextBox txtPassword;
        private TextBox txtConfirm;

        public InitialAdminPasswordForm()
        {
            Text = "Initial Admin Password Setup";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 270);

            Label note = new Label {
                Text = "First-time setup: choose an Admin account and create its password.",
                AutoSize = false, Location = new Point(20, 20), Size = new Size(430, 40)
            };
            Label userLabel = new Label { Text = "Admin account", AutoSize = true, Location = new Point(25, 80) };
            cmbAdmin = new ComboBox { Location = new Point(155, 75), Width = 270,
                DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "UserName", ValueMember = "UserID" };
            Label passLabel = new Label { Text = "New password", AutoSize = true, Location = new Point(25, 125) };
            txtPassword = new TextBox { Location = new Point(155, 120), Width = 270, UseSystemPasswordChar = true };
            Label confirmLabel = new Label { Text = "Confirm password", AutoSize = true, Location = new Point(25, 165) };
            txtConfirm = new TextBox { Location = new Point(155, 160), Width = 270, UseSystemPasswordChar = true };
            Button save = new Button { Text = "Save password", Location = new Point(155, 210), Width = 130, Height = 32 };
            save.Click += SaveClicked;
            Controls.AddRange(new Control[] { note, userLabel, cmbAdmin, passLabel, txtPassword,
                confirmLabel, txtConfirm, save });
            Load += FormLoaded;
        }

        private void FormLoaded(object sender, EventArgs e)
        {
            if (db.HasConfiguredAdminPassword())
            {
                MessageBox.Show("An Admin password has already been configured. Use the normal login.");
                Close();
                return;
            }
            DataTable admins = db.GetAdminUsers();
            cmbAdmin.DataSource = admins;
            if (admins.Rows.Count == 0)
            {
                MessageBox.Show("No unlocked Admin accounts were found.");
                Close();
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            if (cmbAdmin.SelectedValue == null)
            {
                MessageBox.Show("Select an Admin account.");
                return;
            }
            if (txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Use a password with at least 8 characters.");
                return;
            }
            if (txtPassword.Text != txtConfirm.Text)
            {
                MessageBox.Show("The passwords do not match.");
                return;
            }
            try
            {
                db.SetInitialAdminPassword(Convert.ToInt32(cmbAdmin.SelectedValue),
                    PasswordHelper.HashPassword(txtPassword.Text));
                MessageBox.Show("Admin password saved. You can now sign in.");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save password: " + ex.Message);
            }
        }
    }
}
