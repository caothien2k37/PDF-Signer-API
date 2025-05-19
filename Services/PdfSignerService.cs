using System.Security.Cryptography.X509Certificates;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PdfSignerApi.DTOs;

namespace PdfSignerApi.Services;

/// <summary>
/// Service xử lý việc ký PDF với hai phương thức: USB Token và PFX
/// </summary>
public class PdfSignerService : IPdfSignerService
{
    /// <summary>
    /// Ký PDF bằng USB Token
    /// </summary>
    public async Task<byte[]> SignWithToken(SignPdfWithTokenRequest request)
    {
        if (request.PdfFile == null)
            throw new ArgumentException("Vui lòng chọn file PDF cần ký");

        // Tạo các file tạm để xử lý
        var inputPdfPath = Path.GetTempFileName();
        var signatureImagePath = request.SignatureImage != null ? Path.GetTempFileName() : null;
        var outputPdfPath = Path.ChangeExtension(Path.GetTempFileName(), ".pdf");

        try
        {
            // Lưu file PDF vào thư mục tạm
            await using (var stream = File.Create(inputPdfPath))
            {
                await request.PdfFile.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            // Lưu ảnh chữ ký nếu có
            if (request.SignatureImage != null)
            {
                if (signatureImagePath != null)
                    await using (var stream = File.Create(signatureImagePath))
                    {
                        await request.SignatureImage.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }
            }

            // Kiểm tra file PDF có tồn tại và có nội dung
            if (new FileInfo(inputPdfPath).Length == 0)
                throw new ArgumentException("File PDF trống");

            // Lấy chứng chỉ từ USB Token
            X509Certificate2 cert;
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                
                // Tìm chứng chỉ theo số serial
                var certs = store.Certificates.Find(X509FindType.FindBySerialNumber, request.SerialNumber, true);
                if (certs.Count == 0)
                    throw new ArgumentException("Không tìm thấy chứng chỉ trong USB Token. Vui lòng kiểm tra lại.");
                
                cert = certs[0];
                if (!cert.HasPrivateKey)
                    throw new ArgumentException("Chứng chỉ không có private key.");
            }

            return await SignPdf(cert, inputPdfPath, outputPdfPath, signatureImagePath, request);
        }
        finally
        {
            // Dọn dẹp file tạm
            if (File.Exists(inputPdfPath)) File.Delete(inputPdfPath);
            if (File.Exists(signatureImagePath)) File.Delete(signatureImagePath);
            if (File.Exists(outputPdfPath)) File.Delete(outputPdfPath);
        }
    }

