using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL;

public class PhieuNhapDal
{
    private static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public string GetNextMaNcc()
    {
        const string sql = @"
SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(MANCC, 4, 50))), 0) + 1
FROM NHACUNGCAP
WHERE MANCC LIKE 'NCC%'";

        var v = Db.Scalar(sql);
        var next = v == null || v == DBNull.Value ? 1 : Convert.ToInt32(v);
        return next < 100 ? $"NCC{next:00}" : $"NCC{next}";
    }

    public int InsertNcc(string maNcc, string tenNcc, string? sdt, string? diaChi, string? email)
    {
        const string sql = @"
INSERT INTO NHACUNGCAP (MANCC, TENNCC, SDT, DIACHI, EMAIL)
VALUES (@mancc, @tenncc, @sdt, @diachi, @email)";

        return Db.Execute(sql,
            P("@mancc", maNcc),
            P("@tenncc", tenNcc),
            P("@sdt", sdt),
            P("@diachi", diaChi),
            P("@email", email));
    }

    public DataTable GetAll()
    {
        const string sql = @"
SELECT pn.MAPN, pn.NGAYNHAP, pn.MAND, nd.TENND, pn.MANCC, ncc.TENNCC, pn.TONGTIEN, pn.GHICHU
FROM PHIEUNHAP pn
LEFT JOIN NGUOIDUNG nd ON pn.MAND = nd.MAND
LEFT JOIN NHACUNGCAP ncc ON pn.MANCC = ncc.MANCC
ORDER BY pn.MAPN DESC";
        return Db.Query(sql);
    }

    public DataTable GetPhieuNhapList(DateTime? from, DateTime? to, string? keyword)
    {
        keyword ??= string.Empty;
        keyword = keyword.Trim();

        const string sql = @"
SELECT
    pn.MAPN,
    pn.NGAYNHAP,
    pn.MAND,
    nd.TENND,
    pn.MANCC,
    ncc.TENNCC,
    pn.TONGTIEN,
    pn.GHICHU
FROM PHIEUNHAP pn
LEFT JOIN NGUOIDUNG nd ON pn.MAND = nd.MAND
LEFT JOIN NHACUNGCAP ncc ON pn.MANCC = ncc.MANCC
WHERE
    (@from IS NULL OR pn.NGAYNHAP >= @from)
    AND (@to IS NULL OR pn.NGAYNHAP < DATEADD(day, 1, @to))
    AND (
        @kw = ''
        OR CAST(pn.MAPN AS nvarchar(50)) LIKE @kwlike
        OR ncc.TENNCC LIKE @kwlike
        OR nd.TENND LIKE @kwlike
    )
ORDER BY pn.MAPN DESC";

        return Db.Query(
            sql,
            P("@from", from.HasValue ? from.Value.Date : null),
            P("@to", to.HasValue ? to.Value.Date : null),
            P("@kw", keyword),
            P("@kwlike", $"%{keyword}%"));
    }

    public int InsertHeader(string maNd, string maNcc, DateTime ngayNhap, string? ghiChu, decimal tongTien, SqlTransaction tran)
    {
        const string sql = @"
INSERT INTO PHIEUNHAP (MAND, MANCC, NGAYNHAP, GHICHU, TONGTIEN)
VALUES (@mand, @mancc, @ngay, @ghichu, @tong);
SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[]
        {
            P("@mand", maNd),
            P("@mancc", maNcc),
            P("@ngay", ngayNhap),
            P("@ghichu", ghiChu),
            P("@tong", tongTien)
        });

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void InsertDetail(int maPn, string maCt, int sl, decimal donGiaNhap, SqlTransaction tran)
    {
        const string sql = @"
INSERT INTO CTPN (MAPN, MACT, SL, DONGIA_NHAP)
VALUES (@mapn, @mact, @sl, @dg)";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[]
        {
            P("@mapn", maPn),
            P("@mact", maCt),
            P("@sl", sl),
            P("@dg", donGiaNhap)
        });
        cmd.ExecuteNonQuery();
    }

    public void IncreaseTonKho(string maCt, int sl, SqlTransaction tran)
    {
        const string sql = @"
UPDATE SANPHAM_CHITIET
SET TONKHO = TONKHO + @sl
WHERE MACT = @mact";

        using var cmd = new SqlCommand(sql, tran.Connection!, tran);
        cmd.Parameters.AddRange(new[] { P("@sl", sl), P("@mact", maCt) });
        cmd.ExecuteNonQuery();
    }

    public DataTable GetDetails(int maPn)
    {
        const string sql = @"
 SELECT ct.MAPN, ct.MACT, sp.TENSP, spct.SIZE, spct.MAU, ct.SL, ct.DONGIA_NHAP,
       (ct.SL * ct.DONGIA_NHAP) AS THANHTIEN
FROM CTPN ct
LEFT JOIN SANPHAM_CHITIET spct ON ct.MACT = spct.MACT
LEFT JOIN SANPHAM sp ON spct.MASP = sp.MASP
WHERE ct.MAPN = @mapn";

        return Db.Query(sql, P("@mapn", maPn));
    }

    public DataTable GetAllNcc()
    {
        const string sql = "SELECT MANCC, TENNCC FROM NHACUNGCAP ORDER BY TENNCC";
        return Db.Query(sql);
    }

    public DataTable GetAllNhanVien()
    {
        const string sql = "SELECT MAND, TENND FROM NGUOIDUNG WHERE TRANGTHAI = 1 ORDER BY TENND";
        return Db.Query(sql);
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
WHERE ct.MACT LIKE @kw OR sp.TENSP LIKE @kw OR ct.BARCODE LIKE @kw
ORDER BY sp.TENSP";

        return Db.Query(sql, P("@kw", $"%{keyword}%"));
    }

    public (bool success, string message, int maPn) SavePhieuNhap(
        string maNd,
        string maNcc,
        DateTime ngayNhap,
        string? ghiChu,
        System.Collections.Generic.List<(string maCt, int sl, decimal donGiaNhap)> items)
    {
        if (items.Count == 0)
            return (false, "Chưa có dòng hàng nào.", 0);

        using var conn = Db.OpenConnection();
        using var tran = conn.BeginTransaction();

        try
        {
            var tongTien = items.Sum(x => x.sl * x.donGiaNhap);

            int maPn = InsertHeader(maNd, maNcc, ngayNhap, ghiChu, tongTien, tran);

            foreach (var it in items)
            {
                InsertDetail(maPn, it.maCt, it.sl, it.donGiaNhap, tran);
                IncreaseTonKho(it.maCt, it.sl, tran);
            }

            tran.Commit();
            return (true, "Lưu phiếu nhập thành công.", maPn);
        }
        catch (Exception ex)
        {
            try { tran.Rollback(); } catch { /* ignore */ }
            return (false, $"Lỗi khi lưu phiếu nhập: {ex.Message}", 0);
        }
    }
}
