using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QLBH.DTO;

namespace QLBH.DAL
{
    public class KhachHangDal
    {
        public DataTable GetAll()
        {
            const string sql = @"
                SELECT MAKH, HOTEN, GIOITINH, DCHI, SDT, EMAIL 
                FROM KHACHHANG 
                ORDER BY HOTEN";
            
            return Db.Query(sql);
        }

        public DataTable Search(string keyword)
        {
            const string sql = @"
                SELECT MAKH, HOTEN, GIOITINH, DCHI, SDT, EMAIL 
                FROM KHACHHANG 
                WHERE (HOTEN LIKE @keyword OR SDT LIKE @keyword OR EMAIL LIKE @keyword OR MAKH LIKE @keyword)
                ORDER BY HOTEN";
            
            var parameters = new[] { new SqlParameter("@keyword", $"%{keyword}%") };
            return Db.Query(sql, parameters);
        }

        public bool CheckExists(string maKH)
        {
            const string sql = "SELECT COUNT(1) FROM KHACHHANG WHERE MAKH = @MAKH";
            var parameters = new[] { new SqlParameter("@MAKH", maKH) };
            
            var result = Db.Scalar(sql, parameters);
            return Convert.ToInt32(result ?? 0) > 0;
        }

        public bool CheckPhoneExists(string sdt, string? excludeMaKH = null)
        {
            string sql = "SELECT COUNT(1) FROM KHACHHANG WHERE SDT = @SDT";
            var paramList = new List<SqlParameter> { new SqlParameter("@SDT", sdt) };

            if (!string.IsNullOrEmpty(excludeMaKH))
            {
                sql += " AND MAKH != @MAKH";
                paramList.Add(new SqlParameter("@MAKH", excludeMaKH));
            }

            var result = Db.Scalar(sql, paramList.ToArray());
            return Convert.ToInt32(result ?? 0) > 0;
        }

        public bool Insert(KhachHangDto khachHang)
        {
            const string sql = @"
                INSERT INTO KHACHHANG (MAKH, HOTEN, GIOITINH, DCHI, SDT, EMAIL)
                VALUES (@MAKH, @HOTEN, @GIOITINH, @DCHI, @SDT, @EMAIL)";

            var parameters = new[]
            {
                new SqlParameter("@MAKH", khachHang.MaKH),
                new SqlParameter("@HOTEN", khachHang.HoTen),
                new SqlParameter("@GIOITINH", khachHang.GioiTinh ?? (object)DBNull.Value),
                new SqlParameter("@DCHI", khachHang.DChi ?? (object)DBNull.Value),
                new SqlParameter("@SDT", khachHang.SDT),
                new SqlParameter("@EMAIL", khachHang.Email ?? (object)DBNull.Value)
            };

            return Db.Execute(sql, parameters) > 0;
        }

        public bool Update(KhachHangDto khachHang)
        {
            const string sql = @"
                UPDATE KHACHHANG 
                SET HOTEN = @HOTEN, GIOITINH = @GIOITINH, DCHI = @DCHI, 
                    SDT = @SDT, EMAIL = @EMAIL 
                WHERE MAKH = @MAKH";

            var parameters = new[]
            {
                new SqlParameter("@MAKH", khachHang.MaKH),
                new SqlParameter("@HOTEN", khachHang.HoTen),
                new SqlParameter("@GIOITINH", khachHang.GioiTinh ?? (object)DBNull.Value),
                new SqlParameter("@DCHI", khachHang.DChi ?? (object)DBNull.Value),
                new SqlParameter("@SDT", khachHang.SDT),
                new SqlParameter("@EMAIL", khachHang.Email ?? (object)DBNull.Value)
            };

            return Db.Execute(sql, parameters) > 0;
        }

        public bool Delete(string maKH)
        {
            const string sql = "DELETE FROM KHACHHANG WHERE MAKH = @MAKH";
            var parameters = new[] { new SqlParameter("@MAKH", maKH) };
            
            return Db.Execute(sql, parameters) > 0;
        }

        public KhachHangDto? GetById(string maKH)
        {
            const string sql = @"
                SELECT MAKH, HOTEN, GIOITINH, DCHI, SDT, EMAIL 
                FROM KHACHHANG 
                WHERE MAKH = @MAKH";
            
            var parameters = new[] { new SqlParameter("@MAKH", maKH) };
            var dt = Db.Query(sql, parameters);
            
            if (dt.Rows.Count == 0) return null;
            
            var row = dt.Rows[0];
            return new KhachHangDto
            {
                MaKH = row["MAKH"].ToString()!,
                HoTen = row["HOTEN"].ToString()!,
                GioiTinh = row["GIOITINH"] == DBNull.Value ? null : row["GIOITINH"].ToString(),
                DChi = row["DCHI"] == DBNull.Value ? null : row["DCHI"].ToString(),
                SDT = row["SDT"].ToString()!,
                Email = row["EMAIL"] == DBNull.Value ? null : row["EMAIL"].ToString()
            };
        }
    }
}