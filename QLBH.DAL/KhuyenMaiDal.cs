using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL;

public class KhuyenMaiDal
{
    private static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public DataTable GetActive(DateTime ngay)
    {
        const string sql = @"
SELECT MAKM, TENKM, PHANTRAM_GIAM, NGAYBD, NGAYKT
FROM KHUYENMAI
WHERE TRANGTHAI = 1
  AND @ngay >= NGAYBD
  AND @ngay <= NGAYKT
ORDER BY NGAYBD DESC";

        return Db.Query(sql, P("@ngay", ngay));
    }

    public DataTable GetAll()
    {
        const string sql = @"
SELECT MAKM, TENKM, PHANTRAM_GIAM, NGAYBD, NGAYKT, TRANGTHAI
FROM KHUYENMAI
ORDER BY NGAYBD DESC";
        return Db.Query(sql);
    }

    public DataRow? GetById(string maKm)
    {
        const string sql = @"
SELECT TOP 1 MAKM, TENKM, PHANTRAM_GIAM, NGAYBD, NGAYKT, TRANGTHAI
FROM KHUYENMAI
WHERE MAKM = @makm";
        var dt = Db.Query(sql, P("@makm", maKm));
        return dt.Rows.Count == 0 ? null : dt.Rows[0];
    }

    public string GetNextMaKm()
    {
        const string sql = @"
SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(MAKM, 3, 10))), 0)
FROM KHUYENMAI
WHERE MAKM LIKE 'KM%'";

        var v = Db.Scalar(sql);
        int max = v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        return $"KM{(max + 1):000}";
    }

    public (bool success, string message) Upsert(string maKm, string tenKm, int phanTram, DateTime ngayBd, DateTime ngayKt, bool trangThai)
    {
        if (string.IsNullOrWhiteSpace(maKm))
            return (false, "Mã KM không hợp lệ.");
        if (string.IsNullOrWhiteSpace(tenKm))
            return (false, "Tên KM không hợp lệ.");
        if (phanTram is < 1 or > 100)
            return (false, "% giảm phải từ 1 đến 100.");
        if (ngayKt < ngayBd)
            return (false, "Ngày kết thúc phải >= ngày bắt đầu.");

        const string sql = @"
IF EXISTS (SELECT 1 FROM KHUYENMAI WHERE MAKM = @makm)
BEGIN
    UPDATE KHUYENMAI
    SET TENKM = @ten,
        PHANTRAM_GIAM = @pt,
        NGAYBD = @bd,
        NGAYKT = @kt,
        TRANGTHAI = @tt
    WHERE MAKM = @makm;
END
ELSE
BEGIN
    INSERT INTO KHUYENMAI (MAKM, TENKM, PHANTRAM_GIAM, NGAYBD, NGAYKT, TRANGTHAI)
    VALUES (@makm, @ten, @pt, @bd, @kt, @tt);
END";

        try
        {
            Db.Execute(sql,
                P("@makm", maKm.Trim()),
                P("@ten", tenKm.Trim()),
                P("@pt", phanTram),
                P("@bd", ngayBd),
                P("@kt", ngayKt),
                P("@tt", trangThai));

            return (true, "Lưu khuyến mãi thành công.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi lưu khuyến mãi: {ex.Message}");
        }
    }

    public (bool success, string message) Delete(string maKm)
    {
        const string sqlCheck = "SELECT COUNT(1) FROM HOADON WHERE MAKM = @makm";
        try
        {
            var used = Db.Scalar(sqlCheck, P("@makm", maKm));
            int cnt = used == null || used == DBNull.Value ? 0 : Convert.ToInt32(used);
            if (cnt > 0)
                return (false, "Khuyến mãi đã được dùng trong hóa đơn, không thể xóa. Hãy dùng Bật/Tắt để ngưng áp dụng.");
        }
        catch
        {
            // If check fails, fall back to delete attempt.
        }

        const string sql = "DELETE FROM KHUYENMAI WHERE MAKM = @makm";
        try
        {
            Db.Execute(sql, P("@makm", maKm));
            return (true, "Đã xóa khuyến mãi.");
        }
        catch (Exception ex)
        {
            return (false, $"Không thể xóa khuyến mãi: {ex.Message}");
        }
    }

    public (bool success, string message) Toggle(string maKm)
    {
        const string sql = @"
UPDATE KHUYENMAI
SET TRANGTHAI = CASE WHEN TRANGTHAI = 1 THEN 0 ELSE 1 END
WHERE MAKM = @makm";

        try
        {
            Db.Execute(sql, P("@makm", maKm));
            return (true, "Đã cập nhật trạng thái.");
        }
        catch (Exception ex)
        {
            return (false, $"Không thể cập nhật trạng thái: {ex.Message}");
        }
    }

    public DataRow? GetActiveById(string maKm, DateTime ngay)
    {
        const string sql = @"
SELECT TOP 1 MAKM, TENKM, PHANTRAM_GIAM, NGAYBD, NGAYKT
FROM KHUYENMAI
WHERE TRANGTHAI = 1
  AND MAKM = @makm
  AND @ngay >= NGAYBD
  AND @ngay <= NGAYKT";

        var dt = Db.Query(sql, P("@makm", maKm), P("@ngay", ngay));
        return dt.Rows.Count == 0 ? null : dt.Rows[0];
    }
}
