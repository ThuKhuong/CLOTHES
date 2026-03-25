namespace CLOTHES
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip = new MenuStrip();
            menuHeThong = new ToolStripMenuItem();
            menuDoiMatKhau = new ToolStripMenuItem();
            menuSep1 = new ToolStripSeparator();
            menuDangXuat = new ToolStripMenuItem();
            menuThoat = new ToolStripMenuItem();
            menuDanhMuc = new ToolStripMenuItem();
            menuLoaiSanPham = new ToolStripMenuItem();
            menuNhaCungCap = new ToolStripMenuItem();
            menuKhachHang = new ToolStripMenuItem();
            menuSanPham = new ToolStripMenuItem();
            menuQuanLySanPham = new ToolStripMenuItem();
            menuKho = new ToolStripMenuItem();
            menuNhapHang = new ToolStripMenuItem();
            menuXemTonKho = new ToolStripMenuItem();
            menuBanHang = new ToolStripMenuItem();
            menuTaoHoaDon = new ToolStripMenuItem();
            menuKhuyenMai = new ToolStripMenuItem();
            menuBaoCao = new ToolStripMenuItem();
            menuDoanhThu = new ToolStripMenuItem();
            menuTopSanPham = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            lblUser = new ToolStripStatusLabel();
            lblTime = new ToolStripStatusLabel();
            panelMain = new Panel();
            timer = new System.Windows.Forms.Timer(components);

            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // menuStrip
            menuStrip.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            menuStrip.Items.AddRange(new ToolStripItem[] {
                menuHeThong,
                menuDanhMuc,
                menuSanPham,
                menuKho,
                menuBanHang,
                menuBaoCao
            });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1024, 28);

            // menuHeThong
            menuHeThong.DropDownItems.AddRange(new ToolStripItem[] {
                menuDoiMatKhau,
                menuSep1,
                menuDangXuat,
                menuThoat
            });
            menuHeThong.Name = "menuHeThong";
            menuHeThong.Size = new Size(85, 24);
            menuHeThong.Text = "&Hệ thống";

            // menuDoiMatKhau
            menuDoiMatKhau.Name = "menuDoiMatKhau";
            menuDoiMatKhau.Size = new Size(180, 26);
            menuDoiMatKhau.Text = "&Đổi mật khẩu";

            // menuSep1
            menuSep1.Name = "menuSep1";
            menuSep1.Size = new Size(177, 6);

            // menuDangXuat
            menuDangXuat.Name = "menuDangXuat";
            menuDangXuat.Size = new Size(180, 26);
            menuDangXuat.Text = "Đăng &xuất";
            menuDangXuat.Click += menuDangXuat_Click;

            // menuThoat
            menuThoat.Name = "menuThoat";
            menuThoat.Size = new Size(180, 26);
            menuThoat.Text = "&Thoát";
            menuThoat.Click += menuThoat_Click;

            // menuDanhMuc
            menuDanhMuc.DropDownItems.AddRange(new ToolStripItem[] {
                menuLoaiSanPham,
                menuNhaCungCap,
                menuKhachHang
            });
            menuDanhMuc.Name = "menuDanhMuc";
            menuDanhMuc.Size = new Size(90, 24);
            menuDanhMuc.Text = "&Danh mục";

            // menuLoaiSanPham
            menuLoaiSanPham.Name = "menuLoaiSanPham";
            menuLoaiSanPham.Size = new Size(194, 26);
            menuLoaiSanPham.Text = "&Loại sản phẩm";
            menuLoaiSanPham.Click += menuLoaiSanPham_Click;

            // menuNhaCungCap
            menuNhaCungCap.Name = "menuNhaCungCap";
            menuNhaCungCap.Size = new Size(194, 26);
            menuNhaCungCap.Text = "&Nhà cung cấp";

            // menuKhachHang
            menuKhachHang.Name = "menuKhachHang";
            menuKhachHang.Size = new Size(194, 26);
            menuKhachHang.Text = "&Khách hàng";

            // menuSanPham
            menuSanPham.DropDownItems.AddRange(new ToolStripItem[] {
                menuQuanLySanPham
            });
            menuSanPham.Name = "menuSanPham";
            menuSanPham.Size = new Size(88, 24);
            menuSanPham.Text = "&Sản phẩm";

            // menuQuanLySanPham
            menuQuanLySanPham.Name = "menuQuanLySanPham";
            menuQuanLySanPham.Size = new Size(206, 26);
            menuQuanLySanPham.Text = "&Quản lý sản phẩm";

            // menuKho
            menuKho.DropDownItems.AddRange(new ToolStripItem[] {
                menuNhapHang,
                menuXemTonKho
            });
            menuKho.Name = "menuKho";
            menuKho.Size = new Size(48, 24);
            menuKho.Text = "&Kho";

            // menuNhapHang
            menuNhapHang.Name = "menuNhapHang";
            menuNhapHang.Size = new Size(174, 26);
            menuNhapHang.Text = "&Nhập hàng";
            menuNhapHang.Click += menuNhapHang_Click;

            // menuXemTonKho
            menuXemTonKho.Name = "menuXemTonKho";
            menuXemTonKho.Size = new Size(174, 26);
            menuXemTonKho.Text = "&Xem tồn kho";

            // menuBanHang
            menuBanHang.DropDownItems.AddRange(new ToolStripItem[] {
                menuTaoHoaDon,
                menuKhuyenMai
            });
            menuBanHang.Name = "menuBanHang";
            menuBanHang.Size = new Size(85, 24);
            menuBanHang.Text = "&Bán hàng";

            // menuTaoHoaDon
            menuTaoHoaDon.Name = "menuTaoHoaDon";
            menuTaoHoaDon.Size = new Size(173, 26);
            menuTaoHoaDon.Text = "&Tạo hóa đơn";

            // menuKhuyenMai
            menuKhuyenMai.Name = "menuKhuyenMai";
            menuKhuyenMai.Size = new Size(173, 26);
            menuKhuyenMai.Text = "&Khuyến mãi";

            // menuBaoCao
            menuBaoCao.DropDownItems.AddRange(new ToolStripItem[] {
                menuDoanhThu,
                menuTopSanPham
            });
            menuBaoCao.Name = "menuBaoCao";
            menuBaoCao.Size = new Size(77, 24);
            menuBaoCao.Text = "&Báo cáo";

            // menuDoanhThu
            menuDoanhThu.Name = "menuDoanhThu";
            menuDoanhThu.Size = new Size(180, 26);
            menuDoanhThu.Text = "&Doanh thu";

            // menuTopSanPham
            menuTopSanPham.Name = "menuTopSanPham";
            menuTopSanPham.Size = new Size(180, 26);
            menuTopSanPham.Text = "&Top sản phẩm";

            // statusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] {
                lblUser,
                lblTime
            });
            statusStrip.Location = new Point(0, 643);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1024, 22);

            // lblUser
            lblUser.Name = "lblUser";

            // lblTime
            lblTime.Name = "lblTime";
            lblTime.Spring = true;
            lblTime.TextAlign = ContentAlignment.MiddleRight;

            // panelMain
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 28);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1024, 615);

            // timer
            timer.Interval = 1000;
            timer.Tick += timer_Tick;

            // FrmMain
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(panelMain);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            Font = new Font("Segoe UI", 9F);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1000, 700);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý cửa hàng quần áo";
            WindowState = FormWindowState.Maximized;
            Load += FrmMain_Load;
            FormClosing += FrmMain_FormClosing;

            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem menuHeThong;
        private ToolStripMenuItem menuDoiMatKhau;
        private ToolStripSeparator menuSep1;
        private ToolStripMenuItem menuDangXuat;
        private ToolStripMenuItem menuThoat;
        private ToolStripMenuItem menuDanhMuc;
        private ToolStripMenuItem menuLoaiSanPham;
        private ToolStripMenuItem menuNhaCungCap;
        private ToolStripMenuItem menuKhachHang;
        private ToolStripMenuItem menuSanPham;
        private ToolStripMenuItem menuQuanLySanPham;
        private ToolStripMenuItem menuKho;
        private ToolStripMenuItem menuNhapHang;
        private ToolStripMenuItem menuXemTonKho;
        private ToolStripMenuItem menuBanHang;
        private ToolStripMenuItem menuTaoHoaDon;
        private ToolStripMenuItem menuKhuyenMai;
        private ToolStripMenuItem menuBaoCao;
        private ToolStripMenuItem menuDoanhThu;
        private ToolStripMenuItem menuTopSanPham;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblUser;
        private ToolStripStatusLabel lblTime;
        private Panel panelMain;
        private System.Windows.Forms.Timer timer;
    }
}
