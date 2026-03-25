using System;
using System.Data;
using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL;

public class SanPhamService
{
    private readonly SanPhamDal _dal = new();

    public DataTable GetAll() => _dal.GetAll();

    public DataTable GetAllLoai() => _dal.GetAllLoai();

    public DataTable Search(string keyword, string? maLoai) => _dal.Search(keyword, maLoai);

    public string GetNextMaSp() => _dal.GetNextMaSp();

    public (bool success, string message) Insert(SanPhamDto sp)
    {
        if (string.IsNullOrWhiteSpace(sp.TenSP))
            return (false, "Tên sản phẩm không được để trống.");

        if (string.IsNullOrWhiteSpace(sp.MaSP))
            sp.MaSP = GetNextMaSp();

        return _dal.Insert(
            sp.MaSP.Trim(),
            sp.TenSP.Trim(),
            string.IsNullOrWhiteSpace(sp.MaLoai) ? null : sp.MaLoai,
            sp.MoTa,
            sp.HinhSP,
            sp.TrangThai
        );
    }

    public (bool success, string message) Update(SanPhamDto sp)
    {
        if (string.IsNullOrWhiteSpace(sp.MaSP))
            return (false, "Mã sản phẩm không được để trống.");

        if (string.IsNullOrWhiteSpace(sp.TenSP))
            return (false, "Tên sản phẩm không được để trống.");

        return _dal.Update(
            sp.MaSP.Trim(),
            sp.TenSP.Trim(),
            string.IsNullOrWhiteSpace(sp.MaLoai) ? null : sp.MaLoai,
            sp.MoTa,
            sp.HinhSP,
            sp.TrangThai
        );
    }

    public (bool success, string message) Delete(string maSp)
    {
        if (string.IsNullOrWhiteSpace(maSp))
            return (false, "Mã sản phẩm không hợp lệ.");

        return _dal.Delete(maSp.Trim());
    }
}
