using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using B2BWholesale.Data;
using B2BWholesale.Security;

namespace B2BWholesale
{
    public class LoginForm : Form
    {
        private readonly Database db = new Database();
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnSetup;
        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            Text = "B2B Wholesale - Admin Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(430, 290);

            Label title = new Label { Text = "B2B WHOLESALE", Font = new Font("Segoe UI", 19, FontStyle.Bold),
                AutoSize = true, Location = new Point(105, 25) };
            Label subtitle = new Label { Text = "Administrator sign in", AutoSize = true,
                Location = new Point(155, 68) };
            Label userLabel = new Label { Text = "Username", AutoSize = true, Location = new Point(55, 115) };
            txtUsername = new TextBox { Location = new Point(155, 110), Width = 215 };
            Label passLabel = new Label { Text = "Password", AutoSize = true, Location = new Point(55, 155) };
            txtPassword = new TextBox { Location = new Point(155, 150), Width = 215, UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Login", Location = new Point(155, 195), Width = 100, Height = 32 };
            btnSetup = new Button { Text = "Set initial Admin password", Location = new Point(155, 235), Width = 215, Height = 28 };

            btnLogin.Click += LoginClicked;
            btnSetup.Click += SetupClicked;
            AcceptButton = btnLogin;
            Controls.AddRange(new Control[] { title, subtitle, userLabel, txtUsername,
                passLabel, txtPassword, btnLogin, btnSetup });
            RefreshSetupButton();
        }

        private void RefreshSetupButton()
        {
            btnSetup.Visible = !db.HasConfiguredAdminPassword();
        }

        private void SetupClicked(object sender, EventArgs e)
        {
            using (InitialAdminPasswordForm form = new InitialAdminPasswordForm())
                form.ShowDialog(this);
            RefreshSetupButton();
        }

        private void LoginClicked(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            if (username.Length == 0 || txtPassword.Text.Length == 0)
            {
                MessageBox.Show("Enter your username and password.");
                return;
            }

            try
            {
                DataRow user = db.GetUserForLogin(username);
                if (user == null)
                {
                    MessageBox.Show("Invalid username or password.");
                    return;
                }
                if (Convert.ToBoolean(user["IsLocked"]))
                {
                    MessageBox.Show("This account is locked. Contact the system administrator.");
                    return;
                }
                if (!String.Equals(Convert.ToString(user["Role"]), "Admin",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Access denied. Only users with the Admin role can use this application.");
                    return;
                }
                if (!PasswordHelper.VerifyPassword(txtPassword.Text,
                    Convert.ToString(user["PasswordHash"])))
                {
                    MessageBox.Show("Invalid username or password.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                LoggedInUser = Convert.ToString(user["UserName"]);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message, "Database error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
