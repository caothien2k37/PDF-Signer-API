using Microsoft.AspNetCore.Http.Features;
using PdfSignerApi.Services;

var builder = WebApplication.CreateBuilder(args);

{
    var services = builder.Services;
    var config = builder.Configuration;

    // Cấu hình giới hạn kích thước file
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
        serverOptions.ConfigureEndpointDefaults(_ => {});
    });

    services.Configure<FormOptions>(options =>
    {
        // Cho phép upload file dung lượng lớn
        options.MultipartBodyLengthLimit = int.MaxValue;
        options.ValueLengthLimit = int.MaxValue;
    });

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    
    // Đăng ký service xử lý ký PDF
    services.AddScoped<IPdfSignerService, PdfSignerService>();

    // Cấu hình CORS - cho phép truy cập từ mọi nguồn
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition"));
    });
}

var app = builder.Build();

// Cấu hình pipeline xử lý request
{
    // Middleware cho môi trường development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        // Bảo mật cho môi trường production
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // Middleware toàn cục
    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.MapControllers();
}

await app.RunAsync(); 