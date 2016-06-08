using SkimiaOS.Core.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace SkimiaOS.Core.Reflection
{
    public static class ReflectionExtensions
    {
        public class T
        {
        }

        public static Type GetActionType(this MethodInfo method)
        {
            return Expression.GetActionType((
                from entry in method.GetParameters()
                select entry.ParameterType).ToArray<Type>());
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            return type.FindInterfaces(new TypeFilter(ReflectionExtensions.FilterByName), interfaceType).Length > 0;
        }

        private static bool FilterByName(Type typeObj, object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), false) as T[];
        }

        public static T GetCustomAttribute<T>(this ICustomAttributeProvider type) where T : Attribute
        {
            return type.GetCustomAttributes<T>().GetOrDefault(0);
        }

        public static bool IsDerivedFromGenericType(this Type type, Type genericType)
        {
            return !(type == typeof(object)) && !(type == null) && ((type.IsGenericType && type.GetGenericTypeDefinition() == genericType) || type.BaseType.IsDerivedFromGenericType(genericType));
        }

        public static MethodInfo GetMethodExt(this Type thisType, string name, int genericArgumentsCount, params Type[] parameterTypes)
        {
            return thisType.GetMethodExt(name, genericArgumentsCount, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        public static MethodInfo GetMethodExt(this Type thisType, string name, int genericArgumentsCount, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            MethodInfo methodInfo = null;
            ReflectionExtensions.GetMethodExt(ref methodInfo, thisType, name, genericArgumentsCount, bindingFlags, parameterTypes);
            if (methodInfo == null && thisType.IsInterface)
            {
                Type[] interfaces = thisType.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    Type type = interfaces[i];
                    ReflectionExtensions.GetMethodExt(ref methodInfo, type, name, genericArgumentsCount, bindingFlags, parameterTypes);
                }
            }
            return methodInfo;
        }

        private static void GetMethodExt(ref MethodInfo matchingMethod, Type type, string name, int genericArgumentsCount, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            MemberInfo[] member = type.GetMember(name, MemberTypes.Method, bindingFlags);
            for (int i = 0; i < member.Length; i++)
            {
                MethodInfo methodInfo = (MethodInfo)member[i];
                if (methodInfo.GetGenericArguments().Length == genericArgumentsCount)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == parameterTypes.Length)
                    {
                        int num = 0;
                        while (num < parameters.Length && parameters[num].ParameterType.IsSimilarType(parameterTypes[num]))
                        {
                            num++;
                        }
                        if (num == parameters.Length)
                        {
                            if (!(matchingMethod == null))
                            {
                                throw new AmbiguousMatchException("More than one matching method found!");
                            }
                            matchingMethod = methodInfo;
                        }
                    }
                }
            }
        }

        private static bool IsSimilarType(this Type thisType, Type type)
        {
            if (thisType.IsByRef)
            {
                thisType = thisType.GetElementType();
            }
            if (type.IsByRef)
            {
                type = type.GetElementType();
            }
            bool result;
            if (thisType.IsArray && type.IsArray)
            {
                result = thisType.GetElementType().IsSimilarType(type.GetElementType());
            }
            else
            {
                if (thisType == type || ((thisType.IsGenericParameter || thisType == typeof(ReflectionExtensions.T)) && (type.IsGenericParameter || type == typeof(ReflectionExtensions.T))))
                {
                    result = true;
                }
                else
                {
                    if (thisType.IsGenericType && type.IsGenericType)
                    {
                        Type[] genericArguments = thisType.GetGenericArguments();
                        Type[] genericArguments2 = type.GetGenericArguments();
                        if (genericArguments.Length == genericArguments2.Length)
                        {
                            for (int i = 0; i < genericArguments.Length; i++)
                            {
                                if (!genericArguments[i].IsSimilarType(genericArguments2[i]))
                                {
                                    result = false;
                                    return result;
                                }
                            }
                            result = true;
                            return result;
                        }
                    }
                    result = false;
                }
            }
            return result;
        }
        public static Delegate CreateParamsDelegate(this MethodInfo method, params Type[] delegParams)
        {
            var methodParams = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (delegParams.Length != methodParams.Length)
                throw new Exception("Method parameters count != delegParams.Length");

            var dynamicMethod = new DynamicMethod(string.Empty, null, new[] { typeof(object) }.Concat(delegParams).ToArray(), true);
            var ilGenerator = dynamicMethod.GetILGenerator();

            if (!method.IsStatic)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(method.DeclaringType.IsClass ? OpCodes.Castclass : OpCodes.Unbox, method.DeclaringType);
            }

            for (var i = 0; i < delegParams.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i + 1);
                if (delegParams[i] != methodParams[i])
                    if (methodParams[i].IsSubclassOf(delegParams[i]) || methodParams[i].HasInterface(delegParams[i]))
                        ilGenerator.Emit(methodParams[i].IsClass ? OpCodes.Castclass : OpCodes.Unbox, methodParams[i]);
                    else
                        throw new Exception(string.Format("Cannot cast {0} to {1}", methodParams[i].Name, delegParams[i].Name));
            }

            ilGenerator.Emit(OpCodes.Call, method);

            ilGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(Expression.GetActionType(new[] { typeof(object) }.Concat(delegParams).ToArray()));
        }
    }
}
