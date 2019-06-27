using coolc.Visitors;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{

    public class FormalNode : Node
    {
        private CoolParser.FormalContext context;
        private Node s;

        //public FormalNode(CoolParser.FormalContext context)
        //{
        //    this.context = context;
        //}

        public FormalNode(CoolParser.FormalContext context, Node s) : base(s.Childs)
        {
            this.s = s;
            this.context = context;
            this.ID = context.ID().GetText();
            this.SetType(SymbolTable.Classes[context.TYPE().GetText()]);
            //this.Type = (ClassNode)s.Childs[0];
        }

        public string ID { get; internal set; }

        private ClassNode type;

        public new ClassNode GetType()
        {
            return type;
        }

        private void SetType(ClassNode value)
        {
            type = value;
        }
    } 
}