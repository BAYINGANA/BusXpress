using System;
using System.Security.Claims;
using System.Web;
using System.Web.UI;

namespace BusXpress.Pages.Drivers
{
    public partial class IndexModel : Page
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!System.Web.Security.Roles.IsUserInRole("Driver"))
                {
                    Response.Redirect("~/AccessDenied.aspx");
                    return;
                }

                // Access User Claims
                var user = (ClaimsPrincipal)HttpContext.Current.User;
                Email = user.Identity.Name;
                Role = user.FindFirst(ClaimTypes.Role)?.Value;
                UserId = user.FindFirst("UserId")?.Value;
            }
        }
    }
}