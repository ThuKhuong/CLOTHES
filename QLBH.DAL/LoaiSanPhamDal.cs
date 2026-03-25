using Microsoft.Data.SqlClient;
using System.Data;
using QLBH.DTO;

namespace QLBH.DAL
{
    public class LoaiSanPhamDal
    {
        public DataTable GetAll()
        {
            const string sql = @"SELECT MALOAI, TENLOAI FROM LOAISANPHAM ORDER BY TENLOAI COLLATE Vietnamese_CI_AS";
            return Db.Query(sql);
        }

        public LoaiSanPhamDto? GetById(string maLoai)
        {
            const string sql = @"SELECT MALOAI, TENLOAI FROM LOAISANPHAM WHERE MALOAI = @ma";
            var dt = Db.Query(sql, new SqlParameter("@ma", maLoai));
            
            if (dt.Rows.Count == 0) return null;
            
            DataRow r = dt.Rows[0];
            return new LoaiSanPhamDto
            {
                MaLoai = r["MALOAI"].ToString() ?? "",
                TenLoai = r["TENLOAI"].ToString() ?? ""
            };
        }

        public bool Exists(string maLoai)
        {
            const string sql = @"SELECT COUNT(1) FROM LOAISANPHAM WHERE MALOAI = @ma";
            var result = Db.Scalar(sql, new SqlParameter("@ma", maLoai));
            return result != null && Convert.ToInt32(result) > 0;
        }

        public string GetNextMaLoai()
        {
            const string sql = @"SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(MALOAI, 2, 50))), 0) + 1 FROM LOAISANPHAM WHERE MALOAI LIKE 'L%'";
            var result = Db.Scalar(sql);

            var next = result == null || result == DBNull.Value ? 1 : Convert.ToInt32(result);

            // Format: L01, L02, ... L99, L100...
            return next < 100 ? $"L{next:00}" : $"L{next}";
        }

        public int Insert(LoaiSanPhamDto loai)
        {
            const string sql = @"INSERT INTO LOAISANPHAM (MALOAI, TENLOAI) VALUES (@ma, @ten)";
            return Db.Execute(sql, 
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = loai.MaLoai },
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = loai.TenLoai });
        }

        public int Update(LoaiSanPhamDto loai)
        {
            const string sql = @"UPDATE LOAISANPHAM SET TENLOAI = @ten WHERE MALOAI = @ma";
            return Db.Execute(sql,
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = loai.TenLoai },
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = loai.MaLoai });
        }

        public int Delete(string maLoai)
        {
            const string sql = @"DELETE FROM LOAISANPHAM WHERE MALOAI = @ma";
            return Db.Execute(sql, new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = maLoai });
        }

        public bool HasProducts(string maLoai)
        {
            const string sql = @"SELECT COUNT(1) FROM SANPHAM WHERE MALOAI = @ma";
            var result = Db.Scalar(sql, new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = maLoai });
            return result != null && Convert.ToInt32(result) > 0;
        }
    }
}