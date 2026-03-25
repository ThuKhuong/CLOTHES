using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES
{
    public partial class FrmLoaiSanPham : Form
    {
        private readonly LoaiSanPhamService _service = new();
        private DataTable? _dataSource;
        private bool _isEditing = false;
        private string? _editingMaLoai = null;

        public FrmLoaiSanPham()
        {
            InitializeComponent();

            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            SetDefaultFont(this);

            txtMaLoai.ReadOnly = true;
        }

        private void SetDefaultFont(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.Font = new Font("Segoe UI", control.Font.Size, control.Font.Style);

                if (control is Label label)
                {
                    label.UseCompatibleTextRendering = false;
                }
                else if (control is Button button)
                {
                    button.UseCompatibleTextRendering = false;
                }

                SetDefaultFont(control);
            }
        }

        private void FrmLoaiSanPham_Load(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            LoadData();
            SetMode(false);
        }

        private void LoadData()
        {
            try
            {
                _dataSource = _service.GetAll();
                dgvData.DataSource = _dataSource;

                if (dgvData.Columns.Count > 0)
                {
                    dgvData.Columns["MALOAI"].HeaderText = "Mã loại";
                    dgvData.Columns["MALOAI"].Width = 120;
                    dgvData.Columns["TENLOAI"].HeaderText = "Tên loại sản phẩm";
                    dgvData.Columns["TENLOAI"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    dgvData.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                    dgvData.DefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
                    dgvData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(79, 70, 229);
                    dgvData.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

                    dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
                    dgvData.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvData.EnableHeadersVisualStyles = false;
                }

                if (dgvData.Rows.Count > 0)
                {
                    dgvData.ClearSelection();
                    dgvData.Rows[0].Selected = true;
                    LoadSelectedItem();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetMode(bool editMode)
        {
            _isEditing = editMode;

            btnThem.Enabled = !editMode;
            btnSua.Enabled = !editMode && dgvData.SelectedRows.Count > 0;
            btnXoa.Enabled = !editMode && dgvData.SelectedRows.Count > 0;
            btnRefresh.Enabled = !editMode;

            txtTenLoai.Enabled = editMode;
            btnLuu.Enabled = editMode;
            btnHuy.Enabled = editMode;

            dgvData.Enabled = !editMode;
            txtTimKiem.Enabled = !editMode;

            if (!editMode)
            {
                ClearInputs();
                _editingMaLoai = null;
            }
        }

        private void ClearInputs()
        {
            txtMaLoai.Text = string.Empty;
            txtTenLoai.Text = string.Empty;
        }

        private void LoadSelectedItem()
        {
            if (dgvData.SelectedRows.Count == 0) return;

            if (dgvData.SelectedRows[0].DataBoundItem is DataRowView row)
            {
                txtMaLoai.Text = row["MALOAI"].ToString();
                txtTenLoai.Text = row["TENLOAI"].ToString();
            }
        }

        private void dgvData_SelectionChanged(object sender, EventArgs e)
        {
            if (_isEditing) return;

            LoadSelectedItem();
            btnSua.Enabled = dgvData.SelectedRows.Count > 0;
            btnXoa.Enabled = dgvData.SelectedRows.Count > 0;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            _editingMaLoai = null;
            ClearInputs();
            SetMode(true);

            txtMaLoai.Text = _service.GetNextMaLoai();
            txtTenLoai.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count == 0) return;

            _editingMaLoai = txtMaLoai.Text;
            SetMode(true);
            txtTenLoai.Focus();
            txtTenLoai.SelectAll();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count == 0) return;

            string maLoai = txtMaLoai.Text;
            string tenLoai = txtTenLoai.Text;

            var result = MessageBox.Show(
                $"⚠️ Bạn có chắc chắn muốn xóa loại sản phẩm:\n\n" +
                $"🔹 Mã: {maLoai}\n" +
                $"🔸 Tên: {tenLoai}\n\n" +
                $"❗ Thao tác này không thể hoàn tác!",
                "🗑️ Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                var (success, message) = _service.Delete(maLoai);
                MessageBox.Show(
                    (success ? "✅ " : "❌ ") + message,
                    success ? "Thành công" : "Lỗi",
                    MessageBoxButtons.OK,
                    success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

                if (success)
                {
                    LoadData();
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenLoai.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại sản phẩm.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenLoai.Focus();
                return;
            }

            var loai = new LoaiSanPhamDto
            {
                MaLoai = txtMaLoai.Text,
                TenLoai = txtTenLoai.Text.Trim()
            };

            bool isNew = string.IsNullOrEmpty(_editingMaLoai);
            var (success, message) = isNew
                ? _service.Insert(loai)
                : _service.Update(loai);

            MessageBox.Show(
                (success ? "✅ " : "❌ ") + message,
                success ? "Thành công" : "Lỗi",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success)
            {
                LoadData();
                SetMode(false);

                // When used as a dialog for picking/adding categories
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            SetMode(false);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (_dataSource == null) return;

            string filter = txtTimKiem.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                _dataSource.DefaultView.RowFilter = "";
            }
            else
            {
                string escapedFilter = filter.Replace("'", "''");
                _dataSource.DefaultView.RowFilter =
                    $"TENLOAI LIKE '%{escapedFilter}%' OR MALOAI LIKE '%{escapedFilter}%'";
            }
        }
    }
}
