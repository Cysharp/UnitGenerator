using Sample;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnitGenerator;



var id1 = FooId.Empty;
var id2 = new FooId(Guid.Empty);


Console.WriteLine(id1 == id2);

[UnitOf(typeof(int))]
public readonly partial struct NoNamespace
{
}

[UnitOf(typeof(Guid), UnitGenerateOptions.Comparable | UnitGenerateOptions.WithoutComparisonOperator)]
public readonly partial struct FooId { }

[UnitOf(typeof(Ulid), UnitGenerateOptions.Comparable | UnitGenerateOptions.WithoutComparisonOperator | UnitGenerateOptions.MessagePackFormatter)]
public readonly partial struct BarId { }

namespace Sample
{

    [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod)]
    public readonly partial struct Hp
    {
        // public static Hp operator +(in Hp x, in Hp y) => new Hp(checked((int)(x.value + y.value)));

        void Foo()
        {
            _ = this.AsPrimitive();
            _ = this.ToString();

            _ = FooId.NewFooId();
            Guid.NewGuid();
            //public static readonly Guid Empty;
            //Guid.Empty

            // public static readonly Ulid Empty = default(Ulid);
            // Ulid.Empty


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


        }
    }

}





