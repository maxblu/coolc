using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{
    internal class NotNode : Node
    {
        private CoolParser.ExpresionContext expresionContext;
        private Node s;
        
        public NotNode(CoolParser.ExpresionContext expresionContext, Node s) : base(s.Childs)
        {
            this.expresionContext = expresionContext;

            this.s = s;
        }
		public override string GenerateCode()
		{
			string result = "";
			result += s.Childs[0].GenerateCode();
			string reg = MIPS.LastReg();
			result += MIPS.Emit(MIPS.Opcodes.neg, reg,reg);

			return result;
		}
		public int TryGetValue()
		{
			try
			{
				return !(((dynamic)(s.Childs[0])).TryGetValue());
			}
			catch { }
			return 0;
		}
	}
}