using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Reflection.Cecil
{
    public interface IAssemblyReflector
    {
        IEnumerable<IAttributeReflector> GetAttributes<T>() where T : Attribute;

        IEnumerable<ITypeReflector> GetTypes();

        string Location { get; }
        string FileName { get; }
        string FullName { get; }

    }
}
