using System;
using Microsoft.CodeAnalysis;

namespace UnitGenerator;

class ReferenceSymbols
{
    public static ReferenceSymbols Create(Compilation compilation)
    {
        return new ReferenceSymbols
        {
            IParsableInterface = compilation.GetTypeByMetadataName("System.IParsable`1")!,
            IFormattableInterface = compilation.GetTypeByMetadataName("System.IFormattable")!,
            GuidType = compilation.GetTypeByMetadataName("System.Guid"),
            UlidType = compilation.GetTypeByMetadataName("System.Ulid"),
        };
    }

    public INamedTypeSymbol IParsableInterface { get; private set; } = default!;
    public INamedTypeSymbol IFormattableInterface { get; private set; } = default!;
    public INamedTypeSymbol? UlidType { get; private set; }
    public INamedTypeSymbol? GuidType { get; private set; }
}