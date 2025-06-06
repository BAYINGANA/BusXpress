USE [master]
GO
/****** Object:  Database [BusManagementDB]    Script Date: 13/04/2025 21:29:07 ******/
CREATE DATABASE [BusManagementDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'BusManagementDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\BusManagementDB.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'BusManagementDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\BusManagementDB_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [BusManagementDB] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [BusManagementDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [BusManagementDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [BusManagementDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [BusManagementDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [BusManagementDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [BusManagementDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [BusManagementDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [BusManagementDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [BusManagementDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [BusManagementDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [BusManagementDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [BusManagementDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [BusManagementDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [BusManagementDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [BusManagementDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [BusManagementDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [BusManagementDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [BusManagementDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [BusManagementDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [BusManagementDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [BusManagementDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [BusManagementDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [BusManagementDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [BusManagementDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [BusManagementDB] SET  MULTI_USER 
GO
ALTER DATABASE [BusManagementDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [BusManagementDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [BusManagementDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [BusManagementDB] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [BusManagementDB] SET DELAYED_DURABILITY = DISABLED 
GO
USE [BusManagementDB]
GO
/****** Object:  Table [dbo].[Admins]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Admins](
	[AdminId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Admins] PRIMARY KEY CLUSTERED 
(
	[AdminId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Buses]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Buses](
	[BusId] [int] IDENTITY(1,1) NOT NULL,
	[BusNumber] [nvarchar](50) NOT NULL,
	[Capacity] [int] NOT NULL,
	[Model] [nvarchar](50) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Buses] PRIMARY KEY CLUSTERED 
(
	[BusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Clients]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clients](
	[ClientId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DriverAssignments]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverAssignments](
	[AssignmentId] [int] IDENTITY(1,1) NOT NULL,
	[DriverId] [int] NOT NULL,
	[BusId] [int] NOT NULL,
	[AssignmentDate] [datetime] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DriverAssignments] PRIMARY KEY CLUSTERED 
(
	[AssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Drivers]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Drivers](
	[DriverId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[LicenceNumber] [nvarchar](255) NOT NULL,
	[LicencePhoto] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Drivers] PRIMARY KEY CLUSTERED 
(
	[DriverId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Routes]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Routes](
	[RouteId] [int] IDENTITY(1,1) NOT NULL,
	[Origin] [nvarchar](50) NOT NULL,
	[Destination] [nvarchar](100) NOT NULL,
	[Distance] [decimal](18, 0) NOT NULL,
	[Price] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_Routes] PRIMARY KEY CLUSTERED 
(
	[RouteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule](
	[ScheduleId] [int] IDENTITY(1,1) NOT NULL,
	[BusId] [int] NOT NULL,
	[RouteId] [int] NOT NULL,
	[DepartureTime] [datetime] NOT NULL,
	[ArrivalTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[ScheduleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tickets]    Script Date: 13/04/2025 21:29:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tickets](
	[TicketId] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NOT NULL,
	[ScheduleId] [int] NOT NULL,
	[DateIssued] [datetime] NOT NULL,
 CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Add CreatedAt column to Clients table
ALTER TABLE Clients
ADD CreatedAt DATETIME NOT NULL DEFAULT GETDATE();
GO
-- Add CreatedAt column to Drivers table
ALTER TABLE Drivers
ADD CreatedAt DATETIME NOT NULL DEFAULT GETDATE();
GO
-- Create SystemAnalytics table
CREATE TABLE SystemAnalytics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    MetricType NVARCHAR(50) NOT NULL,     -- e.g., 'ResponseTime', 'Error', 'Downtime'
    MetricValue FLOAT NOT NULL,           -- Can be response time in ms, or 1 for an event
    Description NVARCHAR(255) NULL        -- Optional description or details
);
GO
ALTER TABLE [dbo].[DriverAssignments]
ADD [RouteId] INT;
GO
ALTER TABLE [dbo].[DriverAssignments]
ADD CONSTRAINT FK_DriverAssignments_Routes
FOREIGN KEY ([RouteId]) REFERENCES [dbo].[Routes]([RouteId]);
GO
USE [master]
GO
ALTER DATABASE [BusManagementDB] SET  READ_WRITE 
GO
