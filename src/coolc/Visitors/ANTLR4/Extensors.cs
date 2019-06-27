using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coolc.Visitors
{
    static class Extensors
    {
        public static string getText(this IToken s)
        {
            return s.Text;
        }
        public static IToken getIToken(this IToken s)
        {
            return s;
        }
    }

}
