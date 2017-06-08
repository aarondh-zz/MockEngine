using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Http.Interfaces
{
    public interface IMediaTypeParser : IMockComponent
    {
        object Parse(string media, Type type);
    }
}
