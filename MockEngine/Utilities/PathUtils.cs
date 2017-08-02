using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Utilities
{
    public static class PathUtils
    {
        public static string JoinPath(string pathA, string pathB, string pathSepparator = "\\")
        {
            if (string.IsNullOrWhiteSpace(pathA))
            {
                return pathB;
            }
            else if (string.IsNullOrWhiteSpace(pathB))
            {
                return pathA;
            }
            else if (pathA.EndsWith(pathSepparator))
            {
                if (pathB.StartsWith(pathSepparator))
                {
                    return pathA + pathB.Substring(pathSepparator.Length);
                }
                else
                {
                    return pathA + pathB;
                }
            }
            else if (pathB.StartsWith(pathSepparator))
            {
                return pathA + pathB;
            }
            else
            {
                return pathA + pathSepparator + pathB;
            }
        }
    }
}
