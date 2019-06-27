using coolc.Visitors;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    public class ClassNode : Node
    {
        public CoolParser.ClassdefContext Context { get; set; }
        //private ClassNode intc;
        //private ClassNode self;

        public ClassNode(CoolParser.ClassdefContext context, Node s) : base(s.Childs)
        {
            Name = context.TYPE(0).GetText();
            Type = Name;
            //this = SymbolTable.Classes[Name];

            this.Context = context;

            Attributes = new Dictionary<string, AttributeNode>();
            Methods = new Dictionary<string, MethodNode>();
        }
        public ClassNode(string name, ClassNode parent):base(new List<Node>())
        {
            Name = name;
            Type = Name;

            Attributes = new Dictionary<string, AttributeNode>();
            Methods = new Dictionary<string, MethodNode>();
            Parent = parent;
        }

        //public ClassNode(ClassNode intc, ClassNode self) : base(new List<Node>())
        //{
        //    this.intc = intc;
        //    this.self = self;
        //}

        public ClassNode Parent { get; set; }

        //public List<AttributeNode> Attributes { get; set; }
        public Dictionary<string, AttributeNode> Attributes { get; set; }
        public Dictionary<string, MethodNode> Methods { get; set; }
        public string Name { get; set; }

        //public List<MethodNode> Methods { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual AttributeNode GetAtribute(string name)
        {
            if (Attributes.ContainsKey(name))
                return Attributes[name];
            try { return Parent.GetAtribute(name); }//Since SemAnal is fine this should never throw exception :s
            catch (System.Exception) { return null; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MethodNode GetMethod(string name)
        {
            if (Methods.ContainsKey(name))
                return Methods[name];
            try { return Parent.GetMethod(name); }
            catch (System.Exception) { return null; }
        }

        public int Size
        {
            get
            {
                if (Parent!=null)
                    return Attributes.Count * 4 + 12;
                return Attributes.Count * 4 + 8;
            }
        }

        /// <summary>
        /// Build the ctor method with className: label
        /// </summary>
        /// <returns></returns>
        public override string GenerateCode()
        {
            var s = "";
            var t = Attributes.Count;

            var reg1 = "";

            if (Name.ToLower()!="main")
            {
                s += Name + ":\t\t\t\t#Ctor\n";

                //t = (t * 4) + 8;
                reg1 = MIPS.GetReg();

                s += MIPS.Emit(MIPS.Opcodes.li, reg1, Size.ToString());
                s += MIPS.Emit(MIPS.Opcodes.sw, reg1, "($sp)");

                s += MIPS.Emit(MIPS.Opcodes.move, "$s0", "$ra");
                s += MIPS.Emit(MIPS.Opcodes.jal, "makeroom");// this blows $ra
                s += MIPS.Emit(MIPS.Opcodes.move, "$ra", "$s0");

                s += MIPS.Emit(MIPS.Opcodes.move, reg1, "$v0");

                var reg2 = MIPS.GetReg();

                //put class name
                s += MIPS.Emit(MIPS.Opcodes.la, reg2, Name+"_str");
                s += MIPS.Emit(MIPS.Opcodes.sw, reg2, "0(" + reg1 + ")");

                //put class size
                s += MIPS.Emit(MIPS.Opcodes.li, reg2, Size.ToString());
                s += MIPS.Emit(MIPS.Opcodes.sw, reg2, "4(" + reg1 + ")");

                int offset = 4;
                foreach (var item in Attributes)
                {
                    offset += 4;
                    if (item.Value.GetType().Name.ToLower()=="int" || item.Value.GetType().Name.ToLower() == "bool" || item.Value.GetType().Name.ToLower() == "string")
                    {
                        //s += "# this value goes here? " + item.Value.Name + " " + 4 + " " + "\n";
                        s += item.Value.GenerateCode();
                        var i = MIPS.LastReg();
                        s += MIPS.Emit(MIPS.Opcodes.sw, i, offset+"(" + reg1 + ")");

                    }
                    else
                    {
                        s += "#allocating space for " + item.Value.Name + " " + item.Value.GetType().Size + " " + "\n";
                        s += item.Value.GenerateCode();
                        // not sure about this :S
                        var i = MIPS.LastReg();
                        s += MIPS.Emit(MIPS.Opcodes.sw, i, offset + "(" + reg1 + ")");

                    }
                }
                if (Parent!=null&& Parent.Name!="Object")
                {
                    s += MIPS.Emit(MIPS.Opcodes.move, "$s1", "$ra");    // todo save this int the stack
                    s += MIPS.Emit(MIPS.Opcodes.jal, Parent.Name);// this blows $ra
                    s += MIPS.Emit(MIPS.Opcodes.move, "$ra", "$s1");

                    var i = MIPS.GetReg();

                    s += MIPS.Emit(MIPS.Opcodes.move, i, "$v0");//copy your parent addres form v0 to a new reg
                    s += MIPS.Emit(MIPS.Opcodes.sw, i, offset + 4 + "(" + reg1 + ")");// then coy it to the last position of you

                }

                s += MIPS.Emit(MIPS.Opcodes.move, "$v0", reg1);

                s += MIPS.Emit(MIPS.Opcodes.jr);

                //if (Parent != null && Parent.Name != "Object")
                //    s += Parent.GenerateCode(); 

            }
            else
            {
                //main class attribute should be on the heap?

                
            }

            foreach (var item in Methods)
                s += item.Value.GenerateCode();

            return s;// +base.GenerateCode();
        }

        public override string GiveMeTheData()
        {
            return "\t" + Name + "_str:\t.asciiz\t" + "\"" + Name + "\"\n" + base.GiveMeTheData();
        }
    } 
}