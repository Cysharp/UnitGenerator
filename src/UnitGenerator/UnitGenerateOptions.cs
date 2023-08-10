using System;

namespace UnitGenerator
{
    // same as Generated Options(check UnitOfAttributeTemplate.tt).
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
        ArithmeticOperatorAddition = 1 << 13,
        ArithmeticOperatorSubtraction = 1 << 13,
        ArithmeticOperatorMultiply = 1 << 14,
        ArithmeticOperatorDivision = 1 << 15,
        ValueArithmeticOperatorAddition = 1 << 16,
        ValueArithmeticOperatorSubtraction = 1 << 17
    }
}
