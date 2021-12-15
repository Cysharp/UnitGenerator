using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(x => SetDefaultAttribute(x));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var receiver = context.SyntaxReceiver as SyntaxReceiver;
                if (receiver == null) return;

                var list = new List<(StructDeclarationSyntax, UnitOfAttributeProperty)>();
                foreach (var (type, attr) in receiver.Targets)
                {
                    if (attr.ArgumentList is null) continue;

                    var model = context.Compilation.GetSemanticModel(type.SyntaxTree);

                    // retrieve attribute parameter
                    var prop = new UnitOfAttributeProperty();

                    if (attr.ArgumentList is null) goto ADD;
                    for (int i = 0; i < attr.ArgumentList.Arguments.Count; i++)
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

                ADD:
                    list.Add((type, prop));
                }

                foreach (var (type, prop) in list)
                {
                    var typeSymbol = context.Compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type);
                    if (typeSymbol == null) throw new Exception("can not get typeSymbol.");

                    var template = new CodeTemplate()
                    {
                        Name = typeSymbol.Name,
                        Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
                        Type = prop.Type.ToString(),
                        Options = prop.Options,
                        ToStringFormat = prop.ToStringFormat
                    };

                    var text = template.TransformText();
                    if (template.Namespace == null)
                    {
                        context.AddSource($"{template.Name}.Generated.cs", text);
                    }
                    else
                    {
                        context.AddSource($"{template.Namespace}.{template.Name}.Generated.cs", text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private void SetDefaultAttribute(GeneratorPostInitializationContext context)
        {
            var attrCode = new UnitOfAttributeTemplate().TransformText();
            context.AddSource("UnitOfAttribute.cs", attrCode);
        }

        struct UnitOfAttributeProperty
        {
            public ITypeSymbol Type { get; set; }
            public UnitGenerateOptions Options { get; set; }
            public string? ToStringFormat { get; set; }
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<(StructDeclarationSyntax type, AttributeSyntax attr)> Targets { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is StructDeclarationSyntax s && s.AttributeLists.Count > 0)
                {
                    var attr = s.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "UnitOf" or "UnitOfAttribute" or "UnitGenerator.UnitOf" or "UnitGenerator.UnitOfAttribute");
                    if (attr != null)
                    {
                        Targets.Add((s, attr));
                    }
                }
            }
        }
    }
}