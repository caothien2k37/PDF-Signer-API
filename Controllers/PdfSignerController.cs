using Microsoft.AspNetCore.Mvc;
using PdfSignerApi.DTOs;
using PdfSignerApi.Services;

namespace PdfSignerApi.Controllers;

/// <summary>
/// Controller xử lý các yêu cầu ký PDF
/// </summary>
[ApiController]
[Route("api/sign-pdf")]
[DisableRequestSizeLimit]
[RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
public class PdfSignerController(IPdfSignerService pdfSignerService) : ControllerBase
{
    /// <summary>
    /// Phương thức cơ sở để xử lý việc ký PDF
    /// </summary>
    /// <typeparam name="T">Loại request (Token hoặc PFX)</typeparam>
    /// <param name="request">Thông tin yêu cầu ký</param>
    /// <param name="signMethod">Phương thức ký tương ứng từ service</param>
    /// <returns>File PDF đã ký hoặc thông báo lỗi</returns>
    private async Task<IActionResult> SignPdfBase<T>(T request, Func<T, Task<byte[]>> signMethod) where T : SignPdfRequest
    {
        try
        {
            var signedPdfBytes = await signMethod(request);
            return File(signedPdfBytes, "application/pdf", "signed_output.pdf");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Lỗi khi ký PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// API ký PDF bằng USB Token
    /// </summary>
    [HttpPost("token")]
    public Task<IActionResult> SignWithToken([FromForm] SignPdfWithTokenRequest request) => 
        SignPdfBase(request, pdfSignerService.SignWithToken);

    /// <summary>
    /// API ký PDF bằng file PFX
    /// </summary>
    [HttpPost("pfx")]
    public Task<IActionResult> SignWithPfx([FromForm] SignPdfWithPfxRequest request) => 
        SignPdfBase(request, pdfSignerService.SignWithPfx);
} 