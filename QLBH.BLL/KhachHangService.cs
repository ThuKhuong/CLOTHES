using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using QLBH.DAL;
using QLBH.DTO;

namespace QLBH.BLL
{
    public class KhachHangService
    {
        private readonly KhachHangDal _dal = new();

        public DataTable GetAll()
        {
            try
            {
                return _dal.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách khách hàng: {ex.Message}");
            }
        }

        public DataTable Search(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return GetAll();

                return _dal.Search(keyword.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tìm kiếm khách hàng: {ex.Message}");
            }
        }

        public (bool success, string message) Insert(KhachHangDto khachHang)
        {
            try
            {
                // Validate input
                var validationResult = ValidateKhachHang(khachHang);
                if (!validationResult.isValid)
                    return (false, validationResult.message);

                // Generate new customer ID
                khachHang.MaKH = GenerateNewId();

                // Check if phone already exists
                if (_dal.CheckPhoneExists(khachHang.SDT))
                    return (false, "Số điện thoại đã được sử dụng bởi khách hàng khác.");

                // Insert customer
                bool result = _dal.Insert(khachHang);
                return result
                    ? (true, "Thêm khách hàng thành công!")
                    : (false, "Không thể thêm khách hàng. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm khách hàng: {ex.Message}");
            }
        }

        public (bool success, string message) Update(KhachHangDto khachHang)
        {
            try
            {
                // Validate input
                var validationResult = ValidateKhachHang(khachHang);
                if (!validationResult.isValid)
                    return (false, validationResult.message);

                // Check if customer exists
                if (!_dal.CheckExists(khachHang.MaKH))
                    return (false, "Khách hàng không tồn tại.");

                // Check if phone already exists (excluding current customer)
                if (_dal.CheckPhoneExists(khachHang.SDT, khachHang.MaKH))
                    return (false, "Số điện thoại đã được sử dụng bởi khách hàng khác.");

                // Update customer
                bool result = _dal.Update(khachHang);
                return result
                    ? (true, "Cập nhật khách hàng thành công!")
                    : (false, "Không thể cập nhật khách hàng. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật khách hàng: {ex.Message}");
            }
        }

        public (bool success, string message) Delete(string maKH)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maKH))
                    return (false, "Mã khách hàng không hợp lệ.");

                if (!_dal.CheckExists(maKH))
                    return (false, "Khách hàng không tồn tại.");

                bool result = _dal.Delete(maKH);
                return result
                    ? (true, "Xóa khách hàng thành công!")
                    : (false, "Không thể xóa khách hàng. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa khách hàng: {ex.Message}");
            }
        }

        public KhachHangDto? GetById(string maKH)
        {
            try
            {
                return _dal.GetById(maKH);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin khách hàng: {ex.Message}");
            }
        }

        private (bool isValid, string message) ValidateKhachHang(KhachHangDto khachHang)
        {
            if (string.IsNullOrWhiteSpace(khachHang.HoTen))
                return (false, "Họ tên khách hàng không được để trống.");

            if (khachHang.HoTen.Length > 100)
                return (false, "Họ tên khách hàng không được quá 100 ký tự.");

            if (string.IsNullOrWhiteSpace(khachHang.SDT))
                return (false, "Số điện thoại không được để trống.");

            if (!IsValidPhoneNumber(khachHang.SDT))
                return (false, "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại 10-11 chữ số.");

            if (!string.IsNullOrEmpty(khachHang.Email) && !IsValidEmail(khachHang.Email))
                return (false, "Email không hợp lệ.");

            if (!string.IsNullOrEmpty(khachHang.DChi) && khachHang.DChi.Length > 200)
                return (false, "Địa chỉ không được quá 200 ký tự.");

            if (!string.IsNullOrEmpty(khachHang.GioiTinh) &&
                !new[] { "Nam", "Nữ", "Khác" }.Contains(khachHang.GioiTinh))
                return (false, "Giới tính không hợp lệ.");

            return (true, string.Empty);
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces and special characters
            phone = Regex.Replace(phone, @"[^\d]", "");

            // Check if phone number has 10-11 digits and starts with valid prefixes
            return phone.Length >= 10 && phone.Length <= 11 &&
                   (phone.StartsWith("03") || phone.StartsWith("05") || phone.StartsWith("07") ||
                    phone.StartsWith("08") || phone.StartsWith("09") || phone.StartsWith("84"));
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateNewId()
        {
            // Format: KH001, KH002, ...
            var dt = _dal.GetAll();
            int max = 0;

            foreach (DataRow r in dt.Rows)
            {
                var s = r["MAKH"]?.ToString();
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                if (!s.StartsWith("KH", StringComparison.OrdinalIgnoreCase))
                    continue;

                var numPart = s.Substring(2);
                if (int.TryParse(numPart, out int n))
                    max = Math.Max(max, n);
            }

            return $"KH{(max + 1):000}";
        }
    }
}
