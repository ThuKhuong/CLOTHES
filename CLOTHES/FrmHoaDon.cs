using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmHoaDon : Form
{
    private readonly HoaDonService _service = new();

    private readonly int? _focusSoHd;
    private readonly bool _autoPrint;

    private TabControl tabs = null!;

    // Tab list
    private DateTimePicker dtFrom = null!;
    private DateTimePicker dtTo = null!;
    private TextBox txtKeyword = null!;
    private Button btnRefresh = null!;
    private Button btnHuy = null!;
    private Button btnTra = null!;
    private Button btnPrint = null!;
    private DataGridView dgvList = null!;

    // Tab detail
    private Label lblHeader = null!;
    private DataGridView dgvDetail = null!;

    private PrintDocument _printDoc = null!;
    private int _printRowIndex;

    private DataTable? _listCache;

    public FrmHoaDon() : this(null, false)
    {
    }

    public FrmHoaDon(int soHd, bool autoPrint = true) : this((int?)soHd, autoPrint)
    {
    }

    private FrmHoaDon(int? focusSoHd, bool autoPrint)
    {
        Text = "Đơn hàng - Danh sách";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        _focusSoHd = focusSoHd;
        _autoPrint = autoPrint;

        BuildUi();

        // Default to today to avoid an empty list if the DB doesn't have data in the current picker range.
        dtFrom.Value = DateTime.Today;
        dtTo.Value = DateTime.Today;

        LoadList();

        if (_focusSoHd.HasValue)
        {
            Shown += (_, __) =>
            {
                FocusInvoice(_focusSoHd.Value);
                tabs.SelectedIndex = 1;

                if (_autoPrint)
                    BeginInvoke(new Action(() => PrintSelected()));
            };
        }
    }

    private void FocusInvoice(int soHd)
    {
        if (!IsHandleCreated) return;
        if (!Visible) return;
        if (dgvList.Rows.Count == 0) return;

        foreach (DataGridViewRow r in dgvList.Rows)
        {
            if (r.IsNewRow) continue;
            if (r.Cells["SOHD"].Value == null) continue;

            if (int.TryParse(r.Cells["SOHD"].Value.ToString(), out var id) && id == soHd)
            {
                dgvList.ClearSelection();
                r.Selected = true;
                if (r.Index >= 0)
                    dgvList.CurrentCell = r.Cells["SOHD"];

                // Can throw if the grid doesn't have room yet.
                if (dgvList.DisplayRectangle.Height > dgvList.ColumnHeadersHeight)
                {
                    try
                    {
                        dgvList.FirstDisplayedScrollingRowIndex = Math.Max(0, r.Index);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                LoadDetailFromSelected();
                return;
            }
        }
    }

    private void BuildUi()
    {
        tabs = new TabControl { Dock = DockStyle.Fill };
        var tabList = new TabPage("Danh sách");
        var tabDetail = new TabPage("Chi tiết");
        tabs.TabPages.Add(tabList);
        tabs.TabPages.Add(tabDetail);

        // LIST
        var listRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(16) };
        listRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        listRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filter = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true };

        dtFrom = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
        dtTo = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
        txtKeyword = new TextBox { Width = 260, PlaceholderText = "Tìm: số HĐ / tên KH / SĐT" };
        btnRefresh = new Button { Text = "Tải lại", Width = 90, Height = 30 };
        btnHuy = new Button { Text = "Hủy HĐ", Width = 90, Height = 30 };
        btnTra = new Button { Text = "Trả hàng", Width = 90, Height = 30 };
        btnPrint = new Button { Text = "In", Width = 70, Height = 30 };

        btnRefresh.Click += (_, __) => LoadList();
        dtFrom.ValueChanged += (_, __) => LoadList();
        dtTo.ValueChanged += (_, __) => LoadList();
        btnHuy.Click += (_, __) => HuyOrTra(false);
        btnTra.Click += (_, __) => HuyOrTra(true);
        btnPrint.Click += (_, __) => PrintSelected();
        txtKeyword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadList(); } };

        filter.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtFrom);
        filter.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtTo);
        filter.Controls.Add(txtKeyword);
        filter.Controls.Add(btnRefresh);
        filter.Controls.Add(btnHuy);
        filter.Controls.Add(btnTra);
        filter.Controls.Add(btnPrint);

        dgvList = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };
        dgvList.SelectionChanged += (_, __) => LoadDetailFromSelected();
        dgvList.CellDoubleClick += (_, __) => tabs.SelectedIndex = 1;

        listRoot.Controls.Add(filter, 0, 0);
        listRoot.Controls.Add(dgvList, 0, 1);
        tabList.Controls.Add(listRoot);

        // DETAIL
        var detailRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(16) };
        detailRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        detailRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        lblHeader = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };

        dgvDetail = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        detailRoot.Controls.Add(lblHeader, 0, 0);
        detailRoot.Controls.Add(dgvDetail, 0, 1);
        tabDetail.Controls.Add(detailRoot);

        Controls.Add(tabs);

        _printDoc = new PrintDocument();
        _printDoc.PrintPage += PrintDoc_PrintPage;
    }

    private void LoadList()
    {
        _listCache = _service.GetHoaDonList(dtFrom.Value.Date, dtTo.Value.Date, txtKeyword.Text);
        if (_listCache != null && !_listCache.Columns.Contains("TRANGTHAI_VN"))
            _listCache.Columns.Add("TRANGTHAI_VN", typeof(string));

        if (_listCache != null)
        {
            foreach (DataRow r in _listCache.Rows)
            {
                var tt = r["TRANGTHAI"] == DBNull.Value ? "" : r["TRANGTHAI"].ToString();
                var t = (tt ?? string.Empty).Trim().ToUpperInvariant();
                r["TRANGTHAI_VN"] = t switch
                {
                    "DATHANHTOAN" => "Đã thanh toán",
                    "HUY" => "Hủy",
                    "TRAHANG" => "Trả hàng",
                    _ => tt
                };
            }
        }
        dgvList.DataSource = _listCache;

        if (dgvList.Columns.Count > 0)
        {
            dgvList.Columns["SOHD"].HeaderText = "Số HĐ";
            dgvList.Columns["NGHD"].HeaderText = "Ngày";
            dgvList.Columns["TENND"].HeaderText = "Nhân viên";
            dgvList.Columns["HOTEN"].HeaderText = "Khách hàng";
            dgvList.Columns["SDT"].HeaderText = "SĐT";
            dgvList.Columns["TONGTIEN2"].HeaderText = "Tổng tiền";
            dgvList.Columns["HINHTHUCTT"].HeaderText = "HTTT";
            // Use friendly status column
            if (dgvList.Columns.Contains("TRANGTHAI")) dgvList.Columns["TRANGTHAI"].Visible = false;
            if (dgvList.Columns.Contains("TRANGTHAI_VN"))
            {
                dgvList.Columns["TRANGTHAI_VN"].HeaderText = "Trạng thái";
                dgvList.Columns["TRANGTHAI_VN"].DisplayIndex = dgvList.Columns.Contains("HINHTHUCTT") ? dgvList.Columns["HINHTHUCTT"].DisplayIndex + 1 : dgvList.Columns["TRANGTHAI_VN"].DisplayIndex;
            }

            // Hide technical columns
            if (dgvList.Columns.Contains("MAND")) dgvList.Columns["MAND"].Visible = false;
            if (dgvList.Columns.Contains("MAKH")) dgvList.Columns["MAKH"].Visible = false;
            if (dgvList.Columns.Contains("TONGTIEN1")) dgvList.Columns["TONGTIEN1"].Visible = false;
            if (dgvList.Columns.Contains("GIAMGIA")) dgvList.Columns["GIAMGIA"].Visible = false;
            if (dgvList.Columns.Contains("GHICHU")) dgvList.Columns["GHICHU"].Visible = false;

            dgvList.Columns["TONGTIEN2"].DefaultCellStyle.Format = "N0";
            dgvList.Columns["NGHD"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        LoadDetailFromSelected();
    }

    private void LoadDetailFromSelected()
    {
        if (_listCache == null || dgvList.SelectedRows.Count == 0)
        {
            lblHeader.Text = "Chọn 1 hóa đơn để xem chi tiết.";
            dgvDetail.DataSource = null;
            return;
        }

        var soHdStr = dgvList.SelectedRows[0].Cells["SOHD"].Value?.ToString();
        if (!int.TryParse(soHdStr, out var soHd))
        {
            lblHeader.Text = "Chọn 1 hóa đơn hợp lệ.";
            dgvDetail.DataSource = null;
            return;
        }

        var row = _listCache.Rows.Cast<DataRow>().FirstOrDefault(r => Convert.ToInt32(r["SOHD"]) == soHd);
        var ngay = row == null ? "" : Convert.ToDateTime(row["NGHD"]).ToString("dd/MM/yyyy HH:mm");
        var tenKh = row == null ? "" : (row["HOTEN"] == DBNull.Value ? "Khách lẻ" : row["HOTEN"].ToString());
        var tong1 = row == null ? 0 : Convert.ToDecimal(row["TONGTIEN1"]);
        var giam = row == null ? 0 : Convert.ToDecimal(row["GIAMGIA"]);
        var tong2 = row == null ? 0 : Convert.ToDecimal(row["TONGTIEN2"]);
        var trangThaiRaw = row == null ? "" : (row["TRANGTHAI"] == DBNull.Value ? "" : row["TRANGTHAI"].ToString());

        static string VnTrangThai(string? tt)
        {
            tt = (tt ?? string.Empty).Trim().ToUpperInvariant();
            return tt switch
            {
                "DATHANHTOAN" => "Đã thanh toán",
                "HUY" => "Hủy",
                "TRAHANG" => "Trả hàng",
                _ => tt
            };
        }

        lblHeader.Text = $"HĐ #{soHd}  |  {ngay}  |  KH: {tenKh}  |  Tạm tính: {tong1:N0}  |  Giảm: {giam:N0}  |  Tổng: {tong2:N0} VNĐ  |  {VnTrangThai(trangThaiRaw)}";

        var dt = _service.GetHoaDonDetail(soHd);
        dgvDetail.DataSource = dt;

        if (dgvDetail.Columns.Count > 0)
        {
            dgvDetail.Columns["MACT"].HeaderText = "Mã CT";
            dgvDetail.Columns["TENSP"].HeaderText = "Tên SP";
            dgvDetail.Columns["SIZE"].HeaderText = "Size";
            dgvDetail.Columns["MAU"].HeaderText = "Màu";
            dgvDetail.Columns["SL"].HeaderText = "SL";
            dgvDetail.Columns["DONGIA"].HeaderText = "Đơn giá";
            dgvDetail.Columns["GIAMGIA"].HeaderText = "Giảm";
            dgvDetail.Columns["THANHTIEN"].HeaderText = "Thành tiền";

            if (dgvDetail.Columns.Contains("SOHD")) dgvDetail.Columns["SOHD"].Visible = false;

            dgvDetail.Columns["DONGIA"].DefaultCellStyle.Format = "N0";
            dgvDetail.Columns["GIAMGIA"].DefaultCellStyle.Format = "N0";
            dgvDetail.Columns["THANHTIEN"].DefaultCellStyle.Format = "N0";
        }

        return;
    }

    private void HuyOrTra(bool traHang)
    {
        if (dgvList.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var soHdStr = dgvList.SelectedRows[0].Cells["SOHD"].Value?.ToString();
        if (!int.TryParse(soHdStr, out var soHd))
        {
            MessageBox.Show("Hóa đơn không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            traHang ? "Xác nhận TRẢ HÀNG? (tồn kho sẽ được cộng lại)" : "Xác nhận HỦY HÓA ĐƠN? (tồn kho sẽ được cộng lại)",
            "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        var (ok, msg) = _service.HuyHoacTraHang(soHd, traHang);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (ok)
        {
            LoadList();
            tabs.SelectedIndex = 0;
        }

        return;
    }

    private void PrintSelected()
    {
        if (dgvList.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn hóa đơn để in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Ensure detail grid is loaded for the selected invoice
        LoadDetailFromSelected();

        _printRowIndex = 0;

        using var preview = new PrintPreviewDialog
        {
            Document = _printDoc,
            Width = 900,
            Height = 700
        };

        try
        {
            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể in: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        g.PageUnit = GraphicsUnit.Display;

        var left = e.MarginBounds.Left;
        var top = e.MarginBounds.Top;
        var width = e.MarginBounds.Width;

        using var fontTitle = new Font("Segoe UI", 14f, FontStyle.Bold);
        using var fontBold = new Font("Segoe UI", 10f, FontStyle.Bold);
        using var font = new Font("Segoe UI", 10f, FontStyle.Regular);

        int y = top;

        // Header
        g.DrawString("HÓA ĐƠN BÁN HÀNG", fontTitle, Brushes.Black, left, y);
        y += 32;

        var soHdStr = dgvList.SelectedRows[0].Cells["SOHD"].Value?.ToString() ?? "";
        var ngayStr = dgvList.SelectedRows[0].Cells["NGHD"].Value?.ToString() ?? "";
        var nvStr = dgvList.SelectedRows[0].Cells["TENND"].Value?.ToString() ?? "";
        var khStr = dgvList.SelectedRows[0].Cells["HOTEN"].Value == DBNull.Value ? "Khách lẻ" : dgvList.SelectedRows[0].Cells["HOTEN"].Value?.ToString();
        var sdtStr = dgvList.SelectedRows[0].Cells["SDT"].Value?.ToString() ?? "";
        var htStr = dgvList.SelectedRows[0].Cells["HINHTHUCTT"].Value?.ToString() ?? "";
        var tongStr = dgvList.SelectedRows[0].Cells["TONGTIEN2"].Value?.ToString() ?? "";

        g.DrawString($"Số HĐ: {soHdStr}", fontBold, Brushes.Black, left, y); y += 18;
        g.DrawString($"Ngày: {ngayStr}", font, Brushes.Black, left, y); y += 18;
        g.DrawString($"Nhân viên: {nvStr}", font, Brushes.Black, left, y); y += 18;
        g.DrawString($"Khách hàng: {khStr}  {sdtStr}", font, Brushes.Black, left, y); y += 18;
        g.DrawString($"Hình thức: {htStr}", font, Brushes.Black, left, y); y += 22;

        // Table header
        int colMa = left;
        int colTen = left + (int)(width * 0.15);
        int colSize = left + (int)(width * 0.55);
        int colMau = left + (int)(width * 0.65);
        int colSl = left + (int)(width * 0.78);
        int colGia = left + (int)(width * 0.86);

        g.DrawLine(Pens.Black, left, y, left + width, y);
        y += 4;
        g.DrawString("MACT", fontBold, Brushes.Black, colMa, y);
        g.DrawString("Tên SP", fontBold, Brushes.Black, colTen, y);
        g.DrawString("Size", fontBold, Brushes.Black, colSize, y);
        g.DrawString("Màu", fontBold, Brushes.Black, colMau, y);
        g.DrawString("SL", fontBold, Brushes.Black, colSl, y);
        g.DrawString("Giá", fontBold, Brushes.Black, colGia, y);
        y += 20;
        g.DrawLine(Pens.Black, left, y, left + width, y);
        y += 6;

        // Rows
        int rowHeight = 18;
        while (_printRowIndex < dgvDetail.Rows.Count)
        {
            var r = dgvDetail.Rows[_printRowIndex];
            if (r.IsNewRow) { _printRowIndex++; continue; }

            var maCt = r.Cells["MACT"].Value?.ToString() ?? "";
            var tenSp = r.Cells["TENSP"].Value?.ToString() ?? "";
            var size = r.Cells["SIZE"].Value?.ToString() ?? "";
            var mau = r.Cells["MAU"].Value?.ToString() ?? "";
            var sl = r.Cells["SL"].Value?.ToString() ?? "";
            var gia = r.Cells["DONGIA"].Value?.ToString() ?? "";

            // Page break
            if (y + rowHeight > e.MarginBounds.Bottom - 80)
            {
                e.HasMorePages = true;
                return;
            }

            g.DrawString(maCt, font, Brushes.Black, colMa, y);
            g.DrawString(tenSp, font, Brushes.Black, colTen, y);
            g.DrawString(size, font, Brushes.Black, colSize, y);
            g.DrawString(mau, font, Brushes.Black, colMau, y);
            g.DrawString(sl, font, Brushes.Black, colSl, y);
            g.DrawString(gia, font, Brushes.Black, colGia, y);

            y += rowHeight;
            _printRowIndex++;
        }

        // Footer
        y += 10;
        g.DrawLine(Pens.Black, left, y, left + width, y);
        y += 10;
        g.DrawString($"Tổng tiền: {tongStr}", fontBold, Brushes.Black, left + width - 220, y);

        e.HasMorePages = false;
        _printRowIndex = 0;
    }
}

