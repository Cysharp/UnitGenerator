UnitGenerator
===
[![GitHub Actions](https://github.com/Cysharp/UnitGenerator/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/UnitGenerator/actions) [![Releases](https://img.shields.io/github/release/Cysharp/UnitGenerator.svg)](https://github.com/Cysharp/UnitGenerator/releases)

C# Source Generator to create [Value object](https://en.wikipedia.org/wiki/Value_object) pattern, also inspired by [units of measure](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/units-of-measure) to support all arithmetic operators and serialization.

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
    public override string ToString() => value.ToString();
    public static bool operator ==(in UserId x, in UserId y) => x.value.Equals(y.value);
    public static bool operator !=(in UserId x, in UserId y) => !x.value.Equals(y.value);

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

    public int AsPrimitive() => value;
    public static explicit operator int(Hp value) => value.value;
    public static explicit operator Hp(int value) => new Hp(value);
    public bool Equals(Hp other) => value.Equals(other.value);
    public override bool Equals(object? obj) => // snip...
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => value.ToString();
    public static bool operator ==(in Hp x, in Hp y) => x.value.Equals(y.value);
    public static bool operator !=(in Hp x, in Hp y) => !x.value.Equals(y.value);
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
    public int CompareTo(Hp other) => value.CompareTo(other.value);
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
    WithoutComparisonOperator = 2048,
    JsonConverterDictionaryKeySupport = 4096
}
```

UnitGenerateOptions has some serializer support. For example, a result like `Serialize(userId) => { Value = 1111 }` is awful. The value-object should be serialized natively, i.e. `Serialize(useId) => 1111`, and should be able to be added directly to a database, etc.

Currently UnitGenerator supports [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp), System.Text.Json(JsonSerializer), [Dapper](https://github.com/StackExchange/Dapper) and EntityFrameworkCore.

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
  - [WithoutComparisonOperator](#withoutcomparisonoperator)
  - [Validate](#validate)
  - [JsonConverter](#jsonconverter)
  - [JsonConverterDictionaryKeySupport](#jsonconverterdictionarykeysupport)
  - [MessagePackFormatter](#messagepackformatter)
  - [DapperTypeHandler](#dappertypehandler)
  - [EntityFrameworkValueConverter](#entityframeworkvalueconverter)
- [Use for Unity](#use-for-unity)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## UnitOfAttribute
When referring to the UnitGenerator, it generates a internal `UnitOfAttribute`.

```csharp
namespace UnitGenerator
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    internal class UnitOfAttribute : Attribute
    {
        public UnitOfAttribute(Type type, UnitGenerateOptions options = UnitGenerateOptions.None, string toStringFormat = null)
    }
}
```

You can attach this attribute with any specified underlying type to `readonly partial struct`.

```csharp
[UnitOf(typeof(Guid))]
public readonly partial struct GroupId { }

[UnitOf(typeof(string))]
public readonly partial struct Message { }

[UnitOf(typeof(long))]
public readonly partial struct Power { }

[UnitOf(typeof(byte[]))]
public readonly partial struct Image { }

[UnitOf(typeof(DateTime))]
public readonly partial struct StartDate { }

[UnitOf(typeof((string street, string city)))]
public readonly partial struct StreetAddress { }
```

Standard UnitOf(`UnitGenerateOptions.None`) generates value constructor, `explicit operator`, `implement IEquatable<T>`, `override GetHashCode`, `override ToString`, `==` and `!=` operator, `TypeConverter` for ASP.NET Core binding, `AsPrimitive` method.

If you want to retrieve primitive value, use `AsPrimitive()` instead of `.Value`. This is intended to avoid casual getting of primitive values (using the arithmetic operator option if available).

> When type is bool, also implements `true`, `false`, `!` operators.

```csharp 
public static bool operator true(Foo x) => x.value;
public static bool operator false(Foo x) => !x.value;
public static bool operator !(Foo x) => !x.value;
```

> When type is Guid or [Ulid](https://github.com/Cysharp/Ulid), also implements `New()` and `New***()` static operator.

```csharp
public static GroupId New();
public static GroupId NewGroupId();
```

Second parameter `UnitGenerateOptions options` can configure which method to implement, default is `None`.

Third parameter `strign toStringFormat` can configure `ToString` format. Default is null and output as $`{0}`.

## UnitGenerateOptions

When referring to the UnitGenerator, it generates a internal `UnitGenerateOptions` that is bit flag of which method to implement.

```csharp
[Flags]
internal enum UnitGenerateOptions
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

