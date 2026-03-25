using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES;

public class FrmBanHang : Form
{
    private readonly HoaDonService _service = new();
    private readonly KhachHangService _khService = new();
    private readonly KhuyenMaiService _kmService = new();
    private readonly NguoiDungDto? _currentUser;

    private DataTable? _searchCache;

    private ComboBox cboNhanVien = null!;
    private ComboBox cboKhachHang = null!;
    private ComboBox cboHinhThuc = null!;
    private ComboBox cboKhuyenMai = null!;
    private TextBox txtMaKm = null!;
    private DateTimePicker dtNgay = null!;
    private TextBox txtGhiChu = null!;

    private TextBox txtSearch = null!;
    private ComboBox cboSanPham = null!;
    private DataGridView dgvSearch = null!;

    private TextBox txtBarcode = null!;

    private DataGridView dgvCart = null!;
    private Label lblTong = null!;
    private Label lblGiam = null!;
    private Label lblThanhToan = null!;
    private Button btnPay = null!;
    private Button btnRemove = null!;

    private int _kmPercent;

    public FrmBanHang(NguoiDungDto? currentUser = null)
    {
        Text = "Đơn hàng";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        _currentUser = currentUser;

        BuildUi();
        LoadLookups();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 2,
        };
        // Make both panels wider (more balanced) so grids have more room.
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // LEFT: header + barcode + search
        var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        var leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5 };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        leftLayout.Controls.Add(BuildHeader(), 0, 0);

        var barcodeRow = new Panel { Dock = DockStyle.Fill };
        txtBarcode = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Quét/nhập barcode rồi Enter..." };
        txtBarcode.KeyDown += (s, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            AddByBarcode();
        };
        barcodeRow.Controls.Add(txtBarcode);
        leftLayout.Controls.Add(barcodeRow, 0, 1);

        var searchRow = new Panel { Dock = DockStyle.Fill };
        txtSearch = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Tìm biến thể (MACT / tên SP / barcode)..." };
        txtSearch.TextChanged += (_, __) => RefreshSearch();
        searchRow.Controls.Add(txtSearch);
        leftLayout.Controls.Add(searchRow, 0, 2);

        var productRow = new Panel { Dock = DockStyle.Fill };
        cboSanPham = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboSanPham.SelectedIndexChanged += (_, __) => ApplyProductFilter();
        productRow.Controls.Add(cboSanPham);
        leftLayout.Controls.Add(productRow, 0, 3);

        dgvSearch = new DataGridView
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
        dgvSearch.CellDoubleClick += (_, __) => AddSelectedSearchToCart();
        leftLayout.Controls.Add(dgvSearch, 0, 4);

        left.Controls.Add(leftLayout);

        // RIGHT: cart
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

        var rightTop = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        btnRemove = new Button { Text = "🗑️ Xóa dòng", Width = 120, Height = 32 };
        btnRemove.Click += (_, __) => RemoveSelectedCartItem();
        rightTop.Controls.Add(btnRemove);
        rightLayout.Controls.Add(rightTop, 0, 0);

        dgvCart = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "MACT", HeaderText = "Mã CT", ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "TENSP", HeaderText = "Tên SP", ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "SIZE", HeaderText = "Size", ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "MAU", HeaderText = "Màu", ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "TON", HeaderText = "Tồn", ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL", HeaderText = "SL" });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "DONGIA", HeaderText = "Đơn giá" });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "THANHTIEN", HeaderText = "Thành tiền", ReadOnly = true });

        dgvCart.CellEndEdit += (_, e) =>
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            RecalcCartRow(e.RowIndex);
            RecalcTong();
        };

        rightLayout.Controls.Add(dgvCart, 0, 1);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var row1 = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0),
            AutoScroll = false,
            AutoSize = true
        };

        var row2 = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0),
            AutoScroll = false,
            AutoSize = true
        };

        lblTong = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, 0, 24, 0) };
        lblGiam = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, 0, 24, 0) };
        lblThanhToan = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, 0, 24, 0) };

        row1.Controls.Add(lblTong);
        row1.Controls.Add(lblGiam);
        row2.Controls.Add(lblThanhToan);

        var payPanel = new Panel { Dock = DockStyle.Fill };
        btnPay = new Button { Text = "💳 Thanh toán", Width = 150, Height = 38, BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnPay.FlatAppearance.BorderSize = 0;
        btnPay.Location = new Point(payPanel.Width - btnPay.Width - 8, 4);
        payPanel.SizeChanged += (_, __) => btnPay.Left = payPanel.ClientSize.Width - btnPay.Width - 8;
        btnPay.Click += (_, __) => Pay();
        payPanel.Controls.Add(btnPay);

        bottom.Controls.Add(row1, 0, 0);
        bottom.Controls.Add(row2, 0, 1);
        bottom.Controls.Add(payPanel, 1, 0);
        rightLayout.Controls.Add(bottom, 0, 2);

        right.Controls.Add(rightLayout);

        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);

        Controls.Add(root);

        RecalcTong();
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        cboNhanVien = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboKhachHang = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboHinhThuc = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboKhuyenMai = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        txtMaKm = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Nhập mã KM rồi Enter (vd: KM10)..." };
        dtNgay = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        txtGhiChu = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 40 };

        header.Controls.Add(new Label { Text = "Nhân viên", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        header.Controls.Add(cboNhanVien, 1, 0);

        header.Controls.Add(new Label { Text = "Khách hàng", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        header.Controls.Add(cboKhachHang, 1, 1);

        header.Controls.Add(new Label { Text = "Hình thức", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        header.Controls.Add(cboHinhThuc, 1, 2);

        header.Controls.Add(new Label { Text = "Khuyến mãi", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        header.Controls.Add(cboKhuyenMai, 1, 3);

        header.Controls.Add(new Label { Text = "Mã KM", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        header.Controls.Add(txtMaKm, 1, 4);

        header.Controls.Add(new Label { Text = "Ngày", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
        header.Controls.Add(dtNgay, 1, 5);

        header.Controls.Add(new Label { Text = "Ghi chú", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 6);
        header.Controls.Add(txtGhiChu, 1, 6);

        return header;
    }

    private void LoadLookups()
    {
        var dtNv = _service.GetAllNhanVien();
        cboNhanVien.DisplayMember = "TENND";
        cboNhanVien.ValueMember = "MAND";
        cboNhanVien.DataSource = dtNv;
        if (cboNhanVien.Items.Count > 0) cboNhanVien.SelectedIndex = 0;

        if (_currentUser != null)
        {
            cboNhanVien.SelectedValue = _currentUser.MaND;
            cboNhanVien.Enabled = false;
        }

        // Khách hàng: cho chọn "khách lẻ" (null)
        var dtKh = _khService.GetAll();
        var view = dtKh.DefaultView;
        cboKhachHang.DisplayMember = "HOTEN";
        cboKhachHang.ValueMember = "MAKH";
        cboKhachHang.DataSource = view;
        if (cboKhachHang.Items.Count > 0) cboKhachHang.SelectedIndex = 0;

        cboHinhThuc.Items.Clear();
        cboHinhThuc.Items.AddRange(new object[] { "CASH", "BANK", "MOMO" });
        cboHinhThuc.SelectedIndex = 0;

        LoadKhuyenMai();
        cboKhuyenMai.SelectedIndexChanged += (_, __) => RecalcTong();
        txtMaKm.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            ApplyMaKm();
        };
        dtNgay.ValueChanged += (_, __) =>
        {
            LoadKhuyenMai();
            RecalcTong();
        };

        RefreshSearch();
    }

    private void ApplyMaKm()
    {
        var code = (txtMaKm.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            cboKhuyenMai.SelectedIndex = 0;
            RecalcTong();
            return;
        }

        for (int i = 0; i < cboKhuyenMai.Items.Count; i++)
        {
            if (cboKhuyenMai.Items[i] is DataRowView rv)
            {
                var id = rv["MAKM"] == DBNull.Value ? null : rv["MAKM"]?.ToString();
                if (string.Equals(id, code, StringComparison.OrdinalIgnoreCase))
                {
                    cboKhuyenMai.SelectedIndex = i;
                    RecalcTong();
                    return;
                }
            }
        }

        MessageBox.Show("Mã khuyến mãi không hợp lệ hoặc đã hết hạn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        cboKhuyenMai.SelectedIndex = 0;
        RecalcTong();
    }

    private void LoadKhuyenMai()
    {
        var dt = _kmService.GetActive(dtNgay.Value);
        var t = dt.Copy();
        if (!t.Columns.Contains("DISPLAY"))
            t.Columns.Add("DISPLAY", typeof(string));

        foreach (DataRow r in t.Rows)
        {
            var ten = r["TENKM"]?.ToString() ?? "";
            var p = r["PHANTRAM_GIAM"] == DBNull.Value ? 0 : Convert.ToInt32(r["PHANTRAM_GIAM"]);
            r["DISPLAY"] = $"{ten} (-{p}%)";
        }

        var none = t.NewRow();
        none["MAKM"] = DBNull.Value;
        none["DISPLAY"] = "(Không áp dụng)";
        none["PHANTRAM_GIAM"] = 0;
        t.Rows.InsertAt(none, 0);

        cboKhuyenMai.DisplayMember = "DISPLAY";
        cboKhuyenMai.ValueMember = "MAKM";
        cboKhuyenMai.DataSource = t;
        if (cboKhuyenMai.Items.Count > 0) cboKhuyenMai.SelectedIndex = 0;
    }

    private void RefreshSearch()
    {
        _searchCache = _service.SearchSanPhamChiTiet(txtSearch.Text);
        LoadProductFilterFromSearch();
        ApplyProductFilter();
    }

    private void LoadProductFilterFromSearch()
    {
        var dt = _searchCache;
        if (dt == null) return;

        var products = dt.Rows
            .Cast<DataRow>()
            .Select(r => r["TENSP"]?.ToString() ?? string.Empty)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();

        products.Insert(0, "(Tất cả sản phẩm)");

        var current = cboSanPham.SelectedItem?.ToString();
        cboSanPham.BeginUpdate();
        cboSanPham.Items.Clear();
        cboSanPham.Items.AddRange(products.Cast<object>().ToArray());
        cboSanPham.EndUpdate();

        if (!string.IsNullOrWhiteSpace(current) && products.Contains(current))
            cboSanPham.SelectedItem = current;
        else
            cboSanPham.SelectedIndex = 0;
    }

    private void ApplyProductFilter()
    {
        var dt = _searchCache;
        if (dt == null) return;

        var selected = cboSanPham.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selected) || selected == "(Tất cả sản phẩm)")
        {
            dgvSearch.DataSource = dt;
            SetupSearchGridColumns();
            return;
        }

        var view = new DataView(dt)
        {
            RowFilter = $"TENSP = '{selected.Replace("'", "''")}'"
        };
        dgvSearch.DataSource = view.ToTable();
        SetupSearchGridColumns();
    }

    private void SetupSearchGridColumns()
    {
        if (dgvSearch.Columns.Count == 0) return;

        if (dgvSearch.Columns.Contains("MACT")) dgvSearch.Columns["MACT"].HeaderText = "Mã CT";
        if (dgvSearch.Columns.Contains("MASP")) dgvSearch.Columns["MASP"].Visible = false;
        if (dgvSearch.Columns.Contains("TENSP")) dgvSearch.Columns["TENSP"].HeaderText = "Tên SP";
        if (dgvSearch.Columns.Contains("SIZE")) dgvSearch.Columns["SIZE"].HeaderText = "Size";
        if (dgvSearch.Columns.Contains("MAU")) dgvSearch.Columns["MAU"].HeaderText = "Màu";
        if (dgvSearch.Columns.Contains("TONKHO")) dgvSearch.Columns["TONKHO"].HeaderText = "Tồn";
        if (dgvSearch.Columns.Contains("GIABAN")) dgvSearch.Columns["GIABAN"].HeaderText = "Giá bán";
        if (dgvSearch.Columns.Contains("BARCODE")) dgvSearch.Columns["BARCODE"].HeaderText = "Barcode";
    }

    private void AddByBarcode()
    {
        var bc = (txtBarcode.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(bc)) return;

        var maCt = _service.FindMaCtByBarcode(bc);
        if (string.IsNullOrWhiteSpace(maCt))
        {
            MessageBox.Show("Không tìm thấy barcode.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtBarcode.SelectAll();
            return;
        }

        // Find it in current search result, else re-search by barcode
        var dt = dgvSearch.DataSource as DataTable;
        DataRow? r = dt?.Rows.Cast<DataRow>().FirstOrDefault(x => x["MACT"].ToString() == maCt);
        if (r == null)
        {
            var dt2 = _service.SearchSanPhamChiTiet(bc);
            r = dt2.Rows.Cast<DataRow>().FirstOrDefault(x => x["MACT"].ToString() == maCt);
        }

        if (r == null)
        {
            MessageBox.Show("Không tìm thấy biến thể theo barcode.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AddRowToCart(
            maCt,
            r["TENSP"].ToString() ?? "",
            r["SIZE"].ToString() ?? "",
            r["MAU"].ToString() ?? "",
            r["TONKHO"] == DBNull.Value ? 0 : Convert.ToInt32(r["TONKHO"]),
            r["GIABAN"] == DBNull.Value ? 0 : Convert.ToDecimal(r["GIABAN"]));

        txtBarcode.Clear();
    }

    private void AddSelectedSearchToCart()
    {
        if (dgvSearch.SelectedRows.Count == 0) return;
        var row = dgvSearch.SelectedRows[0];

        var maCt = row.Cells["MACT"].Value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(maCt)) return;

        AddRowToCart(
            maCt,
            row.Cells["TENSP"].Value?.ToString() ?? "",
            row.Cells["SIZE"].Value?.ToString() ?? "",
            row.Cells["MAU"].Value?.ToString() ?? "",
            row.Cells["TONKHO"].Value == null ? 0 : Convert.ToInt32(row.Cells["TONKHO"].Value),
            row.Cells["GIABAN"].Value == null ? 0 : Convert.ToDecimal(row.Cells["GIABAN"].Value));
    }

    private void AddRowToCart(string maCt, string tenSp, string size, string mau, int ton, decimal giaBan)
    {
        // If exists in cart: increase quantity
        foreach (DataGridViewRow r in dgvCart.Rows)
        {
            if (r.Cells["MACT"].Value?.ToString() == maCt)
            {
                int sl = TryInt(r.Cells["SL"].Value, 1);
                sl++;
                r.Cells["SL"].Value = sl;
                RecalcCartRow(r.Index);
                RecalcTong();
                return;
            }
        }

        int idx = dgvCart.Rows.Add(maCt, tenSp, size, mau, ton, 1, giaBan, 0);
        RecalcCartRow(idx);
        RecalcTong();
    }

    private void RemoveSelectedCartItem()
    {
        if (dgvCart.SelectedRows.Count == 0) return;
        dgvCart.Rows.Remove(dgvCart.SelectedRows[0]);
        RecalcTong();
    }

    private void RecalcCartRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvCart.Rows.Count) return;

        var r = dgvCart.Rows[rowIndex];
        if (r.IsNewRow) return;

        int ton = TryInt(r.Cells["TON"].Value, 0);
        int sl = TryInt(r.Cells["SL"].Value, 1);
        decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);

        if (sl < 1) sl = 1;
        if (dg < 0) dg = 0;

        if (sl > ton)
        {
            sl = ton;
            MessageBox.Show($"Số lượng vượt tồn kho. Tồn hiện tại: {ton}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        r.Cells["SL"].Value = sl;
        r.Cells["DONGIA"].Value = dg;
        r.Cells["THANHTIEN"].Value = (sl * dg).ToString("N0");
    }

    private void RecalcTong()
    {
        decimal total = 0;
        foreach (DataGridViewRow r in dgvCart.Rows)
        {
            if (r.IsNewRow) continue;
            int sl = TryInt(r.Cells["SL"].Value, 0);
            decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);
            total += sl * dg;
        }

        _kmPercent = 0;
        var kmRow = cboKhuyenMai?.SelectedItem as DataRowView;
        if (kmRow != null && kmRow.Row.Table.Columns.Contains("PHANTRAM_GIAM"))
            _kmPercent = kmRow["PHANTRAM_GIAM"] == DBNull.Value ? 0 : Convert.ToInt32(kmRow["PHANTRAM_GIAM"]);
        if (_kmPercent < 0) _kmPercent = 0;
        if (_kmPercent > 100) _kmPercent = 100;

        decimal giam = Math.Round(total * _kmPercent / 100m, 0);
        if (giam < 0) giam = 0;
        if (giam > total) giam = total;
        decimal thanhToan = total - giam;

        lblTong.Text = $"Tổng: {total:N0} VNĐ";
        lblGiam.Text = $"Giảm: {giam:N0} VNĐ";
        lblThanhToan.Text = $"Thanh toán: {thanhToan:N0} VNĐ";
        // Pay button is anchored to the right.
    }

    private static int TryInt(object? v, int def)
    {
        if (v == null) return def;
        return int.TryParse(v.ToString(), out var n) ? n : def;
    }

    private static decimal TryDecimal(object? v, decimal def)
    {
        if (v == null) return def;
        return decimal.TryParse(v.ToString(), out var n) ? n : def;
    }

    private void Pay()
    {
        var maNd = cboNhanVien.SelectedValue?.ToString() ?? "";
        var maKh = cboKhachHang.SelectedValue?.ToString();
        var ht = cboHinhThuc.SelectedItem?.ToString();

        var items = new List<(string maCt, int sl, decimal donGia)>();
        foreach (DataGridViewRow r in dgvCart.Rows)
        {
            if (r.IsNewRow) continue;
            var maCt = r.Cells["MACT"].Value?.ToString() ?? "";
            int sl = TryInt(r.Cells["SL"].Value, 0);
            decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);
            if (!string.IsNullOrWhiteSpace(maCt) && sl > 0)
                items.Add((maCt, sl, dg));
        }

        var maKm = cboKhuyenMai?.SelectedValue?.ToString();
        var (ok, msg, soHd) = _service.ThanhToan(maNd, maKh, dtNgay.Value, maKm, txtGhiChu.Text, ht, items);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (ok)
        {
            var askPrint = MessageBox.Show(
                $"Đã tạo hóa đơn #{soHd}. Bạn có muốn in hóa đơn không?",
                "In hóa đơn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            try
            {
                ShowInvoiceInHost(soHd, askPrint == DialogResult.Yes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở màn hóa đơn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ShowInvoiceInHost(int soHd, bool autoPrint)
    {
        var host = Parent;
        if (host == null)
            return;

        host.Controls.Clear();

        var frm = new FrmHoaDon(soHd, autoPrint: autoPrint)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(0, 0),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };

        host.Controls.Add(frm);
        frm.Show();
    }
}
