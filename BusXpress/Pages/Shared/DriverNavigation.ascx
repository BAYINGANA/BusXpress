<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DriverNavigation.ascx.cs" Inherits="BusXpress.DriverNavigation" %>

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Driver Dashboard</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" />
</head>

<nav class="navbar navbar-expand-lg navbar-dark bg-dark fixed-top">
    <div class="container-fluid">
        <asp:HyperLink CssClass="navbar-brand" NavigateUrl="~/Drivers/Index.aspx" runat="server">Driver Dashboard</asp:HyperLink>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
            <ul class="navbar-nav ms-auto">
                <li class="nav-item">
                    <asp:HyperLink CssClass="nav-link" NavigateUrl="~/Drivers/Schedule.aspx" runat="server">My Schedule</asp:HyperLink>
                </li>
                <li class="nav-item">
                    <asp:HyperLink CssClass="nav-link text-danger" NavigateUrl="~/Logout.aspx" runat="server">Logout</asp:HyperLink>
                </li>
            </ul>
        </div>
    </div>
</nav>

<style>
    body {
        padding-top: 60px; /* Offset content below the navbar */
    }
</style>