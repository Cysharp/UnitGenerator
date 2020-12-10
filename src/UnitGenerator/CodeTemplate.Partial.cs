using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
