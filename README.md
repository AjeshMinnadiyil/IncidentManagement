# Incident Management System

An ASP.NET Core MVC web application for managing incident tickets, assigning technicians, and tracking incident resolution.

## Features

- Role-based authentication using ASP.NET Core Identity
- Three user roles: Admin, Employee, and Technician
- Create and manage incident tickets
- Search and filter tickets by status
- Assign tickets to technicians
- Update ticket status and track progress
- Close or reopen incidents with a reason
- Entity Framework Core database migrations

## Technologies Used

- C#
- ASP.NET Core MVC
- .NET 10
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Razor Views, HTML, CSS, and Bootstrap

## Getting Started

1. Clone the repository:

   ```bash
   git clone YOUR_GITHUB_REPOSITORY_URL
   ```

2. Open the project in Visual Studio.
3. Update the `DefaultConnection` connection string in `appsettings.json`.
4. Apply database migrations using the Package Manager Console:

   ```powershell
   Update-Database
   ```

5. Run the application.

## Demo Accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin.demo@example.com` | `AdminDemo@123` |
| Employee | `employee.demo@example.com` | `EmployeeDemo@123` |
| Technician | `technician.demo@example.com` | `TechDemo@123` |

*Demo accounts are created when the development seeding code is enabled and runs successfully.*

**Note:** These credentials are for demonstration purposes only. Do not use them in production.

## Author

**Ajesh Minnadiyil**

GitHub: [AjeshMinnadiyil](https://github.com/AjeshMinnadiyil)
