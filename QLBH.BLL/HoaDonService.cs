using System;
using System.Collections.Generic;
using System.Data;
using QLBH.DAL;

namespace QLBH.BLL;

public class HoaDonService
{
    private readonly HoaDonDal _dal = new();

    public DataTable GetAllNhanVien() => _dal.GetAllNhanVien();

    public DataTable SearchSanPhamChiTiet(string keyword) => _dal.SearchSanPhamChiTiet(keyword);

    public DataTable GetHoaDonList(DateTime? from, DateTime? to, string? keyword) => _dal.GetHoaDonList(from, to, keyword);

    public DataTable GetHoaDonDetail(int soHd) => _dal.GetHoaDonDetail(soHd);

    public DataTable GetThongKeTongQuan(DateTime? from, DateTime? to) => _dal.GetThongKeTongQuan(from, to);

    public DataTable GetThongKeTheoNgay(DateTime? from, DateTime? to) => _dal.GetThongKeTheoNgay(from, to);

    public DataTable GetTopSanPhamBySoLuong(DateTime? from, DateTime? to, int top = 10) => _dal.GetTopSanPhamBySoLuong(from, to, top);

    public DataTable GetTopSanPhamByDoanhThu(DateTime? from, DateTime? to, int top = 10) => _dal.GetTopSanPhamByDoanhThu(from, to, top);

    public (bool success, string message) HuyHoacTraHang(int soHd, bool traHang)
        => _dal.UpdateTrangThaiVaHoanTonKho(soHd, traHang ? "TRAHANG" : "HUY");

    public string? FindMaCtByBarcode(string barcode) => _dal.FindMaCtByBarcode(barcode);

    public (bool success, string message, int soHd) ThanhToan(
        string maNd,
        string? maKh,
        DateTime ngay,
        string? maKm,
        string? ghiChu,
        string? hinhThucTt,
        List<(string maCt, int sl, decimal donGia)> items)
    {
        if (string.IsNullOrWhiteSpace(maNd))
            return (false, "Vui lòng chọn nhân viên.", 0);
        if (items == null || items.Count == 0)
            return (false, "Chưa có sản phẩm trong giỏ.", 0);

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.maCt))
                return (false, "Mã CT không hợp lệ.", 0);
            if (it.sl <= 0)
                return (false, "Số lượng phải > 0.", 0);
            if (it.donGia < 0)
                return (false, "Đơn giá không hợp lệ.", 0);
        }

        return _dal.SaveHoaDon(
            maNd.Trim(),
            string.IsNullOrWhiteSpace(maKh) ? null : maKh.Trim(),
            ngay,
            string.IsNullOrWhiteSpace(maKm) ? null : maKm.Trim(),
            string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim(),
            string.IsNullOrWhiteSpace(hinhThucTt) ? null : hinhThucTt.Trim(),
            items);
    }
}
