using System;
using System.Drawing;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmKhuyenMaiEdit : Form
{
    private readonly KhuyenMaiService _kmService = new();
    private readonly bool _isEdit;
    private TextBox txtMa = null!;
    private TextBox txtTen = null!;
    private NumericUpDown numPercent = null!;
    private DateTimePicker dtBd = null!;
    private DateTimePicker dtKt = null!;
    private CheckBox chkActive = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;

    public string? MaKm { get; private set; }
    public string? TenKm { get; private set; }
    public int PhanTram { get; private set; }
    public DateTime NgayBd { get; private set; }
    public DateTime NgayKt { get; private set; }
    public bool TrangThai { get; private set; }

    public FrmKhuyenMaiEdit(
        string? maKm = null,
        string? tenKm = null,
        int phanTram = 10,
        DateTime? ngayBd = null,
        DateTime? ngayKt = null,
        bool trangThai = true)
    {
        _isEdit = !string.IsNullOrWhiteSpace(maKm);
        Text = string.IsNullOrWhiteSpace(maKm) ? "Thêm khuyến mãi" : "Sửa khuyến mãi";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.White;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 300);

        BuildUi();

        var minDate = DateTime.Today;
        if (ngayBd.HasValue && ngayBd.Value.Date < minDate)
        {
            minDate = ngayBd.Value.Date;
        }
        if (ngayKt.HasValue && ngayKt.Value.Date < minDate)
        {
            minDate = ngayKt.Value.Date;
        }
        dtBd.MinDate = minDate;
        dtKt.MinDate = minDate;

        txtMa.Text = string.IsNullOrWhiteSpace(maKm) ? _kmService.GetNextMaKm() : maKm;
        txtTen.Text = tenKm ?? string.Empty;
        numPercent.Value = Math.Max(1, Math.Min(100, phanTram));
        dtBd.Value = ngayBd ?? DateTime.Now;
        dtKt.Value = ngayKt ?? DateTime.Now.AddDays(7);
        chkActive.Checked = trangThai;

        // MAKM is primary key; keep it read-only (auto-generated for new records)
        txtMa.ReadOnly = true;
        txtMa.BackColor = Color.FromArgb(248, 250, 252);
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 7 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 6; i++)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        txtMa = new TextBox { Dock = DockStyle.Fill };
        txtTen = new TextBox { Dock = DockStyle.Fill };
        numPercent = new NumericUpDown { Dock = DockStyle.Left, Width = 120, Minimum = 1, Maximum = 100 };
        dtBd = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        dtKt = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        chkActive = new CheckBox { Dock = DockStyle.Left, Text = "Đang bật", Checked = true };

        btnOk = new Button { Text = "Lưu", Width = 100, Height = 34, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Hủy", Width = 100, Height = 34, DialogResult = DialogResult.Cancel };

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        root.Controls.Add(new Label { Text = "Mã KM", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
        root.Controls.Add(txtMa, 1, 0);

        root.Controls.Add(new Label { Text = "Tên KM", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
        root.Controls.Add(txtTen, 1, 1);

        root.Controls.Add(new Label { Text = "% giảm", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
        root.Controls.Add(numPercent, 1, 2);

        root.Controls.Add(new Label { Text = "Ngày bắt đầu", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
        root.Controls.Add(dtBd, 1, 3);

        root.Controls.Add(new Label { Text = "Ngày kết thúc", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4);
        root.Controls.Add(dtKt, 1, 4);

        root.Controls.Add(new Label { Text = "Trạng thái", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 5);
        root.Controls.Add(chkActive, 1, 5);

        root.Controls.Add(btnPanel, 0, 6);
        root.SetColumnSpan(btnPanel, 2);

        Controls.Add(root);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        dtBd.ValueChanged += (_, __) => dtKt.MinDate = dtBd.Value.Date;

        btnOk.Click += (_, __) =>
        {
            var ma = (txtMa.Text ?? string.Empty).Trim();
            var ten = (txtTen.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(ma))
            {
                MessageBox.Show("Vui lòng nhập Mã KM.");
                DialogResult = DialogResult.None;
                return;
            }
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập Tên KM.");
                DialogResult = DialogResult.None;
                return;
            }
            if (!_isEdit && dtBd.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Ngày bắt đầu không được nhỏ hơn hôm nay.");
                DialogResult = DialogResult.None;
                return;
            }
            if (!_isEdit && dtKt.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Ngày kết thúc không được nhỏ hơn hôm nay.");
                DialogResult = DialogResult.None;
                return;
            }
            if (dtKt.Value < dtBd.Value)
            {
                MessageBox.Show("Ngày kết thúc phải >= ngày bắt đầu.");
                DialogResult = DialogResult.None;
                return;
            }

            MaKm = ma;
            TenKm = ten;
            PhanTram = (int)numPercent.Value;
            NgayBd = dtBd.Value;
            NgayKt = dtKt.Value;
            TrangThai = chkActive.Checked;
        };
    }
}
