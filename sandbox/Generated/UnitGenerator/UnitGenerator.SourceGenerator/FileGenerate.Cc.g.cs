﻿// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY UnitGenerator. DO NOT CHANGE IT.
// </auto-generated>
#pragma warning disable CS8669
using System;
using System.Globalization;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif
namespace FileGenerate
{
    [System.ComponentModel.TypeConverter(typeof(CcTypeConverter))]
    readonly partial struct Cc 
        : IEquatable<Cc>
        , IComparable<Cc>
        , IFormattable
#if NET6_0_OR_GREATER
        , ISpanFormattable
#endif
#if NET7_0_OR_GREATER
        , IComparisonOperators<Cc, Cc, bool>
        , IAdditionOperators<Cc, Cc, Cc>
        , ISubtractionOperators<Cc, Cc, Cc>
        , IMultiplyOperators<Cc, Cc, Cc>
        , IDivisionOperators<Cc, Cc, Cc>
        , IUnaryPlusOperators<Cc, Cc>
        , IUnaryNegationOperators<Cc, Cc>
        , IIncrementOperators<Cc>
        , IDecrementOperators<Cc>
#endif
#if NET8_0_OR_GREATER
        , IEqualityOperators<Cc, Cc, bool>
        , IUtf8SpanFormattable
#endif
    {
        readonly int value;

        public int AsPrimitive() => value;

        public Cc(int value)
        {
            this.value = value;
        }
        
        public static explicit operator int(Cc value)
        {
            return value.value;
        }

        public static explicit operator Cc(int value)
        {
            return new Cc(value);
        }

        public bool Equals(Cc other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var t = obj.GetType();
            if (t == typeof(Cc))
            {
                return Equals((Cc)obj);
            }
            if (t == typeof(int))
            {
                return value.Equals((int)obj);
            }

            return value.Equals(obj);
        }
        
        public static bool operator ==(Cc x, Cc y)
        {
            return x.value.Equals(y.value);
        }

        public static bool operator !=(Cc x, Cc y)
        {
            return !x.value.Equals(y.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString() => value.ToString();

        public string ToString(string? format, IFormatProvider? formatProvider) => value.ToString(format, formatProvider);

#if NET6_0_OR_GREATER
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
            ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, provider);
#endif
#if NET8_0_OR_GREATER        
        public bool TryFormat (Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
            ((IUtf8SpanFormattable)value).TryFormat(utf8Destination, out bytesWritten, format, provider);
#endif

        // UnitGenerateOptions.ArithmeticOperator
        
        public static Cc operator +(Cc x, Cc y)
        {
            checked
            {
                return new Cc((int)(x.value + y.value));
            }
        }

        public static Cc operator -(Cc x, Cc y)
        {
            checked
            {
                return new Cc((int)(x.value - y.value));
            }
        }

        public static Cc operator +(Cc value) => new((int)(+value.value));
        public static Cc operator -(Cc value) => new((int)(-value.value));

        public static Cc operator *(Cc x, Cc y)
        {
            checked
            {
                return new Cc((int)(x.value * y.value));
            }
        }


        public static Cc operator /(Cc x, Cc y)
        {
            checked
            {
                return new Cc((int)(x.value / y.value));
            }
        }

        public static Cc operator ++(Cc x)
        {
            checked
            {
                return new Cc((int)((int)(x.value + 1)));
            }
        }

        public static Cc operator --(Cc x)
        {
            checked
            {
                return new Cc((int)((int)(x.value - 1)));
            }
        }

        // UnitGenerateOptions.ValueArithmeticOperator
        
        public static Cc operator +(Cc x, int y)
        {
            checked
            {
                return new Cc((int)(x.value + y));
            }
        }

        public static Cc operator -(Cc x, int y)
        {
            checked
            {
                return new Cc((int)(x.value - y));
            }
        }

        public static Cc operator *(Cc x, int y)
        {
            checked
            {
                return new Cc((int)(x.value * y));
            }
        }


        public static Cc operator /(Cc x, int y)
        {
            checked
            {
                return new Cc((int)(x.value / y));
            }
        }

        // UnitGenerateOptions.Comparable

        public int CompareTo(Cc other)
        {
            return value.CompareTo(other.value);
        }
        public static bool operator >(Cc x, Cc y)
        {
            return x.value > y.value;
        }

        public static bool operator <(Cc x, Cc y)
        {
            return x.value < y.value;
        }

        public static bool operator >=(Cc x, Cc y)
        {
            return x.value >= y.value;
        }

        public static bool operator <=(Cc x, Cc y)
        {
            return x.value <= y.value;
        }

        // Default
        
        private class CcTypeConverter : System.ComponentModel.TypeConverter
        {
            private static readonly Type WrapperType = typeof(Cc);
            private static readonly Type ValueType = typeof(int);

            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == WrapperType || sourceType == ValueType)
                {
                    return true;
                }

                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == WrapperType || destinationType == ValueType)
                {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value != null)
                {
                    var t = value.GetType();
                    if (t == typeof(Cc))
                    {
                        return (Cc)value;
                    }
                    if (t == typeof(int))
                    {
                        return new Cc((int)value);
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is Cc wrappedValue)
                {
                    if (destinationType == WrapperType)
                    {
                        return wrappedValue;
                    }

                    if (destinationType == ValueType)
                    {
                        return wrappedValue.AsPrimitive();
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
