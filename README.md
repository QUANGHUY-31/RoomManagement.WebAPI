# RoomManagement.WebAPI

Backend API cho hệ thống **Quản lý phòng trọ / phòng thuê**, được xây dựng bằng **ASP.NET Core Web API** theo kiến trúc **Vertical Slice Architecture** kết hợp JWT Authentication, Entity Framework Core và tích hợp thanh toán VNPay.

---

## Công nghệ sử dụng

- ASP.NET Core Web API (.NET)
- Entity Framework Core
- SQL Server
- JWT Authentication & Authorization
- Swagger UI
- VNPay Payment Gateway
- Vertical Slice Architecture
- CORS cho Frontend (React/Vite)

---

## Kiến trúc dự án - Vertical Slice

Dự án được tổ chức theo **feature-based structure** thay vì chia theo tầng truyền thống (**Controller → Service → Repository**).

Mỗi tính năng (**Feature**) sẽ chứa đầy đủ logic xử lý riêng của nó như:

- Request / Response
- Handler xử lý business logic
- DTO
- Validation (nếu có)

### Cấu trúc thư mục

```txt
RoomManagement.WebAPI
│── Controllers/
│── Features/
│   ├── Rooms/
│   ├── Contracts/
│   ├── Bills/
│   ├── Tenants/
│   ├── Dashboard/
│   ├── Identity/
│   └── Payments/
│
│── Domain/
│── Infrastructure/
│── Common/
│── Migrations/
│── wwwroot/
