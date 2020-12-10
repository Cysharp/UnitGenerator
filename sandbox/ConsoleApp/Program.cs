using System;
using UnitGenerator;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // UnitGenerator.UnitOfAttribtue

            // HelloWorld.SayHello();

            // UnitOfGeneraotr.HelloWorld.SayHello();

            // HelloWorldGenerated.HelloWorld.SayHello();

        }
    }



    public class Tako
    {

    }

    [UnitOf(typeof(int), UnitGenerateOptions.WithoutArithmeticOperator | UnitGenerateOptions.WithoutComparable)]
    public struct Foo
    {

    }



    [UnitOf(typeof(long))]
    public struct Bar
    {

    }
}
