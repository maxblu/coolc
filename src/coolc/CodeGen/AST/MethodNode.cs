using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
namespace coolc.CodeGen
{

    public class MethodNode : Node
    {
        private CoolParser.MethodFeatureContext context;
        //private Node Childs;
       
        /// <summary>
        /// this can only be called for the basic clases
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public MethodNode(string name, ClassNode type):base(new List<Node>())
        {
            Name = name;
            SetType(type);
            Params = new Dictionary<string, AttributeNode>();
            //Variables = new Dictionary<string, AttributeNode>();
            Symbols = new Dictionary<string, ClassNode>();
            ClassName = "";
        }
        public MethodNode(CoolParser.MethodFeatureContext context, Node s) : base(s.Childs)
        {
            Symbols = new Dictionary<string, ClassNode>();
            Params = new Dictionary<string, AttributeNode>();
            //Variables = new Dictionary<string, AttributeNode>();

            CoolParser.ClassdefContext p = (CoolParser.ClassdefContext)context.Parent;
            var classname = p.t.Text;
            var type = "Object";
            if (context.TYPE().GetText() != null)
                type = context.TYPE().GetText();
            var name = context.ID().GetText();
            Name = name;
            SetType(SymbolTable.Classes[type]);
            //Method c;
            if (SymbolTable.Classes.ContainsKey(type) && !(SymbolTable.Classes[classname].Methods.ContainsKey(name)))// if you know my type and not myself
            {
                //c = new Method(name, SymbolTable.Classes[type])
                {
                    Expression = context.expresion();// this shouln't be on the ctor
                    Self = context;
                }
                SymbolTable.Classes[classname].Methods.Add(Name, this);//let me introduce myself
                var formals = context.formal();
                //foreach (var item in formals)// add the parameters of the method
                //{
                //    //SymbolTable.Classes[classname].Methods[name].Params.Add(item.id.Text, new FormalNode(item.id.Text, SymbolTable.Classes[item.t.Text]));
                //}
                foreach (var item in Childs)
                {
                    try{SymbolTable.Classes[classname].Methods[name].Params.Add(((FormalNode)item).ID, new AttributeNode(((FormalNode)item).ID, ((FormalNode)item).GetType()));}catch (System.Exception){}
                }
            }
            this.context = context;
            var d = (CoolParser.ClassdefContext)context.Parent;
            this.ClassName = d.TYPE(0).GetText();// + ".";
            //this.Childs = s.Childs;
            this.Name = context.ID().GetText();
            if (ClassName.ToLower() == "main")
                ClassName = "";
            foreach (var item in Params)
            {
                Symbols.Add(item.Key, item.Value.GetType());// maybe here we could make a dict of atributes but i think this will be harder after in other places and since i only need the class :)
            }
        }

        public string ClassName { get; private set; }
        /// <summary>
        /// The name of the Method, useful for search and for index in a class
        /// </summary>
        public string Name { get; set; }

        private ClassNode type;

        public new ClassNode GetType()
        {
            return type;
        }

        public void SetType(ClassNode value)
        {
            type = value;
        }


        /// <summary>
        /// A Dictionary of Parameters
        /// </summary>
        public Dictionary<string, AttributeNode> Params { get; set; }
        /// <summary>
        /// This references to the context body of the expression
        /// It's not a part of the ctor because of the 'basic functions' that don't have any body 
        /// </summary>
        public CoolParser.ExpresionContext Expression { get; internal set; }
        public CoolParser.MethodFeatureContext Self { get; internal set; }
        /// <summary>
        /// A Dictionary of attributes defined in the methods scope?
        /// dont know if needed or even useful yet
        /// </summary>
        public Dictionary<string, ClassNode> Symbols { get; private set; }

        public override string GenerateCode()
        {
            SymbolTable.Symbols.Push(Symbols);

            var s = "";// = ClassName + "." + Name + ":\n";//todo add the calss name :(

            if (ClassName == "" || Name.ToLower() == "main")
                s += Name+":\n";
            else
                s+= ClassName + "." + Name + ":\n";
        
            int offset = 4;
            var reg1 = MIPS.GetReg();//
            foreach (var item in Params)
            {
                s += MIPS.Emit(MIPS.Opcodes.lw, reg1, offset + "($sp)");//pop all your sons
                s += MIPS.Emit(MIPS.Opcodes.usw, reg1,/* Name + "." +*/ item.Key);
                offset += 4;
            }
            s += base.GenerateCode();
            if (ClassName == "" && Name.ToLower() == "main")
                s += "\tli $v0, 10\t\t\t# 10 is the exit syscall.\n\tsyscall\t\t\t\t# do the syscall.\n";
            else
            {
                s += MIPS.Emit(MIPS.Opcodes.move, "$v0", MIPS.LastReg());

                try
                {
                    offset = 8;
                    var c = SymbolTable.Classes[ClassName];

                    var reg0 = MIPS.GetReg();
                    s += MIPS.Emit(MIPS.Opcodes.lw, reg0, 0 + "($sp)");//pop all your sons

                    foreach (var item in c.Attributes)
                    {
                        s += MIPS.Emit(MIPS.Opcodes.lw, reg1, item.Key);//pop all your sons
                        s += MIPS.Emit(MIPS.Opcodes.sw, reg1, offset + "(" + reg0 + ")");//pop all your sons
                        offset += 4;
                    }
                }catch (System.Exception){}
                s += MIPS.Emit(MIPS.Opcodes.jr);
            }
            SymbolTable.Symbols.Pop();
            return s;
        }

        public override string GiveMeTheData()
        {
            var s = "";
            foreach (var item in Params)
            {
                s += "\t"+/*Name+"."+*/item.Key+":\t.word\t0\n";
            }
            return s + base.GiveMeTheData();
        }
    } 
}