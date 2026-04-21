using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;
using QLBH.DTO;

namespace CLOTHES;

public partial class FrmNguoiDung : Form
{
	private readonly NguoiDungDto _currentUser;
	private readonly NguoiDungService _service = new();
	private DataTable? _table;

	public FrmNguoiDung(NguoiDungDto currentUser)
	{
		InitializeComponent();
		_currentUser = currentUser;
		Font = new Font("Segoe UI", 9F);
	}

	private bool IsAdmin => _currentUser.VaiTro.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

	private void FrmNguoiDung_Load(object sender, EventArgs e)
	{
		if (!IsAdmin)
		{
			MessageBox.Show("Bạn không có quyền truy cập chức năng này.", "Thông báo",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);
			Close();
			return;
		}

		LoadUsers();
	}

	private void LoadUsers()
	{
		_table = _service.GetAll();
        NormalizeRoles(_table);
		gridUsers.DataSource = _table;

		if (gridUsers.Columns.Contains("PASS"))
			gridUsers.Columns["PASS"].Visible = false;

       gridUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		gridUsers.SelectionMode = DataGridViewSelectionMode.CellSelect;
		gridUsers.MultiSelect = false;
		gridUsers.AllowUserToAddRows = false;
		gridUsers.ReadOnly = true;
		gridUsers.EditMode = DataGridViewEditMode.EditProgrammatically;

		gridUsers.CurrentCellDirtyStateChanged -= GridUsers_CurrentCellDirtyStateChanged;
		gridUsers.CurrentCellDirtyStateChanged += GridUsers_CurrentCellDirtyStateChanged;
		gridUsers.DataError -= GridUsers_DataError;
		gridUsers.DataError += GridUsers_DataError;
		gridUsers.CellDoubleClick -= GridUsers_CellDoubleClick;
		gridUsers.CellDoubleClick += GridUsers_CellDoubleClick;

		if (gridUsers.Columns.Contains("TRANGTHAI"))
			gridUsers.Columns["TRANGTHAI"].HeaderText = "Hoạt động";

		if (gridUsers.Columns.Contains("MAND"))
			gridUsers.Columns["MAND"].ReadOnly = true;
		if (gridUsers.Columns.Contains("USERNAME"))
			gridUsers.Columns["USERNAME"].ReadOnly = true;
		if (gridUsers.Columns.Contains("TRANGTHAI"))
			gridUsers.Columns["TRANGTHAI"].ReadOnly = true;

		EnsureRoleComboColumn();
	}

	private static void NormalizeRoles(DataTable? table)
	{
		if (table == null || !table.Columns.Contains("VAITRO"))
			return;

		foreach (DataRow row in table.Rows)
		{
			var role = row["VAITRO"] == DBNull.Value ? string.Empty : row["VAITRO"].ToString();
			role = string.IsNullOrWhiteSpace(role) ? "NHANVIENBANHANG" : role.Trim().ToUpperInvariant();
			row["VAITRO"] = role;
		}
	}

