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
}


namespace ConsoleApp
{


    class Program
    {
        static void Foo(short x)
        {
        }

        static void Main(string[] args)
        {
            var a = default(UserId);

            a.AsPrimitive();
        }
    }
}





