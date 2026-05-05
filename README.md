# 📚 BookStoreWeb - E-Commerce Bookstore Platform

![ASP.NET Core](https://img.shields.io/badge/-ASP.NET%20Core%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQLite](https://img.shields.io/badge/-SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Bootstrap](https://img.shields.io/badge/-Bootstrap%205-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![Entity Framework](https://img.shields.io/badge/-EF%20Core-512BD4?style=for-the-badge&logo=.net&logoColor=white)

A full-featured e-commerce bookstore built with **ASP.NET Core MVC** and **Entity Framework Core**. Features multiple payment gateways (Momo, PayOS), comprehensive order management, product reviews with nested replies, wishlist functionality, and an admin dashboard with analytics.

## 📺 Video Demo
Experience the complete e-commerce flow and admin features:


https://github.com/user-attachments/assets/51a6f8fb-a03e-48e1-b16b-e87b60debc66

> [!TIP]
> **Coming Soon** - Demo video showcasing payment integration, order management, and real-time cart updates.

---

## ✨ Key Features

* **E-Commerce Shopping System:** Full-featured product catalog with search, filtering, and pagination. Session-based shopping cart with persistent wishlist (database storage for authenticated users).
* **Multi-Payment Gateway Integration:** Support for COD (Cash on Delivery), Momo e-wallet (QR payment), and PayOS payment links with automatic callback handling and order status synchronization.
* **Product Review & Rating System:** Star ratings (1-5) with comments, nested reply threads for reviews, visual thread-line connections, and purchased verification badges.
* **Admin Dashboard & Analytics:** Real-time statistics with Chart.js visualizations - revenue tracking, order status distribution, top-selling books, and user management interface.
* **Product Comparison:** Compare up to 4 books side-by-side with detailed specifications, price comparison, and add-to-cart functionality from comparison view.
* **Recently Viewed & Recommendations:** Session-based browsing history (max 10 items) and intelligent recommendations based on category/author matching.
* **Role-Based Access Control:** Cookie authentication with Claims-based authorization, distinguishing between Admin (dashboard access) and Customer roles.

---

## ⚙️ Architecture & How It Works

**1. MVC Architecture Pattern**
The application follows the Model-View-Controller design pattern for clean separation of concerns:
- **Models:** Entity classes (SanPham, NguoiDung, DonHang, DanhGia) with Data Annotations for validation
- **Views:** Razor syntax templates with Bootstrap 5 for responsive UI
- **Controllers:** Handle HTTP requests, business logic, and coordinate between Models and Views

**2. Database Layer with EF Core**
Implements Code-First approach with Entity Framework Core:
- **Database:** SQLite for lightweight, file-based storage (ideal for development and small deployments)
- **Relationships:** 7 main entities with One-to-Many and Many-to-Many relationships (resolved via junction tables)
- **Migrations:** Automated schema versioning for database evolution
- **Query Optimization:** Eager Loading with Include/ThenInclude for efficient data retrieval

**3. Session-Based State Management**
- **Shopping Cart:** HttpContext.Session stores CartItem list (JSON serialized) for guest users
- **Product Comparison:** Session storage for up to 4 compared products
- **Recently Viewed:** Session-persisted list of last 10 viewed books with timestamp ordering

**4. Payment Gateway Integration**
- **Momo SDK:** RESTful API integration with HMAC-SHA256 signature verification, QR code generation, and IPN (Instant Payment Notification) handling
- **PayOS API:** Modern payment link generation with webhook callback support
- **Transaction Safety:** Unique order IDs with timestamps, automatic status synchronization via callbacks

**5. Security Implementation**
- **Authentication:** Cookie-based auth with Claims (User ID, Name, Email, Role)
- **Password Security:** SHA-256 hashing with salt (upgradeable to bcrypt)
- **Authorization:** Role-based access with [Authorize] attributes and User.IsInRole() checks
- **CSRF Protection:** Anti-forgery tokens on all state-changing forms

---

## 🛠 Tech Stack

| Category | Technologies |
|----------|-------------|
| **Backend** | ASP.NET Core MVC 8.0, C# 12 |
| **Database** | SQLite, Entity Framework Core 8.0 |
| **ORM** | Entity Framework Core with LINQ |
| **Frontend** | Bootstrap 5, Razor Views, JavaScript (Fetch API) |
| **Payment** | Momo SDK (test environment), PayOS API |
| **Authentication** | ASP.NET Core Identity (Cookie-based) |
| **Charts** | Chart.js for admin dashboard |
| **Icons** | Bootstrap Icons |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, VS Code, or any IDE with C# support
- (Optional) Momo Test Account for payment testing
- (Optional) PayOS Account for payment link generation

### Installation

**1. Clone the repository:**
```bash
git clone https://github.com/thanhnguyen221/BookStoreWeb.git
cd BookStoreWeb
```

**2. Create environment file (REQUIRED for Payment):**
Create a `.env` file in the root directory:
```env
# Momo Payment (Test Environment)
MOMO_PARTNER_CODE=
MOMO_ACCESS_KEY=
MOMO_SECRET_KEY=
MOMO_API_URL=https://test-payment.momo.vn/v2/gateway/api/create

# PayOS Payment (Production)
PAYOS_CLIENT_ID=your_client_id_here
PAYOS_API_KEY=your_api_key_here
PAYOS_CHECKSUM_KEY=your_checksum_key_here

# URLs (update when deploying)
RETURN_URL=http://localhost:5282/Cart/PaymentCallBack
NOTIFY_URL=http://localhost:5282/Cart/PaymentNotify
PAYOS_RETURN_URL=http://localhost:5282/Cart/PayOSCallback
```

> ⚠️ **Security Note:** The `.env` file is already in `.gitignore` and will NOT be committed to GitHub.

**3. Configure appsettings.json:**
Ensure `appsettings.json` contains your database connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=BookStoreWeb.db"
  }
}
```

**4. Run database migrations (if needed):**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**5. Run the application:**
```bash
dotnet run
```

**6. Access the application:**
Open your browser and navigate to:
```
http://localhost:5282
```

---

## 🔐 Default Accounts

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@bookstore.com | admin123 |
| **Customer** | user@bookstore.com | user123 |

> Or create a new account via the Register page.

---

## 📱 Core Features Walkthrough

### 🛒 Shopping Experience
- Browse products with category filtering and search
- Add to cart with quantity selection
- Persistent cart across sessions (for logged-in users)
- Wishlist management (database storage)
- Compare up to 4 books side-by-side

### 💳 Payment Options
- **COD (Cash on Delivery):** Simple cash payment on delivery
- **Momo E-Wallet:** Scan QR code with Momo app (test phone: 0917003000, OTP: 000000)
- **PayOS:** Secure payment link sent to customer
- Real-time order status updates via webhooks

### ⭐ Reviews & Ratings
- Rate books 1-5 stars
- Write detailed reviews
- Nested reply system (admins can respond to reviews)
- "Verified Purchase" badge for reviewers who bought the book
- Thread-line visualization for reply hierarchy

### 📊 Admin Dashboard
- Revenue statistics with monthly charts
- Order status distribution (pie chart)
- Top 5 best-selling books
- Recent orders with quick actions
- Product and user management interfaces

### 🔍 Search & Discovery
- Full-text search across book titles, authors, publishers, categories
- Category filter buttons
- Recently viewed books carousel
- "Related books" recommendations (same category/author)

---

## 📊 Project Structure

```
BookStoreWeb/
├── Controllers/              # MVC Controllers
│   ├── HomeController.cs     # Product listing, search, details
│   ├── CartController.cs     # Shopping cart, checkout, payments
│   ├── TaiKhoanController.cs   # Authentication (login, register)
│   ├── SanPhamsController.cs   # Admin product CRUD
│   ├── DonHangController.cs    # Order history
│   ├── AdminDashboardController.cs # Admin analytics API
│   ├── DanhGiaController.cs    # Reviews API
│   ├── WishlistController.cs   # Wishlist API
│   └── SoSanhController.cs     # Product comparison API
├── Models/                   # Entity Models
│   ├── SanPham.cs            # Book/Product entity
│   ├── NguoiDung.cs          # User entity
│   ├── DonHang.cs            # Order entity
│   ├── ChiTietDonHang.cs     # Order details
│   ├── DanhGia.cs            # Review entity
│   ├── PhanHoiBinhLuan.cs    # Comment/Reply entity
│   └── SanPhamYeuThich.cs    # Wishlist entity
├── Views/                    # Razor Views
│   ├── Home/                 # Homepage, product details
│   ├── Cart/                 # Shopping cart, checkout
│   ├── SanPhams/             # Admin product management
│   └── AdminDashboard/       # Admin dashboard
├── Services/                 # Business Logic
│   ├── Momo/MomoService.cs   # Momo payment integration
│   ├── PayOS/PayOSService.cs # PayOS payment integration
│   └── Vnpay/                # VNPay integration (optional)
├── Data/                     # Database Context
│   └── ApplicationDbContext.cs
├── wwwroot/                  # Static files
│   ├── images/               # Product images
│   ├── css/                  # Custom styles
│   └── js/                   # JavaScript files
├── .env                      # Environment variables (gitignored)
├── appsettings.json          # App configuration
├── Program.cs                # App entry point
└── README.md                 # This file
```

---

## 🔐 Security Features

- **Password Hashing:** SHA-256 with salt (stored in database)
- **Authentication:** Cookie-based with secure flags
- **Authorization:** Role-based access control (Admin/Customer)
- **Session Management:** HttpOnly cookies, timeout after 20 minutes
- **CSRF Protection:** Anti-forgery tokens on all forms
- **Input Validation:** Data Annotations on all Models
- **SQL Injection Prevention:** EF Core parameterized queries

---

## 🛠 Useful Commands

```bash
# Run application
dotnet run

