﻿//using Sample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnitGenerator;

//var a = UnitGenerateOptions.JsonConverterDictionaryKeySupport;

//var has = UnitGenerateOptions.JsonConverterDictionaryKeySupport.HasFlag(UnitGenerateOptions.Validate);
//Console.WriteLine(has);

var json = JsonSerializer.Serialize(new Dictionary<Guid, string> { { Guid.NewGuid(), "hogemoge" } });



Console.WriteLine(json);





[UnitOf<int>] public readonly partial struct MyId;


public readonly struct MyParsable : IParsable<MyParsable>
{
    public static MyParsable Parse(string s)
        => throw new NotImplementedException();

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out MyParsable result)
        => throw new NotImplementedException();

    public static MyParsable Parse(string s, IFormatProvider? provider)
        => throw new NotImplementedException();

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MyParsable result)
        => throw new NotImplementedException();
}

[UnitOf(typeof(MyParsable), UnitGenerateOptions.ParseMethod)]
public readonly partial struct StructInOtherLib
{
    public static void Test()
        => StructInOtherLib.Parse("");
}

[UnitOf(typeof(ulong), UnitGenerateOptions.ArithmeticOperator)]
public readonly partial struct Money
{
}

[UnitOf(typeof(int))]
public readonly partial struct NoNamespace
{
}

[UnitOf(typeof(Guid), UnitGenerateOptions.Comparable | UnitGenerateOptions.WithoutComparisonOperator)]
public readonly partial struct FooId { }

[UnitOf(typeof(Ulid), UnitGenerateOptions.Comparable | UnitGenerateOptions.WithoutComparisonOperator | UnitGenerateOptions.MessagePackFormatter | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.JsonConverterDictionaryKeySupport)]
public readonly partial struct BarId { }

namespace Sample
{
    [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.JsonConverterDictionaryKeySupport)]
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

    [UnitOf(typeof(string), UnitGenerateOptions.ParseMethod)]
    public readonly partial struct StringId { }
}

