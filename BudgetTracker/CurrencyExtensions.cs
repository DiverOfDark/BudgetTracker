namespace BudgetTracker
{
    public static class CurrencyExtensions
    {
        public static string NormalizeCcy(string value)
        {
            if (value.ToLower().Contains("rur") || 
                value.ToLower().Contains("р") || 
                value.Contains("₽"))
                return "RUB";
            if (value.ToLower().Contains("$"))
                return "USD";
            if (value.ToLower().Contains("€"))
                return "EUR";

            return value;
        }
    }
}