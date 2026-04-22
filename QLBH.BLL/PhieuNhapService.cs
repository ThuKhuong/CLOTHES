using System;
using System.Collections.Generic;
using System.Data;
using QLBH.DAL;

namespace QLBH.BLL;

public class PhieuNhapService
{
    private readonly PhieuNhapDal _dal = new();

    public DataTable GetAll() => _dal.GetAll();

    public DataTable GetPhieuNhapList(DateTime? from, DateTime? to, string? keyword) => _dal.GetPhieuNhapList(from, to, keyword);

    public DataTable GetAllNcc() => _dal.GetAllNcc();

    public DataTable GetAllNccDetails() => _dal.GetAllNccDetails();

    public string GetNextMaNcc() => _dal.GetNextMaNcc();

    public DataTable GetAllNhanVien() => _dal.GetAllNhanVien();

    public (bool ok, string message, string? maNcc) AddNcc(string tenNcc, string? sdt, string? diaChi, string? email)
    {
        if (string.IsNullOrWhiteSpace(tenNcc))
            return (false, "Tên nhà cung cấp không được để trống.", null);

        var ma = _dal.GetNextMaNcc();
        var rows = _dal.InsertNcc(ma, tenNcc.Trim(),
            string.IsNullOrWhiteSpace(sdt) ? null : sdt.Trim(),
            string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim());

        return rows > 0 ? (true, "Thêm nhà cung cấp thành công.", ma) : (false, "Không thể thêm nhà cung cấp.", null);
    }

    public (bool ok, string message) UpdateNcc(string maNcc, string tenNcc, string? sdt, string? diaChi, string? email)
    {
        if (string.IsNullOrWhiteSpace(maNcc))
            return (false, "Mã nhà cung cấp không hợp lệ.");
        if (string.IsNullOrWhiteSpace(tenNcc))
            return (false, "Tên nhà cung cấp không được để trống.");

        var rows = _dal.UpdateNcc(maNcc.Trim(), tenNcc.Trim(),
            string.IsNullOrWhiteSpace(sdt) ? null : sdt.Trim(),
            string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim());

        return rows > 0 ? (true, "Cập nhật nhà cung cấp thành công.") : (false, "Không thể cập nhật nhà cung cấp.");
    }

    public (bool ok, string message) DeleteNcc(string maNcc)
    {
        if (string.IsNullOrWhiteSpace(maNcc))
            return (false, "Mã nhà cung cấp không hợp lệ.");

        var rows = _dal.DeleteNcc(maNcc.Trim());
        return rows > 0 ? (true, "Xóa nhà cung cấp thành công.") : (false, "Không thể xóa nhà cung cấp.");
    }

    public DataTable SearchSanPhamChiTiet(string keyword) => _dal.SearchSanPhamChiTiet(keyword);

    public DataTable GetDetails(int maPn) => _dal.GetDetails(maPn);

    public (bool success, string message, int maPn) SavePhieuNhap(
        string maNd,
        string maNcc,
        DateTime ngayNhap,
        string? ghiChu,
        List<(string maCt, int sl, decimal donGiaNhap)> items)
    {
        if (string.IsNullOrWhiteSpace(maNd))
            return (false, "Vui lòng chọn nhân viên.", 0);
        if (string.IsNullOrWhiteSpace(maNcc))
            return (false, "Vui lòng chọn nhà cung cấp.", 0);
        if (items == null || items.Count == 0)
            return (false, "Chưa có dòng hàng nào.", 0);

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.maCt))
                return (false, "Mã CT không hợp lệ.", 0);
            if (it.sl <= 0)
                return (false, "Số lượng phải > 0.", 0);
            if (it.donGiaNhap <= 0)
                return (false, "Đơn giá nhập phải > 0.", 0);
        }

        return _dal.SavePhieuNhap(maNd.Trim(), maNcc.Trim(), ngayNhap, string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim(), items);
    }
}
