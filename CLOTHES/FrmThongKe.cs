using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QLBH.BLL;

namespace CLOTHES;

public class FrmThongKe : Form
{
    private readonly HoaDonService _service = new();

    private DateTimePicker dtFrom = null!;
    private DateTimePicker dtTo = null!;
    private Button btnLoad = null!;
    private Button btnExport = null!;

    private Label lblDoanhThu = null!;
    private Label lblSoDon = null!;
    private Label lblAvg = null!;
    private Label lblLoiNhuan = null!;

    private DataGridView dgvTheoNgay = null!;
    private Panel chartHost = null!;
    private DataGridView dgvTopSl = null!;
    private DataGridView dgvTopDt = null!;

    private DataTable? _dtDay;

    private bool _isInitializing;
    private bool _pendingLoad;
    private readonly System.Windows.Forms.Timer _reloadTimer = new();

    public FrmThongKe()
    {
        Text = "Thống kê doanh thu";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);
        WindowState = FormWindowState.Maximized;

        _reloadTimer.Interval = 250;
        _reloadTimer.Tick += (_, __) =>
        {
            _reloadTimer.Stop();
            if (!_isInitializing)
                SafeCall(LoadData);
        };

        _isInitializing = true;
        BuildUi();

        dtFrom.Value = DateTime.Today;
        dtTo.Value = DateTime.Today;

        _isInitializing = false;
        _pendingLoad = true;

        Shown += (_, __) =>
        {
            if (_pendingLoad && !_isInitializing)
            {
                _pendingLoad = false;
                SafeCall(LoadData);
            }
        };
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Filter
        var filter = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        dtFrom = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
        dtTo = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
        btnLoad = new Button { Text = "Xem", Width = 80, Height = 30 };
        btnExport = new Button { Text = "Xuất Excel", Width = 100, Height = 30 };

        btnLoad.Click += (_, __) => SafeCall(LoadData);
        btnExport.Click += (_, __) => SafeCall(ExportThongKe);

        // Avoid reloading while the user is interacting with the calendar drop-down.
        // We debounce reload to prevent UI interruptions that can make the picker feel "not selectable".
        dtFrom.CloseUp += (_, __) => RequestReload();
        dtTo.CloseUp += (_, __) => RequestReload();
        dtFrom.ValueChanged += (_, __) => RequestReload();
        dtTo.ValueChanged += (_, __) => RequestReload();

