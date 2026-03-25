using Microsoft.Data.SqlClient;
using System.Data;
using QLBH.DTO;

namespace QLBH.DAL
{
    public class NguoiDungDal
    {
        public DataTable GetAll()
        {
            const string sql = @"
SELECT MAND, TENND, SDT, USERNAME, VAITRO, TRANGTHAI
FROM NGUOIDUNG
ORDER BY MAND";
            return Db.Query(sql);
        }

        public int UpdateRole(string maNd, string vaiTro)
        {
            const string sql = "UPDATE NGUOIDUNG SET VAITRO=@role WHERE MAND=@mand";
            return Db.Execute(sql,
                new SqlParameter("@role", vaiTro),
                new SqlParameter("@mand", maNd));
        }

        public int UpdateStatus(string maNd, bool trangThai)
        {
            const string sql = "UPDATE NGUOIDUNG SET TRANGTHAI=@st WHERE MAND=@mand";
            return Db.Execute(sql,
                new SqlParameter("@st", trangThai),
                new SqlParameter("@mand", maNd));
        }

        public int UpdateInfo(string maNd, string tenNd, string sdt)
        {
            const string sql = "UPDATE NGUOIDUNG SET TENND=@ten, SDT=@sdt WHERE MAND=@mand";
            return Db.Execute(sql,
                new SqlParameter("@ten", tenNd),
                new SqlParameter("@sdt", sdt),
                new SqlParameter("@mand", maNd));
        }
        public (NguoiDungDto? user, string? pass) GetByUsername(string username)
        {
            string sql = @"
SELECT MAND, TENND, SDT,    USERNAME, PASS, VAITRO, TRANGTHAI
FROM NGUOIDUNG
WHERE USERNAME = @u";

            var dt = Db.Query(sql, new SqlParameter("@u", username));
            if (dt.Rows.Count == 0) return (null, null);

            DataRow r = dt.Rows[0];
            var user = new NguoiDungDto
            {
                MaND = r["MAND"].ToString() ?? "",
                TenND = r["TENND"].ToString() ?? "",
                Username = r["USERNAME"].ToString() ?? "",
                VaiTro = r["VAITRO"].ToString() ?? "NHANVIEN",
                TrangThai = (bool)r["TRANGTHAI"]
            };

            string pass = r["PASS"].ToString() ?? "";
            return (user, pass);
        }

        public bool UsernameExists(string username)
        {
            const string sql = @"SELECT COUNT(1) FROM NGUOIDUNG WHERE USERNAME = @u";
            var result = Db.Scalar(sql, new SqlParameter("@u", username));
            return result != null && Convert.ToInt32(result) > 0;
        }

        public string GetNextMaNd()
        {
            const string sql = @"SELECT MAX(MAND) FROM NGUOIDUNG WHERE MAND LIKE 'ND%'";
            var result = Db.Scalar(sql);

            string? max = result?.ToString();
            if (string.IsNullOrWhiteSpace(max))
                return "ND01";

            if (max.Length < 3 || !max.StartsWith("ND", StringComparison.Ordinal))
                return "ND01";

            string numberPart = max[2..];
            if (!int.TryParse(numberPart, out int n))
                return "ND01";

            n++;
            return "ND" + n.ToString("D2");
        }

        public int Insert(NguoiDungDto user, string passwordHash)
        {
            const string sql = @"
INSERT INTO NGUOIDUNG (MAND, TENND, SDT, USERNAME, PASS, VAITRO, TRANGTHAI)
VALUES (@mand, @tennd, @sdt, @username, @pass, @vaitro, @trangthai)";

            return Db.Execute(
                sql,            
                new SqlParameter("@mand", user.MaND),
                new SqlParameter("@tennd", user.TenND),
                new SqlParameter("@sdt", user.Sdt),
                new SqlParameter("@username", user.Username),
                new SqlParameter("@pass", passwordHash),
                new SqlParameter("@vaitro", user.VaiTro),
                new SqlParameter("@trangthai", user.TrangThai)
            );
        }

        public int UpdatePassword(string username, string passwordHash)
        {
            const string sql = "UPDATE NGUOIDUNG SET PASS=@pass WHERE USERNAME=@u";
            return Db.Execute(sql,
                new SqlParameter("@pass", passwordHash),
                new SqlParameter("@u", username));
        }

        public int UpdatePasswordByMaNd(string maNd, string passwordHash)
        {
            const string sql = "UPDATE NGUOIDUNG SET PASS=@pass WHERE MAND=@mand";
            return Db.Execute(sql,
                new SqlParameter("@pass", passwordHash),
                new SqlParameter("@mand", maNd));
        }
    }
}
