using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace UnitGenerator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static ctx =>
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                var attrCode = NormalizeNewLines(new UnitOfAttributeTemplate().TransformText());
                ctx.AddSource("UnitOfAttribute.cs", attrCode);
            });

            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform);

            context.RegisterSourceOutput(provider, GenerateSource);
        }

        private static bool Predicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is StructDeclarationSyntax s && s.AttributeLists.Count > 0)
            {
                var attr = s.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "UnitOf" or "UnitOfAttribute" or "UnitGenerator.UnitOf" or "UnitGenerator.UnitOfAttribute");
                if (attr is null || attr.ArgumentList is null)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static (INamedTypeSymbol, UnitOfAttributeProperty) Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var s = (context.Node as StructDeclarationSyntax)!;

            var attr = s.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "UnitOf" or "UnitOfAttribute" or "UnitGenerator.UnitOf" or "UnitGenerator.UnitOfAttribute")!;

            var symbol = context.SemanticModel.GetDeclaredSymbol(s)!;
            var model = context.SemanticModel;

            // retrieve attribute parameter
            var prop = new UnitOfAttributeProperty();

            for (int i = 0; i < attr.ArgumentList!.Arguments.Count; i++)
            {
                var arg = attr.ArgumentList.Arguments[i];
                var expr = arg.Expression;

                if (i == 0) // Type type
                {
                    if (expr is TypeOfExpressionSyntax typeOfExpr)
                    {
                        var typeSymbol = model.GetSymbolInfo(typeOfExpr.Type).Symbol as ITypeSymbol;
                        if (typeSymbol == null) throw new Exception("require type-symbol.");
                        prop.Type = typeSymbol;
                    }
                    else
                    {
                        throw new Exception("require UnitOf attribute and ctor.");
                    }
                }
                else if (i == 1) // UnitGenerateOptions options
                {
                    // e.g. UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.ParseMethod => ImplicitOperatior, ParseMethod
                    var parsed = Enum.ToObject(typeof(UnitGenerateOptions), model.GetConstantValue(expr).Value);
                    prop.Options = (UnitGenerateOptions)parsed;
                }
                else if (i == 2) // string toStringFormat
                {
                    var format = model.GetConstantValue(expr).Value?.ToString();
                    prop.ToStringFormat = format;
                }
            }

            return (symbol, prop);
        }

        private static void GenerateSource(SourceProductionContext context, (INamedTypeSymbol, UnitOfAttributeProperty) data)
        {
            try
            {
                var (typeSymbol, prop) = data;

                var template = new CodeTemplate()
                {
                    Name = typeSymbol.Name,
                    Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
                    Type = prop.Type.ToString(),
                    Options = prop.Options,
                    ToStringFormat = prop.ToStringFormat
                };

                var text = NormalizeNewLines(template.TransformText());
                if (template.Namespace == null)
                {
                    context.AddSource($"{template.Name}.Generated.cs", text);
                }
                else
                {
                    context.AddSource($"{template.Namespace}.{template.Name}.Generated.cs", text);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private static string NormalizeNewLines(string source)
        {
            return source.Replace("\r\n", "\n");
        }

        struct UnitOfAttributeProperty
        {
            public ITypeSymbol Type { get; set; }
            public UnitGenerateOptions Options { get; set; }
            public string? ToStringFormat { get; set; }
        }
    }
}