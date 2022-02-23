using System;
using UnitGenerator;

namespace ConsoleApp
{
    [UnitOf(typeof(Guid), UnitGenerateOptions.ParseMethod | UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct GD
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(DateTime), UnitGenerateOptions.ParseMethod | UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct DT
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(string), UnitGenerateOptions.Validate | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct ST
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
    [UnitOf(typeof(byte[]), UnitGenerateOptions.Validate | UnitGenerateOptions.DapperTypeHandler | UnitGenerateOptions.EntityFrameworkValueConverter)]
    public readonly partial struct BA
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }
}
