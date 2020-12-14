using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitGenerator;

namespace FileGenerate
{
    [UnitOf(typeof(int))]
    public readonly partial struct A
    {
    }

    [UnitOf(typeof(string))]
    public readonly partial struct B
    {
    }
}
