﻿using Microsoft.CodeAnalysis;
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

                var symbols = ReferenceSymbols.Create(context.Compilation);
                var list = new List<(StructDeclarationSyntax, UnitOfAttributeProperty)>();
                foreach (var (type, attr, targetType) in receiver.Targets)
                {
                    var model = context.Compilation.GetSemanticModel(type.SyntaxTree);

                    // retrieve attribute parameter
                    var prop = new UnitOfAttributeProperty
                    {
                        ReferenceSymbols = symbols,
                        ArithmeticOperators = UnitArithmeticOperators.All
                    };

                    if(targetType is not null)
                    {
                        var typeSymbol = model.GetSymbolInfo(targetType).Symbol as ITypeSymbol;
                        if (typeSymbol == null) throw new Exception("require type-symbol.");
                        prop.Type = typeSymbol;
                    }

                    for (int i = 0; i < (attr.ArgumentList?.Arguments.Count ?? 0); i++)
                    {
                        var arg = attr.ArgumentList!.Arguments[i];
                        var expr = arg.Expression;

                        var argName = arg.NameEquals?.Name.ToString();
                        switch (argName)
                        {
                            case "ArithmeticOperators":
                            {
                                var parsed = Enum.ToObject(typeof(UnitArithmeticOperators), model.GetConstantValue(expr).Value);
                                prop.ArithmeticOperators = (UnitArithmeticOperators)parsed;
                                break;
                            }
                            case "ToStringFormat":
                            {
                                var format = model.GetConstantValue(expr).Value?.ToString();
                                prop.ToStringFormat = format;
                                break;
                            }
                            default:
                                if (i == 0) // Type type
                                {
                                    if (expr is TypeOfExpressionSyntax typeOfExpr)
                                    {
                                        var typeSymbol = model.GetSymbolInfo(typeOfExpr.Type).Symbol as ITypeSymbol;
                                        if (typeSymbol == null) throw new Exception("require type-symbol.");
                                        prop.Type = typeSymbol;
                                    }
                                    else if (targetType is not null) // UnitGenerateOptions options
                                    {
                                        // e.g. UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.ParseMethod => ImplicitOperatior, ParseMethod
                                        var parsed = Enum.ToObject(typeof(UnitGenerateOptions), model.GetConstantValue(expr).Value);
                                        prop.Options = (UnitGenerateOptions)parsed;
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
                                break;
                        }
                    }

                    list.Add((type, prop));
                }

                foreach (var (type, prop) in list)
                {
                    var typeSymbol = context.Compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type);
                    if (typeSymbol == null) throw new Exception("can not get typeSymbol.");

                    var source = GenerateType(typeSymbol, prop);

                    var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
                        ? null
                        : typeSymbol.ContainingNamespace.ToDisplayString();

                    var filename = ns == null
                        ? $"{typeSymbol.Name}.g.cs"
                        : $"{ns}.{typeSymbol.Name}.g.cs";

                    context.AddSource(filename, source);
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
#nullable enable
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
        public UnitArithmeticOperators ArithmeticOperators { get; set; } = UnitArithmeticOperators.All;
        public string? ToStringFormat { get; set; }

        public UnitOfAttribute(Type type, UnitGenerateOptions options = UnitGenerateOptions.None)
        {
            this.Type = type;
            this.Options = options;
        }
    }
    
#if NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    internal class UnitOfAttribute<T> : Attribute
    {
        public Type Type { get; }
        public UnitGenerateOptions Options { get; }
        public UnitArithmeticOperators ArithmeticOperators { get; set; } = UnitArithmeticOperators.All;
        public string? ToStringFormat { get; set; }

        public UnitOfAttribute(UnitGenerateOptions options = UnitGenerateOptions.None)
        {
            this.Type = typeof(T);
            this.Options = options;
        }
    }
#endif

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
    internal enum UnitArithmeticOperators
    {
        All = Addition | Subtraction | Multiply | Division | Increment | Decrement,
        Addition = 1,
        Subtraction = 1 << 1,
        Multiply = 1 << 2,
        Division = 1 << 3,
        Increment = 1 << 4,
        Decrement = 1 << 5,
    }
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

            var anyPlatformInterfaces = new List<string>();
            var net6Interfaces = new List<string>();
            var net7Interfaces = new List<string>();
            var net8Interfaces = new List<string>
            {
                $"IEqualityOperators<{unitTypeName}, {unitTypeName}, bool>"
            };

            sb.AppendLine($$"""
    [System.ComponentModel.TypeConverter(typeof({{unitTypeName}}TypeConverter))]
    readonly partial struct {{unitTypeName}} 
        : IEquatable<{{unitTypeName}}>
""");
            if (prop.HasFlag(UnitGenerateOptions.Comparable) &&
                !prop.HasFlag(UnitGenerateOptions.WithoutComparisonOperator))
            {
                anyPlatformInterfaces.Add($"IComparable<{unitTypeName}>");
                net7Interfaces.Add($"IComparisonOperators<{unitTypeName}, {unitTypeName}, bool>");
            }
            if (prop.HasFormattableInterface())
            {
                anyPlatformInterfaces.Add("IFormattable");
            }
            if (prop.HasSpanFormattableInterface())
            {
                net6Interfaces.Add($"ISpanFormattable");
            }
            if (prop.HasUtf8SpanFormattableInterface())
            {
                net8Interfaces.Add($"IUtf8SpanFormattable");
            }
            if (prop.HasFlag(UnitGenerateOptions.ParseMethod))
            {
                if (prop.HasParsableInterface())
                {
                    net7Interfaces.Add($"IParsable<{unitTypeName}>");
                }
                if (prop.HasSpanParsableInterface())
                {
                    net7Interfaces.Add($"ISpanParsable<{unitTypeName}>");
                }
                if (prop.HasUtf8SpanParsableInterface())
                {
                    net8Interfaces.Add($"IUtf8SpanParsable<{unitTypeName}>");
                }
            }

            if (prop.HasFlag(UnitGenerateOptions.ArithmeticOperator))
            {
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Addition))
                {
                    net7Interfaces.Add($"IAdditionOperators<{unitTypeName}, {unitTypeName}, {unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Subtraction))
                {
                    net7Interfaces.Add($"ISubtractionOperators<{unitTypeName}, {unitTypeName}, {unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Multiply))
                {
                    net7Interfaces.Add($"IMultiplyOperators<{unitTypeName}, {unitTypeName}, {unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Division))
                {
                    net7Interfaces.Add($"IDivisionOperators<{unitTypeName}, {unitTypeName}, {unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Multiply) ||
                    prop.HasArithmeticOperator(UnitArithmeticOperators.Division))
                {
                    net7Interfaces.Add($"IUnaryPlusOperators<{unitTypeName}, {unitTypeName}>");
                    net7Interfaces.Add($"IUnaryNegationOperators<{unitTypeName}, {unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Increment))
                {
                    net7Interfaces.Add($"IIncrementOperators<{unitTypeName}>");
                }
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Decrement))
                {
                    net7Interfaces.Add($"IDecrementOperators<{unitTypeName}>");
                }
            }

            foreach (var interfaceName in anyPlatformInterfaces)
            {
                sb.AppendLine($"        , {interfaceName}");
            }
            if (net6Interfaces.Count > 0)
            {
                sb.AppendLine("#if NET6_0_OR_GREATER");
            }
            foreach (var interfaceName in net6Interfaces)
            {
                sb.AppendLine($"        , {interfaceName}");
            }
            if (net6Interfaces.Count > 0)
            {
                sb.AppendLine("#endif");
            }

            if (net7Interfaces.Count > 0)
            {
                sb.AppendLine("#if NET7_0_OR_GREATER");
            }
            foreach (var interfaceName in net7Interfaces)
            {
                sb.AppendLine($"        , {interfaceName}");
            }
            if (net7Interfaces.Count > 0)
            {
                sb.AppendLine("#endif");
            }

            if (net8Interfaces.Count > 0)
            {
                sb.AppendLine("#if NET8_0_OR_GREATER");
            }
            foreach (var interfaceName in net8Interfaces)
            {
                sb.AppendLine($"        , {interfaceName}");
            }
            if (net8Interfaces.Count > 0)
            {
                sb.AppendLine("#endif");
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
        public static implicit operator {{innerTypeName}}({{unitTypeName}} value)
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
            if (prop.IsString())
            {
                if (prop.ToStringFormat is { } format)
                {
                    sb.AppendLine($$"""
        public override string ToString() => value == null ? "null" : string.Format("{{format}}", value);

""");
                }
                else
                {
                    sb.AppendLine("""
        public override string ToString() => value == null ? "null" : value.ToString(); 

""");
                }
            }
            else
            {
                if (prop.ToStringFormat is { } format)
                {
                    sb.AppendLine($$"""
        public override string ToString() => string.Format("{{format}}", value);

""");
                }
                else
                {
                    sb.AppendLine("""
        public override string ToString() => value.ToString();

""");
                }
            }

            if (prop.HasFormattableInterface())
            {
                    sb.AppendLine("""
        public string ToString(string? format, IFormatProvider? formatProvider) => value.ToString(format, formatProvider);

""");
            }
            if (prop.HasSpanFormattableInterface())
            {
                    sb.AppendLine("""
#if NET6_0_OR_GREATER
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
            ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, provider);
#endif
""");
            }
            if (prop.HasUtf8SpanFormattableInterface())
            {
                    sb.AppendLine("""
#if NET8_0_OR_GREATER        
        public bool TryFormat (Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
            ((IUtf8SpanFormattable)value).TryFormat(utf8Destination, out bytesWritten, format, provider);
#endif

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

                if (prop.HasParsableInterface())
                {
                    sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        public static {{unitTypeName}} Parse(string s, IFormatProvider? provider)
        {
            return new {{unitTypeName}}({{innerTypeName}}.Parse(s, provider));
        }
 
        public static bool TryParse(string s, IFormatProvider? provider, out {{unitTypeName}} result)
        {
            if ({{innerTypeName}}.TryParse(s, provider, out var r))
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
#endif

""");
                }
                if (prop.HasSpanParsableInterface())
                {
                    sb.AppendLine($$"""
#if NET7_0_OR_GREATER
        public static {{unitTypeName}} Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            return new {{unitTypeName}}({{innerTypeName}}.Parse(s, provider));
        }
 
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out {{unitTypeName}} result)
        {
            if ({{innerTypeName}}.TryParse(s, provider, out var r))
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
#endif

""");
                }

                if (prop.HasUtf8SpanParsableInterface())
                {
                    sb.AppendLine($$"""
#if NET8_0_OR_GREATER
        public static {{unitTypeName}} Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        {
            return new {{unitTypeName}}({{innerTypeName}}.Parse(utf8Text, provider));
        }
 
        public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out {{unitTypeName}} result)
        {
            if ({{innerTypeName}}.TryParse(utf8Text, provider, out var r))
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
#endif

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
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Addition))
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
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Subtraction))
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

                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Multiply) ||
                    prop.HasArithmeticOperator(UnitArithmeticOperators.Division))
                {
                    sb.AppendLine($$"""
        public static {{unitTypeName}} operator +({{unitTypeName}} value) => new(({{innerTypeName}})(+value.value));
        public static {{unitTypeName}} operator -({{unitTypeName}} value) => new(({{innerTypeName}})(-value.value));

""");
                    if (prop.HasArithmeticOperator(UnitArithmeticOperators.Multiply))
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
                    if (prop.HasArithmeticOperator(UnitArithmeticOperators.Division))
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

                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Increment))
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
                if (prop.HasArithmeticOperator(UnitArithmeticOperators.Decrement))
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
                if (prop.HasValueArithmeticOperator(UnitArithmeticOperators.Addition))
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
                if (prop.HasValueArithmeticOperator(UnitArithmeticOperators.Subtraction))
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
                if (prop.HasValueArithmeticOperator(UnitArithmeticOperators.Multiply))
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
                if (prop.HasValueArithmeticOperator(UnitArithmeticOperators.Division))
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

            if (prop.HasFlag(UnitGenerateOptions.Comparable))
            {
                sb.AppendLine($$"""
        // UnitGenerateOptions.Comparable

        public int CompareTo({{unitTypeName}} other)
        {
            return value.CompareTo(other.value);
        }
""");
            }
            if (prop.HasFlag(UnitGenerateOptions.Comparable) && !prop.HasFlag(UnitGenerateOptions.WithoutComparisonOperator))
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
            public ReferenceSymbols ReferenceSymbols { get; set; }
            public ITypeSymbol Type { get; set; }
            public UnitGenerateOptions Options { get; set; }
            public UnitArithmeticOperators ArithmeticOperators { get; set; }
            public string? ToStringFormat { get; set; }
            public string TypeName => Type.ToString();

            public bool IsString() => TypeName is "string";
            public bool IsBool() => TypeName is "bool";
            public bool IsUlid() => SymbolEqualityComparer.Default.Equals(Type, ReferenceSymbols.UlidType);
            public bool IsGuid() => SymbolEqualityComparer.Default.Equals(Type, ReferenceSymbols.GuidType);

            public bool HasFlag(UnitGenerateOptions options) => Options.HasFlag(options);

            public bool HasArithmeticOperator(UnitArithmeticOperators op)
            {
                return HasFlag(UnitGenerateOptions.ArithmeticOperator) && ArithmeticOperators.HasFlag(op);
            }

            public bool HasValueArithmeticOperator(UnitArithmeticOperators op)
            {
                return HasFlag(UnitGenerateOptions.ValueArithmeticOperator) && ArithmeticOperators.HasFlag(op);
            }

            public bool HasFormattableInterface() => IsImplemented(ReferenceSymbols.FormattableInterface);
            public bool HasParsableInterface() => ReferenceSymbols.ParsableInterface != null && IsImplementedGenericSelfType(ReferenceSymbols.ParsableInterface);
            public bool HasSpanFormattableInterface() => ReferenceSymbols.SpanFormattableInterface != null && IsImplemented(ReferenceSymbols.SpanFormattableInterface);
            public bool HasSpanParsableInterface() => ReferenceSymbols.SpanParsableInterface != null && IsImplementedGenericSelfType(ReferenceSymbols.SpanParsableInterface);
            public bool HasUtf8SpanFormattableInterface() => ReferenceSymbols.Utf8SpanFormattableInterface != null && IsImplemented(ReferenceSymbols.Utf8SpanFormattableInterface);
            public bool HasUtf8SpanParsableInterface() => ReferenceSymbols.ParsableInterface != null && IsImplementedGenericSelfType(ReferenceSymbols.ParsableInterface);

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

            bool IsImplemented(INamedTypeSymbol interfaceSymbol)
            {
                foreach (var x in Type.AllInterfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(x, interfaceSymbol))
                    {
                        foreach (var interfaceMember in x.GetMembers())
                        {
                            if (interfaceMember.IsStatic)
                            {
                                // Do not allow explicit implementation
                                var implementation = Type.FindImplementationForInterfaceMember(interfaceMember);
                                switch (implementation)
                                {
                                    case IMethodSymbol { ExplicitInterfaceImplementations.Length: > 0 }:
                                        return false;
                                    case IPropertySymbol { ExplicitInterfaceImplementations.Length: > 0 }:
                                        return false;
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            bool IsImplementedGenericSelfType(INamedTypeSymbol interfaceSymbol)
            {
                foreach (var x in Type.AllInterfaces)
                {
                    if (x.IsGenericType &&
                        SymbolEqualityComparer.Default.Equals(x.ConstructedFrom, interfaceSymbol) &&
                        SymbolEqualityComparer.Default.Equals(x.TypeArguments[0], Type))
                    {
                        foreach (var interfaceMember in x.GetMembers())
                        {
                            if (interfaceMember.IsStatic)
                            {
                                // Do not allow explicit implementation
                                var implementation = Type.FindImplementationForInterfaceMember(interfaceMember);
                                switch (implementation)
                                {
                                    case IMethodSymbol { ExplicitInterfaceImplementations.Length: > 0 }:
                                        return false;
                                    case IPropertySymbol { ExplicitInterfaceImplementations.Length: > 0 }:
                                        return false;
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<(StructDeclarationSyntax type, AttributeSyntax attr, PredefinedTypeSyntax? targetType)> Targets { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is StructDeclarationSyntax s && s.AttributeLists.Count > 0)
                {
                    var attr = (from attributesList in s.AttributeLists
                                from attribute in attributesList.Attributes
                                let attributeName = attribute.Name switch
                                {
                                    QualifiedNameSyntax qName => qName.Right.Identifier.Text,
                                    AliasQualifiedNameSyntax qName => qName.Name.Identifier.Text,
                                    SimpleNameSyntax name => name.Identifier.Text,
                                    _ => attribute.Name.ToString(),
                                }
                                let targetType = attribute.Name is GenericNameSyntax gName ? gName.TypeArgumentList.ChildNodes().FirstOrDefault() as PredefinedTypeSyntax : null
                                where attributeName is "UnitOf" or "UnitOfAttribute"
                                select new { attribute, targetType }).FirstOrDefault();

                    if (attr != null)
                    {
                        Targets.Add((s, attr.attribute, attr.targetType));
                    }
                }
            }
        }
    }
}