    /// <summary>
    /// Ký PDF bằng file chứng chỉ PFX
    /// </summary>
    public async Task<byte[]> SignWithPfx(SignPdfWithPfxRequest request)
    {
        if (request.PdfFile == null || request.PfxFile == null)
            throw new ArgumentException("Vui lòng chọn đầy đủ file PDF và file chứng chỉ PFX");

        // Tạo các file tạm để xử lý
        var inputPdfPath = Path.GetTempFileName();
        var pfxPath = Path.GetTempFileName();
        var signatureImagePath = request.SignatureImage != null ? Path.GetTempFileName() : null;
        var outputPdfPath = Path.ChangeExtension(Path.GetTempFileName(), ".pdf");

        try
        {
            // Lưu các file vào thư mục tạm
            await using (var stream = File.Create(inputPdfPath))
            {
                await request.PdfFile.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            await using (var stream = File.Create(pfxPath))
            {
                await request.PfxFile.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            if (request.SignatureImage != null)
            {
                if (signatureImagePath != null)
                    await using (var stream = File.Create(signatureImagePath))
                    {
                        await request.SignatureImage.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }
            }

            // Kiểm tra các file
            if (new FileInfo(inputPdfPath).Length == 0)
                throw new ArgumentException("File PDF trống");
            if (new FileInfo(pfxPath).Length == 0)
                throw new ArgumentException("File PFX trống");

            // Tải chứng chỉ từ file PFX
            var cert = new X509Certificate2(pfxPath, request.PfxPassword, X509KeyStorageFlags.Exportable);

            return await SignPdf(cert, inputPdfPath, outputPdfPath, signatureImagePath, request);
        }
        finally
        {
            // Dọn dẹp file tạm
            if (File.Exists(inputPdfPath)) File.Delete(inputPdfPath);
            if (File.Exists(pfxPath)) File.Delete(pfxPath);
            if (File.Exists(signatureImagePath)) File.Delete(signatureImagePath);
            if (File.Exists(outputPdfPath)) File.Delete(outputPdfPath);
        }
    }

    /// <summary>
    /// Phương thức chung để ký PDF với chứng chỉ
    /// </summary>
    /// <param name="cert">Chứng chỉ số để ký</param>
    /// <param name="inputPdfPath">Đường dẫn file PDF gốc</param>
    /// <param name="outputPdfPath">Đường dẫn file PDF sau khi ký</param>
    /// <param name="signatureImagePath">Đường dẫn file ảnh chữ ký (nếu có)</param>
    /// <param name="request">Thông tin yêu cầu ký</param>
    /// <returns>Mảng byte của file PDF đã ký</returns>
    private async Task<byte[]> SignPdf(X509Certificate2 cert, string inputPdfPath, string outputPdfPath, 
        string? signatureImagePath, SignPdfRequest request)
    {
        // Đọc file PDF
        var reader = new PdfReader(inputPdfPath);
        
        // Kiểm tra số trang hợp lệ
        var totalPages = reader.NumberOfPages;
        if (request.PageNumber < 1 || request.PageNumber > totalPages)
            throw new ArgumentException($"Số trang không hợp lệ. PDF có {totalPages} trang. Vui lòng chọn trang từ 1 đến {totalPages}.");

        await using (var os = new FileStream(outputPdfPath, FileMode.Create))
        {
            var stamper = PdfStamper.CreateSignature(reader, os, '\0');
            var appearance = stamper.SignatureAppearance;

            // Tính toán kích thước và vị trí chữ ký
            var pageSize = reader.GetPageSize(request.PageNumber);

            // Xác định vị trí chữ ký
            /*Llx: Toạ độ X của góc trái dưới khung chữ ký trên trang PDF.
               Lly: Toạ độ Y của góc trái dưới khung chữ ký.
               Urx: Toạ độ X của góc phải trên khung chữ ký.
               Ury: Toạ độ Y của góc phải trên khung chữ ký.*/
            if (request is { Llx: not null, Lly: not null, Urx: not null, Ury: not null})
            {
                var signatureRect = new Rectangle(request.Llx.Value, request.Lly.Value, request.Urx.Value, request.Ury.Value);

                // Cấu hình hiển thị chữ ký
                appearance.Reason = request.Reason;
                appearance.Location = request.Location;
                appearance.Layer2Text = $"Người ký: {cert.SubjectName.Name}\n" +
                                        $"Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                                        $"Lý do: {request.Reason}\n" +
                                        $"Địa điểm: {request.Location}";

                // Xử lý ảnh chữ ký nếu có
                if (signatureImagePath != null)
                {
                    var image = Image.GetInstance(signatureImagePath);
                    var sigWidth = signatureRect.Width;
                    var sigHeight = signatureRect.Height;
                    image.ScaleToFit(sigWidth - 10, sigHeight - 30);
                    image.Alignment = Element.ALIGN_CENTER;

                    appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
                    appearance.SignatureGraphic = image;
                }
                else
                {
                    appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.NAME_AND_DESCRIPTION;
                }

                appearance.SetVisibleSignature(signatureRect, request.PageNumber, $"sig_page_{request.PageNumber}");
            }
            else
            {
                throw new Exception($"Vui lòng kiem tra lại các thông số vị trí chữ ký: Llx, Lly, Urx, Ury");
            }

            try
            {
                // Chuyển đổi RSA key sang định dạng BouncyCastle
                var rsa = cert.GetRSAPrivateKey();
                if (rsa != null)
                {
                    var rsaParams = rsa.ExportParameters(true);
                    var bcPrivateKey = new RsaPrivateCrtKeyParameters(
                        new BigInteger(1, rsaParams.Modulus),
                        new BigInteger(1, rsaParams.Exponent),
                        new BigInteger(1, rsaParams.D),
                        new BigInteger(1, rsaParams.P),
                        new BigInteger(1, rsaParams.Q),
                        new BigInteger(1, rsaParams.DP),
                        new BigInteger(1, rsaParams.DQ),
                        new BigInteger(1, rsaParams.InverseQ)
                    );

                    // Thực hiện ký PDF
                    var x509Cert = DotNetUtilities.FromX509Certificate(cert);
                    var chain = new[] { x509Cert };

                    IExternalSignature es = new PrivateKeySignature(bcPrivateKey, "SHA-256");
                    MakeSignature.SignDetached(appearance, es, chain, null, null, null, 0, CryptoStandard.CADES);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi trong quá trình ký: {ex.Message}");
            }
        }

        return await File.ReadAllBytesAsync(outputPdfPath);
    }
} 