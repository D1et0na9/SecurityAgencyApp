using System;
using System.Windows.Forms;
using System.Drawing; // Не забываем про это пространство имен для работы с цветом

namespace SecurityAgencyApp
{
    public partial class Form1 : Form
    {
        // --- 1. Поля класса для доступа к элементам управления ---
        private TextBox logTextBox;
        private Panel logPanel;
        private Button toggleLogButton;
        private Panel centerPanel;
        private MenuStrip mainMenuStrip; // теперь меню доступно на уровне класса

        public Form1()
        {
            // Этот метод создается дизайнером (Form1.Designer.cs) и его не трогаем
            InitializeComponent();

            this.Text = "Агентство Охраны: Главное меню";
            this.MinimumSize = new Size(500, 450); // Установка минимального размера для корректного отображения

            // Вызов метода для настройки интерфейса
            InitializeCustomLayout();

            // Логируем, что приложение запущено, но действия пока недоступны до авторизации
            LogMessage("Приложение успешно запущено. Ожидание авторизации...");

            // Покажем окно авторизации после того как форма отобразится (безопаснее, чем в конструкторе)
            this.Shown += Form1_Shown;
        }

        // Показать диалог авторизации при первом отображении формы
        private void Form1_Shown(object? sender, EventArgs e)
        {
            this.Shown -= Form1_Shown;

            using var login = new LoginForm(ValidateCredentials);

            var result = login.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                // Успешная авторизация — включаем интерфейс и логируем
                SetAppEnabled(true);
                LogMessage($"Пользователь '{login.UserName}' авторизован.");
            }
            else
            {
                // Отмена — закрываем приложение
                LogMessage("Авторизация отменена пользователем. Завершение работы.");
                this.Close();
            }
        }

        // Простейшая проверка учётных данных (пример).
        // Для реального приложения замените на проверку по базе/AD/и т.п.
        private bool ValidateCredentials(string user, string pass)
        {
            // Демонстрационные значения
            return user == "admin" && pass == "password123";
        }

        // --- 2. Метод для создания и размещения всех элементов управления ---
        private void InitializeCustomLayout()
        {
            // --- 1. Нижняя часть: Консоль логов (Panel, TextBox, Button) ---
            InitializeLogConsole();

            // --- 2. Меню: дублирует названия разделов и имеет выпадающие пункты ---
            mainMenuStrip = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = SystemColors.Control,
                Enabled = false // по умолчанию недоступно до авторизации
            };

            // Вспомогательный локальный метод для создания пункта меню с подпунктами "Открыть", "Редактировать", "Скачать"
            ToolStripMenuItem CreateMenuWithDropdown(string title, EventHandler openHandler)
            {
                var topItem = new ToolStripMenuItem(title);

                // Подпункт "Открыть" — вызывает обработчик раздела
                var openItem = new ToolStripMenuItem("Открыть");
                openItem.Click += openHandler;
                topItem.DropDownItems.Add(openItem);

                // Подпункт "Редактировать"
                var editItem = new ToolStripMenuItem("Редактировать");
                editItem.Click += HandleEditClick;
                topItem.DropDownItems.Add(editItem);

                // Подпункт "Скачать"
                var downloadItem = new ToolStripMenuItem("Скачать");
                downloadItem.Click += HandleDownloadClick;
                topItem.DropDownItems.Add(downloadItem);

                return topItem;
            }

            mainMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                CreateMenuWithDropdown("Сотрудники", HandleEmployeesClick),
                CreateMenuWithDropdown("Заказчики", HandleCustomersClick),
                CreateMenuWithDropdown("Договоры", HandleContractsClick),
                CreateMenuWithDropdown("Отчеты", HandleReportsClick)
            });

            // Кнопка "Выйти" в правой части MenuStrip (выделенное место на скриншоте)
            var logoutItem = new ToolStripMenuItem("Выйти")
            {
                Alignment = ToolStripItemAlignment.Right
            };
            logoutItem.Click += HandleLogoutClick;
            mainMenuStrip.Items.Add(logoutItem);

            // Добавляем меню в форму — оно будет над рабочей областью
            this.Controls.Add(mainMenuStrip);

            // --- 3. Средняя часть: Рабочая область (Panel) ---
            centerPanel = new Panel
            {

                Dock = DockStyle.Fill, // Займет все оставшееся пространство
                BackColor = SystemColors.ControlLight,
                BorderStyle = BorderStyle.Fixed3D
            };
            this.Controls.Add(centerPanel);

            // Убеждаемся, что лог-панель и меню находятся поверх центральной панели
            logPanel.BringToFront();
            mainMenuStrip.BringToFront();
        }

        // Утилита включения/выключения функционала приложения
        private void SetAppEnabled(bool enabled)
        {
            // Меню и рабочая область блокируются/разблокируются
            if (mainMenuStrip != null)
                mainMenuStrip.Enabled = enabled;

            if (centerPanel != null)
                centerPanel.Enabled = enabled;
        }

        // Обработчик нажатия "Выйти" — блокируем функционал и показываем форму авторизации
        private void HandleLogoutClick(object? sender, EventArgs e)
        {
            LogMessage("Пользователь вышел. Ожидание новой авторизации...");
            SetAppEnabled(false);

            using var login = new LoginForm(ValidateCredentials);
            var result = login.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                SetAppEnabled(true);
                LogMessage($"Пользователь '{login.UserName}' авторизован.");
            }
            else
            {
                LogMessage("Авторизация отменена после выхода. Завершение работы.");
                this.Close();
            }
        }

        // --- 3. Метод для настройки лог-консоли (с учетом правок) ---
        private void InitializeLogConsole()
        {
            // Лог-панель (контейнер для логов и кнопки)
            logPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150, // Начальная высота консоли
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.Control // Цвет фона как у приложения
            };
            this.Controls.Add(logPanel);

            // Кнопка "Скрыть/Показать"
            toggleLogButton = new Button
            {
                Text = "Скрыть консоль",
                Dock = DockStyle.Bottom, // Закрепляем внизу лог-панели
                Height = 25,
                FlatStyle = FlatStyle.System, // Стандартный стиль кнопки
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText // Цвет текста как у остального
            };
            toggleLogButton.Click += ToggleLogVisibility;
            logPanel.Controls.Add(toggleLogButton);

            // Текстовое поле для логов
            logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill, // Занимает всю оставшуююся часть logPanel
                // Цвет фона и текста как у остального приложения
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Font = new Font("Consolas", 9.75F, FontStyle.Regular) // Можно оставить моноширинный шрифт для "консольного" вида
            };
            logPanel.Controls.Add(logTextBox);

            // Лог-текстовое поле должно быть над кнопкой (визуально)
            toggleLogButton.BringToFront();
        }

        // --- 4. Обработчик для кнопки скрытия/отображения (ИСПРАВЛЕН) ---
        private void ToggleLogVisibility(object sender, EventArgs e)
        {
            const int MIN_HEIGHT = 25; // Высота кнопки, минимальная видимая высота панели

            // Проверяем, если текущая высота панели больше минимальной (т.е. она открыта)
            if (logPanel.Height > MIN_HEIGHT)
            {
                logPanel.Tag = logPanel.Height; // Сохраняем текущую (полную) высоту в Tag
                logPanel.Height = MIN_HEIGHT;   // Устанавливаем минимальную высоту (скрываем логи)
                toggleLogButton.Text = "Показать консоль";
                LogMessage("Консоль логов скрыта.");
            }
            else
            {
                // Если высота минимальная (т.е. консоль скрыта), восстанавливаем полную высоту
                // Используем сохраненное значение в Tag, по умолчанию - 150
                logPanel.Height = (logPanel.Tag is int storedHeight) ? storedHeight : 150;
                toggleLogButton.Text = "Скрыть консоль";
                LogMessage("Консоль логов отображена.");
            }

            // Вынуждаем форму перерисовать элементы (особенно CenterPanel)
            this.PerformLayout();
        }

        // --- 5. Вспомогательные методы ---

        // Метод для записи сообщений в консоль логов
        public void LogMessage(string message)
        {
            // Проверяем, существует ли текстовое поле, прежде чем писать в него
            if (logTextBox != null)
            {
                string timestamp = DateTime.Now.ToString("[HH:mm:ss]");
                // Добавляем текст
                logTextBox.AppendText($"{timestamp} {message}{Environment.NewLine}");
            }
        }

        // --- 6. Обработчики событий для пунктов меню и действий (Номинальный функционал) ---
        private void HandleEmployeesClick(object sender, EventArgs e)
        {
            LogMessage("Нажата команда: 'Открыть -> Сотрудники'. Инициализация интерфейса управления персоналом...");

            // Очистим рабочую область и вставим контроль
            centerPanel.Controls.Clear();

            var employeesControl = new EmployeesControl
            {
                Dock = DockStyle.Fill
            };

            centerPanel.Controls.Add(employeesControl);
        }

        private void HandleCustomersClick(object sender, EventArgs e)
        {
            LogMessage("Нажата команда: 'Открыть -> Заказчики'. Инициализация формы управления клиентами...");
        }

        private void HandleContractsClick(object sender, EventArgs e)
        {
            LogMessage("Нажата команда: 'Открыть -> Договоры'. Инициализация формы регистрации договоров...");
        }

        private void HandleReportsClick(object sender, EventArgs e)
        {
            LogMessage("Нажата команда: 'Открыть -> Отчеты'. Инициализация формы отчетов...");
        }

        // Обработчик для "Редактировать"
        private void HandleEditClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                string section = (item.OwnerItem as ToolStripMenuItem)?.Text ?? item.Text;
                LogMessage($"Выбрано: 'Редактировать' -> {section}.");

                Form editForm = new Form
                {
                    Text = $"Редактировать: {section} (Заглушка)",
                    Width = 600,
                    Height = 400
                };
                editForm.Show();
            }
        }

        // Обработчик для "Скачать"
        private void HandleDownloadClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                string section = (item.OwnerItem as ToolStripMenuItem)?.Text ?? item.Text;
                LogMessage($"Запрошена загрузка: '{section}'.");
                MessageBox.Show(this, $"Начать загрузку данных для раздела '{section}' (заглушка).", "Скачать", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}