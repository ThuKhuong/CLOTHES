namespace CLOTHES
{
    partial class FrmSanPham
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
            panelRoot = new Panel();
            panelHeader = new Panel();
            cboLoai = new ComboBox();
            cboSort = new ComboBox();
            chkLowStockOnly = new CheckBox();
            txtSearch = new TextBox();
            btnAdd = new Button();
            btnDelete = new Button();
            panelGrid = new FlowLayoutPanel();
            panelRoot.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.FromArgb(248, 250, 252);
            panelRoot.Controls.Add(panelGrid);
            panelRoot.Controls.Add(panelHeader);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Padding = new Padding(24);
            panelRoot.Size = new Size(1200, 720);
            panelRoot.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.Transparent;
            panelHeader.Controls.Add(btnDelete);
            panelHeader.Controls.Add(btnAdd);
            panelHeader.Controls.Add(txtSearch);
            panelHeader.Controls.Add(chkLowStockOnly);
            panelHeader.Controls.Add(cboSort);
            panelHeader.Controls.Add(cboLoai);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(24, 24);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1152, 104);
            panelHeader.TabIndex = 0;
            // 
            // cboLoai
            // 
            cboLoai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoai.Font = new Font("Segoe UI", 11F);
            cboLoai.FormattingEnabled = true;
            cboLoai.Location = new Point(4, 58);
            cboLoai.Name = "cboLoai";
            cboLoai.Size = new Size(160, 33);
            cboLoai.TabIndex = 1;
            // 
            // cboSort
            // 
            cboSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSort.Font = new Font("Segoe UI", 11F);
            cboSort.FormattingEnabled = true;
            cboSort.Items.AddRange(new object[] { "Tên SP", "Mã SP" });
            cboSort.Location = new Point(176, 58);
            cboSort.Name = "cboSort";
            cboSort.Size = new Size(160, 33);
            cboSort.TabIndex = 2;

            // 
            // chkLowStockOnly
            // 
            chkLowStockOnly.AutoSize = true;
            chkLowStockOnly.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkLowStockOnly.ForeColor = Color.FromArgb(220, 38, 38);
            chkLowStockOnly.Location = new Point(348, 18);
            chkLowStockOnly.Name = "chkLowStockOnly";
            chkLowStockOnly.Size = new Size(280, 27);
            chkLowStockOnly.TabIndex = 6;
            chkLowStockOnly.Text = "Chỉ hiện tồn thấp (<= 5)";
            chkLowStockOnly.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.Location = new Point(348, 58);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Tìm kiếm";
            txtSearch.Size = new Size(650, 32);
            txtSearch.TabIndex = 3;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.BackColor = Color.FromArgb(239, 68, 68);
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(870, 54);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(130, 40);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "Xóa";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.BackColor = Color.FromArgb(34, 197, 94);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(1010, 54);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(130, 40);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "Thêm";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // panelGrid
            // 
            panelGrid.AutoScroll = true;
            panelGrid.BackColor = Color.Transparent;
            panelGrid.Dock = DockStyle.Fill;
            panelGrid.FlowDirection = FlowDirection.LeftToRight;
            panelGrid.Location = new Point(24, 128);
            panelGrid.Name = "panelGrid";
            panelGrid.Padding = new Padding(0, 8, 0, 0);
            panelGrid.Size = new Size(1152, 568);
            panelGrid.TabIndex = 1;
            panelGrid.WrapContents = true;
            // 
            // FrmSanPham
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1200, 720);
            Controls.Add(panelRoot);
            Font = new Font("Segoe UI", 9F);
            Name = "FrmSanPham";
            Text = "Sản phẩm";
            Load += FrmSanPham_Load;
            panelRoot.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelRoot;
        private Panel panelHeader;
        private ComboBox cboLoai;
        private ComboBox cboSort;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnDelete;
        private FlowLayoutPanel panelGrid;
       private CheckBox chkLowStockOnly;
    }
}
