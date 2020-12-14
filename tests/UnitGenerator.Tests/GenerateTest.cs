using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace UnitGenerator.Tests
{
    public class GenerateTest
    {
        const string PrimitiveTemplate = @"
using System;
using UnitGenerator;

namespace MyApp
{
    [UnitOf(typeof(int), UnitGenerateOptions.All)]
    public readonly partial struct A
    {
        private partial void Validate()
        {
        }
    }
}
";

        [Fact]
        public void Foo()
        {
            //var comp = TestHelper.CreateCompilation(PrimitiveTemplate, typeof(TypeConverter), typeof(MessagePackSerializer), typeof(MessagePackObjectAttribute), typeof(JsonSerializer), typeof(Dapper.SqlMapper), typeof(Microsoft.EntityFrameworkCore.ValueGeneration.ValueGenerator));
            //var newComp = TestHelper.RunGenerators(comp, out var generatorDiags, new SourceGenerator());

            //Assert.Empty(generatorDiags);
            //Assert.Empty(newComp.GetDiagnostics());
        }
    }
}
