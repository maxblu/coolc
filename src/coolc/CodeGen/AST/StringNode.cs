#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

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
    internal class StringNode : Node
    {
        private CoolParser.StringExpContext context;
        private Node s;

        public string V { get; set; }
        public int I { get; internal set; }

        public StringNode(CoolParser.StringExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.s = s;
            this.V = context.STRING().GetText();
            I = MIPS.stringCount++;
            Type = "String";
        }

        public override string GenerateCode()
        {
            var s = "";

            var i = MIPS.GetReg();

            s = MIPS.Emit(MIPS.Opcodes.la, i, "str"+I,"\t\t# "+V);

            return s;
        }

        public override string GiveMeTheData()
        {
            return "\tstr" + I + ":\t.asciiz\t" + V + "\n";
        }

    }
}