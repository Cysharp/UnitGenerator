using System;
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

    [UnitOf(typeof(int), UnitGenerateOptions.Comparable | UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator | UnitGenerateOptions.ParseMethod)]
    public readonly partial struct C
    {
    }

    [UnitOf(typeof(Guid), UnitGenerateOptions.Comparable | UnitGenerateOptions.ParseMethod)]
    public readonly partial struct D
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
