namespace QLBH.DTO;

public class SanPhamChiTietDto
{
    public string MaCT { get; set; } = string.Empty;
    public string MaSP { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Mau { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int TonKho { get; set; }
    public string? BarCode { get; set; }
    public bool TrangThai { get; set; } = true;
}
   