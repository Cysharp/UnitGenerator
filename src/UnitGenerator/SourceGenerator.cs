using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace UnitGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(x => SetDefaultAttribute(x));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var receiver = context.SyntaxReceiver as SyntaxReceiver;
                if (receiver == null) return;

                var list = new List<(StructDeclarationSyntax, UnitOfAttributeProperty)>();
                foreach (var (type, attr) in receiver.Targets)
                {
                    if (attr.ArgumentList is null) continue;

                    var model = context.Compilation.GetSemanticModel(type.SyntaxTree);

                    // retrieve attribute parameter
                    var prop = new UnitOfAttributeProperty();

                    if (attr.ArgumentList is null) goto ADD;

                    for (int i = 0; i < attr.ArgumentList.Arguments.Count; i++)
                    {
                        var arg = attr.ArgumentList.Arguments[i];
                        var expr = arg.Expression;

                        if (i == 0) // Type type
                        {
                            if (expr is TypeOfExpressionSyntax typeOfExpr)
                            {
                                var typeSymbol = model.GetSymbolInfo(typeOfExpr.Type).Symbol as ITypeSymbol;
                                if (typeSymbol == null) throw new Exception("require type-symbol.");
                                prop.Type = typeSymbol;
                            }
                            else
                            {
                                throw new Exception("require UnitOf attribute and ctor.");
                            }
                        }
                        else if (i == 1) // UnitGenerateOptions options
                        {
                            // e.g. UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.ParseMethod => ImplicitOperatior, ParseMethod
                            var parsed = Enum.ToObject(typeof(UnitGenerateOptions), model.GetConstantValue(expr).Value);
                            prop.Options = (UnitGenerateOptions)parsed;
                        }
                        else
                        {
                            var argName = arg.NameEquals?.Name.ToString();
                            switch (argName)
                            {
                                case "ArithmeticOperators":
                                    var parsed = Enum.ToObject(typeof(UnitGenerateArithmeticOperators), model.GetConstantValue(expr).Value);
                                    prop.ArithmeticOperators = (UnitGenerateArithmeticOperators)parsed;
                                    break;
                                case "Format":
                                    var format = model.GetConstantValue(expr).Value?.ToString();
                                    prop.ToStringFormat = format;
                                    break;
                            }
                        }
                    }

                ADD:
                    list.Add((type, prop));
                }

                foreach (var (type, prop) in list)
                {
                    var typeSymbol = context.Compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type);
                    if (typeSymbol == null) throw new Exception("can not get typeSymbol.");

                    var template = new CodeTemplate
                    {
                        Name = typeSymbol.Name,
                        Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
                        Type = prop.Type.ToString(),
                        Options = prop.Options,
                        ToStringFormat = prop.ToStringFormat
                    };

                    var text = GenerateType(typeSymbol, prop);
                    if (template.Namespace == null)
                    {
                        context.AddSource($"{template.Name}.Generated.cs", text);
                    }
                    else
                    {
                        context.AddSource($"{template.Namespace}.{template.Name}.Generated.cs", text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private void SetDefaultAttribute(GeneratorPostInitializationContext context)
        {
            var attrCode = """
// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY UnitGenerator. DO NOT CHANGE IT.
// </auto-generated>
#pragma warning disable CS8669
#pragma warning disable CS8625
using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UnitGenerator
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    internal class UnitOfAttribute : Attribute
    {
        public Type Type { get; }
        public UnitGenerateOptions Options { get; }
        public UnitGenerateArithmeticOperators ArithmeticOperators { get; set; }
        public string Format { get; set; }

        public UnitOfAttribute(Type type, UnitGenerateOptions options = UnitGenerateOptions.None, string toStringFormat = null)
        {
            this.Type = type;
            this.Options = options;
            this.Format = toStringFormat;
        }
    }
    
    [Flags]
    internal enum UnitGenerateOptions
    {
        None = 0,
        ImplicitOperator = 1,
        ParseMethod = 1 << 1,
        MinMaxMethod = 1 << 2,
        ArithmeticOperator = 1 << 3,
        ValueArithmeticOperator = 1 << 4,
        Comparable = 1 << 5,
        Validate = 1 << 6,
        JsonConverter = 1 << 7,
        MessagePackFormatter = 1 << 8,
        DapperTypeHandler = 1 << 9,
        EntityFrameworkValueConverter = 1 << 10,
        WithoutComparisonOperator = 1 << 11,
        JsonConverterDictionaryKeySupport = 1 << 12,
        Normalize = 1 << 13,
    }

    [Flags]
    internal enum UnitGenerateArithmeticOperators
    {
        Number = 0,
        Addition = 1,
        Subtraction = 1 << 1,
        Multiply = 1 << 2,
        Division = 1 << 3,
        Increment = 1 << 4,
        Decrement = 1 << 5,
    }
    
#if NET7_0_OR_GREATER
   internal static class AsNumber<T> where T : INumber<T>
   {
        public static T One => T.One;
        public static int Radix => T.Radix;
        public static T Zero => T.Zero;
        public static T Abs(T value) => T.Abs(value);
        public static T AdditiveIdentity => T.AdditiveIdentity;
        public static T MultiplicativeIdentity => T.MultiplicativeIdentity;
        public static bool IsCanonical(T value) => T.IsCanonical(value);
        public static bool IsComplexNumber(T value) => T.IsComplexNumber(value);
        public static bool IsEvenInteger(T value) => T.IsEvenInteger(value);
        public static bool IsFinite(T value) => T.IsFinite(value);
        public static bool IsImaginaryNumber(T value) => T.IsImaginaryNumber(value);
        public static bool IsInfinity(T value) => T.IsInfinity(value);
        public static bool IsInteger(T value) => T.IsInteger(value);
        public static bool IsNaN(T value) => T.IsNaN(value);
        public static bool IsNegative(T value) => T.IsNegative(value);
        public static bool IsNegativeInfinity(T value) => T.IsNegativeInfinity(value);
        public static bool IsNormal(T value) => T.IsNormal(value);
        public static bool IsOddInteger(T value) => T.IsOddInteger(value);
        public static bool IsPositive(T value) => T.IsPositive(value);
        public static bool IsPositiveInfinity(T value) => T.IsPositiveInfinity(value);
        public static bool IsRealNumber(T value) => T.IsRealNumber(value);
        public static bool IsSubnormal(T value) => T.IsSubnormal(value);
        public static bool IsZero(T value) => T.IsZero(value);
        public static T MaxMagnitude(T x, T y) => T.MaxMagnitude(x, y);
        public static T MaxMagnitudeNumber(T x, T y) => T.MaxMagnitudeNumber(x, y);
        public static T MinMagnitude(T x, T y) => T.MinMagnitude(x, y);
        public static T MinMagnitudeNumber(T x, T y) => T.MinMagnitudeNumber(x, y);
   }
#endif    
}

""";
            context.AddSource("UnitOfAttribute.cs", attrCode);
        }

        private string GenerateType(INamedTypeSymbol symbol, UnitOfAttributeProperty prop)
        {
            var unitTypeName = symbol.Name;
            var innerTypeName = prop.TypeName;
            var ns = symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString();

            var sb = new StringBuilder("""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY UnitGenerator. DO NOT CHANGE IT.
// </auto-generated>
#pragma warning disable CS8669
using System;
using System.Globalization;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

""");
            if (prop.HasFlag(UnitGenerateOptions.MessagePackFormatter))
            {
                sb.AppendLine("""
using MessagePack;
using MessagePack.Formatters;
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.JsonConverter))
            {
                sb.AppendLine("""
using System.Text.Json;
using System.Text.Json.Serialization;
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.DapperTypeHandler))
            {
                sb.AppendLine("""
using System.Runtime.CompilerServices;
""");
            }
            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine($$"""
namespace {{ns}}
{
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.JsonConverter))
            {
                sb.AppendLine($$"""
    [JsonConverter(typeof({{unitTypeName}}JsonConverter))]
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.MessagePackFormatter))
            {
                sb.AppendLine($$"""
    [MessagePackFormatter(typeof({{unitTypeName}}MessagePackFormatter))]
""");
            }

            sb.AppendLine($$"""
    [System.ComponentModel.TypeConverter(typeof({{unitTypeName}}TypeConverter))]
    readonly partial struct {{unitTypeName}}
""");
            if (prop.IsNumber())
            {
                sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        : INumber<{{unitTypeName}}>
#else
        : IEquatable<{{unitTypeName}}>
        , IComparable<{{unitTypeName}}>
        , IComparable
#endif
""");
            }
            else
            {
                sb.AppendLine($$"""
        : IEquatable<{{unitTypeName}}>
#if NET7_0_OR_GREATER
        , IEqualityOperators<{{unitTypeName}}, {{unitTypeName}}, bool>
#endif
""");
                if (prop.HasFlag(UnitGenerateOptions.Comparable))
                {
                    sb.AppendLine($$"""
        , IComparable<{{unitTypeName}}>
""");
                    if (!prop.HasFlag(UnitGenerateOptions.WithoutComparisonOperator))
                    {
                        sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        , IComparisonOperators<{{unitTypeName}}, {{unitTypeName}}, bool>
#endif
""");
                    }
                }
                if (prop.HasFlag(UnitGenerateOptions.ArithmeticOperator))
                {
                    sb.AppendLine("#if NET7_0_OR_GREATER");
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Addition))
                    {
                        sb.AppendLine($$"""
        , IAdditionOperators<{{unitTypeName}}, {{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Subtraction))
                    {
                        sb.AppendLine($$"""
        , ISubtractionOperators<{{unitTypeName}}, {{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Addition) ||
                        prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Subtraction))
                    {
                        sb.AppendLine($$"""
        , IAdditiveIdentity<{{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Multiply))
                    {
                        sb.AppendLine($$"""
        , IMultiplyOperators<{{unitTypeName}}, {{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Division))
                    {
                        sb.AppendLine($$"""
        , IDivisionOperators<{{unitTypeName}}, {{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Multiply) ||
                        prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Division))
                    {
                        sb.AppendLine($$"""
        , IMultiplicativeIdentity<{{unitTypeName}}, {{unitTypeName}}>
        , IUnaryPlusOperators<{{unitTypeName}}, {{unitTypeName}}>
        , IUnaryNegationOperators<{{unitTypeName}}, {{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Increment))
                    {
                        sb.AppendLine($$"""
        , IIncrementOperators<{{unitTypeName}}>
""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Decrement))
                    {
                        sb.AppendLine($$"""
        , IDecrementOperators<{{unitTypeName}}>
""");
                    }
                    sb.AppendLine("#endif");
                }
            }

            sb.AppendLine($$"""
    {
        readonly {{innerTypeName}} value;

        public {{innerTypeName}} AsPrimitive() => value;

        public {{unitTypeName}}({{innerTypeName}} value)
        {
            this.value = value;
""");
            if (prop.HasFlag(UnitGenerateOptions.Normalize))
            {
                sb.AppendLine("""
            this.Normalize(ref this.value);
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.Validate))
            {
                sb.AppendLine("""
            this.Validate();
""");
            }
            sb.AppendLine("""
        }
        
""");
            if (prop.HasFlag(UnitGenerateOptions.Normalize))
            {
                sb.AppendLine($$"""
        private partial void Normalize(ref {{innerTypeName}} value);
        
""");
                
            }
            
            if (prop.HasFlag(UnitGenerateOptions.Validate))
            {
                sb.AppendLine("""
        private partial void Validate();

""");
            }

            var convertModifier = prop.HasFlag(UnitGenerateOptions.ImplicitOperator) ? "implicit"  : "explicit";
            sb.AppendLine($$"""
        public static {{convertModifier}} operator {{innerTypeName}}({{unitTypeName}} value)
        {
            return value.value;
        }

        public static {{convertModifier}} operator {{unitTypeName}}({{innerTypeName}} value)
        {
            return new {{unitTypeName}}(value);
        }

        public bool Equals({{unitTypeName}} other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var t = obj.GetType();
            if (t == typeof({{unitTypeName}}))
            {
                return Equals(({{unitTypeName}})obj);
            }
            if (t == typeof({{innerTypeName}}))
            {
                return value.Equals(({{innerTypeName}})obj);
            }

            return value.Equals(obj);
        }
        
        public static bool operator ==({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return x.value.Equals(y.value);
        }

        public static bool operator !=({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return !x.value.Equals(y.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

""");
            if (prop.ToStringFormat is { } format)
            {
                sb.AppendLine($$"""
        public override string ToString()
        {
            return string.Format({{format}}, value);
        }

""");
            }
            else
            {
                sb.AppendLine("""
        public override string ToString()
        {
            return value.ToString();
        }

""");
            }

            if (prop.IsGuid())
            {
                sb.AppendLine($$"""
        public static readonly {{unitTypeName}} Empty = default({{unitTypeName}});

        public static {{unitTypeName}} New()
        {
            return new {{unitTypeName}}(Guid.NewGuid());
        }

        public static {{unitTypeName}} New{{unitTypeName}}()
        {
            return new {{unitTypeName}}(Guid.NewGuid());
        }

""");
            }

            if (prop.IsUlid())
            {
                sb.AppendLine($$"""
        public static readonly {{unitTypeName}} Empty = default({{unitTypeName}});
        
        public static {{unitTypeName}} New()
        {
            return new {{unitTypeName}}(Ulid.NewUlid());
        }

        public static {{unitTypeName}} New{{unitTypeName}}()
        {
            return new {{unitTypeName}}(Ulid.NewUlid());
        }

""");
            }

            if (prop.HasFlag(UnitGenerateOptions.ParseMethod))
            {
                    sb.AppendLine("""
        // UnitGenerateOptions.ParseMethod
        
""");
                if (prop.IsString())
                {
                    sb.AppendLine($$"""
        public static {{unitTypeName}} Parse(string s)
        {
            return new {{unitTypeName}}(s);
        }
        
        public static bool TryParse(string s, out {{unitTypeName}} result)
        {
            try
            {
                result = Parse(s);    
                return true;
            } 
             catch 
            {
                result = default;
                return false;
            }
        }

""");
                }
                else
                {
                    sb.AppendLine($$"""
        public static {{unitTypeName}} Parse(string s)
        {
            return new {{unitTypeName}}({{innerTypeName}}.Parse(s));
        }
 
        public static bool TryParse(string s, out {{unitTypeName}} result)
        {
            if ({{innerTypeName}}.TryParse(s, out var r))
            {
                result = new {{unitTypeName}}(r);
                return true;
            }
            else
            {
                result = default({{unitTypeName}});
                return false;
            }
        }

""");
                }
            }
            if (prop.HasFlag(UnitGenerateOptions.MinMaxMethod))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.MinMaxMethod

        public static {{unitTypeName}} Min({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return new {{unitTypeName}}(Math.Min(x.value, y.value));
        }

        public static {{unitTypeName}} Max({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return new {{unitTypeName}}(Math.Max(x.value, y.value));
        }

""");
            }

            if (prop.IsBool())
            {
                sb.AppendLine($$"""
        // Default

        public static {{innerTypeName}} operator true({{unitTypeName}} x)
        {
            return x.value;
        }
         
        public static {{innerTypeName}} operator false({{unitTypeName}} x)
        {
            return !x.value;
        }
         
        public static {{innerTypeName}} operator !({{unitTypeName}} x)
        {
            return !x.value;
        }

 """);
            }

            if (prop.HasFlag(UnitGenerateOptions.ArithmeticOperator))
            {
                sb.AppendLine("""
        // UnitGenerateOptions.ArithmeticOperator
        
""");
                if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Addition) ||
                    prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Subtraction))
                {
                    sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        public static {{unitTypeName}} AdditiveIdentity => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.AdditiveIdentity);
#endif

""");
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Addition))
                    {
                       sb.AppendLine($$"""
        public static {{unitTypeName}} operator +({{unitTypeName}} x, {{unitTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value + y.value));
            }
        }

""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Subtraction))
                    {
                        sb.AppendLine($$"""
        public static {{unitTypeName}} operator -({{unitTypeName}} x, {{unitTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value - y.value));
            }
        }

""");
                    }
                } // End Addition, Subtraction

                if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Multiply) ||
                    prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Division))
                {
                    sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        public static {{unitTypeName}} MultiplicativeIdentity => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.MultiplicativeIdentity);
#endif
        public static {{unitTypeName}} operator +({{unitTypeName}} value) => new(({{innerTypeName}})(+value.value));
        public static {{unitTypeName}} operator -({{unitTypeName}} value) => new(({{innerTypeName}})(-value.value));

""");
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Multiply))
                    {
                       sb.AppendLine($$"""
        public static {{unitTypeName}} operator *({{unitTypeName}} x, {{unitTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value * y.value));
            }
        }

""");
                    }
                    if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Division))
                    {
                        sb.AppendLine($$"""

        public static {{unitTypeName}} operator /({{unitTypeName}} x, {{unitTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value / y.value));
            }
        }

""");
                    }
                } // End Multiply, Division

                if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Increment))
                {
                    sb.AppendLine($$"""
        public static {{unitTypeName}} operator ++({{unitTypeName}} x)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(({{innerTypeName}})(x.value + 1)));
            }
        }

""");
                }
                if (prop.HasArithmeticOperator(UnitGenerateArithmeticOperators.Decrement))
                {
                    sb.AppendLine($$"""
        public static {{unitTypeName}} operator --({{unitTypeName}} x)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(({{innerTypeName}})(x.value - 1)));
            }
        }

""");
                }
            } // End ArithmeticOperator

            if (prop.HasFlag(UnitGenerateOptions.ValueArithmeticOperator))
            {
                sb.AppendLine("""
        // UnitGenerateOptions.ValueArithmeticOperator
        
""");
                if (prop.HasValueArithmeticOperator(UnitGenerateArithmeticOperators.Addition))
                {
                       sb.AppendLine($$"""
        public static {{unitTypeName}} operator +({{unitTypeName}} x, {{innerTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value + y));
            }
        }

""");
                }
                if (prop.HasValueArithmeticOperator(UnitGenerateArithmeticOperators.Subtraction))
                {
                        sb.AppendLine($$"""
        public static {{unitTypeName}} operator -({{unitTypeName}} x, {{innerTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value - y));
            }
        }

""");
                }
                if (prop.HasValueArithmeticOperator(UnitGenerateArithmeticOperators.Multiply))
                {
                       sb.AppendLine($$"""
        public static {{unitTypeName}} operator *({{unitTypeName}} x, {{innerTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value * y));
            }
        }

""");
                }
                if (prop.HasValueArithmeticOperator(UnitGenerateArithmeticOperators.Division))
                {
                        sb.AppendLine($$"""

        public static {{unitTypeName}} operator /({{unitTypeName}} x, {{innerTypeName}} y)
        {
            checked
            {
                return new {{unitTypeName}}(({{innerTypeName}})(x.value / y));
            }
        }

""");
                }
            } // End ValueArithmeticOperator

            if (prop.IsNumber() || prop.HasFlag(UnitGenerateOptions.Comparable))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.Comparable

        public int CompareTo({{unitTypeName}} other)
        {
            return value.CompareTo(other.value);
        }
        
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is {{unitTypeName}} other)
            {
                return value.CompareTo(other.value); 
            }
            throw new ArgumentException();        
        }
