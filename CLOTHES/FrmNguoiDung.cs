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
		gridUsers.EditMode = DataGridViewEditMode.EditOnEnter;

		gridUsers.CurrentCellDirtyStateChanged -= GridUsers_CurrentCellDirtyStateChanged;
		gridUsers.CurrentCellDirtyStateChanged += GridUsers_CurrentCellDirtyStateChanged;
		gridUsers.DataError -= GridUsers_DataError;
		gridUsers.DataError += GridUsers_DataError;

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
			role = string.IsNullOrWhiteSpace(role) ? "NHANVIEN" : role.Trim().ToUpperInvariant();
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
			DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
		};
        col.Items.AddRange("ADMIN", "NHANVIEN");
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

	private string? SelectedMaNd => gridUsers.CurrentRow?.Cells["MAND"]?.Value?.ToString();
 private string? SelectedVaiTro => gridUsers.CurrentRow?.Cells["VAITRO"]?.Value?.ToString();

	private void btnReload_Click(object sender, EventArgs e) => LoadUsers();

	private void btnToggleStatus_Click(object sender, EventArgs e)
	{
		var maNd = SelectedMaNd;
		if (string.IsNullOrWhiteSpace(maNd))			
			return;

		var current = false;
		var val = gridUsers.CurrentRow?.Cells["TRANGTHAI"]?.Value;
		if (val is bool b) current = b;
		else if (val != null && bool.TryParse(val.ToString(), out var parsed)) current = parsed;

		var next = !current;
		var (ok, msg) = _service.UpdateStatus(maNd, next);
		MessageBox.Show(msg, ok ? "Thành công" : "Lỗi",
			MessageBoxButtons.OK,
			ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
		if (ok) LoadUsers();
	}

	private void btnUpdateRole_Click(object sender, EventArgs e)
	{
        if (_table == null)
			return;

        gridUsers.EndEdit();
		gridUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);

		var changed = _table.GetChanges();
		if (changed == null || changed.Rows.Count == 0)
		{
			MessageBox.Show("Không có thay đổi để lưu.", "Thông báo",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		int success = 0;
		int failed = 0;

        foreach (DataRow row in changed.Rows)
		{
         var maNd = row["MAND", DataRowVersion.Current]?.ToString();
			var tenNd = row["TENND", DataRowVersion.Current]?.ToString() ?? string.Empty;
			var sdt = row["SDT", DataRowVersion.Current]?.ToString() ?? string.Empty;
			var vaiTro = row["VAITRO", DataRowVersion.Current]?.ToString();
			if (string.IsNullOrWhiteSpace(maNd) || string.IsNullOrWhiteSpace(vaiTro))
				continue;

			var okAll = true;

			var (okInfo, _) = _service.UpdateInfo(maNd, tenNd, sdt);
			okAll &= okInfo;

			var (okRole, _) = _service.UpdateRole(maNd, vaiTro);
			okAll &= okRole;

			if (okAll) success++;
			else failed++;
		}

		MessageBox.Show($"Đã lưu: {success}. Lỗi: {failed}.", "Kết quả",
			MessageBoxButtons.OK,
			failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

		LoadUsers();
	}
}
