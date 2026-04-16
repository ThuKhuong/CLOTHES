using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL
{
    public class AuthService
    {
        private readonly NguoiDungDal _dal = new();

        public (bool ok, string message, NguoiDungDto? user) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Nhập đầy đủ tài khoản và mật khẩu.", null);

            username = username.Trim();
            password = password.Trim();
            var (user, storedPass) = _dal.GetByUsername(username);

            if (user == null) return (false, "Sai tài khoản hoặc mật khẩu.", null);
            if (!user.TrangThai) return (false, "Tài khoản đang bị khóa.", null);

            storedPass = storedPass?.Trim();
            var ok = VerifyPassword(password, storedPass);
            if (!ok)
                return (false, "Sai tài khoản hoặc mật khẩu.", null);

            // Auto-upgrade legacy plaintext passwords to PBKDF2 after successful login.
            if (!string.IsNullOrWhiteSpace(storedPass) && !storedPass.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                var upgradedHash = PasswordHasher.Hash(password);
                _dal.UpdatePassword(username, upgradedHash);
            }

            return (true, "Đăng nhập thành công.", user);
        }

        private static bool VerifyPassword(string password, string? storedPass)
        {
            if (string.IsNullOrWhiteSpace(storedPass))
                return false;

            storedPass = storedPass.Trim();

            // Backward compatibility: some seed scripts store plain text passwords.
            if (storedPass.StartsWith("PBKDF2$", StringComparison.Ordinal))
                return PasswordHasher.Verify(password, storedPass);

            return string.Equals(password, storedPass, StringComparison.Ordinal);
        }

        public (bool ok, string message) Register(string tenNd, string sdt, string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(tenNd) ||
                string.IsNullOrWhiteSpace(sdt) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
                return (false, "Mật khẩu xác nhận không khớp.");

            if (_dal.UsernameExists(username))
                return (false, "Tên đăng nhập đã tồn tại.");

            string maNd = _dal.GetNextMaNd();

            var user = new NguoiDungDto
            {
                MaND = maNd,
                TenND = tenNd.Trim(),
                Sdt = sdt.Trim(),
                Username = username.Trim(),
                VaiTro = "NHANVIEN",
                TrangThai = true
            };

            string hash = PasswordHasher.Hash(password);
            int rows = _dal.Insert(user, hash);

            return rows > 0 ? (true, "Đăng ký thành công.") : (false, "Đăng ký thất bại.");
        }

        public (bool ok, string message) ForgotPassword(string username, string sdt, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(sdt) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
                return (false, "Mật khẩu xác nhận không khớp.");

            username = username.Trim();
            sdt = sdt.Trim();

            var (user, _) = _dal.GetByUsername(username);
            if (user == null)
                return (false, "Tài khoản không tồn tại.");

            if (!user.TrangThai)
                return (false, "Tài khoản đang bị khóa.");

            if (!string.Equals(user.Sdt?.Trim(), sdt, StringComparison.Ordinal))
                return (false, "Số điện thoại không khớp.");

            var hash = PasswordHasher.Hash(newPassword);
            var rows = _dal.UpdatePassword(username, hash);
            return rows > 0 ? (true, "Đặt lại mật khẩu thành công.") : (false, "Không thể đặt lại mật khẩu.");
        }
    }
}
