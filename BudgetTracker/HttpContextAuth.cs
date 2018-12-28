using Hangfire.Dashboard;

namespace BudgetTracker
{
    public class HttpContextAuth : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => context.GetHttpContext().User.Identity.IsAuthenticated;
    }
}