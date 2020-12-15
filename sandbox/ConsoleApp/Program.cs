using Sample;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnitGenerator;

namespace Sample
{

    [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod)]
    public readonly partial struct Hp
    {
        // public static Hp operator +(in Hp x, in Hp y) => new Hp(checked((int)(x.value + y.value)));

        void Foo()
        {
            _ = this.AsPrimitive();
        }

    }

    [UnitOf(typeof(int), UnitGenerateOptions.MessagePackFormatter)]
    public readonly partial struct UserId { }


    [UnitOf(typeof(int), UnitGenerateOptions.Validate)]
    public readonly partial struct SampleValidate
    {
        // impl here.
        private partial void Validate()
        {
            if (value > 9999) throw new Exception("Invalid value range: " + value);
        }
    }

    [UnitOf(typeof(int), UnitGenerateOptions.MessagePackFormatter)]
    public readonly partial struct UserId2 
    {
        public void Foo()
        {
            

            _ = AsPrimitive();
        }
    }

}


namespace ConsoleApp
{


    [UnitOf(typeof((string street, string city)))]
    public readonly partial struct StreetAddress { }

    class Program
    {
        static void Foo(short x)
        {
        }

        static void Main(string[] args)
        {
            new SampleValidate(99999);


            default(StreetAddress).AsPrimitive();

        }
    }

}





