using System;
namespace MockEngine.Interfaces
{
    public class TypeResolverException : Exception
    {

        public TypeResolverException(string typeName, string message) : base(message)
        {
            TypeName = typeName;
        }
        public string TypeName { get; }
    }
}
