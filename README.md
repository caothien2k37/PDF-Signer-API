# PDF Signer API
Add thư viện
cd PdfSignerApi
dotnet add package itextsharp --version 5.5.13.3
dotnet add package BouncyCastle --version 1.8.9

API ký số PDF hỗ trợ hai phương thức ký:
1. Ký bằng USB Token
2. Ký bằng file chứng chỉ số PFX

## Tính năng

- Ký PDF với USB Token hoặc file PFX
- Tùy chỉnh vị trí chữ ký
- Thêm hình ảnh chữ ký
- Chọn trang ký trong PDF nhiều trang
- Thêm thông tin người ký, thời gian, lý do và địa điểm
- Hỗ trợ file dung lượng lớn
- API RESTful chuẩn
- Tự động dọn dẹp file tạm
- Xử lý lỗi chi tiết

## Yêu cầu hệ thống

- .NET 8.0 hoặc cao hơn
- USB Token driver (nếu sử dụng USB Token)
- Không có USB Token
  # B1. Tạo private key
  openssl genrsa -out fake-key.pem 2048

  # B2. Tạo chứng thư tự ký (self-signed)
  openssl req -new -x509 -key fake-key.pem -out fake-cert.pem -days 3650 -subj "/CN=Fake Token Tester"

  # B3. Gộp thành 1 file .pfx
  openssl pkcs12 -export -inkey fake-key.pem -in fake-cert.pem -out fake-token.pfx -password pass:123456



## Cấu trúc Project

```
PdfSignerApi/
├── Controllers/
│   └── PdfSignerController.cs
├── Services/
│   ├── IPdfSignerService.cs
│   └── PdfSignerService.cs
├── DTOs/
│   └── SignPdfRequest.cs
├── Program.cs
├── appsettings.json
└── README.md
```

## API Endpoints

### 1. Ký PDF bằng USB Token

```http
POST /api/sign-pdf/token
Content-Type: multipart/form-data

- pdfFile: File PDF cần ký
- signatureImage: Ảnh chữ ký (tùy chọn)
- serialNumber: Số serial của USB Token
- reason: Lý do ký
- location: Địa điểm ký
- pageNumber: Số trang cần ký (mặc định: 1)
- llx, lly, urx, ury: Tọa độ chữ ký (tùy chọn)
```

### 2. Ký PDF bằng file PFX

```http
POST /api/sign-pdf/pfx
Content-Type: multipart/form-data

- pdfFile: File PDF cần ký
- pfxFile: File chứng chỉ số PFX
- pfxPassword: Mật khẩu file PFX
- signatureImage: Ảnh chữ ký (tùy chọn)
- reason: Lý do ký
- location: Địa điểm ký
- pageNumber: Số trang cần ký (mặc định: 1)
- llx, lly, urx, ury: Tọa độ chữ ký (tùy chọn)

- Curl test : 
curl --location 'http://localhost:5000/api/sign-pdf/pfx' \
--header 'Accept: */*' \
--form 'pdfFile=@"/Users/thiencx2k37/Documents/PdfSignerApi/Fake_data/File_PDF_test.pdf"' \
--form 'pfxFile=@"/Users/thiencx2k37/Documents/PdfSignerApi/Fake_data/fake-token.pfx"' \
--form 'pfxPassword="123456"' \
--form 'reason="Test Ký Số"' \
--form 'location="Thiencx"' \
--form 'signatureImage=@"/Users/thiencx2k37/Documents/PdfSignerApi/Fake_data/image.png"' \
--form 'pageNumber="1"'