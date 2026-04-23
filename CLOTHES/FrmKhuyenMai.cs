using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmKhuyenMai : Form
{
    private readonly KhuyenMaiService _service = new();

    private DataGridView dgv = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;

    public FrmKhuyenMai()
    {
        Text = "Khuyến mãi";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        btnAdd = new Button { Text = "Thêm", Width = 90, Height = 32};
        btnEdit = new Button { Text = "Sửa", Width = 90, Height = 32};
        btnDelete = new Button {Text = "Xóa", Width = 90, Height = 32 };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnEdit.FlatAppearance.BorderSize = 0;
        btnDelete.FlatAppearance.BorderSize = 0;
        btnRefresh = new Button { Text = "Tải lại", Width = 90, Height = 32 };

        btnAdd.Click += (_, __) => Add();
        btnEdit.Click += (_, __) => Edit();
        btnDelete.Click += (_, __) => Delete();
        btnRefresh.Click += (_, __) => LoadData();

        top.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

        dgv = new DataGridView
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
        dgv.CellDoubleClick += (_, __) => Edit();

        root.Controls.Add(top, 0, 0);
        root.Controls.Add(dgv, 0, 1);
        Controls.Add(root);
    }

    private void LoadData()
    {
        // For CRUD screen we want all, not only active.
        var dt = _service.GetAll();
        dgv.DataSource = dt;

        if (dgv.Columns.Count > 0)
        {
            dgv.Columns["MAKM"].HeaderText = "Mã KM";
            dgv.Columns["TENKM"].HeaderText = "Tên KM";
            dgv.Columns["PHANTRAM_GIAM"].HeaderText = "% giảm";
            dgv.Columns["NGAYBD"].HeaderText = "Ngày BD";
            dgv.Columns["NGAYKT"].HeaderText = "Ngày KT";
            dgv.Columns["TRANGTHAI"].HeaderText = "Bật";

            dgv.Columns["NGAYBD"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgv.Columns["NGAYKT"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }
    }

    private string? SelectedMaKm()
    {
        if (dgv.SelectedRows.Count == 0) return null;
        return dgv.SelectedRows[0].Cells["MAKM"].Value?.ToString();
    }

    private void Add()
    {
        using var f = new FrmKhuyenMaiEdit();
        if (f.ShowDialog(this) != DialogResult.OK) return;

        var (ok, msg) = _service.Upsert(f.MaKm!, f.TenKm!, f.PhanTram, f.NgayBd, f.NgayKt, f.TrangThai);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        if (ok) LoadData();
    }

    private void Edit()
    {
        var maKm = SelectedMaKm();
        if (string.IsNullOrWhiteSpace(maKm)) return;

        var row = _service.GetById(maKm);
        if (row == null)
        {
            MessageBox.Show("Không tìm thấy khuyến mãi.");
            return;
        }

        var ngayBd = Convert.ToDateTime(row["NGAYBD"]);
        var ngayKt = Convert.ToDateTime(row["NGAYKT"]);
        var trangThai = Convert.ToBoolean(row["TRANGTHAI"]);

        using var f = new FrmKhuyenMaiEdit(
            maKm: row["MAKM"].ToString()!,
            tenKm: row["TENKM"].ToString()!,
            phanTram: Convert.ToInt32(row["PHANTRAM_GIAM"]),
            ngayBd: ngayBd,
            ngayKt: ngayKt,
            trangThai: trangThai);

        if (f.ShowDialog(this) != DialogResult.OK) return;

        var (ok, msg) = _service.Upsert(f.MaKm!, f.TenKm!, f.PhanTram, f.NgayBd, f.NgayKt, f.TrangThai);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        if (ok) LoadData();
    }

    private void Delete()
    {
        var maKm = SelectedMaKm();
        if (string.IsNullOrWhiteSpace(maKm)) return;

        if (MessageBox.Show($"Xóa khuyến mãi {maKm}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var (ok, msg) = _service.Delete(maKm);
        MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        if (ok) LoadData();
    }

}
