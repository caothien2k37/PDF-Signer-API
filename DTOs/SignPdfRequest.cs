using System.ComponentModel.DataAnnotations;

namespace PdfSignerApi.DTOs;

/// <summary>
/// DTO cơ sở cho các yêu cầu ký PDF
/// </summary>
public class SignPdfRequest
{
    /// <summary>
    /// File PDF cần ký
    /// </summary>
    [Required(ErrorMessage = "Vui lòng chọn file PDF cần ký")]
    public required IFormFile PdfFile { get; set; }
    
    /// <summary>
    /// Hình ảnh chữ ký (tùy chọn)
    /// </summary>
    public IFormFile? SignatureImage { get; set; }
    
    /// <summary>
    /// Lý do ký
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập lý do ký")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Lý do ký phải từ 3 đến 200 ký tự")]
    public required string Reason { get; set; }
    
    /// <summary>
    /// Địa điểm ký
    /// </summary>
    public required string Location { get; set; }
    
    /// <summary>
    /// Số trang cần ký (mặc định là trang 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Chiều rộng mặc định của khung chữ ký
    public float? Width { get; set; }
    
    /// <summary>
    /// Chieều cao mặc định của khung chữ ký
    public float? Height { get; set; }
    
    public string? Position { get; set; }
    
    /// <summary>
    /// Tọa độ X góc trái dưới của chữ ký
    /// </summary>
    [Range(0, float.MaxValue, ErrorMessage = "Tọa độ phải là số dương")]
    public float? Llx { get; set; }
    
    /// <summary>
    /// Tọa độ Y góc trái dưới của chữ ký
    /// </summary>
    [Range(0, float.MaxValue, ErrorMessage = "Tọa độ phải là số dương")]
    public float? Lly { get; set; }
    
    /// <summary>
    /// Tọa độ X góc phải trên của chữ ký
    /// </summary>
    [Range(0, float.MaxValue, ErrorMessage = "Tọa độ phải là số dương")]
    public float? Urx { get; set; }
    
    /// <summary>
    /// Tọa độ Y góc phải trên của chữ ký
    /// </summary>
    [Range(0, float.MaxValue, ErrorMessage = "Tọa độ phải là số dương")]
    public float? Ury { get; set; }
}

/// <summary>
/// DTO cho yêu cầu ký bằng USB Token
/// </summary>
public class SignPdfWithTokenRequest : SignPdfRequest
{
    /// <summary>
    /// Số serial của USB Token
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập số serial của USB Token")]
    public required string SerialNumber { get; set; }
}

public class SignPdfWithPfxRequest : SignPdfRequest
{
    /// <summary>
    /// File chứng chỉ PFX
    /// </summary>
    [Required(ErrorMessage = "Vui lòng chọn file chứng chỉ PFX")]
    public required IFormFile PfxFile { get; set; }
    
    /// <summary>
    /// Mật khẩu của file PFX
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu của file PFX")]
    public required string PfxPassword { get; set; }
} 