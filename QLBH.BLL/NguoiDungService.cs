using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL;

public class NguoiDungService
{
	private readonly NguoiDungDal _dal = new();

	private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
	{
		"ADMIN",
		"QUANLY",
		"NHANVIENNHAP",
		"NHANVIENBANHANG",
		"NHANVIEN"
	};

	public DataTable GetAll() => _dal.GetAll();

	public string GetNextMaNd() => _dal.GetNextMaNd();

	public (bool ok, string message) UpdateRole(string maNd, string vaiTro)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		if (!IsValidRole(vaiTro))
			return (false, "Vai trò không hợp lệ.");

		vaiTro = vaiTro.Trim();

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
		if (_dal.PhoneExists(sdt, maNd))
			return (false, "SĐT đã tồn tại.");

		var rows = _dal.UpdateInfo(maNd, tenNd, sdt);
		return rows > 0 ? (true, "Cập nhật thông tin thành công.") : (false, "Không thể cập nhật thông tin.");
	}

	public (bool ok, string message) CreateUser(string tenNd, string sdt, string username, string password, string vaiTro, bool trangThai)
	{
		if (string.IsNullOrWhiteSpace(tenNd) ||
			string.IsNullOrWhiteSpace(sdt) ||
			string.IsNullOrWhiteSpace(username) ||
			string.IsNullOrWhiteSpace(password))
		{
			return (false, "Vui lòng nhập đầy đủ thông tin.");
		}

		if (!IsValidRole(vaiTro))
			return (false, "Vai trò không hợp lệ.");

		tenNd = tenNd.Trim();
		sdt = sdt.Trim();
		username = username.Trim();
		vaiTro = vaiTro.Trim().ToUpperInvariant();

		if (sdt.Any(c => !char.IsDigit(c)))
			return (false, "SĐT chỉ được chứa chữ số.");
		if (sdt.Length < 9 || sdt.Length > 11)
			return (false, "SĐT phải từ 9 đến 11 số.");
		if (_dal.PhoneExists(sdt))
			return (false, "SĐT đã tồn tại.");
		if (_dal.UsernameExists(username))
			return (false, "Tên đăng nhập đã tồn tại.");

		var user = new NguoiDungDto
		{
			MaND = _dal.GetNextMaNd(),
			TenND = tenNd,
			Sdt = sdt,
			Username = username,
			VaiTro = vaiTro,
			TrangThai = trangThai
		};

		var hash = PasswordHasher.Hash(password);
		var rows = _dal.Insert(user, hash);
		return rows > 0 ? (true, "Thêm người dùng thành công.") : (false, "Không thể thêm người dùng.");
	}

	private static bool IsValidRole(string? role)
	{
		if (string.IsNullOrWhiteSpace(role))
			return false;
		return AllowedRoles.Contains(role.Trim());
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

	public (bool ok, string message) UpdateUser(string maNd, string tenNd, string sdt, string vaiTro, bool trangThai, string? newPassword)
	{
		if (string.IsNullOrWhiteSpace(maNd))
			return (false, "Mã người dùng không hợp lệ.");
		if (string.IsNullOrWhiteSpace(tenNd))
			return (false, "Tên người dùng không được để trống.");
		if (string.IsNullOrWhiteSpace(sdt))
			return (false, "SĐT không được để trống.");
		if (!IsValidRole(vaiTro))
			return (false, "Vai trò không hợp lệ.");

		var (okInfo, msgInfo) = UpdateInfo(maNd, tenNd, sdt);
		if (!okInfo) return (false, msgInfo);

		var (okRole, msgRole) = UpdateRole(maNd, vaiTro);
		if (!okRole) return (false, msgRole);

		var (okStatus, msgStatus) = UpdateStatus(maNd, trangThai);
		if (!okStatus) return (false, msgStatus);

		if (!string.IsNullOrWhiteSpace(newPassword))
		{
			var (okPass, msgPass) = ResetPassword(maNd, newPassword);
			if (!okPass) return (false, msgPass);
		}

		return (true, "Cập nhật người dùng thành công.");
	}
}
