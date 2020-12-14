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