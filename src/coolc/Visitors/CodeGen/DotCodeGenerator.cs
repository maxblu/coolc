using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.IO;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.Visitors
{
    class DotCodeGenerator : CoolBaseVisitor<object>
    {
        string dotcode = "digraph pvn {\n";
        //Stack<string> s = new Stack<string>();
        string filename = "";
        public DotCodeGenerator(string filename)
        {
            this.filename = filename;
        }

        public void WriteDotCode()
        {
            dotcode += "\n}";
            FileStream s = new FileStream(filename + ".gv", FileMode.Create);
            byte[] ddc = new byte[dotcode.Length];
            for (int i = 0; i < dotcode.Length; i++)
                ddc[i] = (byte)dotcode[i];
            s.Write(ddc, 0, ddc.Length); s.Flush(); s.Close();
        }
        public override object VisitProgram([NotNull] CoolParser.ProgramContext context)
        {
            return VisitChildren(context);
        }

        public override object Visit(IParseTree tree)
        {
            base.Visit(tree);
            WriteDotCode();
            return null;
        }

        //public object VisitChildren(IRuleNode node)
        //{
        //    throw new NotImplementedException();
        //}

        //public object VisitTerminal(ITerminalNode node)
        //{
        //    throw new NotImplementedException();
        //}

        //public object VisitErrorNode(IErrorNode node)
        //{
        //    throw new NotImplementedException();
        //}
        //public override 

        public override object VisitClassdef([NotNull] CoolParser.ClassdefContext context)
        {
            dotcode += "program->" + context.t.Text + "\n";
            dotcode += context.t.Text + "[label=\"" + context.t.Text;
            if (context.it != null)
                dotcode += ":" + context.it.Text;
            dotcode += "\nclass\"]\n";
            return VisitChildren(context);
        }

        public override object VisitMethodFeature([NotNull] CoolParser.MethodFeatureContext context)
        {

            CoolParser.ClassdefContext p = (CoolParser.ClassdefContext)(context.Parent);
            dotcode += p.t.Text + "->" + context.id.Text + "\n";
            dotcode += context.id.Text + "[label=\"" + context.id.Text;
            if (context.t != null)
                dotcode += ":" + context.t.Text;
            dotcode += "\nmethod\"]\n";
            // TODO fix signature of methods in dotcode?
            // do not make a new node for each formal
            var fs = context.formal();
            foreach (var item in fs)
            {
                dotcode += context.id.Text + "->" + item.id.Text + "\n";
                dotcode += item.id.Text + "[label=\"" + item.id.Text + ":" + item.t.Text + "\nformal\"]\n";
            }
            return VisitChildren(context);
        }

        public override object VisitAttribFeature([NotNull] CoolParser.AttribFeatureContext context)
        {
            CoolParser.ClassdefContext p = (CoolParser.ClassdefContext)(context.Parent);
            dotcode += p.t.Text + "->" + context.id.Text + "\n";
            dotcode += context.id.Text + "[label=\"" + context.id.Text + ":" + context.t.Text + "\nattribute\"]\n";
            return VisitChildren(context);
        }

        //public override object VisitFormal([NotNull] CoolParser.FormalContext context)
        //{
        //    return VisitChildren(context);
        //}

        //public override object 
        private void VisitExpr(CoolParser.ExpresionContext context)
        {

            try
            {
                CoolParser.MethodFeatureContext p = (CoolParser.MethodFeatureContext)context.Parent;
                dotcode += p.id.Text + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
                return;
            }
            catch (Exception) { }
            try
            {
                CoolParser.AttribFeatureContext p = (CoolParser.AttribFeatureContext)context.Parent;
                dotcode += p.id.Text + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
                return;

            }
            catch (Exception) { }
            try
            {
                CoolParser.ExpresionContext p = (CoolParser.ExpresionContext)context.Parent;
                dotcode += p.GetType().Name.Substring(0, p.GetType().Name.Length - 7) + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
                return;
            }
            catch (Exception) { }

            //switch (context.Parent)
            //{
            //    case CoolParser.MethodFeatureContext p:
            //        dotcode += p.id.Text + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
            //        break;
            //    case CoolParser.AttribFeatureContext p:
            //        dotcode += p.id.Text + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
            //        break;
            //    case CoolParser.ExpresionContext p:
            //        dotcode += p.GetType().Name.Substring(0, p.GetType().Name.Length - 7) + "->" + "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"" + "\n";
            //        break;
            //    default:
            //        break;
            //}
            ////return VisitChildren(context);
        }
        //public override object VisitProgram([NotNull] CoolParser.ProgramContext context)
        //{
        //    VisitExpr(context);
        //    return VisitChildren(context);
        //}
        //public override object VisitClassdef([NotNull] CoolParser.ClassdefContext context)
        //{
        //    VisitExpr(context);
        //    return VisitChildren(context);
        //}
        //public override object VisitMethodFeature([NotNull] CoolParser.MethodFeatureContext context)
        //{
        //    VisitExpr(context);
        //    return VisitChildren(context);
        //}
        //public override object VisitAttribFeature([NotNull] CoolParser.AttribFeatureContext context)
        //{
        //    VisitExpr(context);
        //    return VisitChildren(context);
        //}
        public override object VisitFormal([NotNull] CoolParser.FormalContext context)
        {
            //VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitNewTypeExp([NotNull] CoolParser.NewTypeExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitWhileExp([NotNull] CoolParser.WhileExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitBracedExp([NotNull] CoolParser.BracedExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"{}\"" + "]" + "\n";

            return VisitChildren(context);
        }
        public override object VisitTildeExp([NotNull] CoolParser.TildeExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"~\"" + "]" + "\n";

            return VisitChildren(context);
        }
        public override object VisitIntExp([NotNull] CoolParser.IntExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + context.GetText() + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitLessEqualExp([NotNull] CoolParser.LessEqualExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"<=\"" + "]" + "\n";

            return VisitChildren(context);
        }
        public override object VisitIdentifierExp([NotNull] CoolParser.IdentifierExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + context.GetText() + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitAtsimExp([NotNull] CoolParser.AtsimExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitBoolExp([NotNull] CoolParser.BoolExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + context.GetText() + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitCaseExp([NotNull] CoolParser.CaseExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitNotExp([NotNull] CoolParser.NotExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "not" + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitIsvoidExp([NotNull] CoolParser.IsvoidExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "isvoid" + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitParentExp([NotNull] CoolParser.ParentExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"()\"" + "]" + "\n";

            return VisitChildren(context);
        }
        public override object VisitStringExp([NotNull] CoolParser.StringExpContext context)
        {
            VisitExpr(context);
            //var s = context.s.Text;
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + context.GetText() + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitAssignExp([NotNull] CoolParser.AssignExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"<-\"" + "]" + "\n";

            return VisitChildren(context);
        }
        public override object VisitIfExp([NotNull] CoolParser.IfExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitLetExp([NotNull] CoolParser.LetExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitMethodCallExp([NotNull] CoolParser.MethodCallExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }
        public override object VisitEqualsExp([NotNull] CoolParser.EqualsExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"=\"" + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitLessExp([NotNull] CoolParser.LessExpContext context)
        {
            VisitExpr(context);
            dotcode += "\"" + context.GetType().Name.Substring(0, context.GetType().Name.Length - 7) + "\"[label=" + "\"<\"" + "]" + "\n";
            return VisitChildren(context);
        }
        public override object VisitInfixExp([NotNull] CoolParser.InfixExpContext context)
        {
            VisitExpr(context);
            return VisitChildren(context);
        }

    }
}
