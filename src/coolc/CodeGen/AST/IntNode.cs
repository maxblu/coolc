#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

using Antlr4.Runtime.Tree;

namespace coolc.CodeGen
{
    internal class IntNode : Node
    {
        private CoolParser.IntExpContext context;

        public int V { get; set; }
        
        public IntNode(CoolParser.IntExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.V = int.Parse(context.INT().GetText());
            Type = "Int";

        }
        public override string GenerateCode()
        {
            var s = "";
            var register = MIPS.GetReg();
            s += MIPS.Emit(MIPS.Opcodes.li, register, V.ToString());

            return s;
        }
        public int TryGetValue()
        {
            return V;
        }
    }
}