using System;
using System.Drawing;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmNhaCungCapEdit : Form
{
    private readonly PhieuNhapService _service = new();
    private TextBox txtMa = null!;
    private TextBox txtTen = null!;
    private TextBox txtSdt = null!;
    private TextBox txtDiaChi = null!;
    private TextBox txtEmail = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;

    public string? MaNcc { get; private set; }
    public string? TenNcc { get; private set; }
    public string? Sdt { get; private set; }
    public string? DiaChi { get; private set; }
    public string? Email { get; private set; }

    public FrmNhaCungCapEdit(
        string? maNcc = null,
        string? tenNcc = null,
        string? sdt = null,
        string? diaChi = null,
        string? email = null)
    {
        Text = string.IsNullOrWhiteSpace(maNcc) ? "Thêm nhà cung cấp" : "Sửa nhà cung cấp";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.White;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        ClientSize = new Size(520, 300);

        BuildUi();

        txtMa.Text = string.IsNullOrWhiteSpace(maNcc) ? _service.GetNextMaNcc() : maNcc;
        txtTen.Text = tenNcc ?? string.Empty;
        txtSdt.Text = sdt ?? string.Empty;
        txtDiaChi.Text = diaChi ?? string.Empty;
        txtEmail.Text = email ?? string.Empty;

        txtMa.ReadOnly = true;
        txtMa.BackColor = Color.FromArgb(248, 250, 252);
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 6 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 5; i++)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        txtMa = new TextBox { Dock = DockStyle.Fill };
        txtTen = new TextBox { Dock = DockStyle.Fill };
        txtSdt = new TextBox { Dock = DockStyle.Fill };
        txtDiaChi = new TextBox { Dock = DockStyle.Fill };
        txtEmail = new TextBox { Dock = DockStyle.Fill };

        btnOk = new Button { Text = "Lưu", Width = 100, Height = 34, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Hủy", Width = 100, Height = 34, DialogResult = DialogResult.Cancel };

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        root.Controls.Add(new Label { Text = "Mã NCC", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
        root.Controls.Add(txtMa, 1, 0);

        root.Controls.Add(new Label { Text = "Tên NCC", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
        root.Controls.Add(txtTen, 1, 1);

        root.Controls.Add(new Label { Text = "SĐT", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
        root.Controls.Add(txtSdt, 1, 2);

        root.Controls.Add(new Label { Text = "Địa chỉ", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
        root.Controls.Add(txtDiaChi, 1, 3);

        root.Controls.Add(new Label { Text = "Email", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4);
        root.Controls.Add(txtEmail, 1, 4);

        root.Controls.Add(btnPanel, 0, 5);
        root.SetColumnSpan(btnPanel, 2);

        Controls.Add(root);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        btnOk.Click += (_, __) =>
        {
            var ma = (txtMa.Text ?? string.Empty).Trim();
            var ten = (txtTen.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(ma))
            {
                MessageBox.Show("Vui lòng nhập Mã NCC.");
                DialogResult = DialogResult.None;
                return;
            }
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập Tên NCC.");
                DialogResult = DialogResult.None;
                return;
            }

            MaNcc = ma;
            TenNcc = ten;
            Sdt = string.IsNullOrWhiteSpace(txtSdt.Text) ? null : txtSdt.Text.Trim();
            DiaChi = string.IsNullOrWhiteSpace(txtDiaChi.Text) ? null : txtDiaChi.Text.Trim();
            Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
        };
    }
}
