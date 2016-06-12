using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Reflection.Cecil
{
    public interface ITypeReflector
    {
        IEnumerable<ITypeReflector> GetInterfaces();

        IEnumerable<IAttributeReflector> GetAttributes<T>() where T : Attribute;

        string FullName { get; }
        string Name { get; }

    }
}
