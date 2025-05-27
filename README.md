# 🚍 BusXpress – Bus Management System
**BusXpress** is a web-based Ticket booking and management system built with ASP.NET Razor Pages. It allows admins to manage buses, routes, schedules, and view ticket bookings, while clients can book and download tickets.

## 🛠️ Getting Started
Clone the repository:
```bash
git clone https://github.com/BAYINGANA/BusXpress.git
```
## Table of Contents

* [Features](#features)
* [Tech Stack](#tech-stack)
* [Architecture](#architecture)
* [Prerequisites](#prerequisites)
* [Getting Started](#getting-started)

  * [Database Setup](#database-setup)
  * [Configuration](#configuration)
  * [Running the App](#running-the-app)
* [Usage](#usage)
* [Real-Time Dashboard](#real-time-dashboard)
* [License](#license)

## Features

* **User Roles**: Client, Driver, Admin with role-based authorization.
* **Authentication**: Cookie-based login with SHA-256 password hashing.
* **Signup Flow**: Driver registration with license number and photo upload; Client/Admin basic signup.
* **Bus & Route Management**: CRUD pages for buses, routes, schedules.
* **Ticketing**: Clients select route ▶ bus ▶ schedule ▶ purchase ticket.

  * Capacity checks, available seats tracking, prevents overbooking.
  * PDF ticket generation via iTextSharp.
  * Email delivery using MailKit/MimeKit.
* **Driver Assignments**: Assign drivers to buses and view personal schedule.
* **Admin Dashboard**:

  * Live metrics: total buses, drivers, today’s schedules, tickets sold, revenue.
  * Real-time charts (Chart.js + SignalR).
* **Admin Ticket Management**: View, filter, delete (cancel) tickets and notify clients.

## Tech Stack

* **Frontend**: Razor Pages, Bootstrap 5, Bootstrap Icons, Chart.js, SignalR client
* **Backend**: ASP.NET Core 7, C#, ADO.NET (SqlClient)
* **Real-time**: SignalR Hub + BackgroundService
* **PDF**: iTextSharp
* **Email**: MailKit / MimeKit (SMTP)
* **Database**: SQL Server

## Architecture

```
Presentation (Razor Pages)
 └─ PageModels (C#)  
Data Access (ADO.NET)
 └─ SQL Server
Real-Time (SignalR + HostedService)
```

## Prerequisites

* [.NET 7 SDK or later](https://dotnet.microsoft.com/download)
* SQL Server instance
* SMTP credentials for email (e.g. Gmail)

## Getting Started

### Database Setup

1. Create a new database, e.g., `BusManagementDB`.
2. Run the provided SQL scripts in `/Database/Scripts` to create tables:

   * Clients, Drivers, Admins
   * Buses, Routes, Schedule, Tickets, DriverAssignments
3. Configure connection string in `appsettings.json`:

   ```sql
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=BusManagementDB;Trusted_Connection=True;"
   }
   ```

### Configuration

In `appsettings.json` set:

```sql
"ConnectionStrings": {
  "connstring": "<your connection string>"
},
"Smtp": {
  "Host": "smtp.example.com",
  "Port": 587,
  "User": "your_email@example.com",
  "Pass": "your_smtp_password"
}
```

### Running the App

```bash
# from solution root
dotnet restore
dotnet build
dotnet run --project BusManagement
```

Navigate to `https://localhost:5001` (or port printed) and log in or sign up.

## Usage

* **Admin**: Navigate to `/Admin/Index` after login.

  * Manage buses, routes, schedules.
  * View tickets, filter by client/schedule, cancel tickets.
  * Real-time dashboard at `/Admin/Index`.

* **Driver**: Navigate to `/Drivers/Index`.

  * View personal assignments and today's schedule.

* **Client**: `/Clients/Index` welcomes client.

  * Purchase tickets at `/Clients/Purchase`.
  * View own tickets and export PDFs at `/Clients/Tickets`.

## Real-Time Dashboard

Admins receive live metrics via SignalR. Background service polls every 30s and pushes updates.

## License

This project is licensed under the MIT License.

## Team Members

| **Name**                  | **ID**   |
|---------------------------|----------|
| **Sine Daella Lynda**     | 26589    |
| **Gihozo Bayingana Divine**   | 25429    |
| **Ikirezi Cyuzuzo Melissa**        | 25345    |
| **Ishimwe Uwonizeye Achille**       | 25782    |
| **Hirwa Clement Rhodin** | 25787    |
