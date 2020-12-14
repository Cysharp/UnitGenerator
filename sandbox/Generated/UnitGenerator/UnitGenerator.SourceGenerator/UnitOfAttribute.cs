﻿#pragma warning disable CS8669
#pragma warning disable CS8625
using System;

namespace UnitGenerator
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    internal class UnitOfAttribute : Attribute
    {
        public Type Type { get; }
        public UnitGenerateOptions Options { get; }
        public string Format { get; }

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
        ParseMethod = 2,
        ArithmeticOperator = 4,
        ValueArithmeticOperator = 8,
        Comparable = 16,
        Validate = 32,
        JsonConverter = 64,
        MessagePackFormatter = 128,
        DapperTypeHandler = 256,
        EntityFrameworkValueConverter = 512,

        CalculationType = ArithmeticOperator | ValueArithmeticOperator | Comparable,
        All = ImplicitOperator | ParseMethod | ArithmeticOperator | ValueArithmeticOperator | Comparable | Validate | JsonConverter | MessagePackFormatter | DapperTypeHandler | EntityFrameworkValueConverter,
    }
}