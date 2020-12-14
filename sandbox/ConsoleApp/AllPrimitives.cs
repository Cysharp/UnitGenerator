using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitGenerator;

namespace ConsoleApp
{
    [UnitOf(typeof(int), UnitGenerateOptions.All)]
    public readonly partial struct A
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(uint), UnitGenerateOptions.All)]
    public readonly partial struct B
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(short), UnitGenerateOptions.All)]
    public readonly partial struct C
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(ushort), UnitGenerateOptions.All)]
    public readonly partial struct D
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(byte), UnitGenerateOptions.All)]
    public readonly partial struct E
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(sbyte), UnitGenerateOptions.All)]
    public readonly partial struct F
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(float), UnitGenerateOptions.All)]
    public readonly partial struct G
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(double), UnitGenerateOptions.All)]
    public readonly partial struct H
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(decimal), UnitGenerateOptions.All)]
    public readonly partial struct I
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    [UnitOf(typeof(float), UnitGenerateOptions.All)]
    public readonly partial struct J
    {
        private partial void Validate()
        {
            _ = AsPrimitive();
        }
    }

    //[UnitOf(typeof(bool), UnitGenerateOptions.All)]
    //public readonly partial struct K
    //{
    //    private partial void Validate()
    //    {
    //        _ = AsPrimitive();
    //    }
    //}

}
