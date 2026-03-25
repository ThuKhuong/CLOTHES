using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES
{
    public partial class FrmKhachHang : Form
    {
        private readonly KhachHangService _service = new();
        private List<KhachHangDto> _all = new();
        private List<KhachHangDto> _view = new();

        public FrmKhachHang()
        {
            InitializeComponent();
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        private void FrmKhachHang_Load(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            cboSort.SelectedIndex = 0;
            cboSort.SelectedIndexChanged += (s, _) => ApplyFilterAndRender();

            Reload();
        }

        private void Reload()
        {
            var dt = _service.GetAll();
            _all = dt.Rows.Cast<DataRow>().Select(r => new KhachHangDto
            {
                MaKH = r["MAKH"].ToString()!,
                HoTen = r["HOTEN"].ToString()!,
                GioiTinh = r["GIOITINH"] == DBNull.Value ? null : r["GIOITINH"].ToString(),
                DChi = r["DCHI"] == DBNull.Value ? null : r["DCHI"].ToString(),
                SDT = r["SDT"].ToString()!,
                Email = r["EMAIL"] == DBNull.Value ? null : r["EMAIL"].ToString()
            }).ToList();

            ApplyFilterAndRender();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilterAndRender();

        private void ApplyFilterAndRender()
        {
            var keyword = (txtSearch.Text ?? string.Empty).Trim();

            IEnumerable<KhachHangDto> query = _all;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.MaKH.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.SDT.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(x.DChi) &&
                     x.DChi.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            query = cboSort.SelectedIndex switch
            {
                1 => query.OrderBy(x => x.MaKH),
                2 => query.OrderBy(x => x.SDT),
                _ => query.OrderBy(x => x.HoTen)
            };

            _view = query.ToList();
            RenderCards(_view);
        }

        private void RenderCards(List<KhachHangDto> items)
        {
            panelList.SuspendLayout();
            panelList.Controls.Clear();

            // Header row
            panelList.Controls.Add(CreateHeaderRow());

            foreach (var kh in items)
            {
                panelList.Controls.Add(CreateCustomerRow(kh));
            }

            panelList.ResumeLayout();
        }

        private Control CreateHeaderRow()
        {
            var row = new Panel
            {
                Height = 34,
                Width = panelList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                Margin = new Padding(0, 8, 0, 8),
                BackColor = Color.Transparent
            };

            row.Controls.Add(MkHeaderLabel("MÃ KH", 0, 90));
            row.Controls.Add(MkHeaderLabel("TÊN KHÁCH HÀNG", 100, 280));
            row.Controls.Add(MkHeaderLabel("SỐ ĐIỆN THOẠI", 390, 160));
            row.Controls.Add(MkHeaderLabel("ĐỊA CHỈ", 560, 1000));

            return row;
        }

        private Label MkHeaderLabel(string text, int x, int w)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 8),
                Width = w,
                Height = 20,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99)
            };
        }

        private Control CreateCustomerRow(KhachHangDto kh)
        {
            var row = new Panel
            {
                Height = 44,
                Width = panelList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(79, 70, 229),
                Cursor = Cursors.Hand
            };

            row.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 10);
                using var br = new SolidBrush(row.BackColor);
                e.Graphics.FillPath(br, path);
            };

            row.Controls.Add(MkCellLabel(kh.MaKH, 0, 90));
            row.Controls.Add(MkCellLabel(kh.HoTen, 100, 280));
            row.Controls.Add(MkCellLabel(kh.SDT, 390, 160));
            row.Controls.Add(MkCellLabel(kh.DChi ?? string.Empty, 560, 600));

            void openEdit(object? _, EventArgs __) => ShowEditDialog(kh);
            row.Click += openEdit;
            foreach (Control c in row.Controls) c.Click += openEdit;

            row.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(99, 102, 241);
            row.MouseLeave += (s, e) => row.BackColor = Color.FromArgb(79, 70, 229);

            return row;
        }

        private Label MkCellLabel(string text, int x, int w)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 12),
                Width = w,
                Height = 20,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White
            };
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ShowEditDialog(null);
        }

        private void ShowEditDialog(KhachHangDto? editing)
        {
            using var dlg = new FrmKhachHangEdit(editing);
            dlg.StartPosition = FormStartPosition.CenterParent;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Reload();
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
