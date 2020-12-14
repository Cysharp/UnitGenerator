using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnitGenerator;


namespace ConsoleApp
{


    class Program
    {
        static void Foo(short x)
        {
        }

        static void Main(string[] args)
        {
            var Namespace = "";
            if (!string.IsNullOrEmpty(Namespace))
            {
            }

            short x = 10;
            short y = 20;

            var foo = x + y;




            var jk = JsonSerializer.Serialize(new A(10000));
            Console.WriteLine(jk);


            // UnitGenerator.UnitOfAttribtue

            // HelloWorld.SayHello();

            // UnitOfGeneraotr.HelloWorld.SayHello();

            // HelloWorldGenerated.HelloWorld.SayHello();
            //Foo f = 100;

            //var tako = f * 100;


            //var f = new Foo(true);
            //var b = f.AsPrimitive();
            //if (f)
            //{
            //    Console.WriteLine("true");
            //}
            //else
            //{
            //    Console.WriteLine("false");
            //}

        }
    }
}





