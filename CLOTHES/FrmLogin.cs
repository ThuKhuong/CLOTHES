using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES
{
    public partial class FrmLogin : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;

        private TextBox? _txtUsername;
        private TextBox? _txtPassword;
        private readonly AuthService _authService = new();

        public FrmLogin()
        {
            InitializeComponent();
            BuildUI();
           #if DEBUG
            // Dev convenience: auto login on startup.
            Shown += async (s, e) =>
            {
                try
                {
                    var (success, _, user) = _authService.Login("Khuong", "123");
                    if (success && user != null)
                    {
                        Hide();
                        using var frmMain = new FrmMain(user);
                        var result = frmMain.ShowDialog();
                        if (result == DialogResult.Abort)
                        {
                            Show();
                            if (_txtPassword != null)
                            {
                                _txtPassword.Text = "Mật khẩu";
                                _txtPassword.ForeColor = Color.Gray;
                                _txtPassword.UseSystemPasswordChar = false;
                            }
                        }
                        else
                        {
                            DialogResult = result;
                            Close();
                        }
                    }
                }
                catch
                {
                    // Ignore dev auto-login errors.
                }
            };
            #endif
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
                m.Result = (IntPtr)HTCAPTION;
        }

        private void BuildUI()
        {
            // ===== Form =====
            this.Text = "Login";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(253, 248, 244);
            this.ClientSize = new Size(900, 600); // ✅ Thu nhỏ từ 1300x760
            this.DoubleBuffered = true;
            ApplyRoundedCorners(this, 28);

            // ===== Panel phải =====
            var right = new Panel
            {
                Dock = DockStyle.Right,
                Width = 450, // ✅ Thu nhỏ từ 650
                BackColor = this.BackColor
            };
            this.Controls.Add(right);

            // ===== Panel trái =====
            var left = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = this.BackColor
            };
            this.Controls.Add(left);

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resource.login
            };
            left.Controls.Add(pic);

            // ===== Buttons =====
            var btnMin = CreateTopButton("—");
            btnMin.Location = new Point(right.Width - 80, 14);
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            var btnClose = CreateTopButton("✕");
            btnClose.Location = new Point(right.Width - 44, 14);
            btnClose.Click += (s, e) => Close();

            right.Controls.Add(btnMin);
            right.Controls.Add(btnClose);

            // ===== Title =====
            var lblTitle = new Label
            {
                Text = "Chào mừng trở lại",
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

            // ===== Username =====
       var userRow = CreateInputRow("👤", "Tên đăng nhập", out TextBox txtUser);
            userRow.Location = new Point(50, 180);
            _txtUsername = txtUser;
            right.Controls.Add(userRow);

            // ===== Password =====
        var passRow = CreateInputRow("🔑", "Mật khẩu", out TextBox txtPass, true);
            passRow.Location = new Point(50, 270); 
            _txtPassword = txtPass;
            right.Controls.Add(passRow);

        SetupPlaceholder(txtUser, "Tên đăng nhập");

            // ===== Login Button =====
            var btnLogin = new Button
            {
                Text = "Đăng nhập",
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                ForeColor = Color.White,
                BackColor = Color.FromArgb(92, 85, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(280, 55), 
                Location = new Point(85, 360), 
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Region = new Region(CreateRoundedRect(btnLogin.ClientRectangle, 28));
            right.Controls.Add(btnLogin);

            // ===== Login Event =====
            btnLogin.Click += async (s, e) =>
            {
                string username = GetTextBoxValue(_txtUsername, "Tên đăng nhập");
                string password = GetTextBoxValue(_txtPassword, "Mật khẩu");
                username = username.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng nhập.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var (success, message, user) = _authService.Login(username, password);

                    if (success && user != null)
                    {
                        this.Hide();
                        using var frmMain = new FrmMain(user);
                        var result = frmMain.ShowDialog();

                        if (result == DialogResult.Abort)
                        {
                            // User logged out, show login again
                            this.Show();
                            _txtPassword.Text = "Mật khẩu";
                            _txtPassword.ForeColor = Color.Gray;
                            _txtPassword.UseSystemPasswordChar = false;
                         // Keep login dialog open for the next login
                            return;
                        }

                        // Main form closed normally -> end login dialog
                        this.DialogResult = result;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(message, "Lỗi đăng nhập",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // ===== Links =====
            var linkForgot = new LinkLabel
            {
                Text = "Quên mật khẩu",
                AutoSize = true,
                Font = new Font("Segoe UI", 10), 
                LinkColor = Color.FromArgb(92, 85, 255),
                Location = new Point(120, 480) 
            };
            linkForgot.LinkClicked += LinkForgot_LinkClicked;
            right.Controls.Add(linkForgot);
        }

        private string GetTextBoxValue(TextBox? textBox, string placeholder)
        {
            if (textBox == null) return "";
            return textBox.Text == placeholder ? "" : textBox.Text.Trim();
        }

        private void LinkForgot_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            using var frm = new FrmForgotPassword();
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                if (_txtUsername != null && !string.IsNullOrWhiteSpace(frm.ResetUsername))
                {
                    _txtUsername.Text = frm.ResetUsername;
                    _txtUsername.ForeColor = Color.FromArgb(40, 40, 40);
                }

                if (_txtPassword != null)
                {
                    _txtPassword.Text = "Mật khẩu";
                    _txtPassword.ForeColor = Color.Gray;
                    _txtPassword.UseSystemPasswordChar = false;
                    _txtPassword.Focus();
                }
            }
        }

        // ===== Input Row =====
  private Panel CreateInputRow(string icon, string placeholder, out TextBox textBox, bool withToggle = false)
        {
            var row = new Panel
            {
                Size = new Size(350, 75), 
                BackColor = Color.Transparent
            };

            // Khung viền
            var border = new Panel
            {
                Size = new Size(350, 65), 
                Location = new Point(0, 10),
                BackColor = Color.White
            };
            row.Controls.Add(border);

            var lblIcon = new Label
            {
                Text = icon,
                AutoSize = false,
                Size = new Size(24, border.Height),
                Location = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            border.Controls.Add(lblIcon);

            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                Text = placeholder,
                Width = border.Width - (withToggle ? 86 : 56),
                Location = new Point(44, (border.Height - 24) / 2)
            };
            var tb = textBox;
            border.Controls.Add(textBox);

            if (withToggle)
            {
                var isVisible = false;
             var btnEye = new Button
                {
                    Text = "🙈",
                    Font = new Font("Segoe UI Emoji", 11, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.Gray,
                    Size = new Size(28, 28),
                    Location = new Point(border.Width - 34, (border.Height - 28) / 2),
                    TabStop = false,
                    Cursor = Cursors.Hand
                };
                btnEye.FlatAppearance.BorderSize = 0;
                btnEye.Click += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Focus();
                        return;
                    }

             isVisible = !isVisible;
                tb.UseSystemPasswordChar = !isVisible;
                btnEye.Text = isVisible ? "👁" : "🙈";
                };

                SetupPlaceholder(tb, placeholder, isPlaceholder =>
                {
                    if (isPlaceholder)
                    {
                  isVisible = false;
                    btnEye.Text = "🙈";
                    }
                });

                border.Controls.Add(btnEye);
            }

            // Vẽ viền bo góc
            border.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(200, 200, 200), 2);

                var r = new Rectangle(1, 1, border.Width - 2, border.Height - 2);
                e.Graphics.DrawPath(pen, CreateRoundedRect(r, 12));
            };

            return row;
        }

        // ===== Placeholder =====
   private void SetupPlaceholder(TextBox tb, string placeholder, Action<bool>? placeholderStateChanged = null)
        {
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                    if (placeholder == "Mật khẩu")
                        tb.UseSystemPasswordChar = true;
               placeholderStateChanged?.Invoke(false);
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (placeholder == "Mật khẩu")
                        tb.UseSystemPasswordChar = false;
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
               placeholderStateChanged?.Invoke(true);
                }
            };
        }

        // ===== Helpers =====
        private Button CreateTopButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(92, 85, 255),
                BackColor = this.BackColor,
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

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
