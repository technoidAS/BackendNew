# Backend Retail Ordering Website

A comprehensive ASP.NET Core Web API for a retail ordering application with user authentication, product management, order processing, and address management.

## 🚀 Technologies Used

- **ASP.NET Core 8.0** - Web API Framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database
- **JWT (JSON Web Tokens)** - Authentication & Authorization
- **Swagger/OpenAPI** - API Documentation

## 📋 Features

- **User Management**
  - User registration and login
  - JWT-based authentication
  - Profile management (view, update, delete)
  - Admin role support

- **Address Management**
  - Add, view, update, and delete user addresses
  - One address per user

- **Product Management**
  - Add new products
  - View all products
  - View product by ID
  - Product inventory tracking

- **Order Management**
  - Create orders with multiple items
  - View user orders
  - View specific order details
  - Update order status
  - Cancel pending orders
  - Automatic inventory management

## 🛠️ Setup Instructions

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (Express or higher)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/technoidAS/Backend.git
   cd Backend
   ```

2. **Update the connection string**
   
   Edit `appsettings.json` and update the connection string to match your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "Default": "Server=YOUR_SERVER_NAME;Database=RetailProject;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Update JWT Secret**
   
   Update the JWT secret in `appsettings.json` for production:
   ```json
   "Jwt": {
     "Secret": "YOUR_SECRET_KEY_HERE"
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
   Navigate to: `https://localhost:5001/swagger` (or the port specified in your launch settings)

## 📚 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/user/register` | Register new user | No |
| POST | `/api/user/login` | Login user | No |

### User Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user/profile` | Get current user profile | Yes |
| GET | `/api/user/{id}` | Get user by ID | No |
| GET | `/api/user` | Get all users | No |
| PUT | `/api/user/profile` | Update current user profile | Yes |
| DELETE | `/api/user/profile` | Delete current user | Yes |

### Address Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/address` | Add address for current user | Yes |
| GET | `/api/address` | Get current user's address | Yes |
| GET | `/api/address/{id}` | Get address by ID | No |
| PUT | `/api/address` | Update current user's address | Yes |
| DELETE | `/api/address` | Delete current user's address | Yes |

### Product Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/product` | Add new product | No* |
| GET | `/api/product` | Get all products | No |
| GET | `/api/product/{id}` | Get product by ID | No |

*Note: Consider adding authorization for admin-only product creation

### Order Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/order` | Create new order | Yes |
| GET | `/api/order` | Get all orders for current user | Yes |
| GET | `/api/order/{id}` | Get specific order by ID | Yes |
| PUT | `/api/order/{id}/status` | Update order status | Yes |
| DELETE | `/api/order/{id}` | Cancel order (pending only) | Yes |

## 🗂️ Project Structure

```
Backend/
├── Controllers/
│   ├── AddressController.cs    # Address management endpoints
│   ├── OrderController.cs      # Order management endpoints
│   ├── ProductController.cs    # Product management endpoints
│   └── UserController.cs       # User & auth endpoints
├── Models/
│   ├── AddressModal.cs         # Address entity
│   ├── CartModal.cs            # Cart entity
│   ├── CartItemModal.cs        # Cart item entity
│   ├── OrderModal.cs           # Order entity
│   ├── OrderItemModal.cs       # Order item entity
│   ├── ProductModal.cs         # Product entity
│   └── UserModal.cs            # User entity
├── DTOs/
│   ├── AddressDto/             # Address data transfer objects
│   ├── OrderDto/               # Order data transfer objects
│   ├── ProductDto/             # Product data transfer objects
│   └── UserDto/                # User data transfer objects
├── Config/
│   └── BackendDbContext.cs     # Database context
├── Services/
│   └── TokenService.cs         # JWT token generation
├── Middlewares/
│   └── JwtMiddleware.cs        # JWT authentication middleware
├── appsettings.json            # Configuration file
└── Program.cs                  # Application entry point
```

## 🔐 Authentication

This API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

1. **Register or Login** to receive a JWT token
2. **Include the token** in the Authorization header:
   ```
   Authorization: Bearer YOUR_JWT_TOKEN
   ```

The JWT token contains:
- User ID
- Email
- Username
- Admin status

## 📦 Database Models

### User
- UserId, UserName, Email, PasswordHash, MobileNo, IsAdmin, CreatedAt

### Address
- AddressId, UserId, AddressDetail, City, State, Pincode

### Product
- ProductId, ProductName, Description, Category, Price, Quantity, ImageUrl, IsAvailable, CreatedAt

### Order
- OrderId, UserId, AddressId, OrderDate, TotalAmount, Status, IsPaid

### OrderItem
- OrderItemId, OrderId, ProductId, Quantity, PriceAtOrder

### Cart & CartItem
- Cart management models (implementation pending)

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "YOUR_SQL_SERVER_CONNECTION_STRING"
  },
  "Jwt": {
    "Secret": "YOUR_JWT_SECRET_KEY"
  }
}
```

## 📝 Example Requests

### Register User
```json
POST /api/user/register
{
  "userName": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123",
  "phoneNumber": "1234567890"
}
```

### Create Order
```json
POST /api/order
Authorization: Bearer YOUR_TOKEN
{
  "addressId": 1,
  "orderItems": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
```

### Add Product
```json
POST /api/product
{
  "productName": "Laptop",
  "description": "High-performance laptop",
  "category": "Electronics",
  "price": 999.99,
  "quantity": 10,
  "imageUrl": "https://example.com/laptop.jpg",
  "isAvailable": true
}
```

## 🚦 Running the Project

### Development Mode
```bash
dotnet run
```

### Build for Production
```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## 📄 API Response Format

All endpoints return consistent response formats:

**Success Response:**
```json
{
  "data": { ... }
}
```

**Error Response:**
```json
{
  "message": "Error description",
  "error": "Detailed error message"
}
```

## 🛡️ Security Features

- Password hashing using SHA256
- JWT token-based authentication
- User authorization checks
- Input validation
- SQL injection prevention via Entity Framework

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

## 📧 Contact

For questions or support, please open an issue on the GitHub repository.

## 📜 License

This project is licensed under the MIT License.

---

**Note:** This is a development/learning project. For production use, consider:
- Using a more secure password hashing algorithm (BCrypt/Argon2)
- Adding comprehensive input validation
- Implementing rate limiting
- Adding logging and monitoring
- Implementing proper error handling
- Adding unit and integration tests
- Implementing admin authorization for product/order management
