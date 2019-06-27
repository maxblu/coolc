using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{
    internal class ParentNode : Node
    {
        private CoolParser.ExpresionContext expresionContext;
        private Node s;

        public ParentNode(CoolParser.ExpresionContext expresionContext, Node s) : base(s.Childs)
        {
            this.expresionContext = expresionContext;
            this.s = s;
        }
        public int TryGetValue()
        {
            return ((dynamic)s.Childs[0]).TryGetValue();
        }
    }
}