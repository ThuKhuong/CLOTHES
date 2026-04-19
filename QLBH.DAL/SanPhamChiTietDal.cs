using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL;

public class SanPhamChiTietDal
{
    private static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public DataTable GetLowStock(int threshold)
    {
        const string sql = @"
SELECT MACT, MASP, SIZE, MAU, GIABAN, TONKHO, BARCODE, TRANGTHAI
FROM SANPHAM_CHITIET
WHERE TRANGTHAI = 1 AND TONKHO <= @threshold
ORDER BY TONKHO ASC, MASP, MACT";

        return Db.Query(sql, new[] { P("@threshold", threshold) });
    }

    public DataTable GetByMaSp(string maSp)
    {
        const string sql = @"
SELECT MACT, MASP, SIZE, MAU, GIABAN, TONKHO, BARCODE, TRANGTHAI
FROM SANPHAM_CHITIET
WHERE MASP=@masp
ORDER BY MACT";

        return Db.Query(sql, new[] { P("@masp", maSp) });
    }

    public (bool success, string message) Insert(string maCt, string maSp, string size, string mau, decimal giaBan, int tonKho, string? barCode, bool trangThai)
    {
        const string sql = @"
INSERT INTO SANPHAM_CHITIET (MACT, MASP, SIZE, MAU, GIABAN, TONKHO, BARCODE, TRANGTHAI)
VALUES (@mact, @masp, @size, @mau, @gia, @ton, @barcode, @tt)";

        var p = new[]
        {
            P("@mact", maCt),
            P("@masp", maSp),
            P("@size", size),
            P("@mau", mau),
            P("@gia", giaBan),
            P("@ton", tonKho),
            P("@barcode", barCode),
            P("@tt", trangThai)
        };

        return Db.Execute(sql, p) > 0
            ? (true, "Thêm biến thể thành công")
            : (false, "Không thể thêm biến thể");
    }

    public (bool success, string message) Update(string maCt, string size, string mau, decimal giaBan, string? barCode, bool trangThai)
    {
        const string sql = @"
UPDATE SANPHAM_CHITIET
 SET SIZE=@size, MAU=@mau, GIABAN=@gia, BARCODE=@barcode, TRANGTHAI=@tt
WHERE MACT=@mact";

        var p = new[]
        {
            P("@mact", maCt),
            P("@size", size),
            P("@mau", mau),
            P("@gia", giaBan),
            P("@barcode", barCode),
            P("@tt", trangThai)
        };

        return Db.Execute(sql, p) > 0
            ? (true, "Cập nhật biến thể thành công")
            : (false, "Không thể cập nhật biến thể");
    }

    public (bool success, string message) Delete(string maCt)
    {
        const string sql = "DELETE FROM SANPHAM_CHITIET WHERE MACT=@mact";
        return Db.Execute(sql, new[] { P("@mact", maCt) }) > 0
            ? (true, "Xóa biến thể thành công")
            : (false, "Không thể xóa biến thể");
    }

    public string GetNextMaCt()
    {
        const string sql = @"
SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(MACT, 3, 50))), 0) + 1
FROM SANPHAM_CHITIET
WHERE MACT LIKE 'CT%'";

        var v = Db.Scalar(sql);
        var next = v == null || v == DBNull.Value ? 1 : Convert.ToInt32(v);
        return next < 100 ? $"CT{next:00}" : $"CT{next}";
    }
}
