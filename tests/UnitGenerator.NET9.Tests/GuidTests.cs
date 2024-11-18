using FluentAssertions;
using System;

namespace UnitGenerator.NET9.Tests;

public class GuidTests
{
    [Fact]
    public void Guidv7_v4_Comparison_AsExpected()
    {
        // v7
        TryGetUuidV7Timestamp(Guidv7.New().AsPrimitive(), out var v).Should().BeTrue();
        // ...approximate check
        v?.ToString("yyyyMMdd").Should().Be(DateTime.UtcNow.ToString("yyyyMMdd"));
        // v4
        TryGetUuidV7Timestamp(GuidDefault.New().AsPrimitive(), out var _).Should().BeFalse();
    }

    static bool TryGetUuidV7Timestamp(Guid uuid, out DateTimeOffset? timestamp)
    {
        timestamp = null;
        var uuidString = uuid.ToString("N");
        // version number is the 13th character
        if (uuidString[12] == '7')
        {
            var timestampHex = uuidString.Substring(0, 12);
            var milliseconds = Convert.ToInt64(timestampHex, 16);
            timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return true;
        }
        else return false;
    }
}

[UnitOf<Guid>(UnitGenerateOptions.UUIDv7)]
public readonly partial struct Guidv7 { }

[UnitOf<Guid>()]
public readonly partial struct GuidDefault { }
