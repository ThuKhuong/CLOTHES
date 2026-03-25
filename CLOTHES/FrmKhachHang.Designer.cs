namespace CLOTHES
{
    partial class FrmKhachHang
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
            panelList = new FlowLayoutPanel();
            panelHeader = new Panel();
            btnAdd = new Button();
            txtSearch = new TextBox();
            cboSort = new ComboBox();
            lblTitle = new Label();
            panelRoot.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.FromArgb(248, 250, 252);
            panelRoot.Controls.Add(panelList);
            panelRoot.Controls.Add(panelHeader);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Margin = new Padding(0);
            panelRoot.Name = "panelRoot";
            panelRoot.Padding = new Padding(39, 38, 39, 38);
            panelRoot.Size = new Size(1788, 1120);
            panelRoot.TabIndex = 0;
            // 
            // panelList
            // 
            panelList.AutoScroll = true;
            panelList.BackColor = Color.Transparent;
            panelList.Dock = DockStyle.Fill;
            panelList.FlowDirection = FlowDirection.TopDown;
            panelList.Location = new Point(39, 166);
            panelList.Margin = new Padding(0);
            panelList.Name = "panelList";
            panelList.Padding = new Padding(0, 26, 0, 0);
            panelList.Size = new Size(1710, 916);
            panelList.TabIndex = 1;
            panelList.WrapContents = false;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.Transparent;
            panelHeader.Controls.Add(btnAdd);
            panelHeader.Controls.Add(txtSearch);
            panelHeader.Controls.Add(cboSort);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(39, 38);
            panelHeader.Margin = new Padding(0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1710, 128);
            panelHeader.TabIndex = 0;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.BackColor = Color.FromArgb(79, 70, 229);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(1487, 74);
            btnAdd.Margin = new Padding(5);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(203, 61);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "Thêm";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.Location = new Point(276, 80);
            txtSearch.Margin = new Padding(5);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Tìm kiếm";
            txtSearch.Size = new Size(1184, 47);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // cboSort
            // 
            cboSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSort.Font = new Font("Segoe UI", 11F);
            cboSort.FormattingEnabled = true;
            cboSort.Items.AddRange(new object[] { "Họ tên", "Mã KH", "Số điện thoại" });
            cboSort.Location = new Point(20, 80);
            cboSort.Margin = new Padding(5);
            cboSort.Name = "cboSort";
            cboSort.Size = new Size(225, 48);
            cboSort.TabIndex = 1;
            cboSort.SelectedIndexChanged += cboSort_SelectedIndexChanged;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(79, 70, 229);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(5, 0, 5, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(430, 78);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "KHÁCH HÀNG";
            // 
            // FrmKhachHang
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1788, 1120);
            Controls.Add(panelRoot);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(5);
            Name = "FrmKhachHang";
            Text = "Khách hàng";
            Load += FrmKhachHang_Load;
            panelRoot.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelRoot;
        private Panel panelHeader;
        private Label lblTitle;
        private ComboBox cboSort;
        private TextBox txtSearch;
        private Button btnAdd;
        private FlowLayoutPanel panelList;
        private Button button1;
    }
}