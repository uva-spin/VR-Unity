#if !NET_DOTS
using System;
using System.Globalization;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        IBinaryAdapter<Guid>,
        IBinaryAdapter<DateTime>,
        IBinaryAdapter<TimeSpan>,
        IBinaryAdapter<Version>
    {
        void IBinaryAdapter<Guid>.Serialize(UnsafeAppendBuffer* writer, Guid value)
            => writer->Add(value.ToString("N", CultureInfo.InvariantCulture));

        Guid IBinaryAdapter<Guid>.Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            reader->ReadNext(out string str);
            return Guid.TryParseExact(str, "N", out var value) ? value : default;
        }

        void IBinaryAdapter<DateTime>.Serialize(UnsafeAppendBuffer* writer, DateTime value)
            => writer->Add(value.ToString("o", CultureInfo.InvariantCulture));

        DateTime IBinaryAdapter<DateTime>.Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            reader->ReadNext(out string str);
            return DateTime.TryParseExact(str, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : default;
        }
        
        void IBinaryAdapter<TimeSpan>.Serialize(UnsafeAppendBuffer* writer, TimeSpan value)
            => writer->Add(value.ToString("c", CultureInfo.InvariantCulture));

        TimeSpan IBinaryAdapter<TimeSpan>.Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            reader->ReadNext(out string str);
            return TimeSpan.TryParseExact(str, "c", CultureInfo.InvariantCulture, out var value) ? value : default;
        }

        void IBinaryAdapter<Version>.Serialize(UnsafeAppendBuffer* writer, Version value)
            => writer->Add(value.ToString());

        Version IBinaryAdapter<Version>.Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            reader->ReadNext(out string str);
            return new Version(str);
        }
    }
}
#endif