<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="BusManagement.Pages.Drivers.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Driver Dashboard</title>
</head>
<body>
    <form id="form1" runat="server">
        <%-- Converted partial view --%>
        <uc:DriverNavigation runat="server" ID="DriverNavigation" />
        
        <h2>Welcome, Driver</h2>
        
        <p>This is your dashboard:</p>
        
        <div>
            <h4>User Details:</h4>
            <p><strong>Email:</strong> <%: Context.User.Identity.Name %></p>
            <p><strong>Role:</strong> <%: ((System.Security.Claims.ClaimsPrincipal)Context.User).FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value %></p>
            <p><strong>User ID:</strong> <%: ((System.Security.Claims.ClaimsPrincipal)Context.User).FindFirst("UserId")?.Value %></p>
        </div>
        
        <p><%: DateTime.Now %></p>
    </form>
</body>
</html>