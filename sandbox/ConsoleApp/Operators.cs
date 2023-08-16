using UnitGenerator;

namespace ConsoleApp
{
    namespace ConsoleApp
    {
        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Addition)]
        public readonly partial struct Add
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Subtraction)]
        public readonly partial struct Sub
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Multiply)]
        public readonly partial struct Mul
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Division)]
        public readonly partial struct Div
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Increment)]
        public readonly partial struct Inc
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitArithmeticOperators.Decrement)]
        public readonly partial struct Dec
        {
        }
    }
}
