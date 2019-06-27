using System;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using coolc.CodeGen;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.NewSem
{
    /// <summary>
    /// 
    /// </summary>
    public class SemanticChecker : CoolBaseVisitor<bool>
    {

        public TypeVisitor TV { get; set; }
        public List<string> Errors { get; set; }
        public SemanticChecker()
        {
            Errors = new List<string>();
        }

        public bool Check(IParseTree tree)
        {
            TV = new TypeVisitor();
            TV.MakeSymbolTable(tree);
            return Visit(tree);
        }

        // public override bool Visit(IParseTree tree)
        // {
        //     throw new NotImplementedException();
        // }

        //public override bool VisitTerminal(ITerminalNode node)
        //{
        //    Errors.Add("ERROR: Line 3, Column 4: wtf??");
        //    return true;
        //}

        public override bool VisitChildren(IRuleNode node)
        {
            bool result = DefaultResult;
            int n = node.ChildCount;
            for (int i = 0; i < n; i++)
            {
                if (!ShouldVisitNextChild(node, result))
                {
                    break;
                }
                IParseTree c = node.GetChild(i);
                bool childResult = c.Accept(this);
                //result = AggregateResult(result, childResult);
                result = result || childResult;
            }
            return result;
        }
        //protected override bool ShouldVisitNextChild(IRuleNode node, bool currentResult)
        //{
        //    return !currentResult;//just one error?
        //}
        //protected internal override bool AggregateResult(bool aggregate, bool nextResult)
        //{
        //    return nextResult;
        //}
        //public override bool VisitErrorNode(IErrorNode node)
        //{
        //    return VisitChildren(context);
        //}

        public override bool VisitAssignExp([NotNull] CoolParser.AssignExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitAtsimExp([NotNull] CoolParser.AtsimExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitAttribFeature([NotNull] CoolParser.AttribFeatureContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitBoolExp([NotNull] CoolParser.BoolExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitBracedExp([NotNull] CoolParser.BracedExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitCaseExp([NotNull] CoolParser.CaseExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitClassdef([NotNull] CoolParser.ClassdefContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitEqualsExp([NotNull] CoolParser.EqualsExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitFormal([NotNull] CoolParser.FormalContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitIdentifierExp([NotNull] CoolParser.IdentifierExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitIfExp([NotNull] CoolParser.IfExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitInfixExp([NotNull] CoolParser.InfixExpContext context)
        {
            var s1 = context.le.getCoolType();
            var s2 = context.re.getCoolType();
            if (s1 == s2)
            {
                return VisitChildren(context);
            }
            else
            {
                return true;
            }
            return VisitChildren(context);
        }

        public override bool VisitIntExp([NotNull] CoolParser.IntExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitIsvoidExp([NotNull] CoolParser.IsvoidExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitLessEqualExp([NotNull] CoolParser.LessEqualExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitLessExp([NotNull] CoolParser.LessExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitLetExp([NotNull] CoolParser.LetExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitMethodCallExp([NotNull] CoolParser.MethodCallExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitMethodFeature([NotNull] CoolParser.MethodFeatureContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitNewTypeExp([NotNull] CoolParser.NewTypeExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitNotExp([NotNull] CoolParser.NotExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitParentExp([NotNull] CoolParser.ParentExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitProgram([NotNull] CoolParser.ProgramContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitStringExp([NotNull] CoolParser.StringExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitTildeExp([NotNull] CoolParser.TildeExpContext context)
        {
            return VisitChildren(context);
        }

        public override bool VisitWhileExp([NotNull] CoolParser.WhileExpContext context)
        {
            return VisitChildren(context);
        }


    }


    static class Extensors
    {
        public static string getCoolType(this CoolParser.ExpresionContext s)
        {

            try { var p = (CoolParser.InfixExpContext)(s); return "INT"; } catch (Exception) { }
            try { var p = (CoolParser.IntExpContext)(s); return "INT"; } catch (Exception) { }
            try { var p = (CoolParser.NotExpContext)(s); return "BOOL"; } catch (Exception) { }
            try { var p = (CoolParser.LessEqualExpContext)(s); return "BOOL"; } catch (Exception) { }
            try { var p = (CoolParser.LessExpContext)(s); return "BOOL"; } catch (Exception) { }
            try { var p = (CoolParser.EqualsExpContext)(s); return "BOOL"; } catch (Exception) { }
            try { var p = (CoolParser.StringExpContext)(s); return "STRING"; } catch (Exception) { }
            try { var p = (CoolParser.NewTypeExpContext)(s); return p.t.Text; } catch (Exception) { }
            try { var p = (CoolParser.AssignExpContext)(s); return p.e.getCoolType(); } catch (Exception) { }
            //TODO: fix this :(
            try { var p = (CoolParser.IdentifierExpContext)(s); return SymbolTable.Symbols.Peek()[(p as CoolParser.IdentifierExpContext).id.Text].Type; } catch (Exception) { /*throw; */}


            return "VOID?";
        }

        public static string getCoolType(this CoolParser.InfixExpContext s)
        {
            return "INT";
        }
        public static string getCoolType(this CoolParser.IntExpContext s)
        {
            return "INT";
        }
        public static string getCoolType(this CoolParser.NotExpContext s)
        {
            return "BOOL";
        }
        public static string getCoolType(this CoolParser.LessEqualExpContext s)
        {
            return "BOOL";
        }
        public static string getCoolType(this CoolParser.LessExpContext s)
        {
            return "BOOL";
        }
        public static string getCoolType(this CoolParser.EqualsExpContext s)
        {
            return "BOOL";
        }
        public static string getCoolType(this CoolParser.StringExpContext s)
        {
            return "STRING";
        }
        public static string getCoolType(this CoolParser.NewTypeExpContext s)
        {
            return s.t.Text;
        }
        //public static string getCoolType(this CoolParser.AssignExpContext s)
        //{
        //    return s.e.getCoolType();
        //}
        //public static IToken getIToken(this IToken s)
        //{
        //    return s;
        //}
    }

}