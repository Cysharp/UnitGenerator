using UnitGenerator;

namespace ConsoleApp
{
    namespace ConsoleApp
    {
        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Addition)]
        public readonly partial struct Add
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Subtraction)]
        public readonly partial struct Sub
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Multiply)]
        public readonly partial struct Mul
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Division)]
        public readonly partial struct Div
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Increment)]
        public readonly partial struct Inc
        {
        }

        [UnitOf(typeof(int), UnitGenerateOptions.ArithmeticOperator | UnitGenerateOptions.ValueArithmeticOperator, ArithmeticOperators = UnitGenerateArithmeticOperators.Decrement)]
        public readonly partial struct Dec
        {
        }
    }
}
