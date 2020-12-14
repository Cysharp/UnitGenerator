using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace UnitGenerator.Tests
{
    // https://gist.github.com/chsienki/2955ed9336d7eb22bcb246840bfeb05c

    public static class TestHelper
    {
        public static Compilation CreateCompilation(string source, params Type[] metadataLocations)
        {
            var references = metadataLocations
                .Concat(new[] { typeof(Binder), typeof(object) })
                .Select(x => x.Assembly.Location)
                .Distinct()
                .Select(x => MetadataReference.CreateFromFile(x))
                .ToArray();

            return CSharpCompilation.Create(
                assemblyName: "compilation",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            );
        }

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) => CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generators),
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            optionsProvider: null
        );

        public static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);
            return updatedCompilation;
        }
    }
}