You can use this with `[UnitOf]`.

```csharp
[UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod)]
public readonly partial struct Strength { }

[UnitOf(typeof(DateTime), UnitGenerateOptions.Validate | UnitGenerateOptions.ParseMethod | UnitGenerateOptions.Comparable)]
public readonly partial struct EndDate { }

[UnitOf(typeof(double), UnitGenerateOptions.ParseMethod | UnitGenerateOptions.MinMaxMethod | UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.Validate | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.MessagePackFormatter | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
public readonly partial struct AllOptionsStruct { }
```

You can setup project default options like this.

```csharp
internal static class UnitOfOptions
{
    public const UnitGenerateOptions Default = UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.Comparable | UnitGenerateOptions.MinMaxMethod;
}

[UnitOf(typeof(int), UnitOfOptions.Default)]
public readonly partial struct Hp { }
```

### ImplicitOperator

```csharp
// Default
public static explicit operator U(T value) => value.value;
public static explicit operator T(U value) => new T(value);

// UnitGenerateOptions.ImplicitOperator
public static implicit operator U(T value) => value.value;
public static implicit operator T(U value) => new T(value);
```

### ParseMethod 

```csharp
public static T Parse(string s)
public static bool TryParse(string s, out T result)
```

### MinMaxMethod

```csharp
public static T Min(T x, T y)
public static T Max(T x, T y)
```

### ArithmeticOperator

```csharp
public static T operator +(in T x, in T y) => new T(checked((U)(x.value + y.value)));
public static T operator -(in T x, in T y) => new T(checked((U)(x.value - y.value)));
public static T operator *(in T x, in T y) => new T(checked((U)(x.value * y.value)));
public static T operator /(in T x, in T y) => new T(checked((U)(x.value / y.value)));
```

### ValueArithmeticOperator

```csharp
public static T operator ++(in T x) => new T(checked((U)(x.value + 1)));
public static T operator --(in T x) => new T(checked((U)(x.value - 1)));
public static T operator +(in T x, in U y) => new T(checked((U)(x.value + y)));
public static T operator -(in T x, in U y) => new T(checked((U)(x.value - y)));
public static T operator *(in T x, in U y) => new T(checked((U)(x.value * y)));
public static T operator /(in T x, in U y) => new T(checked((U)(x.value / y)));
```

### Comparable

Implements `IComparable<T>` and `>`, `<`, `>=`, `<=` operators.

```csharp
public U CompareTo(T other) => value.CompareTo(other.value);
public static bool operator >(in T x, in T y) => x.value > y.value;
public static bool operator <(in T x, in T y) => x.value < y.value;
public static bool operator >=(in T x, in T y) => x.value >= y.value;
public static bool operator <=(in T x, in T y) => x.value <= y.value;
```

### WithoutComparisonOperator

Without implements `>`, `<`, `>=`, `<=` operators. For example, useful for Guid.

```csharp
[UnitOf(typeof(Guid), UnitGenerateOptions.Comparable | UnitGenerateOptions.WithoutComparisonOperator)]
public readonly partial struct FooId { }
```

### Validate

Implements `partial void Validate()` method that is called on constructor.

