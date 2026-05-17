using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoomManagement.WebAPI.Features.Bills;
using RoomManagement.WebAPI.Features.Contracts;
using RoomManagement.WebAPI.Features.Dashboard;
using RoomManagement.WebAPI.Features.Rooms;
using RoomManagement.WebAPI.Features.Tenants;
using RoomManagement.WebAPI.Features.Identity;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);       
// Thêm các dịch vụ cần thiết
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.CustomSchemaIds(type => type.FullName.Replace("+", "-")); }); // Cấu hình mặc định, không yêu cầu khóa bảo mật
// convert
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Phân quyền dựa trên Role
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
//Lấy Connection String từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext vào Service Container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<MyRoom.Handler>();
builder.Services.AddScoped<MyContract.Handler>();
builder.Services.AddScoped<MyBill.Handler>();

builder.Services.AddScoped<GetRooms.Handler>();
builder.Services.AddScoped<CreateRoom.Handler>();
builder.Services.AddScoped<DeleteRoom.Handler>();
builder.Services.AddScoped<GetRoomDetail.Handler>();

builder.Services.AddScoped<UploadRoomImages.Handler>();
builder.Services.AddScoped<SetMainRoomImage.Handler>();


builder.Services.AddScoped<RegisterTenant.Handler>();
builder.Services.AddScoped<GetTenants.Handler>();

builder.Services.AddScoped<CreateContract.Handler>();
builder.Services.AddScoped<EndContract.Handler>();
builder.Services.AddScoped<GetContractDetail.Handler>();
builder.Services.AddScoped<GetContracts.Handler>();

builder.Services.AddScoped<GetDashboardSummary.Handler>();

builder.Services.AddScoped<CalculateMonthlyBill.Handler>();
builder.Services.AddScoped<MarkAsPaid.Handler>();
builder.Services.AddScoped<GetBills.Handler>();

builder.Services.AddScoped<Login.Handler>();
builder.Services.AddScoped<RegisterUser.Handler>();

builder.Services.AddScoped<CreateVnPayPayment.Handler>();
builder.Services.AddScoped<VnPayReturn.Handler>();

builder.Services.Configure<VnPayOptions>(
    builder.Configuration.GetSection("VnPay"));

builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL frontend của bạn
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();
// Kích hoạt Swagger cho mọi môi trường để dễ test
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room Management API V1");
    c.RoutePrefix = string.Empty; // Chạy cái là ra giao diện Swagger luôn, không cần gõ thêm /swagger
});
app.UseCors("AllowFrontend"); // ← phải đặt TRƯỚC app.UseAuthentication()
app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép phục vụ file tĩnh (ảnh) từ wwwroot
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
