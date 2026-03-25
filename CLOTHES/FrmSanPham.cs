using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES
{
    public partial class FrmSanPham : Form
    {
        private readonly SanPhamService _spService = new();

        private const int LowStockThreshold = 5;

        private DataTable? _spTable;
        private DataTable? _loaiTable;

        private string? _selectedMaSp;
        private Panel? _selectedCard;

        public FrmSanPham()
        {
            InitializeComponent();
            Font = new Font("Segoe UI", 9F);
            SetDefaultFont(this);
        }

        private void SetDefaultFont(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                c.Font = new Font("Segoe UI", c.Font.Size, c.Font.Style);

                if (c is Label l)
                    l.UseCompatibleTextRendering = false;
                else if (c is Button b)
                    b.UseCompatibleTextRendering = false;

                SetDefaultFont(c);
            }
        }

        private void FrmSanPham_Load(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            cboSort.SelectedIndex = 0;

            LoadLoai();
            LoadSanPham();

            cboLoai.SelectedIndexChanged += (s, _) => ApplyFilter();
            cboSort.SelectedIndexChanged += (s, _) => ApplyFilter();
           chkLowStockOnly.CheckedChanged += (s, _) => ApplyFilter();
        }

        private void LoadLoai()
        {
            _loaiTable = _spService.GetAllLoai();

            var dt = _loaiTable.Copy();
            var row = dt.NewRow();
            row["MALOAI"] = DBNull.Value;
            row["TENLOAI"] = "Tất cả";
            dt.Rows.InsertAt(row, 0);

            cboLoai.DisplayMember = "TENLOAI";
            cboLoai.ValueMember = "MALOAI";
            cboLoai.DataSource = dt;
            cboLoai.SelectedIndex = 0;
        }

        private void LoadSanPham()
        {
            _spTable = _spService.GetAll();
            ApplyFilter();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            if (_spTable == null)
                return;

            var keyword = (txtSearch.Text ?? string.Empty).Trim();
            var maLoai = cboLoai.SelectedValue == DBNull.Value ? null : cboLoai.SelectedValue?.ToString();

            // Re-query for correct SQL filtering (avoid DataView unicode quirks)
            _spTable = _spService.Search(keyword, maLoai);

            var rows = _spTable.Rows.Cast<DataRow>().ToList();

            if (chkLowStockOnly.Checked)
            {
                rows = rows
                    .Where(r => r.Table.Columns.Contains("TONGTON") && r["TONGTON"] != DBNull.Value
                            && Convert.ToInt32(r["TONGTON"]) <= LowStockThreshold)
                    .ToList();
            }

            rows = cboSort.SelectedIndex == 1
                ? rows.OrderBy(r => r["MASP"].ToString()).ToList()
                : rows.OrderBy(r => r["TENSP"].ToString()).ToList();

            RenderCards(rows);
        }

        private void RenderCards(List<DataRow> rows)
        {
            panelGrid.SuspendLayout();
            panelGrid.Controls.Clear();

            _selectedMaSp = null;
            _selectedCard = null;

            int cardWidth = 260;
            int cardHeight = 280;

            int gap = 24;

            foreach (var r in rows)
            {
                var card = (Panel)CreateProductCard(r, cardWidth, cardHeight);
                card.Margin = new Padding(0, 0, gap, 24);
                panelGrid.Controls.Add(card);
            }

            panelGrid.ResumeLayout();
        }

        private Control CreateProductCard(DataRow r, int w, int h)
        {
            var maSp = r["MASP"].ToString() ?? string.Empty;
            var tenSp = r["TENSP"].ToString() ?? string.Empty;
            var hinh = r["HINHSP"] == DBNull.Value ? null : r["HINHSP"].ToString();

            var tongTon = 0;
            if (r.Table.Columns.Contains("TONGTON") && r["TONGTON"] != DBNull.Value)
                tongTon = Convert.ToInt32(r["TONGTON"]);

            string priceText;
            if (r.Table.Columns.Contains("GIATU") && r["GIATU"] != DBNull.Value)
            {
                var gia = Convert.ToDecimal(r["GIATU"]);
                priceText = $"{gia:N0} VNĐ";
            }
            else
            {
                priceText = "--";
            }

            var card = new Panel
            {
                Size = new Size(w, h),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = maSp
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var isSelected = ReferenceEquals(card, _selectedCard);
                var borderColor = isSelected ? Color.FromArgb(79, 70, 229) : Color.FromArgb(229, 231, 235);
                var borderWidth = isSelected ? 2f : 1f;

                using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 18);
                using var br = new SolidBrush(Color.White);
                e.Graphics.FillPath(br, path);

                using var pen = new Pen(borderColor, borderWidth);
                e.Graphics.DrawPath(pen, path);
            };

            var pic = new PictureBox
            {
                Location = new Point(22, 18),
                Size = new Size(w - 44, 150),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            pic.Image = LoadImageSafe(hinh);

            var lblName = new Label
            {
                Text = tenSp,
                Location = new Point(22, 178),
                Size = new Size(w - 44, 44),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            var lblPrice = new Label
            {
                Text = priceText,
                Location = new Point(22, 228),
                Size = new Size(w - 44, 24),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            var lblStock = new Label
            {
                Text = $"Tồn: {tongTon}",
                Location = new Point(22, 252),
                Size = new Size(w - 44, 22),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = tongTon <= LowStockThreshold ? Color.FromArgb(220, 38, 38) : Color.FromArgb(100, 116, 139)
            };

            void select(object? _, EventArgs __)
            {
                SelectCard(card);
            }

            void open(object? _, EventArgs __)
            {
                SelectCard(card);
                OpenDetail(maSp);
            }

            // Single click selects; double click opens detail.
            card.Click += select;
            pic.Click += select;
            lblName.Click += select;
            lblPrice.Click += select;

            card.DoubleClick += open;
            pic.DoubleClick += open;
            lblName.DoubleClick += open;
            lblPrice.DoubleClick += open;
            lblStock.Click += select;
            lblStock.DoubleClick += open;

            card.Controls.Add(pic);
            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblStock);

            return card;
        }

        private void SelectCard(Panel card)
        {
            if (_selectedCard != null)
                _selectedCard.Invalidate();

            _selectedCard = card;
            _selectedMaSp = card.Tag as string;

            _selectedCard.Invalidate();
        }

        private Image? LoadImageSafe(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                // Support relative path stored in DB, e.g. Images\\Products\\tenfile.jpg
                var resolved = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (!File.Exists(resolved))
                    return null;

                using var img = Image.FromFile(resolved);
                return new Bitmap(img);
            }
            catch
            {
                return null;
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenDetail(null);
        }

        private void OpenDetail(string? maSp)
        {
            using var dlg = new FrmSanPhamDetail(maSp);
            dlg.StartPosition = FormStartPosition.CenterParent;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadSanPham();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaSp))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa (click vào card).", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var maSp = _selectedMaSp;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sản phẩm '{maSp}'?\n\n" +
                "Lưu ý: sẽ xóa luôn các biến thể của sản phẩm.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            var (ok, msg) = _spService.Delete(maSp);
            MessageBox.Show(msg, ok ? "Thành công" : "Lỗi",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok)
                LoadSanPham();
        }
    }
}
