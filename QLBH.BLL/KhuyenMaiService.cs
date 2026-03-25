using System;
using System.Data;
using QLBH.DAL;

namespace QLBH.BLL;

public class KhuyenMaiService
{
    private readonly KhuyenMaiDal _dal = new();

    public string GetNextMaKm() => _dal.GetNextMaKm();

    public DataTable GetAll() => _dal.GetAll();

    public DataRow? GetById(string maKm)
        => string.IsNullOrWhiteSpace(maKm) ? null : _dal.GetById(maKm.Trim());

    public (bool ok, string message) Upsert(string maKm, string tenKm, int phanTram, DateTime ngayBd, DateTime ngayKt, bool trangThai)
        => _dal.Upsert(maKm, tenKm, phanTram, ngayBd, ngayKt, trangThai);

    public (bool ok, string message) Delete(string maKm)
        => string.IsNullOrWhiteSpace(maKm) ? (false, "Mã KM không hợp lệ.") : _dal.Delete(maKm.Trim());

    public (bool ok, string message) Toggle(string maKm)
        => string.IsNullOrWhiteSpace(maKm) ? (false, "Mã KM không hợp lệ.") : _dal.Toggle(maKm.Trim());

    public DataTable GetActive(DateTime ngay) => _dal.GetActive(ngay);

    public (bool ok, string? maKm, int phanTram, string? tenKm) ValidateForOrder(string? maKm, DateTime ngay)
    {
        if (string.IsNullOrWhiteSpace(maKm))
            return (true, null, 0, null);

        var row = _dal.GetActiveById(maKm.Trim(), ngay);
        if (row == null)
            return (false, null, 0, null);

        int percent = row["PHANTRAM_GIAM"] == DBNull.Value ? 0 : Convert.ToInt32(row["PHANTRAM_GIAM"]);
        var ten = row["TENKM"] == DBNull.Value ? null : row["TENKM"].ToString();

        return (true, row["MAKM"].ToString(), percent, ten);
    }
}