# Build for production
dotnet build -c Release

# Publish application
dotnet publish -c Release -o ./publish

# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Watch for changes (auto-reload)
dotnet watch run
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| **Port already in use** | Change port in `launchSettings.json` or `Program.cs` |
| **Database locked** | Delete `BookStoreWeb.db-journal` file and restart |
| **Payment callback failed** | Ensure `ReturnUrl` in `.env` matches your domain |
| **Images not displaying** | Check permissions on `wwwroot/images/` directory |
| **Migrations fail** | Run `dotnet ef database drop` then `dotnet ef database update` |

---

## 📝 API Endpoints (Key)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/danhgia/get/{productId}` | GET | Get reviews for product |
| `/api/danhgia/create` | POST | Submit new review |
| `/api/wishlist/add` | POST | Add to wishlist |
| `/api/sosanh/add` | POST | Add to comparison |
| `/Cart/AddToCart` | POST | Add item to cart (AJAX) |
| `/AdminDashboard/GetDashboardStats` | GET | Admin statistics API |

---

## 🦾 Academic Context

This project was developed as a Web Technology Course Project (Dự án kết thúc môn Công nghệ Web), focusing on building a practical E-commerce website. The project explores:
- E-commerce architecture with ASP.NET Core MVC
- Payment gateway integration in .NET applications
- Database design principles for inventory and order management
- Real-world authentication and authorization patterns
- Frontend-backend integration with Razor and AJAX

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## 👨‍💻 Author

**Thanh Nguyen**
- **Role:** Full-Stack Developer / Project Lead
- **Responsibilities:** Backend Architecture (ASP.NET Core MVC), Database Design (EF Core + SQLite), Payment Integration (Momo & PayOS), Admin Dashboard, Security Implementation
- **GitHub:** [@thanhnguyen221](https://github.com/thanhnguyen221)
- **LinkedIn:** [Thanh Nguyen](https://linkedin.com/in/thanh-nguyen)
- **Email:** thanhfff55@gmail.com

---

<p align="center">
  Built with 📖 and ☕ | BookStoreWeb 📚
</p>
