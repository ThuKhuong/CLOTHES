using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES
{
    public partial class FrmForgotPassword : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;

        private readonly AuthService _auth = new();

        public string? ResetUsername { get; private set; }

        public FrmForgotPassword()
        {
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            Name = "FrmForgotPassword";
            ResumeLayout(false);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
                m.Result = (IntPtr)HTCAPTION;
        }

        private void BuildUI()
        {
            Text = "Forgot Password";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(253, 248, 244);
            ClientSize = new Size(900, 600);
            DoubleBuffered = true;
            ApplyRoundedCorners(this, 28);

            var right = new Panel
            {
                Dock = DockStyle.Right,
                Width = 450,
                BackColor = BackColor
            };
            Controls.Add(right);

            var left = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor
            };
            Controls.Add(left);

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resource.login
            };
            left.Controls.Add(pic);

            var btnMin = CreateTopButton("—");
            btnMin.Location = new Point(right.Width - 80, 14);
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            var btnClose = CreateTopButton("✕");
            btnClose.Location = new Point(right.Width - 44, 14);
            btnClose.Click += (s, e) => Close();

            right.Controls.Add(btnMin);
            right.Controls.Add(btnClose);

            var lblTitle = new Label
            {
                Text = "Quên mật khẩu",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = true,
                Location = new Point(50, 80)
            };
            right.Controls.Add(lblTitle);

            var line = new Panel
            {
                BackColor = Color.FromArgb(92, 85, 255),
                Size = new Size(320, 4),
                Location = new Point(50, 130)
            };
            right.Controls.Add(line);

            var userRow = CreateInputRow("👤", "Tên đăng nhập", out TextBox txtUser);
            userRow.Location = new Point(50, 180);
            right.Controls.Add(userRow);

            var phoneRow = CreateInputRow("📞", "Số điện thoại", out TextBox txtPhone);
            phoneRow.Location = new Point(50, 260);
            right.Controls.Add(phoneRow);

            var passRow = CreateInputRow("🔑", "Mật khẩu mới", out TextBox txtPass);
            passRow.Location = new Point(50, 340);
            right.Controls.Add(passRow);

            var confirmRow = CreateInputRow("🔒", "Xác nhận mật khẩu", out TextBox txtConfirm);
            confirmRow.Location = new Point(50, 420);
            right.Controls.Add(confirmRow);

            SetupPlaceholder(txtUser, "Tên đăng nhập");
            SetupPlaceholder(txtPhone, "Số điện thoại");
            SetupPlaceholder(txtPass, "Mật khẩu mới");
            SetupPlaceholder(txtConfirm, "Xác nhận mật khẩu");

            var btnReset = new Button
            {
                Text = "Đặt lại mật khẩu",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(92, 85, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(280, 55),
                Location = new Point(85, 520),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Region = new Region(CreateRoundedRect(btnReset.ClientRectangle, 28));
            right.Controls.Add(btnReset);

            btnReset.Click += (s, e) =>
            {
                string username = GetTextOrEmpty(txtUser, "Tên đăng nhập");
                string phone = GetTextOrEmpty(txtPhone, "Số điện thoại");
                string password = GetTextOrEmpty(txtPass, "Mật khẩu mới");
                string confirm = GetTextOrEmpty(txtConfirm, "Xác nhận mật khẩu");

                var (ok, message) = _auth.ForgotPassword(username, phone, password, confirm);
                MessageBox.Show(message);

                if (ok)
                {
                    ResetUsername = username.Trim();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            var lblBack = new Label
            {
                Text = "Quay lại",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(190, 490)
            };
            right.Controls.Add(lblBack);

            var linkLogin = new LinkLabel
            {
                Text = "Đăng nhập",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                LinkColor = Color.FromArgb(92, 85, 255),
                Location = new Point(lblBack.Right + 6, 490)
            };
            linkLogin.Click += (s, e) => Close();
            right.Controls.Add(linkLogin);
        }

        private static string GetTextOrEmpty(TextBox tb, string placeholder)
            => string.Equals(tb.Text, placeholder, StringComparison.Ordinal) ? "" : tb.Text;

        private Panel CreateInputRow(string icon, string placeholder, out TextBox textBox)
        {
            var row = new Panel
            {
                Size = new Size(350, 65),
                BackColor = Color.Transparent
            };

            var border = new Panel
            {
                Size = new Size(350, 55),
                Location = new Point(0, 10),
                BackColor = Color.White
            };
            row.Controls.Add(border);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 0, 8, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            border.Controls.Add(table);

            var lblIcon = new Label
            {
                Text = icon,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            table.Controls.Add(lblIcon, 0, 0);

            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            table.Controls.Add(host, 1, 0);

            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                Text = placeholder,
                Width = 280,
                Location = new Point(0, 12)
            };
            host.Controls.Add(textBox);

            border.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(200, 200, 200), 2);

                var r = new Rectangle(1, 1, border.Width - 2, border.Height - 2);
                e.Graphics.DrawPath(pen, CreateRoundedRect(r, 12));
            };

            return row;
        }

        private void SetupPlaceholder(TextBox tb, string placeholder)
        {
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                    if (placeholder is "Mật khẩu mới" or "Xác nhận mật khẩu")
                        tb.UseSystemPasswordChar = true;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (placeholder is "Mật khẩu mới" or "Xác nhận mật khẩu")
                        tb.UseSystemPasswordChar = false;
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
        }

        private Button CreateTopButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(92, 85, 255),
                BackColor = BackColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void ApplyRoundedCorners(Control c, int radius)
        {
            c.Region = new Region(CreateRoundedRect(c.ClientRectangle, radius));
            c.SizeChanged += (s, e) =>
            {
                c.Region = new Region(CreateRoundedRect(c.ClientRectangle, radius));
            };
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
