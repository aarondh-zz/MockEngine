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
        public DefaultTypeResolver( )
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
        public Type Resolve(string typeReference)
        {
            if ( typeReference == null)
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
                if ( assembly == null)
                {
                    return null;
                }
                else
                {
                    return assembly.GetType(typeString);
                }
            }
            else
            {
                throw new ArgumentException(nameof(typeReference), "type reference is not correctly formed");
            }
        }
    }
}
