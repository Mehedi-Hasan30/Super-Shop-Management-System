using System;

namespace Super_Shop_Management_System.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsPositiveDecimal(decimal value)
        {
            return value > 0;
        }

        public static bool IsNonNegativeInt(int value)
        {
            return value >= 0;
        }

        public static bool HasProfitMargin(decimal purchasePrice, decimal sellingPrice)
        {
            return sellingPrice >= purchasePrice;
        }

        public static DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
