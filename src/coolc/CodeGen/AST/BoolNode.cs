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
    internal class BoolNode : Node
    {
        private CoolParser.BoolExpContext context;

        public bool V { get; set; }

        public BoolNode(CoolParser.BoolExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            this.V = context.BOOL().GetText().ToLower() == "true";
            Type = "Bool";

        }

        public override string GenerateCode()
        {

            var s = "";
            var reg1 = "";

            //var exp = Childs[0];//why it only has one son
            //var es = exp.GenerateCode();

            //reg1 = MIPS.LastReg().ToReg();// here we asume that an int has been loaded to the last position
            reg1 = MIPS.GetReg();// here we asume that an int has been loaded to the last position

            //s += "\t# Assign\n";
            //s += MIPS.Emit(MIPS.Opcodes.usw, reg1, ID + "\t\t# " + ID);
            s += MIPS.Emit(MIPS.Opcodes.li, reg1, V.GetHashCode().ToString());

            return s;
        }
    }
}