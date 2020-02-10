using System;

namespace BudgetTracker.GrpcServices
{
    public static class UuidExtensions
    {
        public static Guid ToGuid(this UUID from)
        {
            return Guid.Parse(from.Value);
        }

        public static UUID ToUUID(this Guid guid)
        {
            return new UUID {Value = guid.ToString()};
        }
    }
}