```csharp
// You can implement this custom validate method.
[UnitOf(typeof(int), UnitGenerateOptions.Validate)]
public readonly partial struct SampleValidate
{
    // impl here.
    private partial void Validate()
    {
        if (value > 9999) throw new Exception("Invalid value range: " + value);
    }
}

// Source generator generate this codes.
public T(int value)
{
    this.value = value;
    this.Validate();
}
 
private partial void Validate();
```

### JsonConverter

Implements `System.Text.Json`'s `JsonConverter`. It will be used `JsonSerializer` automatically.

```csharp
[JsonConverter(typeof(UserIdJsonConverter))]
public readonly partial struct UserId
{
    class UserIdJsonConverter : JsonConverter<UserId>
}
```

### JsonConverterDictionaryKeySupport

Implements `JsonConverter`'s `WriteAsPropertyName/ReadAsPropertyName`. It supports from .NET 6, supports Dictionary's Key.

```csharp
var dict = Dictionary<UserId, int>
JsonSerializer.Serialize(dict);
````

### MessagePackFormatter

Implements MessagePack for C#'s `MessagePackFormatter`. It will be used `MessagePackSerializer` automatically.

```csharp
[MessagePackFormatter(typeof(UserIdMessagePackFormatter))]
public readonly partial struct UserId
{
    class UserIdMessagePackFormatter : IMessagePackFormatter<UserId>
}
```

### DapperTypeHandler

Implements Dapper's TypeHandler by public accessibility. TypeHandler is automatically registered at the time of Module initialization.

```csharp
public readonly partial struct UserId
{
    public class UserIdTypeHandler : Dapper.SqlMapper.TypeHandler<UserId>
}

[ModuleInitializer]
public static void AddTypeHandler()
{
    Dapper.SqlMapper.AddTypeHandler(new A.ATypeHandler());
}
```

### EntityFrameworkValueConverter

Implements EntityFrameworkCore's ValueConverter by public accessibility. It is not registered automatically so you need to register manually.

```csharp
public readonly partial struct UserId
{
    public class UserIdValueConverter : ValueConverter<UserId, int>
}

// setup handler manually
builder.HasConversion(new UserId.UserIdValueConverter());
```

## Use for Unity

C# Source Generator feature is rely on C# 9.0. If you are using Unity 2021.2, that supports [Source Generators](https://docs.unity3d.com/2021.2/Documentation/Manual/roslyn-analyzers.html). Add the `UnitGenerator.dll` from the [releases page](https://github.com/Cysharp/UnitGenerator/releases), disable Any Platform, disable Include all platforms and set label as `RoslynAnalyzer`.

It works in Unity Editor however does not work on IDE because Unity does not generate analyzer reference to `.csproj`. We provides [CsprojModifer](https://github.com/Cysharp/CsprojModifier) to analyzer support, uses `Add analyzer references to generated .csproj` supports both IDE and Unity Editor.

Unity(2020) does not support C# 9.0 so can not use directly. However, C# Source Genertor supports output source as file.

1. Create `UnitSourceGen.csproj`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>

        <!-- add this two lines and configure output path -->
        <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
        <CompilerGeneratedFilesOutputPath>$(ProjectDir)..\Generated</CompilerGeneratedFilesOutputPath>
    </PropertyGroup>

    <ItemGroup>
        <!-- reference UnitGenerator -->
        <PackageReference Include="UnitGenerator" Version="1.0.0" />

        <!-- add target sources path from Unity -->
        <Compile Include="..\MyUnity\Assets\Scripts\Models\**\*.cs" />
    </ItemGroup>
</Project>
```

2. install [.NET SDK](https://dotnet.microsoft.com/download) and run this command.

```
dotnet build UnitSourceGen.csproj
```

File will be generated under `UnitGenerator\UnitGenerator.SourceGenerator\*.Generated.cs`. `UnitOfAttribute` is also included in generated folder, so at first, run build command and get attribute to configure.

License
---
This library is under the MIT License.
