using System;

namespace UnitGenerator
{
    // same as Generated Options.
    [Flags]
    internal enum UnitGenerateOptions
    {
        None = 0,
        ImplicitOperator = 1,
        ParseMethod = 2,
        ArithmeticOperator = 4,
        ValueArithmeticOperator = 8,
        Comparable = 16,
        JsonConverter = 32,
        MessagePackFormatter = 64,
        DapperTypeHandler = 128,
        EntityFrameworkValueConverter = 256,
    }
}