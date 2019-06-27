using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{
    internal class TildeNode : Node
    {
        private CoolParser.ExpresionContext expresionContext;
        private Node s;
        

        public TildeNode(CoolParser.ExpresionContext expresionContext, Node s) : base(s.Childs)
        {
            this.expresionContext = expresionContext;
            this.s = s;
        }

		public override string GenerateCode()
		{
			int r = TryGetValue();
			
			string result = "";
			foreach (var child in Childs)
				result += child.GenerateCode();

			string lastReg = MIPS.LastReg();
			result += MIPS.Emit(MIPS.Opcodes.not, lastReg,lastReg);

			return result;
		}

		public int TryGetValue()
		{
			try
			{
				return -1 * ((dynamic)Childs[0]).TryGetValue();
			}
			catch { }
			return 0;
		}
	}
}