""");
            }
            if (prop.IsNumber() ||
                (prop.HasFlag(UnitGenerateOptions.Comparable) && !prop.HasFlag(UnitGenerateOptions.WithoutComparisonOperator)))
            {
                sb.AppendLine($$"""
        public static bool operator >({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return x.value > y.value;
        }

        public static bool operator <({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return x.value < y.value;
        }

        public static bool operator >=({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return x.value >= y.value;
        }

        public static bool operator <=({{unitTypeName}} x, {{unitTypeName}} y)
        {
            return x.value <= y.value;
        }

""");
            }

            if (prop.IsNumber())
            {
                sb.AppendLine($$"""
        public static {{unitTypeName}} operator %({{unitTypeName}} x, {{unitTypeName}} y) => new {{unitTypeName}}(({{innerTypeName}})(x.value % y.value)); 
        
        // IFormattable<T>
        
        public string ToString(string? format, IFormatProvider? formatProvider) => value.ToString(format, formatProvider);

#if NET6_0_OR_GREATER
        // ISpanFormattable<T>    
                    
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) 
        {
            return value.TryFormat(destination, out charsWritten, format, provider);
        }
#endif        
#if NET7_0_OR_GREATER
        // IParsable<T>
        
        public static {{unitTypeName}} Parse(string s, IFormatProvider? provider) => new {{unitTypeName}}({{innerTypeName}}.Parse(s, provider));

        public static bool TryParse(string? s, IFormatProvider? provider, out {{unitTypeName}} result) 
        {
            if ({{innerTypeName}}.TryParse(s, provider, out var parsedValue))
            {
                result = new {{unitTypeName}}(parsedValue);
                return true;
            }
            result = default;
            return false;
        }
   
        // ISpanParsable<T>          
                                                                                   
        public static {{unitTypeName}} Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new {{unitTypeName}}({{innerTypeName}}.Parse(s, provider));
        
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out {{unitTypeName}} result) 
        {
            if ({{innerTypeName}}.TryParse(s, provider, out var parsedValue))
            {
                result = new {{unitTypeName}}(parsedValue);
                return true;
            }
            result = default;
            return false;
        }

        // INumberBase<T>
        
        public static {{unitTypeName}} One => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.One);
        public static int Radix => global::UnitGenerator.AsNumber<{{innerTypeName}}>.Radix;
        public static {{unitTypeName}} Zero => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.Zero);
        public static {{unitTypeName}} Abs({{unitTypeName}} value) => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.Abs(value.value));
        public static bool IsCanonical({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsCanonical(value.value);
        public static bool IsComplexNumber({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsComplexNumber(value.value);
        public static bool IsEvenInteger({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsEvenInteger(value.value);
        public static bool IsFinite({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsFinite(value.value);
        public static bool IsImaginaryNumber({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsImaginaryNumber(value.value);
        public static bool IsInfinity({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsInfinity(value.value);
        public static bool IsInteger({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsInteger(value.value);
        public static bool IsNaN({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsNaN(value.value);
        public static bool IsNegative({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsNegative(value.value);
        public static bool IsNegativeInfinity({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsNegativeInfinity(value.value);
        public static bool IsNormal({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsNormal(value.value);
        public static bool IsOddInteger({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsOddInteger(value.value);
        public static bool IsPositive({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsPositive(value.value);
        public static bool IsPositiveInfinity({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsPositiveInfinity(value.value);
        public static bool IsRealNumber({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsRealNumber(value.value);
        public static bool IsSubnormal({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsSubnormal(value.value);
        public static bool IsZero({{unitTypeName}} value) => global::UnitGenerator.AsNumber<{{innerTypeName}}>.IsZero(value.value);
        public static {{unitTypeName}} MaxMagnitude({{unitTypeName}} x, {{unitTypeName}} y) => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.MaxMagnitude(x.value, y.value));
        public static {{unitTypeName}} MaxMagnitudeNumber({{unitTypeName}} x, {{unitTypeName}} y) => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.MaxMagnitudeNumber(x.value, y.value));
        public static {{unitTypeName}} MinMagnitude({{unitTypeName}} x, {{unitTypeName}} y) => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.MinMagnitude(x.value, y.value));
        public static {{unitTypeName}} MinMagnitudeNumber({{unitTypeName}} x, {{unitTypeName}} y) => new(global::UnitGenerator.AsNumber<{{innerTypeName}}>.MinMagnitudeNumber(x.value, y.value));

        public static bool TryConvertFromChecked<TOther>(TOther value, out {{unitTypeName}} result) where TOther : INumberBase<TOther> 
        {
            throw new NotSupportedException();
        }

        public static bool TryConvertFromSaturating<TOther>(TOther value, out {{unitTypeName}} result) where TOther : INumberBase<TOther> 
        {
            throw new NotSupportedException();
        }

        public static bool TryConvertFromTruncating<TOther>(TOther value, out {{unitTypeName}} result) where TOther : INumberBase<TOther>
        {
            throw new NotSupportedException();
        }

        public static bool TryConvertToChecked<TOther>({{unitTypeName}} value, out TOther result) where TOther : INumberBase<TOther>
        {
            throw new NotSupportedException();
        }

        public static bool TryConvertToSaturating<TOther>({{unitTypeName}} value, out TOther result) where TOther : INumberBase<TOther> 
        {
            throw new NotSupportedException();
        }

        public static bool TryConvertToTruncating<TOther>({{unitTypeName}} value, out TOther result) where TOther : INumberBase<TOther>
        {
            throw new NotSupportedException();
        }

        public static {{unitTypeName}} Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => new {{unitTypeName}}({{innerTypeName}}.Parse(s, style, provider));
        public static {{unitTypeName}} Parse(string s, NumberStyles style, IFormatProvider? provider) => new {{unitTypeName}}({{innerTypeName}}.Parse(s, style, provider)); 

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out {{unitTypeName}} result) 
        {
            if ({{innerTypeName}}.TryParse(s, style, provider, out var value))
            {
                result = new {{unitTypeName}}(value);
                return true;
            }
            result = default;
            return false;
        }

        public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out {{unitTypeName}} result)
        {
            if ({{innerTypeName}}.TryParse(s, style, provider, out var value))
            {
                result = new {{unitTypeName}}(value);
                return true;
            }
            result = default;
            return false;
        }        
#endif

""");
            } // End Number

            if (prop.HasFlag(UnitGenerateOptions.JsonConverter))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.JsonConverter
        
        private class {{unitTypeName}}JsonConverter : JsonConverter<{{unitTypeName}}>
        {
            public override void Write(Utf8JsonWriter writer, {{unitTypeName}} value, JsonSerializerOptions options)
            {
                var converter = options.GetConverter(typeof({{innerTypeName}})) as JsonConverter<{{innerTypeName}}>;
                if (converter != null)
                {
                    converter.Write(writer, value.value, options);
                }
                else
                {
                    throw new JsonException($"{typeof({{innerTypeName}})} converter does not found.");
                }
            }

            public override {{unitTypeName}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var converter = options.GetConverter(typeof({{innerTypeName}})) as JsonConverter<{{innerTypeName}}>;
                if (converter != null)
                {
                    return new {{unitTypeName}}(converter.Read(ref reader, typeToConvert, options));
                }
                else
                {
                    throw new JsonException($"{typeof({{innerTypeName}})} converter does not found.");
                }
            }

""");
                if (prop.HasFlag(UnitGenerateOptions.JsonConverterDictionaryKeySupport))
                {
                    if (prop.IsSupportUtf8Formatter())
                    {
                        sb.AppendLine($$"""
            public override void WriteAsPropertyName(Utf8JsonWriter writer, {{unitTypeName}} value, JsonSerializerOptions options)
            {
                Span<byte> buffer = stackalloc byte[36];
                if (System.Buffers.Text.Utf8Formatter.TryFormat(value.value, buffer, out var written))
                {
                    writer.WritePropertyName(buffer.Slice(0, written));
                }
                else
                {
                    writer.WritePropertyName(value.value.ToString());
                }
            }

            public override {{unitTypeName}} ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (System.Buffers.Text.Utf8Parser.TryParse(reader.ValueSpan, out {{innerTypeName}} value, out var consumed))
                {
                    return new {{unitTypeName}}(value);
                }
                else
                {
                    return new {{unitTypeName}}({{innerTypeName}}.Parse(reader.GetString()));
                }
            }

""");
                    }
                    else if (prop.IsUlid())
                    {
                        sb.AppendLine($$"""
            public override void WriteAsPropertyName(Utf8JsonWriter writer, {{unitTypeName}} value, JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.value.ToString());
            }
    
            public override {{unitTypeName}} ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new {{unitTypeName}}({{innerTypeName}}.Parse(reader.GetString()));
            }

""");
                    }
                    else if (prop.IsString())
                    {
                        sb.AppendLine($$"""
            public override void WriteAsPropertyName(Utf8JsonWriter writer, {{unitTypeName}} value, JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.value.ToString());
            }

            public override {{unitTypeName}} ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new {{unitTypeName}}(reader.GetString());            
            }

""");
                    }
                    else
                    {
                        sb.AppendLine($$"""
            public override void WriteAsPropertyName(Utf8JsonWriter writer, {{unitTypeName}} value, JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.value.ToString());
            }

            public override {{unitTypeName}} ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new {{unitTypeName}}({{innerTypeName}}.Parse(reader.GetString()));
            }

""");
                    }
                } // End JsonConverterDictionaryKeySupport

                sb.AppendLine($$"""
        }

""");
            } // End JsonConverter

            if (prop.HasFlag(UnitGenerateOptions.MessagePackFormatter))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.MessagePackFormatter
        private class {{unitTypeName}}MessagePackFormatter : IMessagePackFormatter<{{unitTypeName}}>
        {
            public void Serialize(ref MessagePackWriter writer, {{unitTypeName}} value, MessagePackSerializerOptions options)
            {
                options.Resolver.GetFormatterWithVerify<{{innerTypeName}}>().Serialize(ref writer, value.value, options);
            }

            public {{unitTypeName}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                return new {{unitTypeName}}(options.Resolver.GetFormatterWithVerify<{{innerTypeName}}>().Deserialize(ref reader, options));
            }
        }

""");
            } // End MessagePackFormatter

            if (prop.HasFlag(UnitGenerateOptions.DapperTypeHandler))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.DapperTypeHandler
        public class {{unitTypeName}}TypeHandler : Dapper.SqlMapper.TypeHandler<{{unitTypeName}}>
        {
            public override {{unitTypeName}} Parse(object value)
            {
                return new {{unitTypeName}}(({{innerTypeName}})value);
            }

            public override void SetValue(System.Data.IDbDataParameter parameter, {{unitTypeName}} value)
            {
                parameter.DbType = System.Data.DbType.{{prop.GetDbType()}};
                parameter.Value = value.value;
            }
        }

        [ModuleInitializer]
        public static void AddTypeHandler()
        {
            Dapper.SqlMapper.AddTypeHandler(new {{unitTypeName}}.{{unitTypeName}}TypeHandler());
        }

""");
            } // End DapperTypeHandler

            if (prop.HasFlag(UnitGenerateOptions.EntityFrameworkValueConverter))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.EntityFrameworkValueConverter
        public class {{unitTypeName}}ValueConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<{{unitTypeName}}, {{innerTypeName}}>
        {
            public {{unitTypeName}}ValueConverter()
                : base(
                        convertToProviderExpression: x => x.value,
                        convertFromProviderExpression: x => new {{unitTypeName}}(x))
            {
            }

            public {{unitTypeName}}ValueConverter(Microsoft.EntityFrameworkCore.Storage.ValueConversion.ConverterMappingHints mappingHints = null)
                : base(
                        convertToProviderExpression: x => x.value,
                        convertFromProviderExpression: x => new {{unitTypeName}}(x),
                        mappingHints: mappingHints)
            {
            }
        }

""");
            } // End EntityFrameworkValueConverter


            sb.AppendLine($$"""
        // Default
        
        private class {{unitTypeName}}TypeConverter : System.ComponentModel.TypeConverter
        {
            private static readonly Type WrapperType = typeof({{unitTypeName}});
            private static readonly Type ValueType = typeof({{innerTypeName}});

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
                    if (t == typeof({{unitTypeName}}))
                    {
                        return ({{unitTypeName}})value;
                    }
                    if (t == typeof({{innerTypeName}}))
                    {
                        return new {{unitTypeName}}(({{innerTypeName}})value);
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is {{unitTypeName}} wrappedValue)
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
""");

            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        struct UnitOfAttributeProperty
        {
            public ITypeSymbol Type { get; set; }
            public UnitGenerateOptions Options { get; set; }
            public UnitGenerateArithmeticOperators ArithmeticOperators { get; set; }
            public string? ToStringFormat { get; set; }
            public string TypeName => Type.ToString();

            public bool IsString() => TypeName is "string";
            public bool IsBool() => TypeName is "bool";
            public bool IsUlid() => TypeName is "Ulid" or "System.Ulid";
            public bool IsGuid() => TypeName is "Guid" or "System.Guid";

            public bool HasFlag(UnitGenerateOptions options) => Options.HasFlag(options);

            public bool IsNumber() => HasFlag(UnitGenerateOptions.ArithmeticOperator) &&
                                      ArithmeticOperators == UnitGenerateArithmeticOperators.Number;

            public bool HasArithmeticOperator(UnitGenerateArithmeticOperators op)
            {
                return HasFlag(UnitGenerateOptions.ArithmeticOperator) &&
                       (ArithmeticOperators == UnitGenerateArithmeticOperators.Number ||
                        ArithmeticOperators.HasFlag(op));
            }

            public bool HasValueArithmeticOperator(UnitGenerateArithmeticOperators op)
            {
                return HasFlag(UnitGenerateOptions.ValueArithmeticOperator) &&
                       (ArithmeticOperators == UnitGenerateArithmeticOperators.Number ||
                        ArithmeticOperators.HasFlag(op));
            }

            public DbType GetDbType()
            {
                return TypeName switch
                {
                    "short" => DbType.Int16,
                    "int" => DbType.Int32,
                    "long" => DbType.Int64,
                    "ushort" => DbType.UInt16,
                    "uint" => DbType.UInt32,
                    "ulong" => DbType.UInt64,
                    "string" => DbType.AnsiString,
                    "byte[]" => DbType.Binary,
                    "bool" => DbType.Boolean,
                    "byte" => DbType.Byte,
                    "sbyte" => DbType.SByte,
                    "float" => DbType.Single,
                    "double" => DbType.Double,
                    "System.DateTime" => DbType.DateTime,
                    "System.DateTimeOffset" => DbType.DateTimeOffset,
                    "System.TimeSpan" => DbType.Time,
                    "System.Guid" => DbType.Guid,
                    "decimal" => DbType.Currency,
                    _ => DbType.Object
                };
            }

            public bool IsSupportUtf8Formatter()
            {
                return TypeName switch
                {
                    "short" => true,
                    "int" => true,
                    "long" => true,
                    "ushort" => true,
                    "uint" => true,
                    "ulong" => true,
                    "bool" => true,
                    "byte" => true,
                    "sbyte" => true,
                    "float" => true,
                    "double" => true,
                    "System.DateTime" => true,
                    "System.DateTimeOffset" => true,
                    "System.TimeSpan" => true,
                    "System.Guid" => true,
                    _ => false
                };
            }
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<(StructDeclarationSyntax type, AttributeSyntax attr)> Targets { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is StructDeclarationSyntax s && s.AttributeLists.Count > 0)
                {
                    var attr = s.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "UnitOf" or "UnitOfAttribute" or "UnitGenerator.UnitOf" or "UnitGenerator.UnitOfAttribute");
                    if (attr != null)
                    {
                        Targets.Add((s, attr));
                    }
                }
            }
        }
    }
}