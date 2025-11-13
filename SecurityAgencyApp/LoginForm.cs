//C# SecurityAgencyApp\LoginForm.cs
using FirebirdSql.Data.FirebirdClient;
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

            // Выполнить подключение к Firebird и выполнить тестовый запрос
            string ConnString;
            try
            {
                var cs = new FbConnectionStringBuilder
                {
                    DataSource = "localhost",
                    UserID = "SYSDBA",
                    Password = "masterkey",
                    Database = @"C:\Base\Secure_Base.fdb",
                    Port = 3050,
                    Charset = "utf8"
                };

                ConnString = cs.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Неверно сформирована строка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            object dbTestResult = null;
            try
            {
                using var fbConn = new FbConnection(ConnString);
                fbConn.Open();

                // Выполняем простой тестовый запрос — вернёт одну строку из системной таблицы
                const string testQuery = "SELECT 'OK' AS RESULT FROM RDB$DATABASE";
                using var cmd = new FbCommand(testQuery, fbConn);
                dbTestResult = cmd.ExecuteScalar();

                // Логируем результат в консоль приложения (если Form1 доступна)
                string message = $"Проверка БД: {(dbTestResult?.ToString() ?? "<null>")}";
                if (this.Owner is Form1 ownerForm)
                {
                    ownerForm.LogMessage(message);
                }
                else
                {
                    // fallback — найти открытую Form1 в приложении
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f is Form1 frm)
                        {
                            frm.LogMessage(message);
                            break;
                        }
                    }
                }

                fbConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Не удалось подключиться к базе данных или выполнить тестовый запрос: {ex.Message}", "Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Если у вас есть дополнительная логика проверки логина/пароля — вызываем валидатор
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
