using NLog;
using SkimiaOS.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.BaseServer.Initialization
{
    public class InitializationManager : Singleton<InitializationManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<Type> m_initializedTypes = new List<Type>();
        private readonly Dictionary<Type, List<InitializationMethod>> m_dependances = new Dictionary<Type, List<InitializationMethod>>();
        private readonly Dictionary<InitializationPass, List<InitializationMethod>> m_initializer = new Dictionary<InitializationPass, List<InitializationMethod>>();
        public event Action<string> ProcessInitialization;
        private void OnProcessInitialization(string text)
        {
            Action<string> processInitialization = ProcessInitialization;
            if (processInitialization != null)
            {
                processInitialization(text);
            }
        }
        private InitializationManager()
        {
            foreach (InitializationPass key in Enum.GetValues(typeof(InitializationPass)))
            {
                m_initializer.Add(key, new List<InitializationMethod>());
            }
        }
        public void AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly current in assemblies)
            {
                AddAssembly(current);
            }
        }
        public void AddAssembly(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo methodInfo = methods[j];
                    InitializationAttribute customAttribute = methodInfo.GetCustomAttribute<InitializationAttribute>();
                    if (customAttribute != null)
                    {
                        if (type.IsGenericType)
                        {
                            throw new Exception("Initialization method is within a generic type.");
                        }
                        if (methodInfo.IsGenericMethod)
                        {
                            throw new Exception("Initialization method must not be generic.");
                        }
                        if (methodInfo.ReturnType != typeof(void))
                        {
                            throw new Exception("Invalid initialization method return type.");
                        }
                        if (methodInfo.GetParameters().Length != 0)
                        {
                            throw new Exception("Invalid initialization cannot have parameters");
                        }
                        if (!m_initializer.ContainsKey(customAttribute.Pass))
                        {
                            m_initializer.Add(customAttribute.Pass, new List<InitializationMethod>());
                        }
                        InitializationMethod initializationMethod = new InitializationMethod(customAttribute, methodInfo);
                        if (methodInfo.IsStatic)
                        {
                            initializationMethod.Caller = null;
                        }
                        else
                        {
                            if (!type.IsDerivedFromGenericType(typeof(Singleton<>)))
                            {
                                throw new Exception("Method have to be static or class must inherit Singleton<>");
                            }
                            PropertyInfo property = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                            initializationMethod.Caller = property.GetValue(null, new object[0]);
                        }
                        m_initializer[customAttribute.Pass].Add(initializationMethod);
                    }
                }
            }
        }
        private void ExecuteInitializationMethod(InitializationMethod method)
        {
            if (!method.Initialized)
            {
                if (method.Attribute.Dependance != null && !m_initializedTypes.Contains(method.Attribute.Dependance))
                {
                    if (!m_dependances.ContainsKey(method.Attribute.Dependance))
                    {
                        m_dependances.Add(method.Attribute.Dependance, new List<InitializationMethod>());
                    }
                    m_dependances[method.Attribute.Dependance].Add(method);
                }
                else
                {
                    if (!method.Attribute.Silent && !string.IsNullOrEmpty(method.Attribute.Description))
                    {
                        logger.Info(method.Attribute.Description);
                        OnProcessInitialization(method.Attribute.Description);
                    }
                    else
                    {
                        if (!method.Attribute.Silent)
                        {
                            string text = string.Format("Initialize '{0}'", method.Method.DeclaringType.Name);
                            logger.Info(text);
                            OnProcessInitialization(text);
                        }
                    }
                    method.Method.Invoke(method.Caller, new object[0]);
                    method.Initialized = true;
                    if (!m_initializedTypes.Contains(method.Method.DeclaringType))
                    {
                        m_initializedTypes.Add(method.Method.DeclaringType);
                    }
                    if (m_dependances.ContainsKey(method.Method.DeclaringType))
                    {
                        foreach (InitializationMethod current in m_dependances[method.Method.DeclaringType])
                        {
                            ExecuteInitializationMethod(current);
                        }
                        m_dependances.Remove(method.Method.DeclaringType);
                    }
                }
            }
        }
        public void InitializeAll()
        {
            foreach (InitializationPass current in
                from InitializationPass pass in Enum.GetValues(typeof(InitializationPass))
                where pass != InitializationPass.Database
                select pass)
            {
                Initialize(current);
            }
        }
        public void Initialize(InitializationPass pass)
        {
            foreach (InitializationMethod current in m_initializer[pass])
            {
                ExecuteInitializationMethod(current);
            }
            m_initializer[pass].Clear();
        }
    }
}
