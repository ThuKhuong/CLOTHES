using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmNhaCungCap : Form
{
    private readonly PhieuNhapService _service = new();
    private DataTable? _source;
    private List<DataRow> _view = new();

    private TextBox txtSearch = null!;
    private ComboBox cboSort = null!;
    private FlowLayoutPanel panelList = null!;
    private Button btnAdd = null!;

    public FrmNhaCungCap()
    {
        Text = "Nhà cung cấp";
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var title = new Label
        {
            Text = "NHÀ CUNG CẤP",
            AutoSize = true,
            Location = new Point(0, 0),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        btnAdd = new Button
        {
            Text = "Thêm",
            Width = 100,
            Height = 30,
            BackColor = Color.FromArgb(34, 197, 94),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        btnAdd.FlatStyle = FlatStyle.Flat;
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Location = new Point(0, 60);
        btnAdd.Click += (_, __) => ShowEditDialog(null);
        txtSearch = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            PlaceholderText = "Tìm nhà cung cấp...",
            Font = new Font("Segoe UI", 11F),
            Location = new Point(230, 60),
            Size = new Size(600, 40)
        };
        txtSearch.TextChanged += (_, __) => ApplyFilterAndRender();
        cboSort = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 11F),
            Location = new Point(0, 60),
            Size = new Size(200, 40)
        };
        cboSort.Items.AddRange(new object[] { "Họ tên", "Mã NCC", "SĐT" });
        cboSort.SelectedIndex = 0;
        cboSort.SelectedIndexChanged += (_, __) => ApplyFilterAndRender();
        header.Controls.Add(title);
        header.Controls.Add(cboSort);
        header.Controls.Add(txtSearch);
        header.Controls.Add(btnAdd);

        panelList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        panelList.SizeChanged += (_, __) => UpdateRowWidths();

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(panelList, 0, 1);

        Controls.Add(root);
    }

    private void LoadData()
    {
        _source = _service.GetAllNccDetails();
        ApplyFilterAndRender();
    }

    private void ApplyFilterAndRender()
    {
        if (_source == null) return;

        var keyword = (txtSearch.Text ?? string.Empty).Trim();
        IEnumerable<DataRow> query = _source.Rows.Cast<DataRow>();
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(r =>
            {
                var ma = r["MANCC"]?.ToString() ?? string.Empty;
                var ten = r["TENNCC"]?.ToString() ?? string.Empty;
                var sdt = r["SDT"]?.ToString() ?? string.Empty;
                var diaChi = r["DIACHI"]?.ToString() ?? string.Empty;
                return ma.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || ten.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || sdt.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || diaChi.Contains(keyword, StringComparison.OrdinalIgnoreCase);
            });
        }

        query = cboSort.SelectedIndex switch
        {
            1 => query.OrderBy(r => r["MANCC"]?.ToString()),
            2 => query.OrderBy(r => r["SDT"]?.ToString()),
            _ => query.OrderBy(r => r["TENNCC"]?.ToString())
        };

        _view = query.ToList();
        RenderRows();
    }

    private void RenderRows()
    {
        panelList.SuspendLayout();
        panelList.Controls.Clear();

        panelList.Controls.Add(CreateHeaderRow());
        foreach (var row in _view)
        {
            panelList.Controls.Add(CreateRow(row));
        }

        panelList.ResumeLayout();
        UpdateRowWidths();
    }

    private Control CreateHeaderRow()
    {
        var row = new Panel
        {
            Height = 34,
            Width = panelList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
            Margin = new Padding(0, 8, 0, 8),
            BackColor = Color.Transparent
        };
        row.MinimumSize = new Size(row.Width, row.Height);

        row.Controls.Add(MkHeaderLabel("MÃ NCC", 0, 90));
        row.Controls.Add(MkHeaderLabel("TÊN NCC", 100, 300));
        row.Controls.Add(MkHeaderLabel("SĐT", 420, 120));
        row.Controls.Add(MkHeaderLabel("EMAIL", 550, 200));
        row.Controls.Add(MkHeaderLabel("ĐỊA CHỈ", 760, Math.Max(200, row.Width - 760)));

        return row;
    }

    private Label MkHeaderLabel(string text, int x, int w)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, 8),
            Width = w,
            Height = 20,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(75, 85, 99)
        };
    }

    private Control CreateRow(DataRow data)
    {
        var row = new Panel
        {
            Height = 44,
            Width = panelList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        row.MinimumSize = new Size(row.Width, row.Height);

        row.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 10);
            using var br = new SolidBrush(row.BackColor);
            e.Graphics.FillPath(br, path);
        };

        var ma = data["MANCC"]?.ToString() ?? string.Empty;
        var ten = data["TENNCC"]?.ToString() ?? string.Empty;
        var sdt = data["SDT"]?.ToString() ?? string.Empty;
        var diaChi = data["DIACHI"]?.ToString() ?? string.Empty;
        var email = data["EMAIL"]?.ToString() ?? string.Empty;

        row.Controls.Add(MkCellLabel(ma, 0, 90));
        row.Controls.Add(MkCellLabel(ten, 100, 300));
        row.Controls.Add(MkCellLabel(sdt, 420, 120));
        row.Controls.Add(MkCellLabel(email, 550, 200));
        row.Controls.Add(MkCellLabel(diaChi, 760, Math.Max(200, row.Width - 760)));

        void openEdit(object? _, EventArgs __) => ShowEditDialog(data);
        row.Click += openEdit;
        foreach (Control c in row.Controls) c.Click += openEdit;

        row.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(243, 244, 246);
        row.MouseLeave += (s, e) => row.BackColor = Color.White;

        return row;
    }

    private Label MkCellLabel(string text, int x, int w)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, 12),
            Width = w,
            Height = 20,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            ForeColor = Color.Black
        };
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void UpdateRowWidths()
    {
        if (panelList.Controls.Count == 0)
        {
            return;
        }

        var rowWidth = panelList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
        foreach (Control control in panelList.Controls)
        {
            if (control is Panel row)
            {
                row.Width = rowWidth;
                foreach (Control child in row.Controls)
                {
                    if (child is Label label && label.Text == "ĐỊA CHỈ")
                    {
                        label.Width = Math.Max(200, row.Width - 760);
                    }
                    else if (child is Label valueLabel && valueLabel.Location.X == 760)
                    {
                        valueLabel.Width = Math.Max(200, row.Width - 760);
                    }
                }
            }
        }

        btnAdd.Left = panelList.Parent?.Width - btnAdd.Width - 24 ?? btnAdd.Left;
        txtSearch.Width = Math.Max(200, btnAdd.Left - txtSearch.Left - 16);
    }

    private void ShowEditDialog(DataRow? data)
    {
        var ma = data?["MANCC"]?.ToString();
        var ten = data?["TENNCC"]?.ToString();
        var sdt = data?["SDT"]?.ToString();
        var diaChi = data?["DIACHI"]?.ToString();
        var email = data?["EMAIL"]?.ToString();

        using var dlg = new FrmNhaCungCapEdit(ma, ten, sdt, diaChi, email);
        dlg.StartPosition = FormStartPosition.CenterParent;
        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (string.IsNullOrWhiteSpace(ma))
        {
            var (ok, msg, _) = _service.AddNcc(dlg.TenNcc ?? string.Empty, dlg.Sdt, dlg.DiaChi, dlg.Email);
            MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
        else
        {
            var (ok, msg) = _service.UpdateNcc(ma, dlg.TenNcc ?? string.Empty, dlg.Sdt, dlg.DiaChi, dlg.Email);
            MessageBox.Show(msg, ok ? "Thành công" : "Lỗi", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        LoadData();
    }
}
