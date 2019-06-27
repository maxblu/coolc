using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    internal class IsvoidNode : Node
    {
        private CoolParser.ExpresionContext expresionContext;
        private Node s;
        
        public IsvoidNode(CoolParser.ExpresionContext expresionContext, Node s) : base(s.Childs)
        {
            this.expresionContext = expresionContext;
            this.s = s;
        }
    }
}