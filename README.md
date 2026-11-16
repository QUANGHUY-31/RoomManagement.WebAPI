# 🏠 RoomManagement WebAPI

Hệ thống quản lý phòng trọ được xây dựng bằng **ASP.NET Core (.NET 10)** theo kiến trúc Vertical Slice Architecture, cung cấp RESTful API đầy đủ cho việc quản lý phòng, hợp đồng, hóa đơn và thanh toán trực tuyến qua VnPay.

---

## 📋 Tính năng chính

- **Quản lý phòng** — Thêm, sửa, xóa phòng; upload ảnh phòng; lọc theo trạng thái
- **Quản lý khách thuê (Tenant)** — Đăng ký, tra cứu thông tin khách thuê
- **Quản lý hợp đồng** — Tạo hợp đồng, chấm dứt hợp đồng, xem chi tiết
- **Quản lý hóa đơn** — Tính hóa đơn hàng tháng (tiền phòng, điện, nước, dịch vụ), đánh dấu đã thanh toán
- **Thanh toán VnPay** — Tích hợp cổng thanh toán VnPay Sandbox
- **Xác thực & Phân quyền** — JWT Bearer, phân quyền theo Role (Admin / Staff)
- **Dashboard** — Thống kê tổng quan cho Admin
- **Swagger UI** — Giao diện kiểm thử API tích hợp sẵn

---

## 🛠️ Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core 10 + SQL Server |
| Xác thực | JWT Bearer (BCrypt.Net-Next) |
| Tài liệu API | Swagger / Swashbuckle |
| Thanh toán | VnPay |
| Kiến trúc | Vertical Slice (Features-based) |
| Container | Docker (Windows Nano Server) |

---

## 📁 Cấu trúc thư mục

```
RoomManagement.WebAPI/
├── Common/
│   └── Enums/              # BillStatus, ContractStatus, RoomStatus, UserRole
├── Controllers/            # API Controllers
│   ├── BillsController
│   ├── ContractsController
│   ├── DashboardController
│   ├── IdentityController
│   ├── PaymentController
│   ├── RoomsController
│   └── TenantsController
├── Domain/
│   └── Entities/           # Room, Tenant, Contract, Bill, User, RoomImage, ContractTenant
├── Features/               # Vertical Slice — mỗi feature là 1 file độc lập
│   ├── Bills/
│   ├── Contracts/
│   ├── Dashboard/
│   ├── Identity/
│   ├── Payments/
│   ├── Rooms/
│   └── Tenants/
├── Infrastructure/
│   ├── AppDbContext.cs
│   └── Payments/VnPay/
├── Migrations/
├── wwwroot/images/rooms/   # Ảnh phòng được lưu tại đây
├── appsettings.json
├── Dockerfile
└── Program.cs
```

---

## ⚙️ Cài đặt & Chạy dự án

### Yêu cầu

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (hoặc SQL Server Express)
- (Tuỳ chọn) Docker Desktop

### 1. Clone repository

```bash
git clone https://github.com/<your-username>/RoomManagement.WebAPI.git
cd RoomManagement.WebAPI
```

### 2. Cấu hình `appsettings.json`

Mở file `appsettings.json` và cập nhật các thông số sau:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS01;Database=RoomManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MINIMUM_32_CHARACTERS",
    "Issuer": "RoomManagement",
    "Audience": "RoomManagementUsers"
  },
  "VnPay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:5189/api/payment/vnpay-return"
  }
}
```

> ⚠️ **Lưu ý bảo mật:** Không commit `appsettings.json` chứa key/secret thật lên GitHub. Sử dụng [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) hoặc biến môi trường cho production.

### 2.1 Cấu hình VNPay IPN khi chạy local

VNPay gửi IPN đến URL công khai của API. Khi chạy thử bằng ngrok, cấu hình URL
`https://<ngrok-domain>/api/payment/vnpay-ipn` trong phần cấu hình IPN của
VNPay Sandbox. IPN là server-to-server callback; không thay thế `ReturnUrl`,
đây là URL đưa trình duyệt người dùng quay lại ứng dụng.

API chỉ xác nhận hóa đơn từ IPN hợp lệ. Sau khi xác nhận thành công, người thuê
có thể tải lại danh sách hóa đơn; Admin cũng thấy trạng thái đã cập nhật trong
danh sách hóa đơn. Khi Admin xác nhận tiền mặt, giao dịch VNPay đang chờ chưa
đủ 15 phút sẽ chặn thao tác; các giao dịch Pending đã quá 15 phút được chuyển
sang `Expired` trước khi hóa đơn được xác nhận thanh toán tiền mặt.

