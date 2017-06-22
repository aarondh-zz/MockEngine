using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Resolvers
{
    public class DefaultTypeResolver : ITypeResolver
    {
        private ILogProvider _logProvider;
        private Assembly _defaultAssembly;
        public DefaultTypeResolver()
        {
            _defaultAssembly = Assembly.GetCallingAssembly();
        }
        public void Initialize(IMockContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            this._logProvider = context.LogProvider;
        }
        public Type Resolve(string typeReference, bool throwOnError)
        {
            if (typeReference == null)
            {
                throw new ArgumentNullException(nameof(typeReference));
            }
            string[] components = typeReference.Split(',');
            if (components.Length == 1)
            {
                return _defaultAssembly.GetType(components[0].Trim());
            }
            else if (components.Length > 1)
            {
                var typeString = components[0].Trim();
                var assemblyString = components[1].Trim();
                var assemblyFullString = string.Join(",", components.Skip(1));
                Assembly assembly;
                if (assemblyString == "System.Core")
                {
                    assembly = typeof(int).Assembly;
                }
                else
                {
                    assembly = Assembly.Load(assemblyFullString);
                }
                if (assembly == null)
                {
                    if (throwOnError)
                    {
                        throw new TypeResolverException(typeReference, $"Assembly {assemblyFullString} not found.");
                    }
                    return null;
                }
                else
                {
                    var type = assembly.GetType(typeString);
                    if (type == null && throwOnError)
                    {
                        throw new TypeResolverException(typeReference, $"Type not found in assembly {assemblyFullString}");
                    }
                    return type;
                }
            }
            else
            {
                throw new ArgumentException(nameof(typeReference), "type reference is not correctly formed");
            }
        }
    }
}
