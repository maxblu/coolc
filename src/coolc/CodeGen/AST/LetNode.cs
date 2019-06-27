using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    internal class LetNode : Node
    {
        private CoolParser.LetExpContext context;
        //private Node s;

        public LetNode(CoolParser.LetExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            IDs = new List<IToken>();
            Types = new List<IToken>();
            Exprs = new List<CoolParser.ExpresionContext>();

            for (int i=0; i < context.newvar().Length; i++)
            {
                IDs.Add(context.newvar()[i].id);
                Types.Add(context.newvar()[i].t);
                Exprs.Add(context.newvar()[i].e);
            }
            //IDs = new ITerminalNode[context];
            //Types = context.TYPE();
            //Exprs = context.expresion();
            Symbols = new Dictionary<string, ClassNode>();

            for (int i = 0; i < IDs.Count; i++)
            {
                var g = Types[i].Text;
                //SymbolTable.Classes[Types[i].GetText()] = new ClassNode(g, null);
                Symbols[IDs[i].Text] = SymbolTable.Classes[Types[i].Text];
                //SymbolTable.Symbols.Push(Symbols);
            }
        }
        public List<CoolParser.ExpresionContext> Exprs { get; private set; }
        public List<IToken> IDs { get; private set; }
        public List<IToken> Types { get; private set; }
        public Dictionary<string, ClassNode> Symbols { get; private set; }

        public override string GiveMeTheData()
        {
            var s = "";
            for (int i = 0; i < IDs.Count; i++)
            {
                s += Generate(IDs[i], Types[i]);
            }
            return s + base.GiveMeTheData();
        }
        private string Generate(IToken id, IToken type)
        {
            var s = "";
            s = "\t" /*+ type.GetText() + "_"*/ + id.Text + ":\t.word\t" + 0 + "\n";
            return s;
        }
        public override string GenerateCode()
        {
            SymbolTable.Symbols.Push(Symbols);

            var s = "# Let expresion\n";
            foreach (var item in Childs)
            {
                s+=item.GenerateCode();
            }
            SymbolTable.Symbols.Pop();
            return s;
        }
    }
}