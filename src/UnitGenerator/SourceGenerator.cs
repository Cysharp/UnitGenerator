using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitGenerator;

namespace UnitOfGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
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
                if (attr.ArgumentList is null) throw new Exception("Can't be null here");

                var model = context.Compilation.GetSemanticModel(attr.SyntaxTree);

                // retrieve attribute parameter
                var prop = new UnitOfAttributeProperty();
                for (int i = 0; i < attr.ArgumentList.Arguments.Count; i++)
                {
                    var arg = attr.ArgumentList.Arguments[i];

                    var expr = arg.Expression;

                    var t = model.GetTypeInfo(expr);
                    var v = model.GetConstantValue(expr);

                    if (i == 0 && expr is TypeOfExpressionSyntax typeOfExpr) // Type type
                    {
                        var typeSymbol = model.GetSymbolInfo(typeOfExpr.Type).Symbol as ITypeSymbol;
                        if (typeSymbol == null) throw new Exception("require type-symbol.");
                        prop.Type = typeSymbol;
                    }
                    else if (i == 1) // UnitGenerateOptions options
                    {
                        // e.g. UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.ParseMethod => ImplicitOperatior, ParseMethod
                        var parsed = Enum.Parse(typeof(UnitGenerateOptions), expr.ToString().Replace("UnitGenerateOptions", "").Replace("|", ","));
                        prop.Options = (UnitGenerateOptions)parsed;
                    }
                }

                list.Add((type, prop));
            }

            // TODO:generate source codes




        }

        struct UnitOfAttributeProperty
        {
            public ITypeSymbol Type { get; set; }
            public UnitGenerateOptions Options { get; set; }
        }

        // same as Generated Options.
        [Flags]
        enum UnitGenerateOptions
        {
            None = 0,
            ImplicitOperator = 1,
            ParseMethod = 2,
            WithoutArithmeticOperator = 4,
            WithoutComparable = 8,
            JsonConverter = 16,
            MessagePackFormatter = 32,
            DapperTypeHandler = 64,
            EntityFrameworkValueConverter = 128,
            // TypeConverter
            // MessagePackFormatter
            // [JsonConverter
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