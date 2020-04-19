using System;
using Google.Protobuf.WellKnownTypes;

namespace BudgetTracker.GrpcServices
{
    public static class UuidExtensions
    {
        public static Guid ToGuid(this UUID from)
        {
            if (string.IsNullOrEmpty(from.Value))
            {
                return Guid.Empty;
            }
            return Guid.Parse(from.Value);
        }

        public static UUID ToUUID(this Guid guid) => ToUUID((Guid?)guid);

        public static UUID ToUUID(this Guid? guid)
        {
            var uuid = new UUID();
            if (guid != null)
            {
                uuid.Value = guid.ToString();
            }

            return uuid;
        }

        public static Timestamp ToTimestamp(this DateTime dateTime)
        {
            var protoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(new DateTime(dateTime.Ticks, DateTimeKind.Utc));
            return new Timestamp
            {
                Seconds = protoTimestamp.Seconds,
                Nanos = protoTimestamp.Nanos
            };
        }

        public static DateTime ToDateTime(this Timestamp timestamp)
        {
            var protoTimestamp = new Google.Protobuf.WellKnownTypes.Timestamp
            {
                Nanos = timestamp.Nanos,
                Seconds = timestamp.Seconds
            };
            return protoTimestamp.ToDateTime();
        }
    }
}