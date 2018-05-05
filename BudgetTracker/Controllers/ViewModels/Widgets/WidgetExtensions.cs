using JetBrains.Annotations;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public static class WidgetExtensions
    {
        public static string AsPath([AspMvcView] string path) => path;
    }
}