using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class EmployeesControl : UserControl
    {
        private SplitContainer split;
        private PictureBox pictureBox;
        private Label infoLabel;

        private ListView employeesListView;
        private ColumnHeader colDept;
        private ColumnHeader colName;
        private ColumnHeader colHireDate;

        private List<Employee> employees = new();

        public EmployeesControl()
        {
            InitializeComponents();
            PopulateSampleData();
            FillListView();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;

            // split: левое фиксированное место под фото/инфо, правое — список
            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                // НЕ задаём Panel1MinSize/Panel2MinSize и SplitterDistance здесь:
                // они будут установлены в Load, когда контроль уже получил размер
                IsSplitterFixed = false,
                BackColor = SystemColors.Control
            };
            this.Controls.Add(split);

            // Устанавливаем желаемую стартовую ширину левой панели позже, в Load
            this.Load += EmployeesControl_Load;
            this.ParentChanged += EmployeesControl_ParentChanged;

            // В левой панели используем TableLayout для корректной компоновки (без наложений)
            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F)); // фото
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F)); // информация
            split.Panel1.Controls.Add(leftLayout);

            // PictureBox (занимает верхнюю часть)
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            leftLayout.Controls.Add(pictureBox, 0, 0);

            // Info label (нижняя часть)
            infoLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(6),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BackColor = SystemColors.Control,
                AutoEllipsis = true
            };
            leftLayout.Controls.Add(infoLabel, 0, 1);

            // Правый список сотрудников
            employeesListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false
            };
            colDept = new ColumnHeader { Text = "Отдел", Width = 160 };
            colName = new ColumnHeader { Text = "ФИО", Width = 300 };
            colHireDate = new ColumnHeader { Text = "Дата трудоустройства", Width = 160 };
            employeesListView.Columns.AddRange(new[] { colDept, colName, colHireDate });
            employeesListView.SelectedIndexChanged += EmployeesListView_SelectedIndexChanged;

            split.Panel2.Controls.Add(employeesListView);
        }

        // Устанавливаем корректное SplitterDistance когда контрол уже имеет размер.
        // Если в момент вызова ширина ещё не известна — откладываем установку через BeginInvoke.
        private void EmployeesControl_Load(object? sender, EventArgs e)
        {
            // желаемая ширина левой панели и минимумы
            const int desiredLeftWidth = 360;
            const int panel1Min = 240;
            const int panel2Min = 200;

            // получаем актуальную ширину (используем split.ClientSize, если уже инициализирован)
            int totalWidth = Math.Max(1, this.ClientSize.Width);
            if (totalWidth <= 1)
            {
                // отложим установку, если ещё нет размеров
                this.BeginInvoke((Action)(() => EmployeesControl_Load(sender, e)));
                return;
            }

            // Вычислим безопасное значение splitterDistance до установки минимумов.
            // Важно: сначала назначаем SplitterDistance, затем минимумы — это предотвращает исключение.
            int safeSplitter = Math.Clamp(desiredLeftWidth, 0, Math.Max(0, totalWidth - panel2Min));
            split.SplitterDistance = safeSplitter;

            // Теперь можно установить минимумы — SplitterDistance уже в допустимых границах.
            // Если желаемые минимумы не вмещаются в текущую ширину, уменьшаем panel2Min пропорционально.
            int availableForMins = totalWidth;
            int effectivePanel2Min = Math.Min(panel2Min, Math.Max(0, availableForMins - panel1Min));
            split.Panel1MinSize = Math.Min(panel1Min, Math.Max(0, availableForMins - effectivePanel2Min));
            split.Panel2MinSize = effectivePanel2Min;
        }

        private void EmployeesControl_ParentChanged(object? sender, EventArgs e)
        {
            // Отложим, чтобы размер родителя успел примениться
            this.BeginInvoke((Action)(() => SetSafeSplitterDistance(360, 240, 200)));
        }

        private void SetSafeSplitterDistance(int desired, int panel1Min, int panel2Min)
        {
            int totalWidth = this.ClientSize.Width;
            if (totalWidth <= 0)
            {
                this.BeginInvoke((Action)(() => SetSafeSplitterDistance(desired, panel1Min, panel2Min)));
                return;
            }

            int safe = Math.Clamp(desired, 0, Math.Max(0, totalWidth - panel2Min));
            split.SplitterDistance = safe;

            int effectivePanel2Min = Math.Min(panel2Min, Math.Max(0, totalWidth - panel1Min));
            split.Panel1MinSize = Math.Min(panel1Min, Math.Max(0, totalWidth - effectivePanel2Min));
            split.Panel2MinSize = effectivePanel2Min;
        }

        private void PopulateSampleData()
        {
            employees.Add(new Employee("Охрана", "Иванов Иван Иванович", new DateTime(2018, 3, 12)));
            employees.Add(new Employee("Администрация", "Петрова Мария Сергеевна", new DateTime(2020, 7, 1)));
            employees.Add(new Employee("Технический отдел", "Сидоров Алексей Петрович", new DateTime(2016, 11, 20)));
            employees.Add(new Employee("Охрана", "Кузнецова Ольга Николаевна", new DateTime(2021, 5, 15)));

            foreach (var emp in employees)
            {
                emp.Photo = GeneratePlaceholderImage(emp.FullName, 640, 480);
            }
        }

        private void FillListView()
        {
            employeesListView.Items.Clear();
            foreach (var emp in employees)
            {
                var item = new ListViewItem(new[] {
                    emp.Department,
                    emp.FullName,
                    emp.HireDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                })
                {
                    Tag = emp
                };
                employeesListView.Items.Add(item);
            }

            if (employeesListView.Items.Count > 0)
            {
                employeesListView.Items[0].Selected = true;
                employeesListView.Select();
            }
        }

        private void EmployeesListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (employeesListView.SelectedItems.Count == 0)
            {
                pictureBox.Image = null;
                infoLabel.Text = string.Empty;
                return;
            }

            var item = employeesListView.SelectedItems[0];
            if (item.Tag is Employee emp)
            {
                pictureBox.Image = emp.Photo;
                infoLabel.Text = $"Отдел: {emp.Department}{Environment.NewLine}" +
                                 $"ФИО: {emp.FullName}{Environment.NewLine}" +
                                 $"Дата трудоустройства: {emp.HireDate:yyyy-MM-dd}";
            }
        }

        private Bitmap GeneratePlaceholderImage(string fullName, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.LightSteelBlue);
            using var brush = new SolidBrush(Color.DodgerBlue);
            var rect = new Rectangle(20, 20, width - 40, height - 40);
            g.FillRectangle(brush, rect);

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            for (int i = 0; i < Math.Min(3, parts.Length); i++)
                initials += parts[i][0];

            using var font = new Font("Segoe UI", Math.Max(24, width / 10), FontStyle.Bold, GraphicsUnit.Pixel);
            using var textBrush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(initials.ToUpperInvariant(), font, textBrush, rect, sf);

            return bmp;
        }

        private class Employee
        {
            public string Department { get; }
            public string FullName { get; }
            public DateTime HireDate { get; }
            public Bitmap? Photo { get; set; }

            public Employee(string dept, string fullName, DateTime hire)
            {
                Department = dept;
                FullName = fullName;
                HireDate = hire;
            }
        }
    }
}