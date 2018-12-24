using System;
using System.Collections.Generic;

namespace BudgetTracker
{
    public static class CurrencyExtensions
    {
        public const string RUB = "RUB";
        public const string EUR = "EUR";
        public const string USD = "USD";

        public static readonly IEnumerable<String> KnownCurrencies = new[] {RUB, USD, EUR};

        public static string NormalizeCcy(string value)
        {
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

        public static string ToDisplayText(string ccy)
        {
            switch (NormalizeCcy(ccy))
            {
                case RUB:
                    return "₽";
                case USD:
                    return "$";
                case EUR:
                    return "€";
                default:
                    return ccy;
            }
        }
    }
}