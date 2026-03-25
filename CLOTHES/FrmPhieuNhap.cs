using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmPhieuNhap : Form
{
    private readonly PhieuNhapService _service = new();

    private TabControl tabs = null!;

    // Tab list
    private DateTimePicker dtFrom = null!;
    private DateTimePicker dtTo = null!;
    private TextBox txtKeyword = null!;
    private Button btnRefresh = null!;
    private DataGridView dgvList = null!;

    // Tab detail
    private Label lblHeader = null!;
    private DataGridView dgvDetail = null!;

    private DataTable? _listCache;
    private bool _isInitializing;

    public FrmPhieuNhap()
    {
        Text = "Phiếu nhập - Danh sách";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        _isInitializing = true;
        BuildUi();

        dtFrom.Value = DateTime.Today;
        dtTo.Value = DateTime.Today;
        _isInitializing = false;

        LoadList();
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
        txtKeyword = new TextBox { Width = 280, PlaceholderText = "Tìm: mã PN / NCC / nhân viên" };
        btnRefresh = new Button { Text = "Tải lại", Width = 90, Height = 30 };

        btnRefresh.Click += (_, __) => LoadList();
        dtFrom.ValueChanged += (_, __) => { if (!_isInitializing) LoadList(); };
        dtTo.ValueChanged += (_, __) => { if (!_isInitializing) LoadList(); };
        txtKeyword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadList(); } };

        filter.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtFrom);
        filter.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtTo);
        filter.Controls.Add(txtKeyword);
        filter.Controls.Add(btnRefresh);

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
    }

    private void LoadList()
    {
        var from = dtFrom.Value.Date;
        var to = dtTo.Value.Date;
        if (from > to)
        {
            dtTo.Value = from;
            to = from;
        }

        _listCache = _service.GetPhieuNhapList(from, to, txtKeyword.Text);
        dgvList.DataSource = _listCache;

        if (dgvList.Columns.Count > 0)
        {
            dgvList.Columns["MAPN"].HeaderText = "Mã PN";
            dgvList.Columns["NGAYNHAP"].HeaderText = "Ngày nhập";
            dgvList.Columns["TENND"].HeaderText = "Nhân viên";
            dgvList.Columns["TENNCC"].HeaderText = "Nhà cung cấp";
            dgvList.Columns["TONGTIEN"].HeaderText = "Tổng tiền";

            if (dgvList.Columns.Contains("MAND")) dgvList.Columns["MAND"].Visible = false;
            if (dgvList.Columns.Contains("MANCC")) dgvList.Columns["MANCC"].Visible = false;
            if (dgvList.Columns.Contains("GHICHU")) dgvList.Columns["GHICHU"].Visible = false;

            dgvList.Columns["NGAYNHAP"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvList.Columns["TONGTIEN"].DefaultCellStyle.Format = "N0";
        }

        LoadDetailFromSelected();
    }

    private void LoadDetailFromSelected()
    {
        if (_listCache == null || dgvList.SelectedRows.Count == 0)
        {
            lblHeader.Text = "Chọn 1 phiếu nhập để xem chi tiết.";
            dgvDetail.DataSource = null;
            return;
        }

        var maPnStr = dgvList.SelectedRows[0].Cells["MAPN"].Value?.ToString();
        if (!int.TryParse(maPnStr, out var maPn))
        {
            lblHeader.Text = "Chọn 1 phiếu nhập hợp lệ.";
            dgvDetail.DataSource = null;
            return;
        }

        var row = _listCache.Rows.Cast<DataRow>().FirstOrDefault(r => Convert.ToInt32(r["MAPN"]) == maPn);
        var ngay = row == null ? "" : Convert.ToDateTime(row["NGAYNHAP"]).ToString("dd/MM/yyyy HH:mm");
        var ncc = row == null ? "" : (row["TENNCC"] == DBNull.Value ? "" : row["TENNCC"].ToString());
        var tong = row == null ? 0 : Convert.ToDecimal(row["TONGTIEN"]);

        lblHeader.Text = $"PN #{maPn}  |  {ngay}  |  NCC: {ncc}  |  Tổng: {tong:N0} VNĐ";

        var dt = _service.GetDetails(maPn);
        dgvDetail.DataSource = dt;

        if (dgvDetail.Columns.Count > 0)
        {
            if (dgvDetail.Columns.Contains("MAPN")) dgvDetail.Columns["MAPN"].Visible = false;

            dgvDetail.Columns["MACT"].HeaderText = "Mã CT";
            dgvDetail.Columns["TENSP"].HeaderText = "Tên SP";
            if (dgvDetail.Columns.Contains("SIZE")) dgvDetail.Columns["SIZE"].HeaderText = "Size";
            if (dgvDetail.Columns.Contains("MAU")) dgvDetail.Columns["MAU"].HeaderText = "Màu";
            dgvDetail.Columns["SL"].HeaderText = "SL";
            dgvDetail.Columns["DONGIA_NHAP"].HeaderText = "Đơn giá nhập";
            dgvDetail.Columns["THANHTIEN"].HeaderText = "Thành tiền";

            dgvDetail.Columns["DONGIA_NHAP"].DefaultCellStyle.Format = "N0";
            dgvDetail.Columns["THANHTIEN"].DefaultCellStyle.Format = "N0";
        }
    }
}
