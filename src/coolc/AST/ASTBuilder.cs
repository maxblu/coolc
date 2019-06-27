using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Dynamic;

namespace coolc.AST
{
	/*
	 * Conventions:
	 *	A Type with Coord 0,0 is an unknown type that after the whole ParseTree is traversed will be updated to an actually implemented type or will be deleted from the Inheritance Tree
	 */
	class ASTBuilder : CoolBaseVisitor<object>
	{
		Program p;

		/// <summary>
		/// Method that encapsulates how to get the coordenate of the node in the AST
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private Coord GetCoord(Antlr4.Runtime.ParserRuleContext context)
		{
            return new Coord (context.Start.Line, context.Start.Column);
            //return new Coord(((dynamic)context).Depth(), ((dynamic)context).getAltNumber());
        }

		private bool isDummy(Type t)
		{
			return t.Coord.Col == 0 && t.Coord.Row == 0;
		}
		public override object VisitProgram([NotNull] CoolParser.ProgramContext context)
		{
			p = new Program();
			foreach (var child in context.children)
			{
				if (child is TerminalNodeImpl) { continue; }
				string name = ((CoolParser.ClassdefContext)child).TYPE()[0].GetText();
				// There is an atempt of type redefinition
				if (p.Types.ContainsKey(name) && !isDummy(p.Types[name]))
				{
					p.Errors.Add(new Error("Types can't be redefined", null));
					return p;
				}

				Type type = (Type)Visit(child);
				p.Types[name] = type;
			}

			// This is to remove all the types that were used in the tree but not actually defined in the cool file, they are not cool.

			foreach (var item in p.Types.TakeWhile(e => isDummy(e.Value)))
			{
				p.Types.Remove(item.Key);
			}

			foreach (var t in p.Types)
			{
				// All types not explicitly inheriting, actually are, from Object
				if (t.Value.Parent == null && t.Key != "Object")
				{
					t.Value.Parent = p.Types["Object"];
					p.Types["Object"].Children.Add(t.Value);
				}
			}

			//Run the AST in Program p to update the types of all the nodes
			Typer typer = new Typer(p);
			typer.VisitProgram();

			return p;
		}

		public override object VisitClassdef([NotNull] CoolParser.ClassdefContext context)
		{
			List<Method> methods = new List<Method>();
			List<Attribute> attributes = new List<Attribute>();
			foreach (var child in context.feature())
			{
				var result = Visit(child);
				if (result is Method)
				{
					methods.Add(result as Method);
				}
				else attributes.Add(result as Attribute);
			}
			Coord c = GetCoord(context);
			Type inheritedType = null;
			if (context.INHERITS() != null)
			{
				if (p.Types.ContainsKey(context.TYPE()[1].GetText()))
				{
					inheritedType = p.Types[context.TYPE()[1].GetText()];
				}
				else
				{
					inheritedType = new Type(context.TYPE()[1].GetText(), null, new Coord(0, 0));
					p.Types[context.TYPE()[1].GetText()] = inheritedType;
				}

			}

			return new Type(context.TYPE()[0].GetText(), methods, attributes, inheritedType, c);
		}

		public override object VisitMethodFeature([NotNull] CoolParser.MethodFeatureContext context)
		{
			List<Tuple<string, Type>> parameters = new List<Tuple<string, Type>>();
			foreach (var formal in context.formal())
			{
				Tuple<string, Type> t = (Tuple<string, Type>)(Visit(formal));
				parameters.Add(t);
			}
			Type outputType = new Type(context.TYPE().GetText(), null, new Coord(0, 0));
			if (p.Types.ContainsKey(context.TYPE().GetText()))
			{
				outputType = p.Types[context.TYPE().GetText()];
			}
			else
			{
				p.Types[context.TYPE().GetText()] = outputType;
			}
			Expression exp = (Expression)Visit(context.expresion());

			Coord c = new Coord(context.Depth(), context.getAltNumber());

			return new Method(context.ID().GetText(), outputType, parameters, exp, c);
		}

		public override object VisitAttribFeature([NotNull] CoolParser.AttribFeatureContext context)
		{
			Type attribType;
			if (p.Types.ContainsKey(context.TYPE().GetText()))
				attribType = p.Types[context.TYPE().GetText()];
			else { attribType = new Type(context.TYPE().GetText(), null, new Coord(0, 0)); }

			Expression attribExp = null;
			if (context.expresion() != null)
				attribExp = (Expression)Visit(context.expresion());

			return new Attribute(context.ID().GetText(), attribType, attribExp, GetCoord(context));
		}

