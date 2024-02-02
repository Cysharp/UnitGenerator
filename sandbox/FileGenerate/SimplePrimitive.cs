using UnitGenerator;

namespace FileGenerate
{
    [UnitOf(typeof(int))]
    public readonly partial struct A
    {
    }

    [UnitOf(typeof(string))]
    public readonly partial struct B
    {
    }
    
    [UnitOf(typeof(int), UnitGenerateOptions.Comparable | UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator)]
    public readonly partial struct C
    {
    }


    [UnitOf<int>]
    public readonly partial struct Aa
    {
    }

    [UnitOf<string>()]
    public readonly partial struct Bb
    {
    }

    [UnitOf<int>(UnitGenerateOptions.Comparable | UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator)]
    public readonly partial struct Cc
    {
    }
}
