using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface ITypeResolver : IMockComponent
    {
        Type Resolve(string assemblyQualifiedTypeName);
    }
}
