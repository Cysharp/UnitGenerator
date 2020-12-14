﻿using System.Data;

namespace UnitGenerator
{
    public partial class CodeTemplate
    {
        internal string? Namespace { get; set; }
        internal string? Type { get; set; }
        internal string? Name { get; set; }
        internal UnitGenerateOptions Options { get; set; }
        public string? ToStringFormat { get; set; }

        internal bool HasFlag(UnitGenerateOptions options)
        {
            return Options.HasFlag(options);
        }

        internal DbType GetDbType()
        {
            return Type switch
            {
                "short" => DbType.Int16,
                "int" => DbType.Int32,
                "long" => DbType.Int64,
                "ushort" => DbType.UInt16,
                "uint" => DbType.UInt32,
                "ulong" => DbType.UInt64,
                "string" => DbType.AnsiString,
                "byte[]" => DbType.Binary,
                "bool" => DbType.Boolean,
                "byte" => DbType.Byte,
                "sbyte" => DbType.SByte,
                "float" => DbType.Single,
                "double" => DbType.Double,
                "System.DateTime" => DbType.DateTime,
                "System.DateTimeOffset" => DbType.DateTimeOffset,
                "System.TimeSpan" => DbType.Time,
                "System.Guid" => DbType.Guid,
                "decimal" => DbType.Currency,
                _ => DbType.Object
            };
        }
    }
}
