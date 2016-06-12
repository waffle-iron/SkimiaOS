using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Reflection.Cecil
{
    public class MonoReflector : IReflector
    {
        public IAssemblyReflector LoadAssembly(string path)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(path));
            var reader = new ReaderParameters()
            {
                AssemblyResolver = resolver
            };

            var assembly = AssemblyDefinition.ReadAssembly(path, reader);
            return new MonoAssemblyReflector(assembly);
        }
    }

    public class MonoAssemblyReflector : IAssemblyReflector
    {
        private AssemblyDefinition _assembly;

        public MonoAssemblyReflector(AssemblyDefinition assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<IAttributeReflector> GetAttributes<T>() where T : Attribute
        {
            if (_assembly.HasCustomAttributes)
            {
                var expectedTypeName = typeof(T).Name;
                return _assembly.CustomAttributes
                    .Where(a => a.AttributeType.Name == expectedTypeName)
                    .Select(a => new MonoAttributeReflector(a))
                    .ToList();
            }
            else
            {
                return new IAttributeReflector[] { };
            }
        }

        public IEnumerable<ITypeReflector> GetTypes()
        {
            var result = new List<ITypeReflector>();
            var modules = _assembly.Modules;
            foreach (var module in modules)
            {
                var types = module.GetTypes();
                foreach (var type in types)
                {
                    result.Add(new MonoTypeReflector(type));
                }
            }
            return result;
        }

        public string Location
        {
            get
            {
                return _assembly.MainModule.FullyQualifiedName;
            }
        }

        public string FileName
        {
            get
            {
                return _assembly.MainModule.Name;
            }
        }

        public string FullName
        {
            get
            {
                return _assembly.FullName;
            }
        }
    }

    public class MonoTypeReflector : ITypeReflector
    {
        private TypeDefinition _type;

        public MonoTypeReflector(TypeDefinition type)
        {
            _type = type;
        }

        public IEnumerable<ITypeReflector> GetInterfaces()
        {
            return _type.Interfaces.Select(i => new MonoTypeReflector(i.Resolve()));
        }

        public IEnumerable<IAttributeReflector> GetAttributes<T>() where T : Attribute
        {
            if (_type.HasCustomAttributes)
            {
                var expectedTypeName = typeof(T).Name;
                return _type.CustomAttributes
                    .Where(a => a.AttributeType.Name == expectedTypeName)
                    .Select(a => new MonoAttributeReflector(a))
                    .ToList();
            }
            else
            {
                return new IAttributeReflector[] { };
            }
        }

        public string FullName
        {
            get
            {
                return _type.FullName;
            }
        }

        public string Name
        {
            get
            {
                return _type.Name;
            }
        }
    }

    public class MonoAttributeReflector : IAttributeReflector
    {
        private CustomAttribute _attribute;
        private IDictionary<string, string> _values;

        public MonoAttributeReflector(CustomAttribute attribute)
        {
            _attribute = attribute;
        }

        public IDictionary<string, string> Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new Dictionary<string, string>();
                    var constructorArguments = _attribute.Constructor.Resolve().Parameters.Select(p => p.Name).ToList();
                    var constructorParameters = _attribute.ConstructorArguments.Select(a => a.Value.ToString()).ToList();
                    for (var i = 0; i < constructorArguments.Count; i++)
                    {
                        _values.Add(constructorArguments[i], constructorParameters[i]);
                    }
                    foreach (var prop in _attribute.Properties)
                    {
                        _values.Add(prop.Name, prop.Argument.Value.ToString());
                    }
                }
                return _values;
            }
        }
    }
}
