using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL;

public class SanPhamDal
{
    private static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public string GetNextMaSp()
    {
        const string sql = @"
SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(MASP, 3, 50))), 0) + 1
FROM SANPHAM
WHERE MASP LIKE 'SP%'";

        var v = Db.Scalar(sql);
        var next = v == null || v == DBNull.Value ? 1 : Convert.ToInt32(v);

        // Format: SP01, SP02, ... SP99, SP100...
        return next < 100 ? $"SP{next:00}" : $"SP{next}";
    }

    public DataTable GetAll()
    {
        const string sql = @"
SELECT 
    sp.MASP,
    sp.TENSP,
    sp.MALOAI,
    lsp.TENLOAI,
    sp.MOTA,
    sp.HINHSP,
    sp.TRANGTHAI,
     MIN(ct.GIABAN) AS GIATU,
     ISNULL(SUM(CASE WHEN ct.TRANGTHAI = 1 THEN ct.TONKHO ELSE 0 END), 0) AS TONGTON
FROM SANPHAM sp
LEFT JOIN LOAISANPHAM lsp ON sp.MALOAI = lsp.MALOAI
LEFT JOIN SANPHAM_CHITIET ct ON ct.MASP = sp.MASP AND ct.TRANGTHAI = 1
GROUP BY sp.MASP, sp.TENSP, sp.MALOAI, lsp.TENLOAI, sp.MOTA, sp.HINHSP, sp.TRANGTHAI
ORDER BY sp.MASP";
        return Db.Query(sql);
    }

    public DataTable GetAllLoai()
    {
        const string sql = "SELECT MALOAI, TENLOAI FROM LOAISANPHAM ORDER BY TENLOAI";
        return Db.Query(sql);
    }

    public DataTable Search(string keyword, string? maLoai)
    {
        keyword ??= string.Empty;
        keyword = keyword.Trim();

        var sql = @"
SELECT 
    sp.MASP,
    sp.TENSP,
    sp.MALOAI,
    lsp.TENLOAI,
    sp.MOTA,
    sp.HINHSP,
    sp.TRANGTHAI,
     MIN(ct.GIABAN) AS GIATU,
     ISNULL(SUM(CASE WHEN ct.TRANGTHAI = 1 THEN ct.TONKHO ELSE 0 END), 0) AS TONGTON
FROM SANPHAM sp
LEFT JOIN LOAISANPHAM lsp ON sp.MALOAI = lsp.MALOAI
LEFT JOIN SANPHAM_CHITIET ct ON ct.MASP = sp.MASP AND ct.TRANGTHAI = 1
WHERE (sp.MASP LIKE @kw OR sp.TENSP LIKE @kw)
";

        var list = new System.Collections.Generic.List<SqlParameter>
        {
            P("@kw", $"%{keyword}%")
        };

        if (!string.IsNullOrWhiteSpace(maLoai))
        {
            sql += " AND sp.MALOAI = @maLoai";
            list.Add(P("@maLoai", maLoai));
        }

        sql += @"
GROUP BY sp.MASP, sp.TENSP, sp.MALOAI, lsp.TENLOAI, sp.MOTA, sp.HINHSP, sp.TRANGTHAI
ORDER BY sp.MASP";

        return Db.Query(sql, list.ToArray());
    }

    public (bool success, string message) Insert(string maSp, string tenSp, string? maLoai, string? moTa, string? hinhSp, bool trangThai)
    {
        const string sql = @"
INSERT INTO SANPHAM (MASP, TENSP, MALOAI, MOTA, HINHSP, TRANGTHAI)
VALUES (@masp, @tensp, @maloai, @mota, @hinh, @tt)";

        var p = new[]
        {
            P("@masp", maSp),
            P("@tensp", tenSp),
            P("@maloai", maLoai),
            P("@mota", moTa),
            P("@hinh", hinhSp),
            P("@tt", trangThai)
        };

        return Db.Execute(sql, p) > 0
            ? (true, "Thêm sản phẩm thành công")
            : (false, "Không thể thêm sản phẩm");
    }

    public (bool success, string message) Update(string maSp, string tenSp, string? maLoai, string? moTa, string? hinhSp, bool trangThai)
    {
        const string sql = @"
UPDATE SANPHAM
SET TENSP=@tensp, MALOAI=@maloai, MOTA=@mota, HINHSP=@hinh, TRANGTHAI=@tt
WHERE MASP=@masp";

        var p = new[]
        {
            P("@masp", maSp),
            P("@tensp", tenSp),
            P("@maloai", maLoai),
            P("@mota", moTa),
            P("@hinh", hinhSp),
            P("@tt", trangThai)
        };

        return Db.Execute(sql, p) > 0
            ? (true, "Cập nhật sản phẩm thành công")
            : (false, "Không thể cập nhật sản phẩm");
    }

    public (bool success, string message) Delete(string maSp)
    {
        try
        {
            const string sql1 = "DELETE FROM SANPHAM_CHITIET WHERE MASP=@masp";
            const string sql2 = "DELETE FROM SANPHAM WHERE MASP=@masp";

            Db.Execute(sql1, new[] { P("@masp", maSp) });
            return Db.Execute(sql2, new[] { P("@masp", maSp) }) > 0
                ? (true, "Xóa sản phẩm thành công")
                : (false, "Không thể xóa sản phẩm");
        }
        catch (SqlException ex)
        {
            if (ex.Number == 547)
                return (false, "Không thể xóa sản phẩm vì đã phát sinh dữ liệu liên quan (đơn hàng/chi tiết sản phẩm).");

            return (false, $"Lỗi dữ liệu: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}
