using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    internal class WhileNode : Node
    {
        private CoolParser.WhileExpContext context;
        private Node s;

        public WhileNode(CoolParser.WhileExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.s = s;
            this.Cond = s.Childs[0];
            this.Body = s.Childs[1];
        }

        public Node Cond { get; private set; }
        public Node Body { get; private set; }

        public override string GenerateCode()
        {
            MIPS.whileCount++;
            string code = "";
            string whileName = "while" + MIPS.whileCount.ToString();
            string whileEnd = "whileEnd" + MIPS.whileCount.ToString();
            code += whileName + ":\n";

            code += Cond.GenerateCode();
            string condEval = MIPS.LastReg();
            code += MIPS.Emit(MIPS.Opcodes.bne, condEval, "1", whileEnd);
            code += Body.GenerateCode();

            code += MIPS.Emit(MIPS.Opcodes.j, whileName);

            code += whileEnd + ":\n";
            return code;
        }
    }
}