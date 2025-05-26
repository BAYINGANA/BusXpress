🚍 BusXpress – Bus Management System
BusXpress is a web-based Ticket booking and management system built with ASP.NET Razor Pages. It allows admins to manage buses, routes, schedules, and view ticket bookings, while clients can book and download tickets.

🛠️ Getting Started
Clone the repository:

bash
Copy
Edit
git clone https://github.com/BAYINGANA/BusXpress.git
Set up the database:

Open SQL Server Management Studio (SSMS).

Run the script.sql file located in the project root to create the necessary tables and seed initial data.

Update the connection string:

Open appsettings.json.

Locate the ConnectionStrings section.

Replace the existing connection string with your own SQL Server configuration:

json
Copy
Edit
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=BusManagementDB;Trusted_Connection=True;"
}
Run the application:

Build and run the project in Visual Studio or using dotnet run.