### 3. Chạy Migration và khởi động

```bash
# Restore packages
dotnet restore

# Áp dụng migration vào database
dotnet ef database update

# Chạy ứng dụng
dotnet run
```

Ứng dụng sẽ khởi động tại `https://localhost:5189`. Swagger UI có thể truy cập tại địa chỉ gốc `https://localhost:5189/`.

---

## 🔐 Xác thực

API sử dụng **JWT Bearer Token**.

1. Gọi `POST /api/auth/register` để tạo tài khoản
2. Gọi `POST /api/auth/login` để nhận token
3. Đính kèm token vào header các request tiếp theo:

```
Authorization: Bearer <your_token>
```

### Phân quyền

| Role | Quyền hạn |
|---|---|
| `Admin` | Toàn quyền quản lý phòng, hợp đồng, hóa đơn, dashboard |
| `Staff` | Xem thông tin phòng, hợp đồng, hóa đơn của chính mình |

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Đăng ký tài khoản | ❌ |
| POST | `/api/auth/login` | Đăng nhập, nhận JWT | ❌ |

### Rooms
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| GET | `/api/rooms` | Danh sách phòng (lọc theo status) | Admin |
| POST | `/api/rooms` | Tạo phòng mới | Admin |
| GET | `/api/rooms/{id}` | Chi tiết phòng | Admin |
| DELETE | `/api/rooms/{id}` | Xóa phòng | Admin |
| POST | `/api/rooms/images/{roomId}` | Upload ảnh phòng | Admin |
| PUT | `/api/rooms/images/{imageId}/set-main` | Đặt ảnh đại diện | Admin |
| GET | `/api/rooms/my-room` | Xem phòng của bản thân | User |

### Tenants
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| GET | `/api/tenants` | Danh sách khách thuê | Admin |
| POST | `/api/tenants` | Đăng ký khách thuê mới | Admin |

### Contracts
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| GET | `/api/contracts` | Danh sách hợp đồng | Admin |
| POST | `/api/contracts` | Tạo hợp đồng mới | Admin |
| GET | `/api/contracts/{id}` | Chi tiết hợp đồng | Admin |
| PUT | `/api/contracts/end` | Chấm dứt hợp đồng | Admin |
| GET | `/api/contracts/my-contract` | Xem hợp đồng của bản thân | User |

### Bills
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| GET | `/api/bills` | Danh sách hóa đơn | Admin |
| POST | `/api/bills/calculate` | Tính hóa đơn tháng | Admin |
| PUT | `/api/bills/pay` | Đánh dấu đã thanh toán | Admin |
| GET | `/api/bills/my-bills` | Xem hóa đơn của bản thân | User |

### Payment (VnPay)
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| POST | `/api/payment/pay` | Tạo URL thanh toán VnPay | ❌ |
| GET | `/api/payment/vnpay-return` | Callback từ VnPay | ❌ |

### Dashboard
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| GET | `/api/dashboard/summary` | Thống kê tổng quan | Admin |

---

## 🗄️ Mô hình dữ liệu

```
User ──────────── Tenant (1-1)
                      │
              ContractTenant (nhiều-nhiều)
                      │
                  Contract ──── Room
                      │            └── RoomImage
                   Bill
```

- **Room** — Thông tin phòng (số phòng, giá, trạng thái, sức chứa tối đa)
- **Tenant** — Thông tin khách thuê (tên, CCCD, SĐT)
- **Contract** — Hợp đồng thuê (liên kết Room ↔ Tenant, ngày bắt đầu/kết thúc)
- **ContractTenant** — Bảng trung gian, nhiều người có thể ở cùng 1 hợp đồng
- **Bill** — Hóa đơn tháng (tiền phòng, điện, nước, dịch vụ)
- **User** — Tài khoản đăng nhập (Admin / Staff)

---

## 🤝 Đóng góp

1. Fork repository
2. Tạo branch mới: `git checkout -b feature/ten-tinh-nang`
3. Commit thay đổi: `git commit -m "feat: thêm tính năng X"`
4. Push lên branch: `git push origin feature/ten-tinh-nang`
5. Tạo Pull Request

---

## 📄 License

Dự án này được phân phối dưới [MIT License](LICENSE).
