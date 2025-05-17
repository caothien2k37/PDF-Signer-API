using PdfSignerApi.DTOs;

namespace PdfSignerApi.Services;

public interface IPdfSignerService
{
    /// <summary>
    /// Ký PDF sử dụng USB Token
    /// </summary>
    /// <param name="request">Thông tin yêu cầu ký bao gồm file PDF, ảnh chữ ký, số serial token và các thông tin khác</param>
    /// <returns>Mảng byte của file PDF đã được ký</returns>
    /// <exception cref="ArgumentException">Khi có lỗi về dữ liệu đầu vào</exception>
    /// <exception cref="Exception">Khi có lỗi trong quá trình ký</exception>
    Task<byte[]> SignWithToken(SignPdfWithTokenRequest request);

    /// <summary>
    /// Ký PDF sử dụng file chứng chỉ PFX
    /// </summary>
    /// <param name="request">Thông tin yêu cầu ký bao gồm file PDF, ảnh chữ ký, file PFX, mật khẩu và các thông tin khác</param>
    /// <returns>Mảng byte của file PDF đã được ký</returns>
    /// <exception cref="ArgumentException">Khi có lỗi về dữ liệu đầu vào</exception>
    /// <exception cref="Exception">Khi có lỗi trong quá trình ký</exception>
    Task<byte[]> SignWithPfx(SignPdfWithPfxRequest request);
} 