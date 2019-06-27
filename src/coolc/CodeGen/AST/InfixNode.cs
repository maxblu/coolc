using System;

namespace coolc.CodeGen
{
    internal class InfixNode : Node
    {
        private CoolParser.InfixExpContext context;
        private Node s;
        
        public InfixNode(CoolParser.InfixExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.s = s;
            this.OP = context.op.Text;
            this.LE = s.Childs[0];
            this.RE = s.Childs[1];
        }

        public string OP { get; private set; }
        public Node RE { get; private set; }
        public Node LE { get; private set; }
        public int V { get; private set; }

        public override string GenerateCode()
        {
            /**/
            var s = "";
            var register = "";

            var v = TryGetValue();
            if (v!=null)
            {
                register = MIPS.GetReg();
                s += MIPS.Emit(MIPS.Opcodes.li, register, v.ToString(), "\t\t\t# calculated in compilation time :)");
                return s;
            }
            var l = LE.GenerateCode();
            //var reg2 = (MIPS.regCount - 1).ToReg();
            var reg2 = MIPS.LastReg();//

            var r = RE.GenerateCode();
            //var reg1 = (MIPS.regCount - 1).ToReg();
            var reg1 = MIPS.LastReg();//

            register = MIPS.GetReg();

            switch (OP)
            {
                case "+":
                    s += MIPS.Emit(MIPS.Opcodes.add, register, reg2, reg1);
                    break;
                case "-":
                    s += MIPS.Emit(MIPS.Opcodes.sub, register, reg2, reg1);
                    break;
                case "*":
                    s += MIPS.Emit(MIPS.Opcodes.mul, register, reg2, reg1);
                    break;
                case "/":
                    s += MIPS.Emit(MIPS.Opcodes.div, register, reg2, reg1);
                    break;
                default:
                    break;
            }
            return r+l+s;
        }

        public int? TryGetValue()
        {
            if (this.V!=0)
            {
                return this.V;
            }
            try
            {
                var g = ((dynamic)LE).TryGetValue();
                var h = ((dynamic)RE).TryGetValue();
                switch (OP)
                {
                    case "+":
                        this.V = g + h;
                        break;
                    case "-":
                        this.V = g - h;
                        break;
                    case "*":
                        this.V = g * h;
                        break;
                    case "/":
                        this.V = g / h;
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception)
            {

                return null;
            }
            return V;
        }
    }
}