		public override object VisitFormal([NotNull] CoolParser.FormalContext context)
		{
			Type outputType = new Type(context.TYPE().GetText(), null, null);
			if (p.Types.ContainsKey(context.TYPE().GetText()))
			{
				outputType = p.Types[context.TYPE().GetText()];
			}
			return new Tuple<string, Type>(context.ID().GetText(), outputType);
		}
		public override object VisitAssignExp([NotNull] CoolParser.AssignExpContext context)
		{
			var exp = (Expression)Visit(context.expresion());
			Coord c = GetCoord(context);
			Identifier i = new Identifier(context.ID().GetText(), null, null, c);
			return new Assign(i, exp, c);
		}

		public override object VisitAtsimExp([NotNull] CoolParser.AtsimExpContext context)
		{
			Expression invokerExp = (Expression)Visit(context.expresion()[0]);
			Type invokedType = null;
			if (context.TYPE() != null)
			{
				string s = context.TYPE().GetText();
				if (p.Types.ContainsKey(s))
				{
					invokedType = p.Types[context.TYPE().GetText()];
				}
				else { invokedType = new Type(context.TYPE().GetText(), null, GetCoord(context)); }
			}

			List<Expression> parameters = new List<Expression>();
			for (int i = 1; i < context.expresion().Length; i++)
				parameters.Add((Expression)Visit(context.expresion()[i]));

			var d = new Dispatch(invokerExp, invokedType, context.ID().GetText(), parameters, GetCoord(context));
			return d;
		}


		public override object VisitBracedExp([NotNull] CoolParser.BracedExpContext context)
		{
			List<Expression> exps = new List<Expression>();
			foreach (var exp in context.expresion())
				exps.Add((Expression)Visit(exp));
			return new Block(exps, GetCoord(context));
		}

		public override object VisitCaseExp([NotNull] CoolParser.CaseExpContext context)
		{
			Expression mainExp = (Expression)Visit(context.expresion()[0]);
			List<Identifier> ids = new List<Identifier>();

			for (int i = 0; i < context.ID().Length; i++)
			{
				Expression ithExp = (Expression)Visit(context.expresion()[i]);
				Type ithType = null;
				if (p.Types.ContainsKey(context.TYPE()[i].GetText()))
					ithType = p.Types[context.TYPE()[i].GetText()];
				else
					ithType = new Type(context.TYPE()[i].GetText(), "Object", new Coord(-1, -1));

				Identifier ithId = new Identifier(context.ID()[i].GetText(), ithExp, ithType, GetCoord(context));
				ids.Add(ithId);
			}

			return new Case(mainExp, ids, GetCoord(context));
		}

		
		public override object VisitBoolExp([NotNull] CoolParser.BoolExpContext context)
		{
			return new Constant(context.BOOL().GetText().ToLower() == "true", p.Types["Bool"], GetCoord(context));
		}

		public override object VisitIdentifierExp([NotNull] CoolParser.IdentifierExpContext context)
		{
			Identifier id = new Identifier(context.ID().GetText(), null, null, new Coord(-1, -1));
			return id;
		}

		public override object VisitEqualsExp([NotNull] CoolParser.EqualsExpContext context)
		{
			Expression left = (Expression)Visit(context.expresion()[0]);
			Expression right = (Expression)Visit(context.expresion()[1]);

			return new BinCompExp(left, right, BoolOp.equal, GetCoord(context));
		}


		#region Expressions
		public override object VisitIfExp([NotNull] CoolParser.IfExpContext context)
		{
			Expression ifExp = (Expression)Visit(context.expresion()[0]);
			Expression thenExp = (Expression)Visit(context.expresion()[1]);
			Expression elseExp = null;
			if (context.expresion().Length > 2)
				elseExp = (Expression)Visit(context.expresion()[2]);
			return new Conditional(ifExp, thenExp, elseExp, GetCoord(context));
		}

		public override object VisitInfixExp([NotNull] CoolParser.InfixExpContext context)
		{
			Expression left = (Expression)Visit(context.expresion()[0]);
			Expression right = (Expression)Visit(context.expresion()[1]);
			if (context.PLUS() != null)
			{
				return new BinArithExp(left, right, Op.plus, GetCoord(context));
			}
			else if (context.MINUS() != null)
			{
				return new BinArithExp(left, right, Op.minus, GetCoord(context));
			}
			else if (context.STAR() != null)
			{
				return new BinArithExp(left, right, Op.times, GetCoord(context));
			}
			else if (context.SLASH() != null)
			{
				return new BinArithExp(left, right, Op.div, GetCoord(context));
			}
			return new BinArithExp(left, right, Op.unknown, GetCoord(context));
		}

