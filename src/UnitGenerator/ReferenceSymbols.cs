using System;
using Microsoft.CodeAnalysis;

namespace UnitGenerator;

class ReferenceSymbols
{
    public static ReferenceSymbols Create(Compilation compilation)
    {
        return new ReferenceSymbols
        {
            GuidType = compilation.GetTypeByMetadataName("System.Guid")!,
            UlidType = compilation.GetTypeByMetadataName("System.Ulid"),
            FormattableInterface = compilation.GetTypeByMetadataName("System.IFormattable")!,
            ParsableInterface = compilation.GetTypeByMetadataName("System.IParsable`1"),
            SpanFormattableInterface = compilation.GetTypeByMetadataName("System.ISpanFormattable"),
            SpanParsableInterface = compilation.GetTypeByMetadataName("System.ISpanParsable`1"),
            Utf8SpanFormattableInterface = compilation.GetTypeByMetadataName("System.IUtf8SpanFormattable"),
            Utf8SpanParsableInterface = compilation.GetTypeByMetadataName("System.IUtf8SpanParsable`1"),
        };
    }

    public INamedTypeSymbol GuidType { get; private set; } = default!; 
    public INamedTypeSymbol? UlidType { get; private set; }
    public INamedTypeSymbol FormattableInterface { get; private set; } = default!;
    public INamedTypeSymbol? ParsableInterface { get; private set; }
    public INamedTypeSymbol? SpanFormattableInterface { get; private set; }
    public INamedTypeSymbol? SpanParsableInterface { get; private set; }
    public INamedTypeSymbol? Utf8SpanFormattableInterface { get; private set; }
    public INamedTypeSymbol? Utf8SpanParsableInterface { get; private set; }
}