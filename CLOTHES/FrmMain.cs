using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QLBH.DTO;

namespace CLOTHES
{
    public partial class FrmMain : Form
    {
        private readonly NguoiDungDto _currentUser;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel userInfoPanel;
        private Button selectedButton;

        private const bool ShowExperimentalMenuItems = false;

        private bool IsRole(string role) =>
            _currentUser.VaiTro.Equals(role, StringComparison.OrdinalIgnoreCase);

        private bool IsAdmin => IsRole("ADMIN");
        private bool IsQuanLy => IsRole("QUANLY");
        private bool IsNhanVienNhap => IsRole("NHANVIENNHAP");
        private bool IsNhanVienBanHang => IsRole("NHANVIENBANHANG");

        public FrmMain(NguoiDungDto user)
        {
            InitializeComponent();
            _currentUser = user;
            
           
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            SetDefaultFont(this);
            
            CreateModernSidebar();
        }

        private void SetDefaultFont(Control parent)
        {
            // Recursively set font for all controls to ensure Vietnamese text displays correctly
            foreach (Control control in parent.Controls)
            {
                // Preserve controls that intentionally use emoji/icon fonts.
                // Overriding them to Segoe UI can make the icon glyphs disappear.
                var family = control.Font?.FontFamily?.Name ?? string.Empty;
                if (!family.Contains("Emoji", StringComparison.OrdinalIgnoreCase) &&
                    !family.Contains("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    control.Font = new Font("Segoe UI", control.Font.Size, control.Font.Style);
                }
                SetDefaultFont(control);
            }
        }

        private void CreateModernSidebar()
        {
            // Hide the original menu strip
            menuStrip.Visible = false;

            // Remove previous panels if re-created
            if (sidebarPanel != null && Controls.Contains(sidebarPanel))
                Controls.Remove(sidebarPanel);
            if (contentPanel != null && Controls.Contains(contentPanel))
                Controls.Remove(contentPanel);

            // Create sidebar panel
            sidebarPanel = new Panel
            {
                Width = 300,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(88, 86, 214),
                Padding = new Padding(0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            // A dedicated container ensures Dock layout order (TOP then FILL) is stable.
            var sidebarContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            sidebarPanel.Controls.Add(sidebarContainer);

            // Create content panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Dock order matters: add LEFT first, then FILL.
            Controls.Add(sidebarPanel);
            Controls.Add(contentPanel);

            // Never bring sidebar to front; it can visually overlap the fill area.
            contentPanel.BringToFront();

            // Keep status strip always on main form and docked bottom
            statusStrip.Parent = this;
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.BringToFront();

            // Host main area inside content panel
            panelMain.Parent = contentPanel;
            panelMain.Dock = DockStyle.Fill;
            panelMain.Margin = new Padding(0);
            panelMain.Padding = new Padding(0);

            CreateUserInfo(sidebarContainer);
            CreateNavigationButtons(sidebarContainer);
            ApplyGradientToSidebar();

            // Force layout recalculation
            PerformLayout();
            contentPanel.PerformLayout();
        }

        private void CreateUserInfo(Control parent)
        {
            userInfoPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(15, 10, 15, 10),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular) // Ensure Vietnamese support
            };

            var userCircle = new Panel
            {
                Size = new Size(50, 50),
                Location = new Point(15, 15),
                BackColor = Color.White
            };
            userCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // Improve text rendering
                using (var brush = new SolidBrush(Color.FromArgb(255, 107, 107)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 49, 49);
                }
                
                // Draw user initials
                string initials = GetUserInitials(_currentUser.TenND);
                using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var size = e.Graphics.MeasureString(initials, font);
                    var x = (50 - size.Width) / 2;
                    var y = (50 - size.Height) / 2;
                    e.Graphics.DrawString(initials, font, brush, x, y);
                }
            };

            var userNameLabel = new Label
            {
                Text = _currentUser.TenND,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(75, 20),
                AutoSize = true
            };

            var userRoleLabel = new Label
            {
                Text = _currentUser.VaiTro,
                ForeColor = Color.FromArgb(200, 200, 255),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Location = new Point(75, 42),
                AutoSize = true
            };

            userInfoPanel.Controls.AddRange(new Control[] { userCircle, userNameLabel, userRoleLabel });
            parent.Controls.Add(userInfoPanel);
        }

