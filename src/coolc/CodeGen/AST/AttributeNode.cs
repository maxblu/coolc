using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{
    public class AttributeNode : Node
    {
        private CoolParser.AttribFeatureContext context;
        //private string v;
        //private ObjClass Type;

        //private Node s;

        public AttributeNode(CoolParser.AttribFeatureContext context, Node s) : base(s.Childs)
        {
            CoolParser.ClassdefContext p = (CoolParser.ClassdefContext)context.Parent;
            var classname = p.t.Text;
            var type = "Object";
            if (context.t.Text != null)// if I have a type
                type = context.t.Text;
            var name = context.id.Text;
            this.SetType(SymbolTable.Classes[type]);
            //Atribute c;
            //if (SymbolTable.Classes.ContainsKey(type) && !(SymbolTable.Classes[classname].Attributes.ContainsKey(name)))// if you know my type and not myself
            {
                this.Name = name;
                //c = new Atribute(name, SymbolTable.Classes[type])
                {
                    this.Expression = context.expresion();
                    this.Self = context;
                }//;
                SymbolTable.Classes[classname].Attributes.Add(Name, this);//let me introduce myself
            }
            this.context = context;
            //this.s = s;
        }

        public AttributeNode(string v, ClassNode intc) : base(new List<Node>())
        {
            this.Name = v;
            this.SetType(intc);
        }

        private ClassNode type;

        public new ClassNode GetType()
        {
            return type;
        }

        private void SetType(ClassNode value)
        {
            type = value;
        }

        public CoolParser.ExpresionContext Expression { get; private set; }
        public CoolParser.AttribFeatureContext Self { get; private set; }
        public string Name { get; private set; }

        public override string GenerateCode()
        {
            var s = "\t# "+Name + ":"+ GetType().Name+"\n";
            foreach (var item in Childs)
            {
                s += item.GenerateCode();
            }
            return s;// base.GenerateCode();
        }
        public override string GiveMeTheData()
        {
            var s = "\t"/* + Type.Name +"."*/+ Name + ":\t.word\t0"  + "\n";

            return s+ base.GiveMeTheData();
        }
    } 
}