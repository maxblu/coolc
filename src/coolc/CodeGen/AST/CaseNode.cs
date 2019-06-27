using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    internal class CaseNode : Node
    {
        private CoolParser.CaseExpContext context;
        //private Node s;

        public CaseNode(CoolParser.CaseExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;

            //this.s = s;
            this.Expresion = context.expresion(0);
            this.IDs = context.ID();
            this.Types = context.TYPE();
            this.Expresions = context.expresion();
        }

        public CoolParser.ExpresionContext Expresion { get; private set; }
        public ITerminalNode[] IDs { get; private set; }
        public ITerminalNode[] Types { get; private set; }
        public CoolParser.ExpresionContext[] Expresions { get; private set; }
        public override string GenerateCode()
        {
            return base.GenerateCode();
        }
        
    }
}