using System;
using System.Drawing;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES
{
    public class FrmKhachHangEdit : Form
    {
        private readonly KhachHangService _service = new();
        private readonly KhachHangDto? _editing;

        private TextBox txtHoTen = null!;
        private ComboBox cboGioiTinh = null!;
        private TextBox txtSDT = null!;
        private TextBox txtDiaChi = null!;
        private TextBox txtEmail = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public FrmKhachHangEdit(KhachHangDto? editing)
        {
            _editing = editing;

            Text = editing == null ? "Thêm khách hàng" : "Cập nhật khách hàng";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 320);

            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 2,
                RowCount = 6
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtHoTen = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            cboGioiTinh = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            txtSDT = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            txtDiaChi = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            txtEmail = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

            layout.Controls.Add(new Label { Text = "Họ tên", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(txtHoTen, 1, 0);
            layout.Controls.Add(new Label { Text = "Giới tính", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(cboGioiTinh, 1, 1);
            layout.Controls.Add(new Label { Text = "Số điện thoại", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            layout.Controls.Add(txtSDT, 1, 2);
            layout.Controls.Add(new Label { Text = "Địa chỉ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            layout.Controls.Add(txtDiaChi, 1, 3);
            layout.Controls.Add(new Label { Text = "Email", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            layout.Controls.Add(txtEmail, 1, 4);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 34
            };

            btnSave.Click += (_, __) => Save();
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;

            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnCancel);

            layout.Controls.Add(buttons, 0, 5);
            layout.SetColumnSpan(buttons, 2);

            Controls.Add(layout);
        }

        private void LoadData()
        {
            cboGioiTinh.SelectedIndex = 0;

            if (_editing == null) return;

            txtHoTen.Text = _editing.HoTen;
            txtSDT.Text = _editing.SDT;
            txtDiaChi.Text = _editing.DChi ?? string.Empty;
            txtEmail.Text = _editing.Email ?? string.Empty;

            cboGioiTinh.SelectedIndex = _editing.GioiTinh switch
            {
                "Nữ" => 1,
                "Khác" => 2,
                _ => 0
            };
        }

        private void Save()
        {
            var dto = new KhachHangDto
            {
                MaKH = _editing?.MaKH ?? string.Empty,
                HoTen = (txtHoTen.Text ?? string.Empty).Trim(),
                GioiTinh = cboGioiTinh.SelectedItem?.ToString(),
                SDT = (txtSDT.Text ?? string.Empty).Trim(),
                DChi = string.IsNullOrWhiteSpace(txtDiaChi.Text) ? null : txtDiaChi.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim()
            };

            var (ok, msg) = _editing == null
                ? _service.Insert(dto)
                : _service.Update(dto);

            if (!ok)
            {
                MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
