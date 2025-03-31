<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Schedule.aspx.cs" 
    Inherits="BusXpress.Pages.Drivers.Schedule" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Schedule</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body>
    <form id="form1" runat="server">
        <uc:DriverNavigation runat="server" ID="DriverNav" />
        
        <div class="container">
            <h2>Your Schedule</h2>
            
            <asp:Panel ID="pnlNoAssignments" runat="server" Visible="false">
                <p>No assignments found.</p>
            </asp:Panel>
            
            <asp:GridView ID="gvAssignments" runat="server" CssClass="table" AutoGenerateColumns="false"
                Visible="false">
                <Columns>
                    <asp:BoundField DataField="AssignmentId" HeaderText="Assignment ID" />
                    <asp:BoundField DataField="BusName" HeaderText="Bus Name" />
                    <asp:BoundField DataField="DriverName" HeaderText="Driver Name" />
                    <asp:BoundField DataField="FormattedAssignmentDate" HeaderText="Assignment Date" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>