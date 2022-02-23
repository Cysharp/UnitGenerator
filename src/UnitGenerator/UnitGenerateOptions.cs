using System;

namespace UnitGenerator
{
    // same as Generated Options(check UnitOfAttributeTemplate.tt).
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
        WithoutComparisonOperator = 2048,
        JsonConverterDictionaryKeySupport = 4096
    }
}