		public override object VisitIsvoidExp([NotNull] CoolParser.IsvoidExpContext context)
		{
            var s = VisitChildren(context);

			return new Isvoid( ((coolc.AST.Expression)VisitChildren(context)), GetCoord(context));
		}

		public override object VisitLessEqualExp([NotNull] CoolParser.LessEqualExpContext context)
		{
			Expression left = (Expression)Visit(context.expresion()[0]);
			Expression right = (Expression)Visit(context.expresion()[1]);

			return new BinCompExp(left, right, BoolOp.lesserEq, GetCoord(context));
		}

		public override object VisitLessExp([NotNull] CoolParser.LessExpContext context)
		{
			Expression left = (Expression)Visit(context.expresion()[0]);
			Expression right = (Expression)Visit(context.expresion()[1]);

			return new BinCompExp(left, right, BoolOp.lesser, GetCoord(context));
		}

		public override object VisitLetExp([NotNull] CoolParser.LetExpContext context)
		{
            
			Coord c = GetCoord(context);
		
			List<Identifier> letInits = new List<Identifier>();
            //for (int i = 0; i < context.ID().Length; i++)
            //{
            //	Type idType = new Type(context.TYPE()[i].GetText(), null, null);
            //	if (p.Types.ContainsKey(context.TYPE()[i].GetText()))
            //	{
            //		idType = p.Types[context.TYPE()[i].GetText()];
            //	}
            //	Expression exp = null;
            //	if (i<context.ASSIGN().Length)
            //		 exp = (Expression)Visit(context.expresion()[i]);

            //	Identifier id = new Identifier(context.ID()[i].GetText(), exp, idType, new Coord(context.Depth(), context.getAltNumber()));
            //	letInits.Add(id);
            //	//Shady ...
            //	// ANTLR  stores in context the def in grammar for Let:
            //	//LET ID COLON TYPE(ASSIGN expresion)? (COMMA ID COLON TYPE(ASSIGN expresion) ? )*IN expresion			#letExp
            //	// as:
            //	// an array of IDs,array of COLON, array of ASSIGN, array of expressions
            //	// so im not sure if the lengths of these are equal since you theoretically can skip the assign part of any identifier and just declare it, so in that case i would not know if the ith assign correponds to the ith ID, is that so?, need to check
            //	//Console.WriteLine();
            //	//if(context.expresion[i])
            //}
            foreach( var x in context.newvar())
            {
                if (x.e!=null)
                    letInits.Add(new Identifier(x.id.Text, (Expression)Visit(x.e), new Type(x.t.Text, null, null), GetCoord(context)));
                else
                    letInits.Add(new Identifier(x.id.Text, null, new Type(x.t.Text, null, null), GetCoord(context)));
            }
            var new_body = (Expression)Visit(context.body);
			//Expression body = (Expression)Visit(context.expresion()[0]);
            //context.
                
			return new Let(letInits, new_body, GetCoord(context));
		}


		public override object VisitMethodCallExp([NotNull] CoolParser.MethodCallExpContext context)
		{
			string name = context.ID().GetText();
			Coord c = GetCoord(context);

			List<Expression> parameters = new List<Expression>();
			foreach (var exp in context.expresion())
				parameters.Add((Expression)Visit(exp));

			return new Dispatch(name, parameters, c);
		}


		public override object VisitNewTypeExp([NotNull] CoolParser.NewTypeExpContext context)
		{
			Type t = null;
			if (p.Types.ContainsKey(context.TYPE().GetText()))
			{
				t = p.Types[context.TYPE().GetText()];
			}
			else t = new Type(context.TYPE().GetText(), "Object", new Coord(-1, -1));

			return new New(t, GetCoord(context));
		}

		public override object VisitNotExp([NotNull] CoolParser.NotExpContext context)
		{
            var s = (Expression)Visit(context.expresion());

            return new Not(s, GetCoord(context));
		}

		public override object VisitParentExp([NotNull] CoolParser.ParentExpContext context)
		{
			return new Parenth((Expression)Visit(context.expresion()), GetCoord(context));
		}
		public override object VisitTildeExp([NotNull] CoolParser.TildeExpContext context)
		{
			return new Tilde((Expression)Visit(context.expresion()), GetCoord(context));
		}
		public override object VisitWhileExp([NotNull] CoolParser.WhileExpContext context)
		{
			Expression condition = (Expression)Visit(context.expresion()[0]);
			Expression body = (Expression)Visit(context.expresion()[1]);
			return new Loop(condition, body, GetCoord(context));
		}
		#endregion

