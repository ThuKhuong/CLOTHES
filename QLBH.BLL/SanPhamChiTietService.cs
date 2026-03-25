using System;
using System.Data;
using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL;

public class SanPhamChiTietService
{
    private readonly SanPhamChiTietDal _dal = new();

    public DataTable GetLowStock(int threshold)
    {
        if (threshold < 0)
            threshold = 0;
        return _dal.GetLowStock(threshold);
    }

    public string GetNextMaCt() => _dal.GetNextMaCt();

    public DataTable GetByMaSp(string maSp) => _dal.GetByMaSp(maSp);

    public (bool success, string message) Insert(SanPhamChiTietDto ct)
    {
        if (string.IsNullOrWhiteSpace(ct.MaSP))
            return (false, "Mã sản phẩm không được để trống");
        if (string.IsNullOrWhiteSpace(ct.Size))
            return (false, "Size không được để trống");
        if (string.IsNullOrWhiteSpace(ct.Mau))
            return (false, "Màu không được để trống");
        if (ct.GiaBan < 0)
            return (false, "Giá bán không hợp lệ");
        if (ct.TonKho < 0)
            return (false, "Tồn kho không hợp lệ");

        if (string.IsNullOrWhiteSpace(ct.MaCT))
            ct.MaCT = GetNextMaCt();

        return _dal.Insert(
            ct.MaCT.Trim(),
            ct.MaSP.Trim(),
            ct.Size.Trim(),
            ct.Mau.Trim(),
            ct.GiaBan,
            ct.TonKho,
            ct.BarCode,
            ct.TrangThai
        );
    }

    public (bool success, string message) Update(SanPhamChiTietDto ct)
    {
        if (string.IsNullOrWhiteSpace(ct.MaCT))
            return (false, "Mã biến thể (MACT) không được để trống.");
        if (string.IsNullOrWhiteSpace(ct.Size))
            return (false, "Size không được để trống.");
        if (string.IsNullOrWhiteSpace(ct.Mau))
            return (false, "Màu không được để trống.");
        if (ct.GiaBan < 0)
            return (false, "Giá bán không hợp lệ.");
        if (ct.TonKho < 0)
            return (false, "Tồn kho không hợp lệ.");

        return _dal.Update(
            ct.MaCT.Trim(),
            ct.Size.Trim(),
            ct.Mau.Trim(),
            ct.GiaBan,
            ct.TonKho,
            ct.BarCode,
            ct.TrangThai
        );
    }

    public (bool success, string message) Delete(string maCt)
    {
        if (string.IsNullOrWhiteSpace(maCt))
            return (false, "Mã biến thể không hợp lệ.");

        return _dal.Delete(maCt.Trim());
    }
}
