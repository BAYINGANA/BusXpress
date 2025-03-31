using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BusXpress.Models;

namespace BusXpress.Pages.Drivers
{
    public partial class Schedule : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindScheduleData();
            }
        }

        private void BindScheduleData()
        {
            // This would come from your business logic/database
            var assignments = GetDriverAssignments();

            if (assignments.Count == 0)
            {
                pnlNoAssignments.Visible = true;
                gvAssignments.Visible = false;
            }
            else
            {
                pnlNoAssignments.Visible = false;
                gvAssignments.Visible = true;

                // Format dates before binding
                assignments.ForEach(a => a.FormattedAssignmentDate =
                    a.AssignmentDate.ToString("yyyy-MM-dd HH:mm"));

                gvAssignments.DataSource = assignments;
                gvAssignments.DataBind();
            }
        }

        private List<DriverAssignment> GetDriverAssignments()
        {
            // Implement your actual data access here
            // This is just a placeholder
            return new List<DriverAssignment>
            {
                new DriverAssignment
                {
                    AssignmentId = 1,
                    BusName = "Bus 101",
                    DriverName = "John Doe",
                    AssignmentDate = DateTime.Now,
                    Status = "Active"
                }
                // Add more sample data or real data access
            };
        }
    }
}