# PDF Signer API

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
- Git

## Cài đặt và chạy local

1. Clone repository:
```bash
git clone <repository-url>
cd PdfSignerApi
```

2. Khôi phục các packages:
```bash
dotnet restore
```

3. Build project:
```bash
dotnet build
```

4. Chạy project:
```bash
dotnet run
```

API sẽ chạy tại:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## Đẩy code lên Git

1. Khởi tạo Git repository (nếu chưa có):
```bash
git init
```

2. Tạo file .gitignore:
```bash
# .NET Core
bin/
obj/
*.user
*.suo
*.vs/
.vscode/

# Temporary files
*.tmp
*.log

# Environment files
*.env
appsettings.*.json
!appsettings.json
!appsettings.Development.json
```

3. Thêm files vào staging:
```bash
git add .
```

4. Commit các thay đổi:
```bash
git commit -m "Initial commit: PDF Signer API"
```

5. Thêm remote repository (nếu chưa có):
```bash
git remote add origin <repository-url>
```

6. Push code lên repository:
```bash
git push -u origin main
```

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
```

## Phát triển

1. Fork repository này
2. Tạo branch mới cho tính năng: `git checkout -b feature/ten-tinh-nang`
3. Commit các thay đổi: `git commit -am 'Thêm tính năng mới'`
4. Push lên branch: `git push origin feature/ten-tinh-nang`
5. Tạo Pull Request

## Lưu ý bảo mật

- KHÔNG commit các file chứng chỉ số (.pfx)
- KHÔNG commit các file cấu hình chứa thông tin nhạy cảm
- Sử dụng biến môi trường cho các thông tin nhạy cảm
- Kiểm tra kỹ file .gitignore trước khi commit

## Hỗ trợ

Nếu bạn gặp vấn đề hoặc có câu hỏi, vui lòng tạo issue trong repository. # PDF-Signer-API
