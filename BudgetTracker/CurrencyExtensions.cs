namespace BudgetTracker
{
    public static class CurrencyExtensions
    {
        public const string RUB = "RUB";
        public const string EUR = "EUR";
        public const string USD = "USD";

        public static string NormalizeCcy(string value)
        {
            if (value == null)
                return null;
            
            if (value.ToLower().Contains("rur") || 
                value.ToLower().Contains("р") || 
                value.Contains("₽"))
                return RUB;
            if (value.ToLower().Contains("$"))
                return USD;
            if (value.ToLower().Contains("€"))
                return EUR;

            return value;
        }
    }
}