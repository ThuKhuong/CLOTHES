namespace QLBH.DTO;

public class SanPhamDto
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? MaLoai { get; set; }
    public string? MoTa { get; set; }
    public string? HinhSP { get; set; }
    public bool TrangThai { get; set; } = true;
}
    