        private void CreateNavigationButtons(Control parent)
        {
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 10, 10, 10),
                AutoScroll = true,
                AutoScrollMargin = new Size(0, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            var topSpacer = new Panel
            {
                Dock = DockStyle.Top,
                Height = userInfoPanel?.Height ?? 80,
                BackColor = Color.Transparent
            };
            buttonsPanel.Controls.Add(topSpacer);

            var list = new System.Collections.Generic.List<(string Text, string Icon, int Indent, Action Action)>();
            list.Add(("TRANG CHỦ", "🏠", 0, ShowDashboard));

            if (IsAdmin || IsQuanLy || IsNhanVienBanHang)
            {
                list.Add(("BÁN HÀNG", "📦", 0, () => OpenChildForm(() => new FrmBanHang(_currentUser))));
                list.Add(("ĐƠN HÀNG", "📄", 1, () => OpenChildForm<FrmHoaDon>()));
                list.Add(("KHÁCH HÀNG", "👤", 0, () => OpenChildForm<FrmKhachHang>()));
            }

            if (IsAdmin || IsQuanLy || IsNhanVienNhap || IsNhanVienBanHang)
            {
                list.Add(("SẢN PHẨM", "🛒", 0, () => OpenChildForm<FrmSanPham>()));
            }

            if (IsAdmin || IsQuanLy || IsNhanVienNhap)
            {
                list.Add(("NHẬP HÀNG", "📥", 0, () => OpenChildForm(() => new FrmNhapHang(_currentUser))));
                list.Add(("PHIẾU NHẬP", "🧾", 0, () => OpenChildForm<FrmPhieuNhap>()));
            }

            if (IsAdmin || IsQuanLy)
            {
                list.Add(("KHUYẾN MÃI", "🏷", 0, () => OpenChildForm<FrmKhuyenMai>()));
                list.Add(("THỐNG KÊ", "📊", 0, () => OpenChildForm<FrmThongKe>()));
            }

            if (IsAdmin)
            {
                list.Add(("NGƯỜI DÙNG", "👥", 0, () => OpenChildForm(() => new FrmNguoiDung(_currentUser))));
            }

            if (ShowExperimentalMenuItems && IsAdmin)
            {
                list.Add(("QUẢN LÝ", "⚙", 0, () => ShowUnderDevelopment("Quản lý")));
                list.Add(("CÀI ĐẶT", "🔧", 0, () => ShowUnderDevelopment("Cài đặt")));
                list.Add(("THÔNG BÁO", "🔔", 0, () => ShowUnderDevelopment("Thông báo")));
                list.Add(("BÁO LỖI", "❗", 0, () => ShowUnderDevelopment("Báo lỗi")));
            }

            list.Add(("ĐĂNG XUẤT", "🚪", 0, Logout));

            var menuItems = list.ToArray();

            int yPos = topSpacer.Height;
            for (int i = 0; i < menuItems.Length; i++)
            {
                var menuItem = menuItems[i];
                var button = CreateSidebarButton(menuItem.Text, menuItem.Icon, menuItem.Action, menuItem.Indent);
                button.Location = new Point(buttonsPanel.Padding.Left, yPos);
                button.Width = buttonsPanel.ClientSize.Width - buttonsPanel.Padding.Left - buttonsPanel.Padding.Right;
                button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                if (i == 0)
                {
                    SelectButton(button);
                    menuItem.Action.Invoke();
                }

                buttonsPanel.Controls.Add(button);
                yPos += button.Height + 5;
            }

            buttonsPanel.AutoScrollMinSize = new Size(0, yPos + buttonsPanel.Padding.Bottom);
            parent.Controls.Add(buttonsPanel);
        }

        private Button CreateSidebarButton(string text, string icon, Action clickAction, int indentLevel = 0)
        {
            int indentPx = 16 + (indentLevel * 26);
            var iconText = string.IsNullOrWhiteSpace(icon) ? "" : $"{icon}   ";
            var button = new Button
            {
                Text = $"{iconText}{text}",
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(indentPx, 0, 0, 0),    // tăng padding + hỗ trợ menu con
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),

                UseCompatibleTextRendering = true
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(120, 255, 255, 255);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 255, 255, 255);

            button.MouseEnter += (s, e) =>
            {
                if (button != selectedButton)
                    button.BackColor = Color.FromArgb(60, 255, 255, 255);
            };

            button.MouseLeave += (s, e) =>
            {
                if (button != selectedButton)
                    button.BackColor = Color.Transparent;
            };

