UnitGenerator
===
[![GitHub Actions](https://github.com/Cysharp/UnitGenerator/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/UnitGenerator/actions) [![Releases](https://img.shields.io/github/release/Cysharp/UnitGenerator.svg)](https://github.com/Cysharp/UnitGenerator/releases)

C# Source Generator to create [Value object](https://en.wikipedia.org/wiki/Value_object) pattern, also inspired by [units of measure](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/units-of-measure) to support all arithmetic operators and serialization.

[📖 Table of contents](#-table-of-contents)

NuGet: [UnitGenerator](https://www.nuget.org/packages/UnitGenerator)

```
Install-Package UnitGenerator
```

## Introduction

For example, Identifier, UserId is comparable only to UserId, and cannot be assigned to any other type. Also, arithmetic operations are not allowed.

```csharp
using UnitGenerator;

[UnitOf(typeof(int))]
public readonly partial struct UserId { }
```

will generates

```csharp
[System.ComponentModel.TypeConverter(typeof(UserIdTypeConverter))]
public readonly partial struct UserId : IEquatable<UserId> 
{
    readonly int value;
    
    public UserId(int value)
    {
        this.value = value;
    }

    public readonly int AsPrimitive() => value;
    public static explicit operator int(UserId value) => value.value;
    public static explicit operator UserId(int value) => new UserId(value);
    public bool Equals(UserId other) => value.Equals(other.value);
    public override bool Equals(object? obj) => // snip...
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => "UserId(" + value + ")";
    public static bool operator ==(in UserId x, in UserId y) => x.value == y.value;
    public static bool operator !=(in UserId x, in UserId y) => x.value != y.value;

    private class UserIdTypeConverter : System.ComponentModel.TypeConverter
    {
        // snip...
    }
}
```

However, Hp in games, should not be allowed to be assigned to other types, but should support arithmetic operations with int. For example double heal = `target.Hp = Hp.Min(target.Hp * 2, target.MaxHp)`.

```csharp
[UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod)]
public readonly partial struct Hp { }

// -- generates

[System.ComponentModel.TypeConverter(typeof(HpTypeConverter))]
public readonly partial struct Hp : IEquatable<Hp> , IComparable<Hp>
{
    readonly int value;

    public Hp(int value)
    {
        this.value = value;
    }

    public readonly int AsPrimitive() => value;
    public static explicit operator int(Hp value) => value.value;
    public static explicit operator Hp(int value) => new Hp(value);
    public bool Equals(Hp other) => value.Equals(other.value);
    public override bool Equals(object? obj) => // snip...
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => "Hp(" + value + ")";
    public static bool operator ==(in Hp x, in Hp y) => x.value == y.value;
    public static bool operator !=(in Hp x, in Hp y) => x.value != y.value;
    private class HpTypeConverter : System.ComponentModel.TypeConverter { /* snip... */ }

    // UnitGenerateOptions.ArithmeticOperator
    public static Hp operator +(in Hp x, in Hp y) => new Hp(checked((int)(x.value + y.value)));
    public static Hp operator -(in Hp x, in Hp y) => new Hp(checked((int)(x.value - y.value)));
    public static Hp operator *(in Hp x, in Hp y) => new Hp(checked((int)(x.value * y.value)));
    public static Hp operator /(in Hp x, in Hp y) => new Hp(checked((int)(x.value / y.value)));

    // UnitGenerateOptions.ValueArithmeticOperator
    public static Hp operator ++(in Hp x) => new Hp(checked((int)(x.value + 1)));
    public static Hp operator --(in Hp x) => new Hp(checked((int)(x.value - 1)));
    public static Hp operator +(in Hp x, in int y) => new Hp(checked((int)(x.value + y)));
    public static Hp operator -(in Hp x, in int y) => new Hp(checked((int)(x.value - y)));
    public static Hp operator *(in Hp x, in int y) => new Hp(checked((int)(x.value * y)));
    public static Hp operator /(in Hp x, in int y) => new Hp(checked((int)(x.value / y)));

    // UnitGenerateOptions.Comparable
    public int CompareTo(Hp other) => value.CompareTo(other);
    public static bool operator >(in Hp x, in Hp y) => x.value > y.value;
    public static bool operator <(in Hp x, in Hp y) => x.value < y.value;
    public static bool operator >=(in Hp x, in Hp y) => x.value >= y.value;
    public static bool operator <=(in Hp x, in Hp y) => x.value <= y.value;

    // UnitGenerateOptions.MinMaxMethod
    public static Hp Min(Hp x, Hp y) => new Hp(Math.Min(x.value, y.value));
    public static Hp Max(Hp x, Hp y) => new Hp(Math.Max(x.value, y.value));
}
```

You can configure with `UnitGenerateOptions`, which method to implement.

```csharp
[Flags]
enum UnitGenerateOptions
{
    None = 0,
    ImplicitOperator = 1,
    ParseMethod = 2,
    MinMaxMethod = 4,
    ArithmeticOperator = 8,
    ValueArithmeticOperator = 16,
    Comparable = 32,
    Validate = 64,
    JsonConverter = 128,
    MessagePackFormatter = 256,
    DapperTypeHandler = 512,
    EntityFrameworkValueConverter = 1024,
}
```

UnitGenerateOptions has some serializer support. For example, a result like `Serialize(userId) => { Value = 1111 }` is awful. The value-object should be serialized natively, i.e. `Serialzie(useId) => 1111`, and should be able to be added directly to a database, etc.

Currently UnitGenerator supports [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp), System.Text.Json, [Dapper](https://github.com/StackExchange/Dapper) and EntityFrameworkCore.

```csharp
[UnitOf(typeof(int), UnitGenerateOptions.MessagePackFormatter)]
public readonly partial struct UserId { }

// -- generates

[MessagePackFormatter(typeof(UserIdMessagePackFormatter))]
public readonly partial struct UserId 
{
    class UserIdMessagePackFormatter : IMessagePackFormatter<UserId>
    {
        public void Serialize(ref MessagePackWriter writer, UserId value, MessagePackSerializerOptions options)
        {
            options.Resolver.GetFormatterWithVerify<int>().Serialize(ref writer, value.value, options);
        }

        public UserId Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new UserId(options.Resolver.GetFormatterWithVerify<int>().Deserialize(ref reader, options));
        }
    }
}
```

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [UnitOfAttribute](#unitofattribute)
- [UnitGenerateOptions](#unitgenerateoptions)
  - [ImplicitOperator](#implicitoperator)
  - [ParseMethod](#parsemethod)
  - [MinMaxMethod](#minmaxmethod)
  - [ArithmeticOperator](#arithmeticoperator)
  - [ValueArithmeticOperator](#valuearithmeticoperator)
  - [Comparable](#comparable)
  - [Validate](#validate)
  - [JsonConverter](#jsonconverter)
  - [MessagePackFormatter](#messagepackformatter)
  - [DapperTypeHandler](#dappertypehandler)
  - [EntityFrameworkValueConverter](#entityframeworkvalueconverter)
- [Use for Unity](#use-for-unity)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## UnitOfAttribute
TODO:

## UnitGenerateOptions
TODO:

### ImplicitOperator
### ParseMethod 
### MinMaxMethod
### ArithmeticOperator
### ValueArithmeticOperator
### Comparable
### Validate
### JsonConverter
### MessagePackFormatter
### DapperTypeHandler
### EntityFrameworkValueConverter

## Use for Unity
TODO:

License
---
This library is under the MIT License.
