using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
#endif 

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // setup default attributes.
            var attrCode = new UnitOfAttributeTemplate().TransformText();
            context.AddSource("UnitOfAttribute.cs", attrCode);

            var receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver == null) return;

            var list = new List<(StructDeclarationSyntax, UnitOfAttributeProperty)>();
            foreach (var (type, attr) in receiver.Targets)
            {
                if (attr.ArgumentList is null) continue;

                var model = context.Compilation.GetSemanticModel(type.SyntaxTree);

                // retrieve attribute parameter
                var prop = new UnitOfAttributeProperty();
                {
                    var arg = attr.ArgumentList.Arguments[0];
                    var expr = arg.Expression;
                    if (expr is TypeOfExpressionSyntax typeOfExpr) // Type type
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
                // Option1: GenerateFlags
                var generateFlagsAttr = type.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "GenerateFlags" or "GenerateFlagsAttribute");
                if (generateFlagsAttr != null)
                {
                    if (generateFlagsAttr.ArgumentList is null) goto ADD;
                    var arg = generateFlagsAttr.ArgumentList.Arguments[0];
                    var expr = arg.Expression;

                    // e.g. UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.ParseMethod => ImplicitOperatior, ParseMethod
                    var parsed = Enum.Parse(typeof(UnitGenerateOptions), expr.ToString().Replace("UnitGenerateOptions.", "").Replace("|", ","));
                    prop.Options = (UnitGenerateOptions)parsed;
                }
                // Option1: ToStringFormat
                var toStringAttr = type.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "ToStringFormat" or "ToStringFormatAttribute");
                if (toStringAttr != null)
                {
                    if (toStringAttr.ArgumentList is null) goto ADD;
                    var arg = toStringAttr.ArgumentList.Arguments[0];
                    var expr = arg.Expression;
                    var format = model.GetConstantValue(expr).Value?.ToString();
                    prop.ToStringFormat = format;
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
                    Namespace = typeSymbol.ContainingNamespace.Name,
                    Type = prop.Type.ToString(),
                    Options = prop.Options,
                    ToStringFormat = prop.ToStringFormat
                };

                var text = template.TransformText();
                context.AddSource($"{template.Namespace}.{template.Name}.Generated.cs", text);
            }
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
                    var attr = s.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() is "UnitOf" or "UnitOfAttribute");
                    if (attr != null)
                    {
                        Targets.Add((s, attr));
                    }
                }
            }
        }
    }
}