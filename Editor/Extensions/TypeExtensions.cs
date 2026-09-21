using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace fefek5.Toys.Editor.Extensions
{
    public static class TypeExtensions
    {
        private const BindingFlags CONSTRUCTOR_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Every concrete type a [SerializeReference] field of <paramref name="baseType"/> can hold, from any level
        /// of inheritance, <paramref name="baseType"/> itself included.
        /// </summary>
        /// <param name="baseType">Type of the field, a class or an interface, generic ones with closed arguments</param>
        /// <returns>Non abstract classes with a parameterless constructor; generic ones like Foo&lt;T&gt; : Bar&lt;T&gt;
        /// come closed with the arguments of <paramref name="baseType"/></returns>
        public static IEnumerable<Type> GetAssignableTypes(this Type baseType)
        {
            // Searching by the generic definition also finds generic types like Foo<T> : StatTransport<T>.
            var searchType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;

            return TypeCache.GetTypesDerivedFrom(searchType)
                .Prepend(baseType)
                .Select(type => type.IsGenericTypeDefinition ? CloseGeneric(type, baseType) : type)
                .Where(type => type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
                               baseType.IsAssignableFrom(type) &&
                               !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
                               type.GetConstructor(CONSTRUCTOR_FLAGS, null, Type.EmptyTypes, null) != null)
                .Distinct();
        }

        /// <summary>
        /// Name of a type as the inspector shows it, e.g. "Steam Int Stat Transport" for SteamIntStatTransport or
        /// "Foo" for Foo&lt;T&gt;.
        /// </summary>
        public static string GetDisplayName(this Type type)
        {
            var name = type.Name;
            var arityIndex = name.IndexOf('`');

            return ObjectNames.NicifyVariableName(arityIndex < 0 ? name : name[..arityIndex]);
        }

        private static Type CloseGeneric(Type definition, Type baseType)
        {
            var arguments = baseType.GetGenericArguments();
            if (definition.GetGenericArguments().Length != arguments.Length) return null;

            try
            {
                return definition.MakeGenericType(arguments);
            }
            catch (ArgumentException)
            {
                // Generic constraints not satisfied.
                return null;
            }
        }
    }
}
