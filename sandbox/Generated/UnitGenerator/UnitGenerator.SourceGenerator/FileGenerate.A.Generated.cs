#pragma warning disable CS8669
using System;namespace FileGenerate
{
    [System.ComponentModel.TypeConverter(typeof(ATypeConverter))]
    readonly partial struct A : IEquatable<A>
    {
        readonly int value;

        public int AsPrimitive() => value;

        public A(int value)
        {
            this.value = value;
        }
        
        public static implicit operator int(A value)
        {
            return value.value;
        }

        public static implicit operator A(int value)
        {
            return new A(value);
        }

        public bool Equals(A other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var t = obj.GetType();
            if (t == typeof(A))
            {
                return Equals((A)obj);
            }
            if (t == typeof(int))
            {
                return value.Equals((int)obj);
            }

           return value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(in A x, in A y)
        {
            return x.value.Equals(y.value);
        }

        public static bool operator !=(in A x, in A y)
        {
            return !x.value.Equals(y.value);
        }

        // Default
        private class ATypeConverter : System.ComponentModel.TypeConverter
        {
            private static readonly Type WrapperType = typeof(A);
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
                    if (t == typeof(A))
                    {
                        return (A)value;
                    }
                    if (t == typeof(int))
                    {
                        return new A((int)value);
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is A wrappedValue)
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
