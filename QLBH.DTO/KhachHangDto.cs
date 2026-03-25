using System;

namespace QLBH.DTO
{
    public class KhachHangDto
    {
        public string MaKH { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? DChi { get; set; }
        public string SDT { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}