using System;
using System.Data;
using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL
{
    public class LoaiSanPhamService
    {
        private readonly LoaiSanPhamDal _dal = new();

        public DataTable GetAll()
        {
            return _dal.GetAll();
        }

        public LoaiSanPhamDto? GetById(string maLoai)
        {
            return _dal.GetById(maLoai);
        }

        public (bool success, string message) Insert(LoaiSanPhamDto loai)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(loai.TenLoai))
                    return (false, "Tên loại sản phẩm không được để trống.");

                if (loai.TenLoai.Length > 100)
                    return (false, "Tên loại sản phẩm không được quá 100 ký tự.");

                // Auto generate MaLoai if empty
                if (string.IsNullOrWhiteSpace(loai.MaLoai))
                    loai.MaLoai = _dal.GetNextMaLoai();

                // Check duplicate
                if (_dal.Exists(loai.MaLoai))
                    return (false, "Mã loại đã tồn tại.");

                // Insert
                int result = _dal.Insert(loai);
                return result > 0
                    ? (true, "Thêm loại sản phẩm thành công.")
                    : (false, "Không thể thêm loại sản phẩm.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public (bool success, string message) Update(LoaiSanPhamDto loai)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(loai.TenLoai))
                    return (false, "Tên loại sản phẩm không được để trống.");

                if (loai.TenLoai.Length > 100)
                    return (false, "Tên loại sản phẩm không được quá 100 ký tự.");

                if (!_dal.Exists(loai.MaLoai))
                    return (false, "Loại sản phẩm không tồn tại.");

                // Update
                int result = _dal.Update(loai);
                return result > 0
                    ? (true, "Cập nhật loại sản phẩm thành công.")
                    : (false, "Không thể cập nhật loại sản phẩm.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public (bool success, string message) Delete(string maLoai)
        {
            try
            {
                if (!_dal.Exists(maLoai))
                    return (false, "Loại sản phẩm không tồn tại.");

                // Check if has products
                if (_dal.HasProducts(maLoai))
                    return (false, "Không thể xóa loại sản phẩm vì đang có sản phẩm sử dụng.");

                int result = _dal.Delete(maLoai);
                return result > 0
                    ? (true, "Xóa loại sản phẩm thành công.")
                    : (false, "Không thể xóa loại sản phẩm.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public string GetNextMaLoai()
        {
            return _dal.GetNextMaLoai();
        }
    }
}
