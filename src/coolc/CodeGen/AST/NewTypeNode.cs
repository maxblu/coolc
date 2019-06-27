using Antlr4.Runtime.Tree;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    internal class NewTypeNode : Node
    {
        private CoolParser.NewTypeExpContext context;
        
        public NewTypeNode(CoolParser.NewTypeExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.Type = context.TYPE().GetText();
        }

        public override string Type { get;  set; }

        public override string GenerateCode()
        {
            var s = "";
            s += MIPS.Emit(MIPS.Opcodes.jal, Type, "\t\t\t# jump to ctor");
            s += MIPS.Emit(MIPS.Opcodes.move, MIPS.GetReg(), "$v0\t\t#Ctor return value");
            return s;
        }
    }
}