        filter.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtFrom);
        filter.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(dtTo);
        filter.Controls.Add(btnLoad);
        filter.Controls.Add(btnExport);

        // KPIs
        var kpi = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        lblDoanhThu = BuildKpiLabel("Doanh thu", "0");
        lblSoDon = BuildKpiLabel("Số đơn", "0");
        lblAvg = BuildKpiLabel("Trung bình/đơn", "0");
        lblLoiNhuan = BuildKpiLabel("Lợi nhuận", "0");

        kpi.Controls.Add(WrapKpi(lblDoanhThu), 0, 0);
        kpi.Controls.Add(WrapKpi(lblSoDon), 1, 0);
        kpi.Controls.Add(WrapKpi(lblAvg), 2, 0);
        kpi.Controls.Add(WrapKpi(lblLoiNhuan), 3, 0);

        // Grids
        var grids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grids.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grids.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        dgvTheoNgay = BuildGrid();
        chartHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        chartHost.Paint += ChartHost_Paint;
        dgvTopSl = BuildGrid();
        dgvTopDt = BuildGrid();

        grids.Controls.Add(WrapGroup("Biểu đồ doanh thu theo ngày", chartHost), 0, 0);
        grids.Controls.Add(WrapGroup("Bảng doanh thu theo ngày", dgvTheoNgay), 1, 0);

        grids.Controls.Add(WrapGroup("Top sản phẩm theo số lượng", dgvTopSl), 0, 1);
        grids.Controls.Add(WrapGroup("Top sản phẩm theo doanh thu", dgvTopDt), 1, 1);

        root.Controls.Add(filter, 0, 0);
        root.Controls.Add(kpi, 0, 1);
        root.Controls.Add(grids, 0, 2);

        Controls.Add(root);
    }

    private static DataGridView BuildGrid()
        => new()
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

    // Charting via System.Windows.Forms.DataVisualization caused a native crash on some machines.
    // Keep the statistics screen stable by using tables only.

    private static Label BuildKpiLabel(string title, string value)
    {
        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Padding = new Padding(12),
            Text = $"{title}: {value}"
        };
        return lbl;
    }

    private static Control WrapKpi(Control c)
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(0),
            Margin = new Padding(6),
            Controls = { c }
        };
    }

    private static Control WrapGroup(string title, Control body)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(6) };
        var lbl = new Label { Text = title, Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        panel.Controls.Add(body);
        panel.Controls.Add(lbl);
        body.Dock = DockStyle.Fill;
        return panel;
    }

    private void LoadData()
    {
        var from = dtFrom.Value.Date;
        var to = dtTo.Value.Date;

        if (from > to)
        {
            // Auto-fix invalid range
            dtTo.Value = from;
            to = from;
        }

        // Summary
        var dtSum = _service.GetThongKeTongQuan(from, to);
        decimal doanhThu = 0;
        decimal giaVon = 0;
        decimal loiNhuan = 0;
        int soDon = 0;
        if (dtSum.Rows.Count > 0)
        {
            var r = dtSum.Rows[0];
            soDon = r["SO_DON"] == DBNull.Value ? 0 : Convert.ToInt32(r["SO_DON"]);
            doanhThu = r["DOANH_THU"] == DBNull.Value ? 0 : Convert.ToDecimal(r["DOANH_THU"]);
            giaVon = r["GIA_VON"] == DBNull.Value ? 0 : Convert.ToDecimal(r["GIA_VON"]);
            loiNhuan = r["LOI_NHUAN"] == DBNull.Value ? 0 : Convert.ToDecimal(r["LOI_NHUAN"]);
        }

        decimal avg = soDon == 0 ? 0 : doanhThu / soDon;

        lblDoanhThu.Text = $"Doanh thu: {doanhThu:N0} VNĐ";
        lblSoDon.Text = $"Số đơn: {soDon:N0}";
        lblAvg.Text = $"Trung bình/đơn: {avg:N0} VNĐ";
        lblLoiNhuan.Text = $"Lợi nhuận: {loiNhuan:N0} VNĐ";

        // By day
        _dtDay = _service.GetThongKeTheoNgay(from, to);
        dgvTheoNgay.DataSource = _dtDay;
        if (dgvTheoNgay.Columns.Count > 0)
        {
            dgvTheoNgay.Columns["NGAY"].HeaderText = "Ngày";
            dgvTheoNgay.Columns["SO_DON"].HeaderText = "Số đơn";
            dgvTheoNgay.Columns["DOANH_THU"].HeaderText = "Doanh thu";
            dgvTheoNgay.Columns["GIA_VON"].HeaderText = "Giá vốn";
            dgvTheoNgay.Columns["LOI_NHUAN"].HeaderText = "Lợi nhuận";

            dgvTheoNgay.Columns["NGAY"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvTheoNgay.Columns["DOANH_THU"].DefaultCellStyle.Format = "N0";
            dgvTheoNgay.Columns["GIA_VON"].DefaultCellStyle.Format = "N0";
            dgvTheoNgay.Columns["LOI_NHUAN"].DefaultCellStyle.Format = "N0";
        }

        chartHost.Invalidate();

        // Top by qty
        var topSl = _service.GetTopSanPhamBySoLuong(from, to, 10);
        dgvTopSl.DataSource = topSl;
        if (dgvTopSl.Columns.Count > 0)
        {
            dgvTopSl.Columns["TENSP"].HeaderText = "Sản phẩm";
            dgvTopSl.Columns["SO_LUONG"].HeaderText = "Số lượng";
            dgvTopSl.Columns["DOANH_THU"].HeaderText = "Doanh thu";
            dgvTopSl.Columns["DOANH_THU"].DefaultCellStyle.Format = "N0";
        }

        // Top by revenue
        var topDt = _service.GetTopSanPhamByDoanhThu(from, to, 10);
        dgvTopDt.DataSource = topDt;
        if (dgvTopDt.Columns.Count > 0)
        {
            dgvTopDt.Columns["TENSP"].HeaderText = "Sản phẩm";
            dgvTopDt.Columns["SO_LUONG"].HeaderText = "Số lượng";
            dgvTopDt.Columns["DOANH_THU"].HeaderText = "Doanh thu";
            dgvTopDt.Columns["DOANH_THU"].DefaultCellStyle.Format = "N0";
        }

        return;
    }

    private void ChartHost_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = chartHost.ClientRectangle;
        if (rect.Width <= 10 || rect.Height <= 10)
            return;

        g.Clear(Color.White);

        using var fontTitle = new Font("Segoe UI", 10f, FontStyle.Bold);
        using var font = new Font("Segoe UI", 9f);
        using var penAxis = new Pen(Color.FromArgb(200, 200, 200));
        using var brushBar = new SolidBrush(Color.FromArgb(79, 70, 229));
        using var brushText = new SolidBrush(Color.FromArgb(51, 65, 85));

        var title = "Doanh thu theo ngày";
        g.DrawString(title, fontTitle, brushText, rect.Left + 10, rect.Top + 10);

        var plot = new Rectangle(rect.Left + 40, rect.Top + 40, rect.Width - 60, rect.Height - 70);
        if (plot.Width <= 10 || plot.Height <= 10)
            return;

        // Axes
        g.DrawLine(penAxis, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
        g.DrawLine(penAxis, plot.Left, plot.Top, plot.Left, plot.Bottom);

        var dt = _dtDay;
        if (dt == null || dt.Rows.Count == 0)
        {
            g.DrawString("Không có dữ liệu", font, Brushes.Gray, plot.Left + 10, plot.Top + 10);
            return;
        }

        var points = dt.Rows.Cast<DataRow>()
            .Select(r =>
            {
                var ngay = r["NGAY"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["NGAY"]);
                var val = r["DOANH_THU"] == DBNull.Value ? 0m : Convert.ToDecimal(r["DOANH_THU"]);
                return (ngay, val);
            })
            .OrderBy(x => x.ngay)
            .ToList();

        decimal max = points.Max(x => x.val);
        if (max <= 0) max = 1;

        int n = points.Count;
        int gap = 6;
        int barWidth = Math.Max(6, (plot.Width - (gap * (n + 1))) / n);
        if (barWidth > 40) barWidth = 40;

        int totalBarsWidth = (barWidth * n) + (gap * (n + 1));
        int startX = plot.Left + Math.Max(0, (plot.Width - totalBarsWidth) / 2);

        // y labels (0 and max)
        g.DrawString("0", font, Brushes.Gray, rect.Left + 6, plot.Bottom - 10);
        g.DrawString($"{max:N0}", font, Brushes.Gray, rect.Left + 6, plot.Top - 6);

        for (int i = 0; i < n; i++)
        {
            var (ngay, val) = points[i];
            double ratio = (double)(val / max);
            int h = (int)Math.Round(ratio * plot.Height);
            if (h < 0) h = 0;

            int x = startX + gap + i * (barWidth + gap);
            int y = plot.Bottom - h;

            var bar = new Rectangle(x, y, barWidth, h);
            g.FillRectangle(brushBar, bar);

            // x label (dd/MM)
            var label = ngay == DateTime.MinValue ? "" : ngay.ToString("dd/MM");
            if (!string.IsNullOrEmpty(label))
            {
                var size = g.MeasureString(label, font);
                g.DrawString(label, font, Brushes.Gray, x + (barWidth - size.Width) / 2, plot.Bottom + 4);
            }
        }
    }

    private void SafeCall(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi thống kê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RequestReload()
    {
        if (_isInitializing)
            return;

        _reloadTimer.Stop();
        _reloadTimer.Start();
    }

    private void ExportThongKe()
    {
        var dt = _dtDay;
        if (dt == null || dt.Rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Excel (*.csv)|*.csv",
            FileName = $"ThongKe_{dtFrom.Value:yyyyMMdd}_{dtTo.Value:yyyyMMdd}.csv",
            Title = "Xuất thống kê"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        using var writer = new StreamWriter(dialog.FileName, false, new System.Text.UTF8Encoding(true));

        var headers = dgvTheoNgay.Columns
            .Cast<DataGridViewColumn>()
            .Select(c => EscapeCsv(c.HeaderText))
            .ToArray();
        writer.WriteLine(string.Join(",", headers));

        foreach (DataRow row in dt.Rows)
        {
            var values = dgvTheoNgay.Columns
                .Cast<DataGridViewColumn>()
                .Select(c =>
                {
                    var key = string.IsNullOrWhiteSpace(c.DataPropertyName) ? c.Name : c.DataPropertyName;
                    return EscapeCsv(dt.Columns.Contains(key) ? row[key] : null);
                })
                .ToArray();
            writer.WriteLine(string.Join(",", values));
        }

        MessageBox.Show("Đã xuất thống kê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string EscapeCsv(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "";

        var text = value switch
        {
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            _ => Convert.ToString(value)
        } ?? string.Empty;

        text = text.Replace("\r", " ").Replace("\n", " ");
        return text.Contains(',') || text.Contains('"')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}
