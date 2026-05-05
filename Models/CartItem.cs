namespace BookStoreWeb.Models
{
    public class CartItem
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => Gia * SoLuong;
    }
}