		public override object VisitTerminal(ITerminalNode node)
		{
			int n;
			float f;
			if (int.TryParse(node.GetText(), out n)) { return new Constant(n,p.Types["Int"],new Coord(0,0)); }
			if (node.GetText() == "true" || node.GetText() == "false") return node.GetText() == "true" ?
					 new Constant(true, p.Types["Bool"],new Coord(0,0)) :
					 new Constant(false, p.Types["Bool"], new Coord(0, 0));
			return new Constant(node.GetText(), p.Types["String"], new Coord(0, 0));
		}
		public override object VisitErrorNode(IErrorNode node)
		{
			throw new NotImplementedException();
		}
		/*dont know if there is a need to override other methods, Check this out, TODO*/
	}

    class Typer
    {
        Program p;
        public Typer(Program p)
        {
            this.p = p;
        }

        public void VisitProgram()
        {
            foreach (var t in p.Types)
            {
                if (t.Key == "IO" || t.Key == "String" || t.Key == "Int" || t.Key == "Object") continue;
                VisitType(t.Value);
            }
        }
        private void VisitType(Type t)
        {
            if (t.Parent != null)
            {
                t.Parent = p.Types[t.Parent.Name];
                t.Parent.Children.Add(t);
            }
            if (t.Methods != null)
            {
                foreach (Method m in t.Methods)
                {
                    VisitMethod(m);
                }
            }
            if (t.Attributes != null)
            {
                foreach (Attribute a in t.Attributes)
                {
                    VisitAttribute(a);
                }
            }
        }

        private void VisitMethod(Method m)
        {
            m.OutputType = p.Types[m.OutputType.Name];
            for (int i = 0; i < m.InputParams.Count; i++)
            {
                m.InputParams[i] = new Tuple<string, Type>(m.InputParams[i].Item1, p.Types[m.InputParams[i].Item2.Name]);
            }
            VisitExpression(m.Body);
        }
        private void VisitAttribute(Attribute a)
        {
            a.Type = p.Types[a.Type.Name];
            VisitExpression(a.Exp);
        }

        private void VisitExpression(Expression e)
        {
            if (e is Identifier)
            {
                //e.Type = p.Types[e.Type.Name];
                VisitExpression((e as Identifier).Exp);
            }
            if (e is Assign)
            {
                //The type of the left of the assign is the type of the expression resulting of evaluating the assigned expression
                //((Assign)e).Left.Type = p.Types[((Assign)e).Left.Type.Name];
                VisitExpression(((Assign)e).Exp);
            }
            if (e is Dispatch)
            {
                VisitExpression(((Dispatch)e).InvokerExp);
                if (((Dispatch)e).InvokedType != null)
                {
                    ((Dispatch)e).InvokedType = p.Types[((Dispatch)e).InvokedType.Name];
                }
                // The type of the dispatch is the type of the expression resulting from evaluating it so the next lines shouldn't be?
                //((Dispatch)e).Type = p.Types[((Dispatch)e).Type.Name];
                //((Dispatch)e).InvokedType = p.Types[((Dispatch)e).InvokedType.Name];
                foreach (Expression exp in ((Dispatch)e).Parameters)
                {
                    VisitExpression(exp);
                }
            }
            if (e is Conditional)
            {
                VisitExpression(((Conditional)e).IfExp);
                VisitExpression(((Conditional)e).ThenExp);
                VisitExpression(((Conditional)e).ElseExp);
            }
            if (e is Loop)
            {
                VisitExpression(((Loop)e).Condition);
                VisitExpression(((Loop)e).Body);
            }
            if (e is Block)
            {
                foreach (Expression exp in ((Block)e).Exps)
                {
                    VisitExpression(exp);
                }
            }
            if (e is Let)
            {
                foreach (Identifier item in ((Let)e).Parameters)
                {
                    item.Type = p.Types[item.Type.Name];
                    VisitExpression(item.Exp);
                }
                VisitExpression(((Let)e).Body);
            }
            if (e is Case)
            {
                VisitExpression(((Case)e).Exp0);
                foreach (var item in ((Case)e).Ids)
                {
                    VisitExpression(item.Exp);
                }
            }
            if (e is New)
            {
                return;
            }
            if (e is Isvoid)
            {
                VisitExpression(((Isvoid)e).Exp);
            }

            if (e is Not)
            {
                VisitExpression(((Not)e).Exp);
            }
            if (e is Parenth)
            {
                VisitExpression(((Parenth)e).Exp);
            }
            if (e is Tilde)
            {
                VisitExpression(((Tilde)e).Exp);
            }
            if (e is BinArithExp)
            {
                VisitExpression(((BinArithExp)e).Left);
                VisitExpression(((BinArithExp)e).Right);
            }
            if (e is BinCompExp)
            {
                VisitExpression(((BinCompExp)e).Left);
                VisitExpression(((BinCompExp)e).Right);
            }

        }
    }

}
