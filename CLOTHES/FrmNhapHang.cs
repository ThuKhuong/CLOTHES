using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES;

public class FrmNhapHang : Form
{
    private readonly PhieuNhapService _service = new();
    private readonly NguoiDungDto? _currentUser;

    private ComboBox cboNcc = null!;
    private ComboBox cboNhanVien = null!;
    private DateTimePicker dtNgay = null!;
    private TextBox txtGhiChu = null!;
    private Button btnAddNcc = null!;

    private TextBox txtSearch = null!;
    private DataGridView dgvSearch = null!;
    private DataGridView dgvItems = null!;

    private Label lblTong = null!;
    private Button btnSave = null!;
    private Button btnRemove = null!;

    public FrmNhapHang(NguoiDungDto? currentUser = null)
    {
        Text = "Nhập hàng";
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
            Padding = new Padding(16),
            ColumnCount = 2,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

        // Left: header + search results
        var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        var leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        leftLayout.Controls.Add(BuildHeader(), 0, 0);

        var searchRow = new Panel { Dock = DockStyle.Fill };
        txtSearch = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Tìm biến thể (MACT / tên SP / barcode)..." };
        txtSearch.TextChanged += (_, __) => RefreshSearch();
        searchRow.Controls.Add(txtSearch);
        leftLayout.Controls.Add(searchRow, 0, 1);

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
        dgvSearch.CellDoubleClick += (_, __) => AddSelectedSearchToItems();
        leftLayout.Controls.Add(dgvSearch, 0, 2);

        left.Controls.Add(leftLayout);

        // Right: items
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        var rightTop = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        btnRemove = new Button { Text = "🗑️ Xóa dòng", Width = 120, Height = 32 };
        btnRemove.Click += (_, __) => RemoveSelectedItem();
        rightTop.Controls.Add(btnRemove);
        rightLayout.Controls.Add(rightTop, 0, 0);

        dgvItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MACT", HeaderText = "Mã CT", ReadOnly = true });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "TENSP", HeaderText = "Tên SP", ReadOnly = true });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SIZE", HeaderText = "Size", ReadOnly = true });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MAU", HeaderText = "Màu", ReadOnly = true });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL", HeaderText = "SL" });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "DONGIA", HeaderText = "Đơn giá nhập" });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "THANHTIEN", HeaderText = "Thành tiền", ReadOnly = true });

        dgvItems.Columns["DONGIA"].DefaultCellStyle.Format = "N0";
        dgvItems.Columns["THANHTIEN"].DefaultCellStyle.Format = "N0";

        dgvItems.CellEndEdit += (_, e) =>
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            RecalcRow(e.RowIndex);
            RecalcTong();
        };

        rightLayout.Controls.Add(dgvItems, 0, 1);

        var bottom = new Panel { Dock = DockStyle.Fill };
        lblTong = new Label { Dock = DockStyle.Left, AutoSize = false, Width = 300, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        btnSave = new Button { Text = "💾 Lưu phiếu nhập", Width = 150, Height = 38, BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        btnSave.Location = new Point(0, 8);
        btnSave.Click += (_, __) => Save();

        bottom.Controls.Add(btnSave);
        bottom.Controls.Add(lblTong);
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
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        cboNhanVien = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboNcc = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        btnAddNcc = new Button { Text = "+", Width = 32, Height = 26 };
        btnAddNcc.Click += (_, __) => AddNcc();
        dtNgay = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        txtGhiChu = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 40 };

        header.Controls.Add(new Label { Text = "Nhân viên", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        header.Controls.Add(cboNhanVien, 1, 0);
        header.Controls.Add(new Label { Text = "Nhà cung cấp", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        var nccRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        nccRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        nccRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        cboNcc.Margin = new Padding(0, 0, 6, 0);
        btnAddNcc.Dock = DockStyle.Fill;
        nccRow.Controls.Add(cboNcc, 0, 0);
        nccRow.Controls.Add(btnAddNcc, 1, 0);
        header.Controls.Add(nccRow, 1, 1);
        header.Controls.Add(new Label { Text = "Ngày nhập", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        header.Controls.Add(dtNgay, 1, 2);
        header.Controls.Add(new Label { Text = "Ghi chú", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        header.Controls.Add(txtGhiChu, 1, 3);

        return header;
    }

    private void AddNcc()
    {
        using var dlg = new Form
        {
            Text = "Thêm nhà cung cấp",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(420, 260)
        };

        var lblTen = new Label { Text = "Tên NCC", Left = 20, Top = 20, Width = 100 };
        var txtTen = new TextBox { Left = 130, Top = 16, Width = 260 };
        var lblSdt = new Label { Text = "SĐT", Left = 20, Top = 60, Width = 100 };
        var txtSdt = new TextBox { Left = 130, Top = 56, Width = 260 };
        var lblDiaChi = new Label { Text = "Địa chỉ", Left = 20, Top = 100, Width = 100 };
        var txtDiaChi = new TextBox { Left = 130, Top = 96, Width = 260 };
        var lblEmail = new Label { Text = "Email", Left = 20, Top = 140, Width = 100 };
        var txtEmail = new TextBox { Left = 130, Top = 136, Width = 260 };

        var btnOk = new Button { Text = "Lưu", Left = 230, Top = 190, Width = 75, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Hủy", Left = 315, Top = 190, Width = 75, DialogResult = DialogResult.Cancel };

        dlg.Controls.AddRange(new Control[] { lblTen, txtTen, lblSdt, txtSdt, lblDiaChi, txtDiaChi, lblEmail, txtEmail, btnOk, btnCancel });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        var (ok, msg, ma) = _service.AddNcc(txtTen.Text, txtSdt.Text, txtDiaChi.Text, txtEmail.Text);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (!ok)
            return;

        var dtNcc = _service.GetAllNcc();
        cboNcc.DataSource = dtNcc;
        if (!string.IsNullOrWhiteSpace(ma))
            cboNcc.SelectedValue = ma;
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

        var dtNcc = _service.GetAllNcc();
        cboNcc.DisplayMember = "TENNCC";
        cboNcc.ValueMember = "MANCC";
        cboNcc.DataSource = dtNcc;
        if (cboNcc.Items.Count > 0) cboNcc.SelectedIndex = 0;

        RefreshSearch();
    }

    private void RefreshSearch()
    {
        dgvSearch.DataSource = _service.SearchSanPhamChiTiet(txtSearch.Text);
        if (dgvSearch.Columns.Count > 0)
        {
            dgvSearch.Columns["MACT"].HeaderText = "Mã CT";
            dgvSearch.Columns["MASP"].Visible = false;
            dgvSearch.Columns["TENSP"].HeaderText = "Tên SP";
            dgvSearch.Columns["SIZE"].HeaderText = "Size";
            dgvSearch.Columns["MAU"].HeaderText = "Màu";
            dgvSearch.Columns["TONKHO"].HeaderText = "Tồn";
            dgvSearch.Columns["GIABAN"].HeaderText = "Giá bán";
            dgvSearch.Columns["BARCODE"].HeaderText = "Barcode";

            dgvSearch.Columns["GIABAN"].DefaultCellStyle.Format = "N0";
        }
    }

    private void AddSelectedSearchToItems()
    {
        if (dgvSearch.SelectedRows.Count == 0) return;
        var row = dgvSearch.SelectedRows[0];

        var maCt = row.Cells["MACT"].Value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(maCt)) return;

        // if exists in items, just focus
        foreach (DataGridViewRow r in dgvItems.Rows)
        {
            if (r.Cells["MACT"].Value?.ToString() == maCt)
            {
                dgvItems.ClearSelection();
                r.Selected = true;
                try { dgvItems.FirstDisplayedScrollingRowIndex = r.Index; } catch { /* ignore */ }
                return;
            }
        }

        var tenSp = row.Cells["TENSP"].Value?.ToString() ?? "";
        var size = row.Cells["SIZE"].Value?.ToString() ?? "";
        var mau = row.Cells["MAU"].Value?.ToString() ?? "";

        // User must enter purchase price.
        int idx = dgvItems.Rows.Add(maCt, tenSp, size, mau, 1, 0, 0);
        RecalcRow(idx);
        RecalcTong();
    }

    private void RemoveSelectedItem()
    {
        if (dgvItems.SelectedRows.Count == 0) return;
        dgvItems.Rows.Remove(dgvItems.SelectedRows[0]);
        RecalcTong();
    }

    private void RecalcRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvItems.Rows.Count) return;

        var r = dgvItems.Rows[rowIndex];
        if (r.IsNewRow) return;

        int sl = TryInt(r.Cells["SL"].Value, 1);
        decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);

        if (sl < 0) sl = 0;
        if (dg < 0) dg = 0;

        r.Cells["SL"].Value = sl;
        r.Cells["DONGIA"].Value = dg;
        r.Cells["THANHTIEN"].Value = (sl * dg);
    }

    private void RecalcTong()
    {
        decimal total = 0;
        foreach (DataGridViewRow r in dgvItems.Rows)
        {
            if (r.IsNewRow) continue;
            int sl = TryInt(r.Cells["SL"].Value, 0);
            decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);
            total += sl * dg;
        }

        lblTong.Text = $"Tổng tiền: {total:N0} VNĐ";
        btnSave.Left = (lblTong.Width + 16);
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

    private void Save()
    {
        var maNd = cboNhanVien.SelectedValue?.ToString() ?? "";
        var maNcc = cboNcc.SelectedValue?.ToString() ?? "";

        foreach (DataGridViewRow r in dgvItems.Rows)
        {
            if (r.IsNewRow) continue;
            var maCt0 = r.Cells["MACT"].Value?.ToString() ?? "";
            int sl0 = TryInt(r.Cells["SL"].Value, 0);
            decimal dg0 = TryDecimal(r.Cells["DONGIA"].Value, 0);

            if (!string.IsNullOrWhiteSpace(maCt0) && sl0 > 0 && dg0 <= 0)
            {
                MessageBox.Show("Đơn giá nhập phải > 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dgvItems.CurrentCell = r.Cells["DONGIA"];
                dgvItems.BeginEdit(true);
                return;
            }
        }

        var items = new List<(string maCt, int sl, decimal donGiaNhap)>();
        foreach (DataGridViewRow r in dgvItems.Rows)
        {
            if (r.IsNewRow) continue;
            var maCt = r.Cells["MACT"].Value?.ToString() ?? "";
            int sl = TryInt(r.Cells["SL"].Value, 0);
            decimal dg = TryDecimal(r.Cells["DONGIA"].Value, 0);
            if (!string.IsNullOrWhiteSpace(maCt) && sl > 0)
                items.Add((maCt, sl, dg));
        }

        var (ok, msg, maPn) = _service.SavePhieuNhap(maNd, maNcc, dtNgay.Value, txtGhiChu.Text, items);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (ok)
        {
            dgvItems.Rows.Clear();
            txtGhiChu.Clear();
            RecalcTong();
        }
    }
}
