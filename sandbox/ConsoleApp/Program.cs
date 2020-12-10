using System;
using System.ComponentModel;
using System.Globalization;
using UnitGenerator;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // UnitGenerator.UnitOfAttribtue

            // HelloWorld.SayHello();

            // UnitOfGeneraotr.HelloWorld.SayHello();

            // HelloWorldGenerated.HelloWorld.SayHello();
            //Foo f = 100;

            //var tako = f * 100;


            //var f = new Foo(true);
            //var b = f.AsPrimitive();
            //if (f)
            //{
            //    Console.WriteLine("true");
            //}
            //else
            //{
            //    Console.WriteLine("false");
            //}

        }
    }



    public class Tako
    {

    }

    //[UnitOf(typeof(int), UnitGenerateOptions.WithoutArithmeticOperator | UnitGenerateOptions.WithoutComparable)]

    //[UnitOf(typeof(int))]


    //[UnitOf(typeof(int)), GenerateFlags(UnitGenerateOptions. | UnitGenerateOptions.MessagePackFormatter)]


    // [UnitOf(typeof(int)), GenerateFlags(UnitGenerateOptions. | UnitGenerateOptions.MessagePackFormatter)]

    [UnitOf(typeof(int))]
    [GenerateFlags(UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator)]
    public readonly partial struct Foo
    {
        class TakoYaki
        {
            private static readonly Type WrapperType = typeof(Foo);
        }

        public void Tako()
        {
            //UnitGenerateOptions
            // new UnitOfAttribute(typeof(int), UnitGenerateOptions.DapperTypeHandler);
            Foo nano = default;
            Foo tako = default;
            Foo aaa = nano * 100;


            // this.AsPrimitive();
        }

        // public static implicit operator

        //private class FooTypeConverter : System.ComponentModel.TypeConverter
        //{
        //    private static readonly Type WrapperType = typeof(string);
        //    private static readonly Type ValueType = typeof(Guid);

        //    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        //    {
        //        if (sourceType == WrapperType || sourceType == ValueType)
        //        {
        //            return true;
        //        }

        //        return base.CanConvertFrom(context, sourceType);
        //    }

        //    public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        //    {
        //        if (destinationType == WrapperType || destinationType == ValueType)
        //        {
        //            return true;
        //        }

        //        return base.CanConvertTo(context, destinationType);
        //    }

        //    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        //    {
        //        switch (value)
        //        {
        //            case bool underlyingValue:
        //                return new Foo(underlyingValue);
        //            case Foo wrapperValue:
        //                return wrapperValue;
        //        }

        //        return base.ConvertFrom(context, culture, value);
        //    }

        //    public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        //    {
        //        if (value is Foo wrappedValue)
        //        {
        //            if (destinationType == WrapperType)
        //            {
        //                return wrappedValue;
        //            }

        //            if (destinationType == ValueType)
        //            {
        //                return wrappedValue.AsPrimitive();
        //            }
        //        }

        //        return base.ConvertTo(context, culture, value, destinationType);
        //    }
        //}
    }



    //[UnitOf(typeof(long))]
    //public struct Bar
    //{

    //}

}