            button.Click += (s, e) =>
            {
                SelectButton(button);
                clickAction?.Invoke();
            };

            return button;
        }

        private void SelectButton(Button button)
        {
            // Reset previous selection
            if (selectedButton != null)
            {
                selectedButton.BackColor = Color.Transparent;
                selectedButton.ForeColor = Color.White;
                selectedButton.Font = new Font("Segoe UI Emoji", 11, FontStyle.Regular);
            }

            // Set new selection
            selectedButton = button;
            button.BackColor = Color.FromArgb(150, 255, 255, 255);
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold); // Keep icons + make selected button bold
        }

        private void ApplyGradientToSidebar()
        {
            sidebarPanel.Paint += (s, e) =>
            {
                // During initialization/resize the panel can briefly have 0 height/width.
                // LinearGradientBrush throws if the rectangle has 0 in any dimension.
                if (sidebarPanel.ClientRectangle.Width <= 0 || sidebarPanel.ClientRectangle.Height <= 0)
                    return;

                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // Better text rendering
                using (var brush = new LinearGradientBrush(
                    sidebarPanel.ClientRectangle,
                    Color.FromArgb(88, 86, 214),    // Top color
                    Color.FromArgb(118, 75, 255),   // Bottom color
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, sidebarPanel.ClientRectangle);
                }
            };
        }

        private string GetUserInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "U";
            
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            
            return fullName[0].ToString().ToUpper();
        }

        private void ShowDashboard()
        {
            ClearContentPanel();

            var dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            var titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(30, 30),
                UseCompatibleTextRendering = false
            };

            var welcomeCard = CreateDashboardCard(
                "Chào mừng trở lại",
                $"Xin chào {_currentUser.TenND}, hôm nay bạn có gì mới?\nHệ thống quản lý cửa hàng quần áo hoạt động tốt.",
                Color.FromArgb(139, 92, 246),
                "👋"
            );
            welcomeCard.Location = new Point(30, 100);
            welcomeCard.Size = new Size(500, 120);

            dashboardPanel.Controls.AddRange(new Control[] { titleLabel, welcomeCard });
            panelMain.Controls.Add(dashboardPanel);
        }

        private Panel CreateDashboardCard(string title, string subtitle, Color backgroundColor, string icon)
        {
            var card = new Panel
            {
                Size = new Size(400, 100),
                BackColor = backgroundColor,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using var path = CreateRoundedRectangle(card.ClientRectangle, 12);
                using var brush = new SolidBrush(backgroundColor);
                e.Graphics.FillPath(brush, path);
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 24, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15),
                UseCompatibleTextRendering = false
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(70, 15),
                UseCompatibleTextRendering = false
            };

            var subtitleLabel = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 255),
                AutoSize = true,
                Location = new Point(70, 45),
                MaximumSize = new Size(300, 50),
                UseCompatibleTextRendering = false
            };

            card.Controls.AddRange(new Control[] { iconLabel, titleLabel, subtitleLabel });
            return card;
        }

        private Panel CreateStatsCard(string value, string label, string icon, Color color)
        {
            var card = new Panel
            {
                Margin = new Padding(5),
                BackColor = Color.White,
                Padding = new Padding(20),
                Height = 130,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using var path = CreateRoundedRectangle(card.ClientRectangle, 16);
                using var brush = new LinearGradientBrush(card.ClientRectangle, color,
                    Color.FromArgb(color.A, Math.Min(255, color.R + 30), Math.Min(255, color.G + 30), Math.Min(255, color.B + 30)),
                    LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillPath(brush, path);
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 24, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15),
                UseCompatibleTextRendering = false
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 50),
                UseCompatibleTextRendering = false
            };

            var labelText = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(240, 240, 255),
                AutoSize = true,
                Location = new Point(20, 85),
                UseCompatibleTextRendering = false
            };

            card.Controls.AddRange(new Control[] { iconLabel, valueLabel, labelText });
            return card;
        }

        private Panel CreateCustomerCard(string name, string phone, string category, Color accentColor)
        {
            var card = new Panel
            {
                Size = new Size(580, 80),
                BackColor = Color.White,
                Margin = new Padding(0, 5, 0, 5),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using var path = CreateRoundedRectangle(card.ClientRectangle, 12);
                using var brush = new SolidBrush(Color.White);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(Color.FromArgb(200, 200, 200), 1);
                e.Graphics.DrawPath(pen, path);
            };

            var avatar = new Panel
            {
                Size = new Size(50, 50),
                Location = new Point(15, 15),
                BackColor = accentColor
            };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using var brush = new SolidBrush(accentColor);
                e.Graphics.FillEllipse(brush, 0, 0, 49, 49);

                using var font = new Font("Segoe UI Emoji", 20, FontStyle.Regular);
                using var textBrush = new SolidBrush(Color.White);
                var size = e.Graphics.MeasureString("❓", font);
                var x = (50 - size.Width) / 2;
                var y = (50 - size.Height) / 2;
                e.Graphics.DrawString("❓", font, textBrush, x, y);
            };

            var nameLabel = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(75, 15),
                AutoSize = true,
                UseCompatibleTextRendering = false
            };

            var phoneLabel = new Label
            {
                Text = phone,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(75, 35),
                AutoSize = true,
                UseCompatibleTextRendering = false
            };

            var categoryLabel = new Label
            {
                Text = category,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(75, 50),
                AutoSize = true,
                UseCompatibleTextRendering = false
            };

            card.Controls.AddRange(new Control[] { avatar, nameLabel, phoneLabel, categoryLabel });
            return card;
        }


        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void ShowUnderDevelopment(string feature)
        {
            ClearContentPanel();
            
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular) // Ensure Vietnamese support
            };

            var messageLabel = new Label
            {
                Text = $"{feature}\n\nTính năng đang được phát triển...\nVui lòng quay lại sau!",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false,
                UseCompatibleTextRendering = false // Better Unicode support for Vietnamese
            };

            panel.Controls.Add(messageLabel);
            panelMain.Controls.Add(panel);
        }

        private void ClearContentPanel()
        {
            panelMain.Controls.Clear();
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        // Keep existing methods
        private void FrmMain_Load(object sender, EventArgs e)
        {
            // Hi?n th? thông tin user
            lblUser.Text = $"Đăng nhập: {_currentUser.TenND} ({_currentUser.VaiTro})";
            lblUser.Font = new Font("Segoe UI", lblUser.Font.Size, lblUser.Font.Style); // Fix Vietnamese text in status bar
            
            // B?t ??u timer ?? hi?n th? gi?
            timer.Start();
            
            // Phân quy?n menu theo vai trò
            SetupPermissions();
        }

        private void SetupPermissions()
        {
            menuBaoCao.Visible = IsAdmin || IsQuanLy;

            menuNhapHang.Visible = IsAdmin || IsQuanLy || IsNhanVienNhap;
            menuKho.Visible = IsAdmin || IsQuanLy || IsNhanVienNhap;

            menuBanHang.Visible = IsAdmin || IsQuanLy || IsNhanVienBanHang;
            menuKhachHang.Visible = IsAdmin || IsQuanLy || IsNhanVienBanHang;

            menuSanPham.Visible = IsAdmin || IsQuanLy || IsNhanVienNhap || IsNhanVienBanHang;
            menuLoaiSanPham.Visible = IsAdmin || IsQuanLy || IsNhanVienNhap || IsNhanVienBanHang;

            menuKhuyenMai.Visible = IsAdmin || IsQuanLy;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            lblTime.Font = new Font("Segoe UI", lblTime.Font.Size, lblTime.Font.Style); // Fix Vietnamese text in status bar
        }

        private void menuDangXuat_Click(object sender, EventArgs e)
        {
            Logout();
        }

        private void menuThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát ứng dụng?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }

        private void menuLoaiSanPham_Click(object sender, EventArgs e)
        {
            OpenChildForm<FrmSanPham>();
        }

        private void menuNhapHang_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FrmNhapHang(_currentUser));
        }

        private void OpenChildForm<T>() where T : Form, new()
        {
            ClearContentPanel();

            // A dedicated host panel avoids weird Dock/Location interactions
            var host = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = panelMain.BackColor
            };
            panelMain.Controls.Add(host);

            var childForm = new T
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            SetDefaultFont(childForm);

            host.Controls.Add(childForm);
            childForm.Show();
        }

        private void OpenChildForm(Func<Form> factory)
        {
            ClearContentPanel();

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = panelMain.BackColor
            };
            panelMain.Controls.Add(host);

            var childForm = factory();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            childForm.StartPosition = FormStartPosition.Manual;
            childForm.Location = new Point(0, 0);
            childForm.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            SetDefaultFont(childForm);
            host.Controls.Add(childForm);
            childForm.Show();
        }
    }
}