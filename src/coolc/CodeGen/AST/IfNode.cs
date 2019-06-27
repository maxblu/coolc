namespace coolc.CodeGen
{
    internal class IfNode : Node
    {
        private CoolParser.IfExpContext context;
        private Node s;

        public IfNode(CoolParser.IfExpContext context, Node s) : base(s.Childs)
        {
            this.s = s;
            this.context = context;
            IfExp = s.Childs[0];
            ThenExp = s.Childs[1];
            ElseExp = s.Childs[2];

        }

        public Node IfExp { get; private set; }
        public Node ThenExp { get; private set; }
        public Node ElseExp { get; private set; }

        public int TryGetValue()
        {
            int ifExpVal = ((dynamic)IfExp).TryGetValue();
            return ifExpVal > 0 ? ((dynamic)ThenExp).TryGetValue() :
                ((dynamic)ElseExp).TryGetValue();
        }

        public override string GenerateCode()
        {
            MIPS.ifCount++;

            string tempReg = MIPS.GetReg();

            string result = "\t#if \n";

            string ifCode = IfExp.GenerateCode();
            string ifReg = MIPS.LastReg();

            result += ifCode;

            result += MIPS.Emit(MIPS.Opcodes.bne, ifReg, "1", "else" + MIPS.ifCount.ToString());

            string thenCode = ThenExp.GenerateCode();

            result += thenCode;
            result += MIPS.Emit(MIPS.Opcodes.move, tempReg, MIPS.LastReg());

            result += MIPS.Emit(MIPS.Opcodes.b, "endif" + MIPS.ifCount.ToString());

            result += "\telse" + MIPS.ifCount.ToString() + ":\n";

            string elseCode = ElseExp.GenerateCode();

            result += elseCode;
            result += MIPS.Emit(MIPS.Opcodes.move, tempReg, MIPS.LastReg());
            result += "\tendif" + MIPS.ifCount.ToString() + ":\n";

            string resultReg = MIPS.GetReg();

            result += MIPS.Emit(MIPS.Opcodes.move, resultReg, tempReg);

            return result;
        }

    }
}