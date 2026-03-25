namespace CLOTHES
{
    partial class FrmLoaiSanPham
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            toolStrip = new ToolStrip();
            btnThem = new ToolStripButton();
            btnSua = new ToolStripButton();
            btnXoa = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnRefresh = new ToolStripButton();
            splitContainer = new SplitContainer();
            groupBox1 = new GroupBox();
            txtTimKiem = new TextBox();
            label1 = new Label();
            dgvData = new DataGridView();
            groupBox2 = new GroupBox();
            btnHuy = new Button();
            btnLuu = new Button();
            txtTenLoai = new TextBox();
            txtMaLoai = new TextBox();
            label3 = new Label();
            label2 = new Label();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.Font = new Font("Segoe UI", 10F);
            toolStrip.ImageScalingSize = new Size(24, 24);
            toolStrip.Items.AddRange(new ToolStripItem[] { btnThem, btnSua, btnXoa, toolStripSeparator1, btnRefresh });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(8, 6, 8, 6);
            toolStrip.Size = new Size(1200, 44);
            toolStrip.TabIndex = 0;
            toolStrip.Text = "toolStrip1";
            // 
            // btnThem
            // 
            btnThem.Name = "btnThem";
            btnThem.Padding = new Padding(6, 0, 6, 0);
            btnThem.Size = new Size(86, 31);
            btnThem.Text = "➕ Thêm";
            btnThem.Click += btnThem_Click;
            // 
            // btnSua
            // 
            btnSua.Name = "btnSua";
            btnSua.Padding = new Padding(6, 0, 6, 0);
            btnSua.Size = new Size(72, 31);
            btnSua.Text = "✏️ Sửa";
            btnSua.Click += btnSua_Click;
            // 
            // btnXoa
            // 
            btnXoa.Name = "btnXoa";
            btnXoa.Padding = new Padding(6, 0, 6, 0);
            btnXoa.Size = new Size(74, 31);
            btnXoa.Text = "🗑️ Xóa";
            btnXoa.Click += btnXoa_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 32);
            // 
            // btnRefresh
            // 
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new Padding(6, 0, 6, 0);
            btnRefresh.Size = new Size(110, 31);
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel2;
            splitContainer.Location = new Point(0, 44);
            splitContainer.Name = "splitContainer";
            splitContainer.Panel1.Padding = new Padding(16);
            splitContainer.Panel2.Padding = new Padding(16);
            splitContainer.Size = new Size(1200, 656);
            splitContainer.SplitterDistance = 760;
            splitContainer.SplitterWidth = 6;
            splitContainer.TabIndex = 1;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(groupBox1);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(groupBox2);
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtTimKiem);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(dgvData);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            groupBox1.ForeColor = Color.FromArgb(51, 65, 85);
            groupBox1.Location = new Point(16, 16);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(16);
            groupBox1.Size = new Size(728, 624);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "📋 Danh sách loại sản phẩm";
            // 
            // txtTimKiem
            // 
            txtTimKiem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTimKiem.Font = new Font("Segoe UI", 11F);
            txtTimKiem.Location = new Point(110, 36);
            txtTimKiem.Name = "txtTimKiem";
            txtTimKiem.PlaceholderText = "Nhập từ khóa tìm kiếm...";
            txtTimKiem.Size = new Size(602, 32);
            txtTimKiem.TabIndex = 2;
            txtTimKiem.TextChanged += txtTimKiem_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 11F);
            label1.ForeColor = Color.FromArgb(75, 85, 99);
            label1.Location = new Point(18, 39);
            label1.Name = "label1";
            label1.Size = new Size(86, 25);
            label1.TabIndex = 1;
            label1.Text = "🔎 Tìm kiếm:";
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.BackgroundColor = Color.White;
            dgvData.BorderStyle = BorderStyle.None;
            dgvData.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvData.ColumnHeadersHeight = 42;
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvData.EnableHeadersVisualStyles = false;
            dgvData.Location = new Point(18, 84);
            dgvData.MultiSelect = false;
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dgvData.RowHeadersVisible = false;
            dgvData.RowTemplate.Height = 36;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.Size = new Size(694, 522);
            dgvData.TabIndex = 0;
            dgvData.SelectionChanged += dgvData_SelectionChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnHuy);
            groupBox2.Controls.Add(btnLuu);
            groupBox2.Controls.Add(txtTenLoai);
            groupBox2.Controls.Add(txtMaLoai);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label2);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            groupBox2.ForeColor = Color.FromArgb(51, 65, 85);
            groupBox2.Location = new Point(16, 16);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(16);
            groupBox2.Size = new Size(402, 624);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "🧾 Thông tin loại sản phẩm";
            // 
            // btnHuy
            // 
            btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHuy.BackColor = Color.FromArgb(239, 68, 68);
            btnHuy.FlatAppearance.BorderSize = 0;
            btnHuy.FlatStyle = FlatStyle.Flat;
            btnHuy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnHuy.ForeColor = Color.White;
            btnHuy.Location = new Point(292, 230);
            btnHuy.Name = "btnHuy";
            btnHuy.Size = new Size(94, 40);
            btnHuy.TabIndex = 5;
            btnHuy.Text = "❌ Hủy";
            btnHuy.UseVisualStyleBackColor = false;
            btnHuy.Click += btnHuy_Click;
            // 
            // btnLuu
            // 
            btnLuu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLuu.BackColor = Color.FromArgb(34, 197, 94);
            btnLuu.FlatAppearance.BorderSize = 0;
            btnLuu.FlatStyle = FlatStyle.Flat;
            btnLuu.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLuu.ForeColor = Color.White;
            btnLuu.Location = new Point(190, 230);
            btnLuu.Name = "btnLuu";
            btnLuu.Size = new Size(94, 40);
            btnLuu.TabIndex = 4;
            btnLuu.Text = "💾 Lưu";
            btnLuu.UseVisualStyleBackColor = false;
            btnLuu.Click += btnLuu_Click;
            // 
            // txtTenLoai
            // 
            txtTenLoai.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTenLoai.Font = new Font("Segoe UI", 11F);
            txtTenLoai.Location = new Point(18, 180);
            txtTenLoai.Name = "txtTenLoai";
            txtTenLoai.PlaceholderText = "Nhập tên loại sản phẩm...";
            txtTenLoai.Size = new Size(368, 32);
            txtTenLoai.TabIndex = 3;
            // 
            // txtMaLoai
            // 
            txtMaLoai.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMaLoai.BackColor = Color.FromArgb(249, 250, 251);
            txtMaLoai.Font = new Font("Segoe UI", 11F);
            txtMaLoai.Location = new Point(18, 100);
            txtMaLoai.Name = "txtMaLoai";
            txtMaLoai.ReadOnly = true;
            txtMaLoai.Size = new Size(368, 32);
            txtMaLoai.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F);
            label3.ForeColor = Color.FromArgb(75, 85, 99);
            label3.Location = new Point(18, 155);
            label3.Name = "label3";
            label3.Size = new Size(143, 23);
            label3.TabIndex = 1;
            label3.Text = "Tên loại sản phẩm";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.ForeColor = Color.FromArgb(75, 85, 99);
            label2.Location = new Point(18, 75);
            label2.Name = "label2";
            label2.Size = new Size(132, 23);
            label2.TabIndex = 0;
            label2.Text = "Mã loại sản phẩm";
            // 
            // FrmLoaiSanPham
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1200, 700);
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Font = new Font("Segoe UI", 9F);
            Name = "FrmLoaiSanPham";
            Text = "Loại sản phẩm";
            Load += FrmLoaiSanPham_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton btnThem;
        private ToolStripButton btnSua;
        private ToolStripButton btnXoa;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnRefresh;
        private SplitContainer splitContainer;
        private GroupBox groupBox1;
        private TextBox txtTimKiem;
        private Label label1;
        private DataGridView dgvData;
        private GroupBox groupBox2;
        private Button btnHuy;
        private Button btnLuu;
        private TextBox txtTenLoai;
        private TextBox txtMaLoai;
        private Label label3;
        private Label label2;
    }
}