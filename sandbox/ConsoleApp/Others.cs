using System;
using System.Globalization;
using System.Numerics;
using UnitGenerator;

namespace ConsoleApp
{
    internal static class NumberProxy<T> where T : INumber<T>
    {
        private static T One() => T.One;
    }

    [UnitOf(typeof(Guid), UnitGenerateOptions.ParseMethod | UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct GD
    {
        private partial void Validate()
        {
            UnitGenerator.NumberProxy<int>.One();
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(DateTime), UnitGenerateOptions.ParseMethod | UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct DT
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(string), UnitGenerateOptions.Validate | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct ST
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(byte[]), UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct BA
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    public class N2 : IEquatable<N2>
    {
        public int X { get; }

        public bool Equals(N2? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((N2)obj);
        }

        public override int GetHashCode() => X;
    }

    public class N : INumber<N>
    {
        public int CompareTo(object? obj) => throw new NotImplementedException();

        public int CompareTo(N? other) => throw new NotImplementedException();

        public bool Equals(N? other) => throw new NotImplementedException();

        public string ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();

        public static N Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();

        public static bool TryParse(string? s, IFormatProvider? provider, out N result) => throw new NotImplementedException();

        public static N Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out N result) => throw new NotImplementedException();

        public static N operator +(N left, N right) => throw new NotImplementedException();

        public static N AdditiveIdentity { get; }
        public static bool operator ==(N? left, N? right) => throw new NotImplementedException();

        public static bool operator !=(N? left, N? right) => throw new NotImplementedException();

        public static bool operator >(N left, N right) => throw new NotImplementedException();

        public static bool operator >=(N left, N right) => throw new NotImplementedException();

        public static bool operator <(N left, N right) => throw new NotImplementedException();

        public static bool operator <=(N left, N right) => throw new NotImplementedException();

        public static N operator --(N value) => throw new NotImplementedException();

        public static N operator /(N left, N right) => throw new NotImplementedException();

        public static N operator ++(N value) => throw new NotImplementedException();

        public static N operator %(N left, N right) => throw new NotImplementedException();

        public static N MultiplicativeIdentity { get; }
        public static N operator *(N left, N right) => throw new NotImplementedException();

        public static N operator -(N left, N right) => throw new NotImplementedException();

        public static N operator -(N value) => throw new NotImplementedException();

        public static N operator +(N value) => throw new NotImplementedException();

        public static N Abs(N value) => throw new NotImplementedException();

        public static bool IsCanonical(N value) => throw new NotImplementedException();

        public static bool IsComplexNumber(N value) => throw new NotImplementedException();

        public static bool IsEvenInteger(N value) => throw new NotImplementedException();

        public static bool IsFinite(N value) => throw new NotImplementedException();

        public static bool IsImaginaryNumber(N value) => throw new NotImplementedException();

        public static bool IsInfinity(N value) => throw new NotImplementedException();

        public static bool IsInteger(N value) => throw new NotImplementedException();

        public static bool IsNaN(N value) => throw new NotImplementedException();

        public static bool IsNegative(N value) => throw new NotImplementedException();

        public static bool IsNegativeInfinity(N value) => throw new NotImplementedException();

        public static bool IsNormal(N value) => throw new NotImplementedException();

        public static bool IsOddInteger(N value) => throw new NotImplementedException();

        public static bool IsPositive(N value) => throw new NotImplementedException();

        public static bool IsPositiveInfinity(N value) => throw new NotImplementedException();

        public static bool IsRealNumber(N value) => throw new NotImplementedException();

        public static bool IsSubnormal(N value) => throw new NotImplementedException();

        public static bool IsZero(N value) => throw new NotImplementedException();

        public static N MaxMagnitude(N x, N y) => throw new NotImplementedException();

        public static N MaxMagnitudeNumber(N x, N y) => throw new NotImplementedException();

        public static N MinMagnitude(N x, N y) => throw new NotImplementedException();

        public static N MinMagnitudeNumber(N x, N y) => throw new NotImplementedException();

        public static N Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();

        public static N Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();

        public static bool TryConvertFromChecked<TOther>(TOther value, out N result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryConvertFromSaturating<TOther>(TOther value, out N result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryConvertFromTruncating<TOther>(TOther value, out N result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryConvertToChecked<TOther>(N value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryConvertToSaturating<TOther>(N value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryConvertToTruncating<TOther>(N value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out N result) => throw new NotImplementedException();

        public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out N result) => throw new NotImplementedException();

        public static N One { get; }
        public static int Radix { get; }
        public static N Zero { get; }
    }
}
