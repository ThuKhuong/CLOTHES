using System;
using System.Data;
using System.Linq;
using QLBH.DAL;

namespace QLBH.BLL;

public class NguoiDungService
{
	private readonly NguoiDungDal _dal = new();

	public DataTable GetAll() => _dal.GetAll();

	public (bool ok, string message) UpdateRole(string maNd, string vaiTro)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		if (string.IsNullOrWhiteSpace(vaiTro))
			return (false, "Vai trò không hợp lệ.");

		vaiTro = vaiTro.Trim();
		if (!vaiTro.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) &&
			!vaiTro.Equals("NHANVIEN", StringComparison.OrdinalIgnoreCase))
			return (false, "Vai trò chỉ được là ADMIN hoặc NHANVIEN.");

		var rows = _dal.UpdateRole(maNd.Trim(), vaiTro.ToUpperInvariant());
		return rows > 0 ? (true, "Cập nhật vai trò thành công.") : (false, "Không thể cập nhật vai trò.");
	}

	public (bool ok, string message) UpdateStatus(string maNd, bool trangThai)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		var rows = _dal.UpdateStatus(maNd.Trim(), trangThai);
		return rows > 0 ? (true, "Cập nhật trạng thái thành công.") : (false, "Không thể cập nhật trạng thái.");
	}

	public (bool ok, string message) UpdateInfo(string maNd, string tenNd, string sdt)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		if (string.IsNullOrWhiteSpace(tenNd))
			return (false, "Tên người dùng không được để trống.");
		if (string.IsNullOrWhiteSpace(sdt))
			return (false, "SĐT không được để trống.");

		maNd = maNd.Trim();
		tenNd = tenNd.Trim();
		sdt = sdt.Trim();

		if (sdt.Any(c => !char.IsDigit(c)))
			return (false, "SĐT chỉ được chứa chữ số.");
		if (sdt.Length < 9 || sdt.Length > 11)
			return (false, "SĐT phải từ 9 đến 11 số.");

		var rows = _dal.UpdateInfo(maNd, tenNd, sdt);
		return rows > 0 ? (true, "Cập nhật thông tin thành công.") : (false, "Không thể cập nhật thông tin.");
	}

	public (bool ok, string message) ResetPassword(string maNd, string newPassword)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		if (string.IsNullOrWhiteSpace(newPassword))
			return (false, "Mật khẩu mới không hợp lệ.");

		var hash = PasswordHasher.Hash(newPassword);
		var rows = _dal.UpdatePasswordByMaNd(maNd.Trim(), hash);
		return rows > 0 ? (true, "Đặt lại mật khẩu thành công.") : (false, "Không thể đặt lại mật khẩu.");
	}
}
