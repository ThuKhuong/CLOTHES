using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL;

public class HoaDonDal
{
    private static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    // Must match DB CHECK constraint CK_HD_TRANGTHAI
    // definition: ([TRANGTHAI]=N'HUY' OR [TRANGTHAI]=N'DATHANHTOAN')
    private const string TrangThaiDaThanhToan = "DATHANHTOAN";
    private const string TrangThaiHuy = "HUY";
    private const string TrangThaiTraHang = "TRAHANG";

    private static bool IsPaidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        status = status.Trim();
        return status.Equals(TrangThaiDaThanhToan, StringComparison.OrdinalIgnoreCase);
    }

    public DataTable GetAllNhanVien()
    {
        const string sql = "SELECT MAND, TENND FROM NGUOIDUNG WHERE TRANGTHAI = 1 ORDER BY TENND";
        return Db.Query(sql);
    }

    public (bool success, string message) UpdateTrangThaiVaHoanTonKho(int soHd, string trangThai)
    {
        trangThai = (trangThai ?? string.Empty).Trim().ToUpperInvariant();
        if (trangThai is not ("HUY" or "TRAHANG"))
            return (false, "Trạng thái không hợp lệ.");

        var trangThaiDb = trangThai == "TRAHANG" ? TrangThaiTraHang : TrangThaiHuy;

        using var conn = Db.OpenConnection();
        using var tran = conn.BeginTransaction();

        try
        {
            // Only allow change from paid to cancelled/returned
            var current = GetTrangThai(soHd, tran);
            if (!IsPaidStatus(current))
                return (false, $"Không thể cập nhật. Trạng thái hiện tại: {current}.");

            // Load details and restore inventory
            var items = GetCthdItems(soHd, tran);
            foreach (var it in items)
            {
                IncreaseTonKho(it.maCt, it.sl, tran);
            }

            SetTrangThai(soHd, trangThaiDb, tran);
            tran.Commit();

            return (true, trangThai == "HUY" ? "Đã hủy hóa đơn." : "Đã trả hàng.");
        }
        catch (Exception ex)
        {
            try { tran.Rollback(); } catch { /* ignore */ }
            return (false, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
        }
    }

    public DataTable GetHoaDonList(DateTime? from, DateTime? to, string? keyword)
    {
        keyword ??= string.Empty;
        keyword = keyword.Trim();

        const string sql = @"
SELECT
    hd.SOHD,
    hd.NGHD,
    hd.MAND,
    nd.TENND,
    hd.MAKH,
    kh.HOTEN,
    kh.SDT,
    hd.TONGTIEN1,
    hd.GIAMGIA,
    hd.TONGTIEN2,
    hd.HINHTHUCTT,
    hd.TRANGTHAI,
    hd.GHICHU
FROM HOADON hd
LEFT JOIN NGUOIDUNG nd ON hd.MAND = nd.MAND
LEFT JOIN KHACHHANG kh ON hd.MAKH = kh.MAKH
WHERE
    (@from IS NULL OR hd.NGHD >= @from)
    AND (@to IS NULL OR hd.NGHD < DATEADD(day, 1, @to))
    AND (
        @kw = ''
        OR CAST(hd.SOHD AS nvarchar(50)) LIKE @kwlike
        OR kh.HOTEN LIKE @kwlike
        OR kh.SDT LIKE @kwlike
    )
ORDER BY hd.SOHD DESC";

        return Db.Query(
            sql,
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null),
            P("@kw", keyword),
            P("@kwlike", $"%{keyword}%"));
    }

    public DataTable GetHoaDonDetail(int soHd)
    {
        const string sql = @"
SELECT
    ct.SOHD,
    ct.MACT,
    sp.TENSP,
    spct.SIZE,
    spct.MAU,
    ct.SL,
    ct.DONGIA,
    ct.GIAMGIA,
    (ct.SL * ct.DONGIA - ct.GIAMGIA) AS THANHTIEN
FROM CTHD ct
LEFT JOIN SANPHAM_CHITIET spct ON ct.MACT = spct.MACT
LEFT JOIN SANPHAM sp ON spct.MASP = sp.MASP
WHERE ct.SOHD = @sohd";

        return Db.Query(sql, P("@sohd", soHd));
    }

    public DataTable GetThongKeTongQuan(DateTime? from, DateTime? to)
    {
        const string sql = @"
WITH HoaDonFiltered AS (
    SELECT hd.SOHD, hd.TONGTIEN2
    FROM HOADON hd
    WHERE
        hd.TRANGTHAI = N'DATHANHTOAN'
        AND (@from IS NULL OR hd.NGHD >= @from)
        AND (@to IS NULL OR hd.NGHD < DATEADD(day, 1, @to))
),
TongHop AS (
    SELECT
        (SELECT COUNT(*) FROM HoaDonFiltered) AS SO_DON,
        (SELECT ISNULL(SUM(TONGTIEN2), 0) FROM HoaDonFiltered) AS DOANH_THU,
        (
            SELECT ISNULL(SUM(ct.SL * ISNULL(pn.GIA_NHAP, 0)), 0)
            FROM CTHD ct
            JOIN HoaDonFiltered hd ON hd.SOHD = ct.SOHD
            OUTER APPLY (
                SELECT TOP 1 ctpn.DONGIA_NHAP AS GIA_NHAP
                FROM CTPN ctpn
                JOIN PHIEUNHAP pn ON pn.MAPN = ctpn.MAPN
                WHERE ctpn.MACT = ct.MACT
                ORDER BY pn.NGAYNHAP DESC, pn.MAPN DESC
            ) pn
        ) AS GIA_VON
)
SELECT
    SO_DON,
    DOANH_THU,
    GIA_VON,
    (DOANH_THU - GIA_VON) AS LOI_NHUAN
FROM TongHop";

        return Db.Query(
            sql,
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null));
    }

    public DataTable GetThongKeTheoNgay(DateTime? from, DateTime? to)
    {
        const string sql = @"
WITH HoaDonFiltered AS (
    SELECT hd.SOHD, CAST(hd.NGHD AS date) AS NGAY, hd.TONGTIEN2
    FROM HOADON hd
    WHERE
        hd.TRANGTHAI = N'DATHANHTOAN'
        AND (@from IS NULL OR hd.NGHD >= @from)
        AND (@to IS NULL OR hd.NGHD < DATEADD(day, 1, @to))
),
DoanhThuByDay AS (
    SELECT NGAY, COUNT(*) AS SO_DON, ISNULL(SUM(TONGTIEN2), 0) AS DOANH_THU
    FROM HoaDonFiltered
    GROUP BY NGAY
),
GiaVonByDay AS (
    SELECT
        h.NGAY,
        ISNULL(SUM(ct.SL * ISNULL(pn.GIA_NHAP, 0)), 0) AS GIA_VON
    FROM HoaDonFiltered h
    JOIN CTHD ct ON h.SOHD = ct.SOHD
    OUTER APPLY (
        SELECT TOP 1 ctpn.DONGIA_NHAP AS GIA_NHAP
        FROM CTPN ctpn
        JOIN PHIEUNHAP pn ON pn.MAPN = ctpn.MAPN
        WHERE ctpn.MACT = ct.MACT
        ORDER BY pn.NGAYNHAP DESC, pn.MAPN DESC
    ) pn
    GROUP BY h.NGAY
)
SELECT
    d.NGAY,
    d.SO_DON,
    d.DOANH_THU,
    ISNULL(g.GIA_VON, 0) AS GIA_VON,
    (d.DOANH_THU - ISNULL(g.GIA_VON, 0)) AS LOI_NHUAN
FROM DoanhThuByDay d
LEFT JOIN GiaVonByDay g ON d.NGAY = g.NGAY
ORDER BY d.NGAY";

        return Db.Query(
            sql,
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null));
    }

    public DataTable GetTopSanPhamBySoLuong(DateTime? from, DateTime? to, int top)
    {
        const string sql = @"
SELECT TOP (@top)
    sp.TENSP,
    ISNULL(SUM(ct.SL), 0) AS SO_LUONG,
    ISNULL(SUM(ct.SL * ct.DONGIA - ct.GIAMGIA), 0) AS DOANH_THU
FROM HOADON hd
JOIN CTHD ct ON hd.SOHD = ct.SOHD
LEFT JOIN SANPHAM_CHITIET spct ON ct.MACT = spct.MACT
LEFT JOIN SANPHAM sp ON spct.MASP = sp.MASP
WHERE
    hd.TRANGTHAI = N'DATHANHTOAN'
    AND (@from IS NULL OR hd.NGHD >= @from)
    AND (@to IS NULL OR hd.NGHD < DATEADD(day, 1, @to))
GROUP BY sp.TENSP
ORDER BY SO_LUONG DESC";

        return Db.Query(
            sql,
            P("@top", top <= 0 ? 10 : top),
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null));
    }

    public DataTable GetTopSanPhamByDoanhThu(DateTime? from, DateTime? to, int top)
    {
        const string sql = @"
SELECT TOP (@top)
    sp.TENSP,
    ISNULL(SUM(ct.SL), 0) AS SO_LUONG,
    ISNULL(SUM(ct.SL * ct.DONGIA - ct.GIAMGIA), 0) AS DOANH_THU
FROM HOADON hd
JOIN CTHD ct ON hd.SOHD = ct.SOHD
LEFT JOIN SANPHAM_CHITIET spct ON ct.MACT = spct.MACT
LEFT JOIN SANPHAM sp ON spct.MASP = sp.MASP
WHERE
    hd.TRANGTHAI = N'DATHANHTOAN'
    AND (@from IS NULL OR hd.NGHD >= @from)
    AND (@to IS NULL OR hd.NGHD < DATEADD(day, 1, @to))
GROUP BY sp.TENSP
ORDER BY DOANH_THU DESC";

        return Db.Query(
            sql,
            P("@top", top <= 0 ? 10 : top),
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null));
    }

    public DataTable SearchSanPhamChiTiet(string keyword)
    {
        keyword ??= string.Empty;
        keyword = keyword.Trim();

        const string sql = @"
SELECT TOP 50
    ct.MACT, ct.MASP, sp.TENSP, ct.SIZE, ct.MAU, ct.TONKHO, ct.GIABAN, ct.BARCODE
FROM SANPHAM_CHITIET ct
JOIN SANPHAM sp ON ct.MASP = sp.MASP
WHERE sp.TRANGTHAI = 1
  AND ct.TRANGTHAI = 1
  AND (ct.MACT LIKE @kw OR sp.TENSP LIKE @kw OR ct.BARCODE LIKE @kw)
ORDER BY sp.TENSP";

        return Db.Query(sql, P("@kw", $"%{keyword}%"));
    }

    public string? FindMaCtByBarcode(string barcode)
    {
        const string sql = @"
SELECT TOP 1 ct.MACT
FROM SANPHAM_CHITIET ct
JOIN SANPHAM sp ON ct.MASP = sp.MASP
WHERE ct.BARCODE = @bc
  AND ct.TRANGTHAI = 1
  AND sp.TRANGTHAI = 1";
        var v = Db.Scalar(sql, P("@bc", barcode));
        return v == null || v == DBNull.Value ? null : v.ToString();
    }

    public (bool success, string message, int soHd) SaveHoaDon(
        string maNd,
        string? maKh,
        DateTime ngay,
        string? maKm,
        string? ghiChu,
        string? hinhThucTt,
        int diemDoi,
        List<(string maCt, int sl, decimal donGia)> items)
    {
        if (items.Count == 0)
            return (false, "Chưa có sản phẩm trong giỏ.", 0);

        using var conn = Db.OpenConnection();
        using var tran = conn.BeginTransaction();

        try
        {
            decimal tong1 = items.Sum(x => x.sl * x.donGia);
            var (maKmOk, phanTram) = ValidateKhuyenMai(maKm, ngay, tran);
            decimal giamKm = maKmOk == null ? 0 : Math.Round(tong1 * phanTram / 100m, 0);
            if (giamKm < 0) giamKm = 0;
            if (giamKm > tong1) giamKm = tong1;

            int diemHienCo = 0;
            int diemSuDung = 0;
            decimal giamDiem = 0;
            if (!string.IsNullOrWhiteSpace(maKh) && diemDoi > 0)
            {
                diemHienCo = GetDiemKhachHang(maKh!, tran);
                diemSuDung = Math.Min(diemDoi, diemHienCo);
                giamDiem = diemSuDung * 1000m;
                var maxDiemDiscount = Math.Round(tong1 * 0.2m, 0);
                if (giamDiem > maxDiemDiscount)
                {
                    giamDiem = maxDiemDiscount;
                    diemSuDung = (int)Math.Floor(giamDiem / 1000m);
                }
            }

            decimal giam = giamKm + giamDiem;
            if (giam < 0) giam = 0;
            if (giam > tong1) giam = tong1;
            decimal tong2 = tong1 - giam;

            int soHd = InsertHeader(maNd, maKh, ngay, maKmOk, tong1, giam, tong2, hinhThucTt, ghiChu, tran);

            // Distribute header discount down to line items so invoice details match header totals.
            // We allocate proportional to line amount and ensure the sum equals 'giam' using remainder.
            var lineAmounts = items.Select(x => x.sl * x.donGia).ToList();
            decimal allocated = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                EnsureTonKhoEnough(it.maCt, it.sl, tran);

                decimal lineTotal = lineAmounts[i];
                decimal lineGiam;

                if (giam <= 0 || tong1 <= 0)
                {
                    lineGiam = 0;
                }
                else if (i == items.Count - 1)
                {
                    lineGiam = giam - allocated;
                }
                else
                {
                    lineGiam = Math.Round(giam * lineTotal / tong1, 0);
                    allocated += lineGiam;
                }

                if (lineGiam < 0) lineGiam = 0;
                if (lineGiam > lineTotal) lineGiam = lineTotal;

                InsertDetail(soHd, it.maCt, it.sl, it.donGia, lineGiam, tran);
                DecreaseTonKho(it.maCt, it.sl, tran);
            }

            if (!string.IsNullOrWhiteSpace(maKh))
            {
                int diemCong = (int)Math.Floor(tong2 / 20000m);
                int diemMoi = Math.Max(0, diemHienCo - diemSuDung) + diemCong;
                UpdateDiemKhachHang(maKh!, diemMoi, tran);
            }

            tran.Commit();
            return (true, "Thanh toán thành công.", soHd);
        }
        catch (Exception ex)
        {
            try { tran.Rollback(); } catch { /* ignore */ }
            return (false, $"Lỗi khi thanh toán: {ex.Message}", 0);
        }

        return (false, "Lỗi khi thanh toán.", 0);
    }

    private static int GetDiemKhachHang(string maKh, SqlTransaction tran)
    {
        const string sql = "SELECT DIEM FROM KHACHHANG WHERE MAKH = @makh";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@makh", maKh) });
        var v = cmd.ExecuteScalar();
        return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
    }

    private static void UpdateDiemKhachHang(string maKh, int diem, SqlTransaction tran)
    {
        const string sql = "UPDATE KHACHHANG SET DIEM = @diem WHERE MAKH = @makh";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@diem", diem), P("@makh", maKh) });
        cmd.ExecuteNonQuery();
    }

    private static int InsertHeader(
        string maNd,
        string? maKh,
        DateTime ngay,
        string? maKm,
        decimal tong1,
        decimal giam,
        decimal tong2,
        string? hinhThucTt,
        string? ghiChu,
        SqlTransaction tran)
    {
        const string sql = @"
INSERT INTO HOADON (MAND, MAKH, NGHD, MAKM, TONGTIEN1, GIAMGIA, TONGTIEN2, HINHTHUCTT, TRANGTHAI, GHICHU)
VALUES (@mand, @makh, @ng, @makm, @t1, @giam, @t2, @ht, @tt, @gc);
SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[]
        {
            P("@mand", maNd),
            P("@makh", string.IsNullOrWhiteSpace(maKh) ? DBNull.Value : maKh),
            P("@ng", ngay),
            P("@makm", string.IsNullOrWhiteSpace(maKm) ? DBNull.Value : maKm),
            P("@t1", tong1),
            P("@giam", giam),
            P("@t2", tong2),
            P("@ht", string.IsNullOrWhiteSpace(hinhThucTt) ? DBNull.Value : hinhThucTt),
            P("@tt", TrangThaiDaThanhToan),
            P("@gc", string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu)
        });

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static (string? maKm, int phanTram) ValidateKhuyenMai(string? maKm, DateTime ngay, SqlTransaction tran)
    {
        if (string.IsNullOrWhiteSpace(maKm))
            return (null, 0);

        const string sql = @"
SELECT TOP 1 MAKM, PHANTRAM_GIAM
FROM KHUYENMAI
WHERE TRANGTHAI = 1
  AND MAKM = @makm
  AND @ngay >= NGAYBD
  AND @ngay <= NGAYKT";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@makm", maKm.Trim()), P("@ngay", ngay) });
        using var rdr = cmd.ExecuteReader();

        if (!rdr.Read())
            return (null, 0);

        var id = rdr["MAKM"] == DBNull.Value ? null : rdr["MAKM"].ToString();
        var percent = rdr["PHANTRAM_GIAM"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PHANTRAM_GIAM"]);
        if (percent < 0) percent = 0;
        if (percent > 100) percent = 100;
        return (id, percent);
    }

    private static void InsertDetail(int soHd, string maCt, int sl, decimal donGia, decimal giamGia, SqlTransaction tran)
    {
        const string sql = @"
INSERT INTO CTHD (SOHD, MACT, SL, DONGIA, GIAMGIA)
VALUES (@sohd, @mact, @sl, @dg, @gg)";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[]
        {
            P("@sohd", soHd),
            P("@mact", maCt),
            P("@sl", sl),
            P("@dg", donGia),
            P("@gg", giamGia)
        });
        cmd.ExecuteNonQuery();
    }

    private static void EnsureTonKhoEnough(string maCt, int sl, SqlTransaction tran)
    {
        const string sql = "SELECT TONKHO FROM SANPHAM_CHITIET WHERE MACT = @mact";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@mact", maCt) });
        var v = cmd.ExecuteScalar();
        int ton = v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        if (ton < sl)
            throw new InvalidOperationException($"Tồn kho không đủ cho {maCt}. Tồn: {ton}, cần: {sl}.");
    }

    private static void DecreaseTonKho(string maCt, int sl, SqlTransaction tran)
    {
        const string sql = @"
UPDATE SANPHAM_CHITIET
SET TONKHO = TONKHO - @sl
WHERE MACT = @mact";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@sl", sl), P("@mact", maCt) });
        cmd.ExecuteNonQuery();
    }

    private static void IncreaseTonKho(string maCt, int sl, SqlTransaction tran)
    {
        const string sql = @"
UPDATE SANPHAM_CHITIET
SET TONKHO = TONKHO + @sl
WHERE MACT = @mact";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@sl", sl), P("@mact", maCt) });
        cmd.ExecuteNonQuery();
    }

    private static string GetTrangThai(int soHd, SqlTransaction tran)
    {
        const string sql = "SELECT TRANGTHAI FROM HOADON WHERE SOHD = @sohd";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@sohd", soHd) });
        var v = cmd.ExecuteScalar();
        return v == null || v == DBNull.Value ? string.Empty : v.ToString()!;
    }

    private static void SetTrangThai(int soHd, string trangThai, SqlTransaction tran)
    {
        const string sql = "UPDATE HOADON SET TRANGTHAI = @tt WHERE SOHD = @sohd";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@tt", trangThai), P("@sohd", soHd) });
        cmd.ExecuteNonQuery();
    }

    private static List<(string maCt, int sl)> GetCthdItems(int soHd, SqlTransaction tran)
    {
        const string sql = "SELECT MACT, SL FROM CTHD WHERE SOHD = @sohd";
        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@sohd", soHd) });
        using var rd = cmd.ExecuteReader();

        var list = new List<(string maCt, int sl)>();
        while (rd.Read())
        {
            var maCt = rd["MACT"]?.ToString() ?? string.Empty;
            var sl = rd["SL"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SL"]);
            if (!string.IsNullOrWhiteSpace(maCt) && sl > 0)
                list.Add((maCt, sl));
        }

        return list;
    }
}
