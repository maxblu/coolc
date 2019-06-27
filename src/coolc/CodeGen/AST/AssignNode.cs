namespace coolc.CodeGen
{
    internal class AssignNode : Node
    {
        private CoolParser.AssignExpContext context;

        public AssignNode(CoolParser.AssignExpContext context, Node s) :base(s.Childs)
        {
            this.context = context;
            this.ID = context.ID().GetText();
            this.Expresion = context.expresion();
        }

        public string ID { get; private set; }
        public CoolParser.ExpresionContext Expresion { get; private set; }

        public override string GenerateCode()
        {

            var s = "";
            var reg1 = "";

            var exp = Childs[0];//why it only has one son
            var es  = exp.GenerateCode();

            reg1 = MIPS.LastReg();// here we asume that an int has been loaded to the last position

            //s += "\t# Assign\n";
            s += MIPS.Emit(MIPS.Opcodes.usw, reg1, ID  + "\t\t\t# " + ID);

            return es + s;
        }
    }
}