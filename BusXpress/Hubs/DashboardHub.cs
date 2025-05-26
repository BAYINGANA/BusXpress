using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BusManagement.Hubs
{
    [Authorize(Roles = "Admin")]
    public class DashboardHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            await base.OnConnectedAsync();
        }
    }
}
