namespace CLOTHES;

partial class FrmNguoiDung
{
	private System.ComponentModel.IContainer components = null;
	private System.Windows.Forms.DataGridView gridUsers;
	private System.Windows.Forms.Button btnReload;
	private System.Windows.Forms.Button btnToggleStatus;
  private System.Windows.Forms.Button btnUpdateRole;

	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
			components.Dispose();
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		gridUsers = new System.Windows.Forms.DataGridView();
		btnReload = new System.Windows.Forms.Button();
		btnToggleStatus = new System.Windows.Forms.Button();
		btnUpdateRole = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)gridUsers).BeginInit();
		SuspendLayout();
		// 
		// gridUsers
		// 
		gridUsers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		gridUsers.Location = new System.Drawing.Point(12, 58);
		gridUsers.Name = "gridUsers";
		gridUsers.RowHeadersWidth = 51;
		gridUsers.Size = new System.Drawing.Size(960, 490);
		gridUsers.TabIndex = 0;
		// 
		// btnReload
		// 
		btnReload.Location = new System.Drawing.Point(12, 12);
		btnReload.Name = "btnReload";
		btnReload.Size = new System.Drawing.Size(94, 29);
		btnReload.TabIndex = 1;
		btnReload.Text = "Tải lại";
		btnReload.UseVisualStyleBackColor = true;
		btnReload.Click += btnReload_Click;
		// 
		// btnToggleStatus
		// 
		btnToggleStatus.Location = new System.Drawing.Point(112, 12);
		btnToggleStatus.Name = "btnToggleStatus";
		btnToggleStatus.Size = new System.Drawing.Size(140, 29);
		btnToggleStatus.TabIndex = 2;
		btnToggleStatus.Text = "Khóa/Mở khóa";
		btnToggleStatus.UseVisualStyleBackColor = true;
		btnToggleStatus.Click += btnToggleStatus_Click;
		// 
		// cboRole
		// 
		// btnUpdateRole
		// 
     btnUpdateRole.Location = new System.Drawing.Point(270, 12);
		btnUpdateRole.Name = "btnUpdateRole";
		btnUpdateRole.Size = new System.Drawing.Size(126, 29);
     btnUpdateRole.TabIndex = 3;
		btnUpdateRole.Text = "Lưu thay đổi";
		btnUpdateRole.UseVisualStyleBackColor = true;
		btnUpdateRole.Click += btnUpdateRole_Click;
		// 
		// FrmNguoiDung
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		ClientSize = new System.Drawing.Size(984, 561);
		Controls.Add(btnUpdateRole);
		Controls.Add(btnToggleStatus);
		Controls.Add(btnReload);
		Controls.Add(gridUsers);
		Name = "FrmNguoiDung";
		Text = "Quản lý người dùng";
		Load += FrmNguoiDung_Load;
		((System.ComponentModel.ISupportInitialize)gridUsers).EndInit();
		ResumeLayout(false);
	}
}
