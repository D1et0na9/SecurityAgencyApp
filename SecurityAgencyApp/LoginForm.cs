//C# SecurityAgencyApp\LoginForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    // Простейшая форма авторизации. При успешной валидации возвращает DialogResult.OK.
    public class LoginForm : Form
    {
        private readonly Func<string, string, bool> validator;

        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblUser;
        private Label lblPass;

        public string UserName => txtUser.Text.Trim();

        public LoginForm(Func<string, string, bool> validateCredentials)
        {
            validator = validateCredentials ?? throw new ArgumentNullException(nameof(validateCredentials));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Авторизация";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(320, 150);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            lblUser = new Label { Text = "Логин:", Left = 12, Top = 15, Width = 60 };
            txtUser = new TextBox { Left = 80, Top = 12, Width = 220 };

            lblPass = new Label { Text = "Пароль:", Left = 12, Top = 48, Width = 60 };
            txtPass = new TextBox { Left = 80, Top = 45, Width = 220, UseSystemPasswordChar = true };

            btnLogin = new Button { Text = "Авторизоваться", Left = 60, Top = 85, Width = 140, Height = 28 };
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button { Text = "Отмена", Left = 200, Top = 85, Width = 100, Height = 28 };
            btnCancel.Click += BtnCancel_Click;

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            this.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, btnLogin, btnCancel });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var user = txtUser.Text.Trim();
            var pass = txtPass.Text;

            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show(this, "Введите логин.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUser.Focus();
                return;
            }

            if (validator(user, pass))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(this, "Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPass.SelectAll();
                txtPass.Focus();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}