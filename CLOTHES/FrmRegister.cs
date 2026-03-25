using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES
{
    public partial class FrmRegister : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;

        private readonly AuthService _auth = new();

        public string? RegisteredUsername { get; private set; }

        public FrmRegister()
        {
            InitializeComponent();
            BuildUI();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
                m.Result = (IntPtr)HTCAPTION;
        }

        private void BuildUI()
        {
            Text = "Register";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(253, 248, 244);
            ClientSize = new Size(900, 600); // ✅ Fixed size to match login form
            DoubleBuffered = true;
            ApplyRoundedCorners(this, 28);

            var right = new Panel
            {
                Dock = DockStyle.Right,
                Width = 450, // ✅ Reduced from 650 to match login form
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
                Text = "Tạo tài khoản",
                Font = new Font("Segoe UI", 18, FontStyle.Bold), // ✅ Reduced from 22 to match login form
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = true,
                Location = new Point(50, 80) // ✅ Adjusted position
            };
            right.Controls.Add(lblTitle);

            var line = new Panel
            {
                BackColor = Color.FromArgb(92, 85, 255),
                Size = new Size(320, 4), // ✅ Reduced from 420x5 to match login form
                Location = new Point(50, 130) // ✅ Adjusted position
            };
            right.Controls.Add(line);

            var nameRow = CreateInputRow("🧑", "Họ tên", out TextBox txtTenNd);
            nameRow.Location = new Point(50, 160); // ✅ Adjusted positions to fit smaller form
            right.Controls.Add(nameRow);

            var phoneRow = CreateInputRow("📞", "Số điện thoại", out TextBox txtSdt);
            phoneRow.Location = new Point(50, 230); // ✅ Adjusted spacing
            right.Controls.Add(phoneRow);

            var userRow = CreateInputRow("👤", "Tên đăng nhập", out TextBox txtUser);
            userRow.Location = new Point(50, 300); // ✅ Adjusted spacing
            right.Controls.Add(userRow);

            var passRow = CreateInputRow("🔑", "Mật khẩu", out TextBox txtPass);
            passRow.Location = new Point(50, 370); // ✅ Adjusted spacing
            right.Controls.Add(passRow);

            var confirmRow = CreateInputRow("🔒", "Xác nhận mật khẩu", out TextBox txtConfirm);
            confirmRow.Location = new Point(50, 440); // ✅ Adjusted spacing
            right.Controls.Add(confirmRow);

            SetupPlaceholder(txtTenNd, "Họ tên");
            SetupPlaceholder(txtSdt, "Số điện thoại");
            SetupPlaceholder(txtUser, "Tên đăng nhập");
            SetupPlaceholder(txtPass, "Mật khẩu");
            SetupPlaceholder(txtConfirm, "Xác nhận mật khẩu");

            var btnRegister = new Button
            {
                Text = "Đăng ký",
                Font = new Font("Segoe UI", 12, FontStyle.Bold), // ✅ Reduced from 14 to match login form
                ForeColor = Color.White,
                BackColor = Color.FromArgb(92, 85, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(280, 55), // ✅ Reduced from 320x65 to match login form
                Location = new Point(85, 520), // ✅ Adjusted position
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Region = new Region(CreateRoundedRect(btnRegister.ClientRectangle, 28));
            right.Controls.Add(btnRegister);

            btnRegister.Click += (s, e) =>
            {
                string tenNd = GetTextOrEmpty(txtTenNd, "Họ tên");
                string sdt = GetTextOrEmpty(txtSdt, "Số điện thoại");
                string username = GetTextOrEmpty(txtUser, "Tên đăng nhập");
                string password = GetTextOrEmpty(txtPass, "Mật khẩu");
                string confirm = GetTextOrEmpty(txtConfirm, "Xác nhận mật khẩu");

                var (ok, message) = _auth.Register(tenNd, sdt, username, password, confirm);
                MessageBox.Show(message);

                if (ok)
                {
                    RegisteredUsername = username;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            var linkLogin = new LinkLabel
            {
                Text = "Quay lại đăng nhập",
                AutoSize = true,
                Font = new Font("Segoe UI", 10), // ✅ Reduced from 11 to match login form
                LinkColor = Color.FromArgb(92, 85, 255),
                Location = new Point(120, 480) // ✅ Adjusted position to fit smaller form
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
                Size = new Size(350, 65), // ✅ Reduced from 500x90 to match login form
                BackColor = Color.Transparent
            };

            var border = new Panel
            {
                Size = new Size(350, 55), // ✅ Reduced from 500x80 to match login form
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
                Padding = new Padding(8, 0, 8, 0) // ✅ Reduced padding to match login form
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); // ✅ Reduced from 60 to match login form
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            border.Controls.Add(table);

            var lblIcon = new Label
            {
                Text = icon,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular), // ✅ Reduced from 16 to match login form
                ForeColor = Color.Gray
            };
            table.Controls.Add(lblIcon, 0, 0);

            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            table.Controls.Add(host, 1, 0);

            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11, FontStyle.Regular), // ✅ Reduced from 13 to match login form
                ForeColor = Color.Gray,
                BackColor = Color.White,
                Text = placeholder,
                Width = 280, // ✅ Reduced from 390 to match login form
                Location = new Point(0, 12) // ✅ Adjusted vertical position
            };
            host.Controls.Add(textBox);

            border.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(200, 200, 200), 2);

                var r = new Rectangle(1, 1, border.Width - 2, border.Height - 2);
                e.Graphics.DrawPath(pen, CreateRoundedRect(r, 12)); // ✅ Reduced radius from 14 to match login form
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
                    if (placeholder is "Mật khẩu" or "Xác nhận mật khẩu")
                        tb.UseSystemPasswordChar = true;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (placeholder is "Mật khẩu" or "Xác nhận mật khẩu")
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