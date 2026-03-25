namespace QLBH.DTO;

public class PhieuNhapDto
{
    public int MaPN { get; set; }
    public string MaND { get; set; } = string.Empty;
    public string MaNCC { get; set; } = string.Empty;
    public DateTime NgayNhap { get; set; }
    public string? GhiChu { get; set; }
    public decimal TongTien { get; set; }
}
