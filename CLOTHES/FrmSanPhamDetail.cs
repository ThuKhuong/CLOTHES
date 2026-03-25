using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES
{
    public class FrmSanPhamDetail : Form
    {
        private readonly string? _maSp;
        private readonly SanPhamService _spService = new();
        private readonly SanPhamChiTietService _ctService = new();

        private TextBox txtMaSp = null!;
        private TextBox txtTenSp = null!;
        private ComboBox cboLoai = null!;
        private Button btnAddLoai = null!;
        private TextBox txtMoTa = null!;
        private TextBox txtHinh = null!;
        private Button btnBrowseHinh = null!;
        private CheckBox chkTrangThai = null!;

        private DataGridView dgvCt = null!;

        private Button btnSave = null!;
        private Button btnCancel = null!;
        private Button btnAddCt = null!;
        private Button btnEditCt = null!;
        private Button btnDelCt = null!;

        public FrmSanPhamDetail(string? maSp)
        {
            _maSp = maSp;

            Text = maSp == null ? "Thêm sản phẩm" : "Chi tiết sản phẩm";
            Font = new Font("Segoe UI", 9F);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(920, 640);

            BuildUi();
            LoadLoai();

            if (_maSp != null)
            {
                LoadSanPham();
            }
            else
            {
                // Auto-generate product code
                txtMaSp.Text = _spService.GetNextMaSp();
                txtMaSp.ReadOnly = true;
            }

            LoadChiTiet();
        }

        private void BuildUi()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var tabInfo = new TabPage("Thông tin");
            var tabCt = new TabPage("Biến thể");

            tabs.TabPages.Add(tabInfo);
            tabs.TabPages.Add(tabCt);

            // Info layout
            var info = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Padding = new Padding(16),
                ColumnCount = 3,
                RowCount = 6,
                Height = 300
            };
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            txtMaSp = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            txtTenSp = new TextBox { Dock = DockStyle.Fill };
            cboLoai = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            btnAddLoai = new Button { Text = "Thêm loại...", Dock = DockStyle.Fill };
            btnAddLoai.Click += (_, __) => AddLoai();
            txtMoTa = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };
            txtHinh = new TextBox { Dock = DockStyle.Fill };
            btnBrowseHinh = new Button { Text = "Chọn ảnh...", Dock = DockStyle.Fill };
            btnBrowseHinh.Click += (_, __) => BrowseImage();
            chkTrangThai = new CheckBox { Dock = DockStyle.Left, Text = "Đang kinh doanh", Checked = true };

            info.Controls.Add(new Label { Text = "Mã SP", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            info.Controls.Add(txtMaSp, 1, 0);
            info.SetColumnSpan(txtMaSp, 2);

            info.Controls.Add(new Label { Text = "Tên SP", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            info.Controls.Add(txtTenSp, 1, 1);
            info.SetColumnSpan(txtTenSp, 2);

            info.Controls.Add(new Label { Text = "Loại", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            info.Controls.Add(cboLoai, 1, 2);
            info.Controls.Add(btnAddLoai, 2, 2);

            info.Controls.Add(new Label { Text = "Mô tả", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            info.Controls.Add(txtMoTa, 1, 3);
            info.SetColumnSpan(txtMoTa, 2);

            info.Controls.Add(new Label { Text = "Hình (đường dẫn)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            info.Controls.Add(txtHinh, 1, 4);
            info.Controls.Add(btnBrowseHinh, 2, 4);

            info.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
            info.Controls.Add(chkTrangThai, 1, 5);
            info.SetColumnSpan(chkTrangThai, 2);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(16),
                Height = 70
            };

            btnSave = new Button { Text = "Lưu", Width = 110, Height = 38, BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnCancel = new Button { Text = "Hủy", Width = 110, Height = 38 };

            btnSave.Click += (_, __) => Save();
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;

            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnCancel);

            tabInfo.Controls.Add(buttons);
            tabInfo.Controls.Add(info);

            // Variants
            var topCt = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(16, 10, 16, 10)
            };

            btnAddCt = new Button { Text = "➕ Thêm biến thể", Width = 150, Height = 34 };
            btnEditCt = new Button { Text = "✏️ Sửa", Width = 90, Height = 34 };
            btnDelCt = new Button { Text = "🗑️ Xóa", Width = 90, Height = 34 };

            btnAddCt.Click += (_, __) => AddOrEditCt(null);
            btnEditCt.Click += (_, __) =>
            {
                var mact = GetSelectedMaCt();
                if (mact != null) AddOrEditCt(mact);
            };
            btnDelCt.Click += (_, __) => DeleteCt();

            topCt.Controls.AddRange(new Control[] { btnAddCt, btnEditCt, btnDelCt });

            dgvCt = new DataGridView
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

            tabCt.Controls.Add(dgvCt);
            tabCt.Controls.Add(topCt);

            Controls.Add(tabs);
        }

        private void LoadLoai()
        {
            var dt = _spService.GetAllLoai();

            cboLoai.DisplayMember = "TENLOAI";
            cboLoai.ValueMember = "MALOAI";
            cboLoai.DataSource = dt;

            // If no category exists yet, keep the combobox usable.
            cboLoai.Enabled = dt.Rows.Count > 0;
        }

        private void LoadSanPham()
        {
            var dt = _spService.Search(_maSp!, null);
            if (dt.Rows.Count == 0) return;

            var r = dt.Rows[0];
            txtMaSp.Text = r["MASP"].ToString();
            txtTenSp.Text = r["TENSP"].ToString();
            cboLoai.SelectedValue = r["MALOAI"] == DBNull.Value ? null : r["MALOAI"];
            txtMoTa.Text = r["MOTA"] == DBNull.Value ? "" : r["MOTA"].ToString();
            txtHinh.Text = r["HINHSP"] == DBNull.Value ? "" : r["HINHSP"].ToString();
            chkTrangThai.Checked = r["TRANGTHAI"] != DBNull.Value && Convert.ToBoolean(r["TRANGTHAI"]);
        }

        private void LoadChiTiet()
        {
            if (string.IsNullOrWhiteSpace(txtMaSp.Text))
            {
                dgvCt.DataSource = null;
                btnAddCt.Enabled = false;
                btnEditCt.Enabled = false;
                btnDelCt.Enabled = false;
                return;
            }

            btnAddCt.Enabled = true;
            var dt = _ctService.GetByMaSp(txtMaSp.Text);
            dgvCt.DataSource = dt;

            if (dgvCt.Columns.Count > 0)
            {
                dgvCt.Columns["MACT"].HeaderText = "Mã CT";
                dgvCt.Columns["MASP"].Visible = false;
                dgvCt.Columns["SIZE"].HeaderText = "Size";
                dgvCt.Columns["MAU"].HeaderText = "Màu";
                dgvCt.Columns["GIABAN"].HeaderText = "Giá bán";
                dgvCt.Columns["TONKHO"].HeaderText = "Tồn kho";
                dgvCt.Columns["BARCODE"].HeaderText = "Barcode";
                dgvCt.Columns["TRANGTHAI"].HeaderText = "TT";
            }

            bool has = dt.Rows.Count > 0;
            btnEditCt.Enabled = has;
            btnDelCt.Enabled = has;
        }

        private void Save()
        {
            var dto = new SanPhamDto
            {
                MaSP = (txtMaSp.Text ?? string.Empty).Trim(),
                TenSP = (txtTenSp.Text ?? string.Empty).Trim(),
                MaLoai = cboLoai.SelectedValue?.ToString(),
                MoTa = string.IsNullOrWhiteSpace(txtMoTa.Text) ? null : txtMoTa.Text.Trim(),
                HinhSP = string.IsNullOrWhiteSpace(txtHinh.Text) ? null : txtHinh.Text.Trim(),
                TrangThai = chkTrangThai.Checked
            };

            var (ok, msg) = _maSp == null
                ? _spService.Insert(dto)
                : _spService.Update(dto);

            if (!ok)
            {
                MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private string? GetSelectedMaCt()
        {
            if (dgvCt.SelectedRows.Count == 0) return null;
            return dgvCt.SelectedRows[0].Cells["MACT"].Value?.ToString();
        }

        private void AddOrEditCt(string? maCt)
        {
            if (string.IsNullOrWhiteSpace(txtMaSp.Text)) return;

            using var dlg = new FrmSanPhamChiTietEdit(txtMaSp.Text, maCt);
            dlg.StartPosition = FormStartPosition.CenterParent;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadChiTiet();
            }
        }

        private void DeleteCt()
        {
            var maCt = GetSelectedMaCt();
            if (maCt == null) return;

            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa biến thể này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            var (ok, msg) = _ctService.Delete(maCt);
            MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) LoadChiTiet();
        }

        private void BrowseImage()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = "Chọn ảnh sản phẩm"
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            var imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
            System.IO.Directory.CreateDirectory(imagesDir);

            var fileName = System.IO.Path.GetFileName(ofd.FileName);
            var destFullPath = System.IO.Path.Combine(imagesDir, fileName);

            // If file name exists, append timestamp
            if (System.IO.File.Exists(destFullPath))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                var ext = System.IO.Path.GetExtension(fileName);
                fileName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                destFullPath = System.IO.Path.Combine(imagesDir, fileName);
            }

            System.IO.File.Copy(ofd.FileName, destFullPath, false);

            // Store relative path in DB textbox
            txtHinh.Text = $"Images\\Products\\{fileName}";
        }

        private void AddLoai()
        {
            using var dlg = new FrmLoaiSanPham();
            dlg.StartPosition = FormStartPosition.CenterParent;

            var result = dlg.ShowDialog(this);
            if (result != DialogResult.OK)
                return;

            var prev = cboLoai.SelectedValue?.ToString();
            LoadLoai();

            // Try keep previous selection; if not exists anymore, select last row.
            if (!string.IsNullOrWhiteSpace(prev))
            {
                try { cboLoai.SelectedValue = prev; } catch { /* ignore */ }
            }

            if (cboLoai.SelectedIndex < 0 && cboLoai.Items.Count > 0)
                cboLoai.SelectedIndex = cboLoai.Items.Count - 1;
        }

        private class FrmSanPhamChiTietEdit : Form
        {
            private readonly string _maSp;
            private readonly string? _maCt;
            private readonly SanPhamChiTietService _ctService = new();

            private TextBox txtMaCt = null!;
            private TextBox txtSize = null!;
            private TextBox txtMau = null!;
            private NumericUpDown numGia = null!;
            private NumericUpDown numTon = null!;
            private TextBox txtBar = null!;
            private CheckBox chkTt = null!;
            private Label lblHint = null!;

            public FrmSanPhamChiTietEdit(string maSp, string? maCt)
            {
                _maSp = maSp;
                _maCt = maCt;

                Text = maCt == null ? "Thêm biến thể" : "Sửa biến thể";
                Font = new Font("Segoe UI", 9F);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                ClientSize = new Size(520, 370);

                Build();
                if (_maCt != null) LoadVariant();
            }

            private void Build()
            {
                var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 8 };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                txtMaCt = new TextBox { Dock = DockStyle.Fill };
                txtSize = new TextBox { Dock = DockStyle.Fill };
                txtMau = new TextBox { Dock = DockStyle.Fill };
                numGia = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 999999999, DecimalPlaces = 0, ThousandsSeparator = true };
                numTon = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 9999999, DecimalPlaces = 0, ThousandsSeparator = true };
                txtBar = new TextBox { Dock = DockStyle.Fill };
                chkTt = new CheckBox { Text = "Đang bán", Dock = DockStyle.Left, Checked = true };
                lblHint = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(107, 114, 128) };

                layout.Controls.Add(new Label { Text = "Mã CT", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
                layout.Controls.Add(txtMaCt, 1, 0);
                layout.Controls.Add(new Label { Text = "Size", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
                layout.Controls.Add(txtSize, 1, 1);
                layout.Controls.Add(new Label { Text = "Màu", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
                layout.Controls.Add(txtMau, 1, 2);
                layout.Controls.Add(new Label { Text = "Giá bán", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
                layout.Controls.Add(numGia, 1, 3);
                layout.Controls.Add(new Label { Text = "Tồn kho", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
                layout.Controls.Add(numTon, 1, 4);
                layout.Controls.Add(new Label { Text = "Barcode", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
                layout.Controls.Add(txtBar, 1, 5);
                layout.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 6);
                layout.Controls.Add(chkTt, 1, 6);
                layout.Controls.Add(lblHint, 0, 7);
                layout.SetColumnSpan(lblHint, 2);

                var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(16), Height = 64 };
                var btnSave = new Button { Text = "Lưu", Width = 110, Height = 36 };
                var btnCancel = new Button { Text = "Hủy", Width = 110, Height = 36 };
                btnSave.Click += (_, __) => Save();
                btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;

                buttons.Controls.Add(btnSave);
                buttons.Controls.Add(btnCancel);

                Controls.Add(layout);
                Controls.Add(buttons);

                if (_maCt != null)
                {
                    txtMaCt.Text = _maCt;
                    txtMaCt.ReadOnly = true;
                    lblHint.Text = "";
                }
                else
                {
                    txtMaCt.Text = _ctService.GetNextMaCt();
                    txtMaCt.ReadOnly = true;
                    lblHint.Text = "Có thể nhập nhiều size/màu bằng dấu phẩy. Ví dụ: Size = S,M,L | Màu = Đen,Trắng";
                }
            }

            private static string[] SplitList(string? s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    return Array.Empty<string>();

                return s
                    .Split(new[] { ',', ';', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            private void Save()
            {
                // Edit mode: keep old behavior (single record)
                if (_maCt != null)
                {
                    var dtoEdit = new SanPhamChiTietDto
                    {
                        MaCT = (txtMaCt.Text ?? string.Empty).Trim(),
                        MaSP = _maSp,
                        Size = (txtSize.Text ?? string.Empty).Trim(),
                        Mau = (txtMau.Text ?? string.Empty).Trim(),
                        GiaBan = numGia.Value,
                        TonKho = (int)numTon.Value,
                        BarCode = string.IsNullOrWhiteSpace(txtBar.Text) ? null : txtBar.Text.Trim(),
                        TrangThai = chkTt.Checked
                    };

                    var (okEdit, msgEdit) = _ctService.Update(dtoEdit);
                    if (!okEdit)
                    {
                        MessageBox.Show(msgEdit, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    return;
                }

                // Add mode: allow bulk create combinations
                var sizes = SplitList(txtSize.Text);
                var colors = SplitList(txtMau.Text);

                if (sizes.Length == 0)
                {
                    MessageBox.Show("Vui lòng nhập size.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (colors.Length == 0)
                {
                    MessageBox.Show("Vui lòng nhập màu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int okCount = 0;
                var failed = new System.Collections.Generic.List<string>();

                foreach (var size in sizes)
                {
                    foreach (var mau in colors)
                    {
                        var dto = new SanPhamChiTietDto
                        {
                            MaCT = string.Empty, // service will auto-generate
                            MaSP = _maSp,
                            Size = size,
                            Mau = mau,
                            GiaBan = numGia.Value,
                            TonKho = (int)numTon.Value,
                            BarCode = string.IsNullOrWhiteSpace(txtBar.Text) ? null : txtBar.Text.Trim(),
                            TrangThai = chkTt.Checked
                        };

                        var (ok, msg) = _ctService.Insert(dto);
                        if (ok)
                            okCount++;
                        else
                            failed.Add($"{size}-{mau}: {msg}");
                    }
                }

                if (okCount == 0)
                {
                    MessageBox.Show(
                        "Không thể thêm biến thể.\n" + string.Join("\n", failed.Take(6)) + (failed.Count > 6 ? "\n..." : ""),
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (failed.Count > 0)
                {
                    MessageBox.Show(
                        $"Đã thêm {okCount} biến thể. Có {failed.Count} biến thể bị bỏ qua (trùng size/màu hoặc lỗi).",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }

            private void LoadVariant()
            {
                var dt = _ctService.GetByMaSp(_maSp);
                var row = dt.Rows.Cast<DataRow>().FirstOrDefault(r => r["MACT"].ToString() == _maCt);
                if (row == null) return;

                txtSize.Text = row["SIZE"].ToString();
                txtMau.Text = row["MAU"].ToString();
                numGia.Value = Convert.ToDecimal(row["GIABAN"]);
                numTon.Value = Convert.ToDecimal(row["TONKHO"]);
                txtBar.Text = row["BARCODE"] == DBNull.Value ? "" : row["BARCODE"].ToString();
                chkTt.Checked = row["TRANGTHAI"] != DBNull.Value && Convert.ToBoolean(row["TRANGTHAI"]);
            }
        }
    }
}