	private void GridUsers_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
	{
		if (gridUsers.IsCurrentCellDirty)
			gridUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);
	}

	private void GridUsers_DataError(object? sender, DataGridViewDataErrorEventArgs e)
	{
		e.ThrowException = false;
	}

	private void EnsureRoleComboColumn()
	{
		if (!gridUsers.Columns.Contains("VAITRO"))
			return;

		if (gridUsers.Columns["VAITRO"] is DataGridViewComboBoxColumn)
			return;

		var roleIndex = gridUsers.Columns["VAITRO"].Index;
		gridUsers.Columns.Remove("VAITRO");

		var col = new DataGridViewComboBoxColumn
		{
			Name = "VAITRO",
			HeaderText = "VAITRO",
			DataPropertyName = "VAITRO",
			FlatStyle = FlatStyle.Flat,
			DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
			DisplayStyleForCurrentCellOnly = true,
			ReadOnly = true
		};
		col.Items.AddRange("ADMIN");
		if (_table != null && _table.Columns.Contains("VAITRO"))
		{
			foreach (var role in _table.AsEnumerable()
				.Select(r => r["VAITRO"] == DBNull.Value ? string.Empty : r["VAITRO"].ToString())
				.Where(r => !string.IsNullOrWhiteSpace(r))
				.Select(r => r!.Trim().ToUpperInvariant())
				.Distinct())
			{
				if (!col.Items.Contains(role))
					col.Items.Add(role);
			}
		}

		gridUsers.Columns.Insert(roleIndex, col);
	}

	private DataGridViewRow? CurrentRow
	{
		get
		{
			if (gridUsers.CurrentRow != null)
				return gridUsers.CurrentRow;
			if (gridUsers.SelectedCells.Count > 0)
				return gridUsers.SelectedCells[0].OwningRow;
			return null;
		}
	}

	private string? SelectedMaNd => CurrentRow?.Cells["MAND"]?.Value?.ToString();
	private string? SelectedVaiTro => CurrentRow?.Cells["VAITRO"]?.Value?.ToString();

	private void btnReload_Click(object sender, EventArgs e) => LoadUsers();

	private void btnAddUser_Click(object sender, EventArgs e)
	{
		using var dlg = new FrmNguoiDungCreate();
		dlg.StartPosition = FormStartPosition.CenterParent;
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		var (ok, msg) = _service.CreateUser(
			dlg.TenNd,
			dlg.Sdt,
			dlg.Username,
			dlg.Password,
			dlg.VaiTro,
			dlg.TrangThai);

		MessageBox.Show(msg, ok ? "Thành công" : "Lỗi",
			MessageBoxButtons.OK,
			ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
		if (ok) LoadUsers();
	}

	private void GridUsers_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0)
			return;
		if (_table == null)
			return;

		var row = gridUsers.Rows[e.RowIndex];
		var maNd = row.Cells["MAND"].Value?.ToString() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(maNd))
			return;

		var dlg = new FrmNguoiDungCreate(row);
		dlg.StartPosition = FormStartPosition.CenterParent;
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		var (ok, msg) = _service.UpdateUser(
			dlg.MaNd,
			dlg.TenNd,
			dlg.Sdt,
			dlg.VaiTro,
			dlg.TrangThai,
			dlg.Password);

		MessageBox.Show(msg, ok ? "Thành công" : "Lỗi",
			MessageBoxButtons.OK,
			ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
		if (ok) LoadUsers();
	}

	private sealed class FrmNguoiDungCreate : Form
	{
		public string MaNd => txtMaNd.Text.Trim();
		public string TenNd => txtTenNd.Text.Trim();
		public string Sdt => txtSdt.Text.Trim();
		public string Username => txtUsername.Text.Trim();
		public string Password => txtPassword.Text;
		public string VaiTro => cboRole.SelectedItem?.ToString() ?? string.Empty;
		public bool TrangThai => chkActive.Checked;

		private readonly bool _isEdit;
		private readonly string _originalUsername;

		private readonly TextBox txtMaNd = new();
		private readonly TextBox txtTenNd = new();
		private readonly TextBox txtSdt = new();
		private readonly TextBox txtUsername = new();
		private readonly TextBox txtPassword = new();
		private readonly ComboBox cboRole = new();
		private readonly CheckBox chkActive = new();

		public FrmNguoiDungCreate()
		{
			Text = "Thêm người dùng";
			_isEdit = false;
			_originalUsername = string.Empty;
			Font = new Font("Segoe UI", 9F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(420, 360);
			MaximizeBox = false;
			MinimizeBox = false;
			BuildUi();
		}

		public FrmNguoiDungCreate(DataGridViewRow row)
		{
			Text = "Sửa người dùng";
			_isEdit = true;
			Font = new Font("Segoe UI", 9F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(420, 360);
			MaximizeBox = false;
			MinimizeBox = false;
			BuildUi();

			txtMaNd.Text = row.Cells["MAND"].Value?.ToString() ?? string.Empty;
			txtTenNd.Text = row.Cells["TENND"].Value?.ToString() ?? string.Empty;
			txtSdt.Text = row.Cells["SDT"].Value?.ToString() ?? string.Empty;
			var username = row.Cells["USERNAME"].Value?.ToString() ?? string.Empty;
			_originalUsername = username;
			txtUsername.Text = username;
			txtUsername.ReadOnly = true;

			var role = row.Cells["VAITRO"].Value?.ToString() ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(role) && cboRole.Items.Contains(role))
				cboRole.SelectedItem = role;
			else if (cboRole.Items.Count > 0)
				cboRole.SelectedIndex = 0;

			var status = row.Cells["TRANGTHAI"].Value;
			chkActive.Checked = status is bool b
				? b
				: (status is int i ? i != 0
				: (status != null && bool.TryParse(status.ToString(), out var parsed) ? parsed : true));

			txtPassword.PlaceholderText = "Để trống nếu không đổi";
		}

		private void BuildUi()
		{
			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(16),
				ColumnCount = 2,
				RowCount = 7
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

			layout.Controls.Add(new Label { Text = "Mã ND", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
			layout.Controls.Add(txtMaNd, 1, 0);
			layout.Controls.Add(new Label { Text = "Họ tên", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
			layout.Controls.Add(txtTenNd, 1, 1);
			layout.Controls.Add(new Label { Text = "SĐT", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
			layout.Controls.Add(txtSdt, 1, 2);
			layout.Controls.Add(new Label { Text = "Tài khoản", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
			layout.Controls.Add(txtUsername, 1, 3);
			layout.Controls.Add(new Label { Text = "Mật khẩu", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
			layout.Controls.Add(txtPassword, 1, 4);
			layout.Controls.Add(new Label { Text = "Vai trò", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
			layout.Controls.Add(cboRole, 1, 5);
			layout.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 6);
			layout.Controls.Add(chkActive, 1, 6);

			foreach (Control control in new Control[] { txtMaNd, txtTenNd, txtSdt, txtUsername, txtPassword, cboRole })
			{
				control.Dock = DockStyle.Fill;
			}

			txtPassword.UseSystemPasswordChar = true;
			cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
			cboRole.Items.AddRange(new object[] { "ADMIN", "QUANLY", "NHANVIENNHAP", "NHANVIENBANHANG" });
			cboRole.SelectedIndex = 3;
			chkActive.Text = "Hoạt động";
			chkActive.Checked = true;
			chkActive.Dock = DockStyle.Left;
			txtMaNd.ReadOnly = true;
			if (!_isEdit)
				txtMaNd.Text = new NguoiDungService().GetNextMaNd();

			var buttons = new FlowLayoutPanel
			{
				Dock = DockStyle.Bottom,
				Height = 64,
				FlowDirection = FlowDirection.RightToLeft,
				Padding = new Padding(16)
			};
			var btnSave = new Button { Text = "Lưu", Width = 100, Height = 32 };
			var btnCancel = new Button { Text = "Hủy", Width = 100, Height = 32 };
			btnSave.Click += (_, __) => DialogResult = DialogResult.OK;
			btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;
			buttons.Controls.Add(btnSave);
			buttons.Controls.Add(btnCancel);

			Controls.Add(layout);
			Controls.Add(buttons);
		}
	}
}
