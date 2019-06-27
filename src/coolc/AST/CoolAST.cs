using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.AST
{
	public class Program  // a Program node in the AST
	{
		Dictionary<string, Type> types;
		List<Error> errors;
		List<string> basicTypes;
		public Program()
		{
			types = new Dictionary<string, Type>();
			errors = new List<Error>();

			Type Object = new Type("Object", new List<Method>(), new List<Attribute>(), null, new Coord(-1, -1));
			Type Int = new Type("Int", new List<Method>(), new List<Attribute>(), Object, new Coord(-1, -1));
			Type String = new Type("String", new List<Method>(), new List<Attribute>(), Object, new Coord(-1, -1));
			Type SelfType = new Type("SELF_TYPE", new List<Method>(), new List<Attribute>(), Object, new Coord(-1, -1));
			Type IO = new Type("IO", new List<Method>(), new List<Attribute>(), Object, new Coord(-1, -1));
			Type Bool = new Type("Bool", new List<Method>(), new List<Attribute>(), Object, new Coord(-1, -1));
        

			/*
			 * The methods out string and out int print their argument and return their self parameter. The
				method in string reads a string from the standard input, up to but not including a newline character.
				The method in int reads a single integer, which may be preceded by whitespace. Any characters following
				the integer, up to and including the next newline, are discarded by in int.
				A class can make use of the methods in the IO class by inheriting from IO. It is an error to redefine
				the IO class.
			 */

			#region Object
			List<Method> objMeth = new List<Method>();
			objMeth.Add(new Method("abort", Object, new List<Tuple<string, Type>>(), null, new Coord(-1, -1)));
			objMeth.Add(new Method("type_name", String, new List<Tuple<string, Type>>(), null, new Coord(-1, -1)));
			objMeth.Add(new Method("copy", SelfType, new List<Tuple<string, Type>>(), null, new Coord(-1, -1)));

			Object.Methods = objMeth;
			#endregion

			#region String
			List<Tuple<string, Type>> strInput = new List<Tuple<string, Type>>();
			List<Method> strMeth = new List<Method>();
			strMeth.Add(new Method("length", Int, new List<Tuple<string, Type>>(), null, new Coord(-1, -1)));
			strInput.Add(new Tuple<string, Type>("s", String));
			strMeth.Add(new Method("concat", String, strInput, null, new Coord(-1, -1)));

			List<Tuple<string, Type>> strInput2 = new List<Tuple<string, Type>>();
			strInput2.Add(new Tuple<string, Type>("i", Int));
			strInput2.Add(new Tuple<string, Type>("l", Int));
			strMeth.Add(new Method("substr", String, strInput2, null, new Coord(-1, -1)));

			String.Methods = strMeth;
			#endregion

			#region IO
			List<Method> ioMeth = new List<Method>();
			List<Tuple<string, Type>> ioInput = new List<Tuple<string, Type>>();
			ioInput.Add(new Tuple<string, Type>("str", String));
			ioMeth.Add(new Method("out_string", IO, ioInput, null, new Coord(-1, -1)));

			List<Tuple<string, Type>> ioInput2 = new List<Tuple<string, Type>>();
			ioInput2.Add(new Tuple<string, Type>("x", Int));
			ioMeth.Add(new Method("out_int", IO, ioInput2, null, new Coord(-1, -1)));
			ioMeth.Add(new Method("in_string", String, new List<Tuple<string, Type>>(), null, new Coord(-1, -1)));
			ioMeth.Add(new Method("in_int", Int, new List<Tuple<string, Type>>(),null, new Coord(-1, -1)));

			IO.Methods = ioMeth;
			#endregion


			types.Add("Object", Object);
			types.Add("Int", Int);
			types.Add("String", String);
			types.Add("IO", IO);
			types.Add("Bool", Bool);
			types.Add("SELF_TYPE", SelfType);

			types["Object"].Children.AddRange(new List<Type>() { Int, String, IO, Bool, SelfType });

			basicTypes = new List<string>() { "Object", "Int", "String", "IO", "Bool", "SELF_TYPE" };
		}
        

		public List<Error> CheckSemantics()
		{
			Scope scope = new Scope();
			scope.Errors = errors;
			scope.DefVars.Find(v => v.Name == "self").Type = types["SELF_TYPE"];

			List<Type> l = new List<Type>();
			foreach (var t in types)
			{
				scope.Types.Add(t.Key, t.Value);
				l.Add(t.Value);
			}

			//If there are new types, check for inheritance cycles
			if (types.Count > basicTypes.Count && !CheckInheritanceCycles(l, scope))
				return scope.Errors;

			// Every type if does not inherit from someone explicitly, then they inher from Object
			foreach (var kvp in types)
			{
				if (kvp.Value.Parent == null && kvp.Key != "Object")
				{
					kvp.Value.Parent = types["Object"];
					types["Object"].Children.Add(kvp.Value);
				}
			}

			//Make every type inherit the methods and attributes from its parent
			inheritMethodsAndAttributes();

			foreach (var type in scope.Types.Values)
			{
				//Don't Check semantically the basic types
				if (basicTypes.Exists(t => t == type.Name)) continue;

				//Semantic Check for every type
				Scope s = new Scope(scope);
				if (!type.CheckSemantics(s))
					return s.Errors;
			}
			return scope.Errors;
		}


		private bool CheckInheritanceCycles(List<Type> ts, Scope scope)
		{
			Type current;
			Dictionary<string, Type> visited;
			foreach (Type t in ts)
			{
				current = t;
				visited = new Dictionary<string, Type>();
				if (visited.ContainsKey(t.Name)) continue;

				while (current.Parent != null)
				{
					if (visited.ContainsKey(current.Name))
					{
						scope.Errors.Add(new Error("There is an inheritance cycle: " + printInheritanceCycle(visited), current.Coord));
						return false;
					}
					visited[current.Name] = current;
					current = current.Parent;
				}
			}
			return true;
		}

		private string printInheritanceCycle(Dictionary<string, Type> visited)
		{
			string result = "";
			List<Type> l = new List<Type>();
			foreach (var item in visited.Values) l.Add(item);
			Type current = l[0];
			for (int i = 0; i < visited.Count; i++)
			{
				if (i == 0) { result += current.Name; }
				else result += " => " + current.Name;
				current = current.Parent;
			}
			current = l[0];
			return result + " => " + current.Name;
		}

		private void inheritMethodsAndAttributes()
		{
			Type currntType = types["Object"];
			foreach (Type t in Types["Object"].Children)
			{
				//if (t.Name == "IO" || t.Name == "String" || t.Name == "Int") continue;
				inherit(t);
			}
		}
		private void inherit(Type t)
		{
			// If the current type doesn't have a method that it's parent has, add it
			foreach (Method m in t.Parent.Methods)
			{
				if (!t.Methods.Exists(e => e.Name == m.Name))
					t.Methods.Add(m);
			}

			// If the current type doesn't have a attribute that it's parent has, add it
			foreach (Attribute a in t.Parent.Attributes)
			{
				if (!t.Attributes.Exists(at => at.Name == a.Name))
				{
					Attribute at = new Attribute(a.Name, a.Type, a.Exp, true, t.Parent, null);
					t.Attributes.Add(at);
				}
			}
			foreach (Type child in t.Children)
				inherit(child);
		}

		public Dictionary<string, Type> Types { get { return types; } private set { types = value; } }
		public List<string> BasicTypes { get { return basicTypes; } private set { basicTypes = value; } }
		public List<Error> Errors { get { return errors; } private set { errors = value; } }
	}

	public class Type : Statement //i.e a class
	{
		string name;
		List<Method> methods;
		List<Attribute> attributes;
		Type parent;
		List<Type> children;
		public Type(string name, string parent, Coord c) : base(c)
		{
			this.name = name;
			methods = new List<Method>();
			attributes = new List<Attribute>();
			parent = null;
			children = new List<Type>();
		}
		public Type(string name, List<Method> methods, List<Attribute> attrib, Type parent, Coord c) : base(c)
		{
			this.name = name;
			this.methods = methods;
			this.attributes = attrib;
			this.parent = parent;
			Coord = c;
			children = new List<Type>();
			if(this.parent != null && !this.parent.Children.Exists(t=> t.Name == name))
				this.parent.Children.Add(this);
		}
		public string Name { get { return name; } set { name = value; } }
		public List<Method> Methods { get { return methods; } set { methods = value; } }
		public List<Attribute> Attributes { get { return attributes; } set { attributes = value; } }
		public Type Parent { get { return parent; } set { parent = value; } }
		public List<Type> Children { get { return children; } set { children = value; } }

		// To test
		public override bool CheckSemantics(Scope scope)
		{
			scope.EnclosingType = this;
			string msg = "";
			bool typeIsCorrect = true;
			if (!scope.Types.ContainsKey(Name))
			{
				msg = Name + " is used as a type that isn't defined";
				typeIsCorrect = false;
				scope.Errors.Add(new Error(msg, Coord));
			}
			if (Name == parent.Name)
			{
				msg = "The type " + Name + " has the same name as its parent";
				typeIsCorrect = false;
				scope.Errors.Add(new Error(msg, Coord));
			}


			//methods.Sort(new MethodComparer());
			for (int i = 0; i < methods.Count - 1; i++)
			{
				var m1 = methods[i];
				var m2 = methods[i + 1];
				if (m1.Name == m2.Name)
				{
					scope.Errors.Add(new Error("The method: '" + m1.Name + "' is already defined,use another name ", Coord));
					typeIsCorrect = false;
				}
			}

			scope.DefMethod = methods;
            //scope.DefVars
			scope.Attributes = attributes;

			foreach (Method m in methods)
			{
				Method baseMethod = Parent.Methods.Find(e => e.Name == m.Name); // At least parent will always be Object
				if (baseMethod != null)
				{
                    m.CheckSemantics(new Scope(scope));
                    if (baseMethod.OutputType != m.OutputType && !IsDescendant(m.OutputType,baseMethod.OutputType))
					{
                        var s = LeastType(baseMethod.OutputType, m.OutputType);
						scope.Errors.Add(new Error("Method:'" + m.Name + "' can't be redefined with diferent output type:'" + m.OutputType.Name + "'", Coord));
						typeIsCorrect = false;
					}
					if (inputParamsAreDistinct(baseMethod.InputParams, m.InputParams))
					{
						scope.Errors.Add(new Error("Method:'" + m.Name + "' can't be redefined with diferent input types", Coord));
						typeIsCorrect = false;
					}
				}
				typeIsCorrect = m.CheckSemantics(new Scope(scope));
			}

			attributes.Sort(new AttributeComparer());
			for (int i = 0; i < Attributes.Count - 1; i += 2)
			{
				var a1 = attributes[i];
				var a2 = attributes[i + 1];
				if (a1.Name == a2.Name)
				{
					scope.Errors.Add(new Error("There is already an attribute with the name: " + a2.Name + " defined", Coord));
					typeIsCorrect = false;
				}
			}
            foreach (Attribute a in Attributes)
                scope.DefVars.Add(a.Id);

            foreach (Attribute a in Attributes)
			{
				//propertys can't be redefined, it's illegal according to the manual
				if (!a.IsInherited.Item1 && parent.Attributes.Exists(attrb => attrb.Name == a.Name))
				{
					scope.Errors.Add(new Error("Attribute:'" + a.Name + "' can't be redefined by:'" + Name + "' since it is inherited from:'" + parent.Name + "'", Coord));
					typeIsCorrect = false;
				}
				typeIsCorrect = a.CheckSemantics(scope);// WFT??
			}
			return typeIsCorrect;
		}
		public static bool IsDescendant(Type t1, Type t0)
		{
			if (t0 == null || t1 == null || t0.Name == t1.Name)
				return true;
			Type crnt_type = t1;
			while (crnt_type != null)
			{
				if (crnt_type.name == t0.name) return true;
				crnt_type = crnt_type.parent;
			}
			return false;
		}

		//To test
		public static Type LeastType(Type t0, Type t1)
		{
			if (t0.Name == t1.Name) return t0;

            if (t0.Name == "Object" )
                return t0;


            if ( t1.Name == "Object")
                return t1;
            if (IsDescendant(t0, t1))
            {
                return t1;
            }
            if (IsDescendant(t1,t0))
            {
                return t0;
            }

			Type t0parent = t0.parent;
			Type t1parent = t1.parent;

            if (t0parent.Name == "Object")
                return t0parent;

            if (t1parent.Name == "Object")
                return t1parent;

			Dictionary<string, Type> t0FamTree = new Dictionary<string, Type>();
			while (t0parent.parent != null)
			{
				t0FamTree.Add(t0parent.Name, t0parent);
				t0parent = t0parent.parent;
			}
			while (t1parent.parent != null)
			{
				if (t0FamTree.ContainsKey(t1parent.name))
				{
					return t1parent;
				}
				t1parent = t1parent.parent;
			}
			throw new Exception("LeastType did not work correctly"); // this should never be thrown cus it should at least return Object
		}
		public static Identifier LeastType(Type t0, List<Identifier> lt)
		{
			Type current = t0;
			while (current != null)
			{
				Identifier id = lt.Find(e => e.Type.Name == current.Name);
				if (id != null)
					return id;
				current = current.parent;
			}
			return null; // This should never be throw 'cus at least Object is like Darth Vader you know ...
		}

		// to test
		public static Type Leasts(List<Identifier> ids)
		{
			Type currentLeast = ids[0].Type;  // ids has at least length 1
			foreach (Identifier id in ids)
				currentLeast = LeastType(currentLeast, id.Type);
			return currentLeast;
		}
		//To test
		/// <summary>
		/// Input parameters are distinct if there is a diferent number of arguments or they have diferent types
		/// </summary>
		/// <param name="iP1"></param>
		/// <param name="iP2"></param>
		/// <returns></returns>
		private bool inputParamsAreDistinct(List<Tuple<string, Type>> iP1, List<Tuple<string, Type>> iP2)
		{
			if (iP1.Count != iP2.Count) return true;
			for (int i = 0; i < iP1.Count; i++)
				if (iP1[i].Item2 != iP2[i].Item2) return true;
			return false;
		}


		public  void initType()
		{
			foreach (Attribute a in Attributes)
			{

			}
		}
	}

	class MethodComparer : IComparer<Method>
	{
		public int Compare(Method x, Method y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}
	class AttributeComparer : IComparer<Attribute>
	{
		public int Compare(Attribute x, Attribute y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	public class Scope
	{
		Scope parentScope;
		List<Identifier> defVars;
		List<Attribute> attributes;
		List<Method> defMeth;
		Dictionary<string, Type> types; //Types defined
		List<Error> errors;
		Type enclosingType;

		public Scope()
		{
			Reset();
		}
		public Scope(Scope father)
		{
			Reset(father);
		}

		public void Reset(Scope father = null)
		{
			parentScope = father;
			if (parentScope != null)
			{
				defVars = cloneIdentifiers(parentScope.defVars);
				attributes = cloneAttributes(parentScope.attributes);
				defMeth = cloneMethods(parentScope.defMeth);
				types = parentScope.types;
				errors = parentScope.errors;
				enclosingType = parentScope.EnclosingType;
				return;
			}
			defVars = new List<Identifier>();
			defVars.Add(new Identifier("self", null, null, null));
			attributes = new List<Attribute>();
			defMeth = new List<Method>();
			types = new Dictionary<string, Type>();
			errors = new List<Error>();
		}
		private List<Identifier> cloneIdentifiers(List<Identifier> origin)
		{
			List<Identifier> copy = new List<Identifier>();
			foreach (Identifier elem in origin)
				copy.Add(new Identifier(elem.Name,elem.Exp,elem.Type,elem.Coord));
			return copy;
		}
		private List<Attribute> cloneAttributes(List<Attribute> origin)
		{
			List<Attribute> copy = new List<Attribute>();
			foreach (Attribute elem in origin)
				copy.Add(new Attribute(elem.Name,elem.Type,elem.Exp, elem.Coord));
			return copy;
		}
		private List<Method> cloneMethods(List<Method> origin)
		{
			List<Method> copy = new List<Method>();
			foreach (Method elem in origin)
				copy.Add(new Method(elem.Name,elem.OutputType,elem.InputParams,elem.Body,elem.Coord));
			return copy;
		}
		private Dictionary<T,R> cloneDict<T,R>(Dictionary<T,R> origin)
		{
			Dictionary<T, R> copy = new Dictionary<T, R>();
			foreach (var item in origin)
				copy[item.Key] = item.Value;
			return copy;
		}

		public Scope ParentScope { get { return parentScope; } set { parentScope = value; } }
		public List<Identifier> DefVars { get { return defVars; } set { DefVars = value; } }
		public List<Attribute> Attributes { get { return attributes; } set { attributes = value; } }
		public List<Method> DefMethod { get { return defMeth; } set { defMeth = value; } }
		public Dictionary<string, Type> Types { get { return types; } set { types = value; } }
		public List<Error> Errors { get { return errors; } set { errors = value; } }
		public Type EnclosingType { get { return enclosingType; } set { enclosingType = value; } }
	}

	public abstract class Statement
	{
		Coord coord;
		public Statement(Coord c)
		{
			coord = c;
		}
		public abstract bool CheckSemantics(Scope scope);

		public Coord Coord { get { return coord; } protected set { coord = value; } }
	}


	public abstract class Expression : Statement
	{
		// Every expression in Cool has a value and a type:
		Type type;
		object expValue;
		List<Identifier> fields;
		public Expression(Coord c) : base(c)
		{
		}
		public abstract object Evaluate(Scope scope);
		public Type Type { get { return type; } set { type = value; } }
		public object ExpValue { get { return expValue; } set { expValue = value; } }
		public List<Identifier> Attributes { get { return fields; } set { fields = value; } }
	}


	#region Features
	//Where to put initializations, info:
	/*
		When a new object of a class is created, all of the inherited and local attributes must be initialized.
		Inherited attributes are initialized first in inheritance order beginning with the attributes of the greatest
		ancestor class
	*/

	public class Method : Statement
	{
		string name;
		Type outputType;
		List<Tuple<string, Type>> inputParameters;
		Expression body;

		public Method(string name, Type outputType, List<Tuple<string, Type>> inputParameters, Expression exp, Coord c) : base(c)
		{
			this.name = name;
			this.outputType = outputType;
			this.inputParameters = inputParameters;
			body = exp;
		}

		public override bool CheckSemantics(Scope scope)
		{
			// The identifiers used in the formal parameter list must be distinct.
			List<Tuple<string, Type>> l = inputParameters;
			l.Sort();
			for (int i = 0; i < l.Count - 1; i++)
			{
				if (l[i].Item1 == "self" || l[i + 1].Item1 == "self")
				{
					scope.Errors.Add(new Error("Parameters can't be named 'self'", Coord));
					return false;
				}
				if (l[i].Item1 == l[i + 1].Item1)
				{
					scope.Errors.Add(new Error("There are parameters with the same name", Coord));
					return false;
				}
			}

			// Including parameter variables in the scope internal to the method. 
			Scope s = new Scope(scope);
			foreach (Tuple<string, Type> param in inputParameters)
				s.DefVars.Add(new Identifier(param.Item1, null, param.Item2, false, true, Coord));
			foreach (Attribute a in scope.Attributes)
			{
				// Formal parameters override any attribute with the same name, so only add the attributes not overriden by formal parameters
				if (!s.DefVars.Exists(v => v.Name == a.Name))
					s.DefVars.Add(a.Id);
			}

			// This is for non implemented default types methods such as abort, etc...
			if (body == null)
				return true;

			if (!body.CheckSemantics(s))
				return false;

			
			if (body.Type == null)
				return true;
			if (body.Type.Name == "SELF_TYPE")
				body.Type = s.EnclosingType;
			if (outputType.Name == "SELF_TYPE")
				outputType = s.EnclosingType;

			// The type of the method body must conform to the declared return type.
			if (!Type.IsDescendant(body.Type,outputType))
			{
				scope.Errors.Add(new Error("The type of the body:'" + body.Type.Name + "' must conform to the output type:'" + outputType.Name + "' defined by the method: " + this.Name , Coord));
				return false;
			}
			return true;
		}

		public Type OutputType { get { return outputType; } set { outputType = value; } }
		public List<Tuple<string, Type>> InputParams { get { return inputParameters; } set { inputParameters = value; } }
		public Expression Body { get { return body; } set { body = value; } }
		public string Name { get { return name; } private set { name = value; } }
	}

	#endregion

	#region Expressions
	public class Attribute : Expression
	{
		/*
		 	Attributes are local to the class in which they are defined or inherited. Inherited attributes cannot
			be redefined. 
		*/
		Identifier id;
		Tuple<bool, Type> isInherited;

		public Attribute(string name, Type type, Expression exp, Coord c) : base(c)
		{
			id = new Identifier(name, exp, type, false, true, c);
			Type = type;
			this.Exp = exp;
			isInherited = new Tuple<bool, Type>(false, null);
			Attributes = new List<Identifier>();
		}
		public Attribute(string name, Type type, Expression exp, bool isInherited, Type inheritedFrom, Coord c) : base(c)
		{
			id = new Identifier(name, exp, type, false, true, c);
			Type = type;
			this.Exp = exp;
			this.isInherited = new Tuple<bool, Type>(isInherited, inheritedFrom);
			Attributes = new List<Identifier>();

		}

		public override bool CheckSemantics(Scope scope)
		{
			//It is illegal to have attributes named "self"
			if (id.Name == "self")
			{
				scope.Errors.Add(new Error("It is illegal to have attributes named 'self'", Coord));
				return false;
			}

			if (id.Exp != null)
			{
				if (!id.Exp.CheckSemantics(scope)) { return false; }

				if (!Type.IsDescendant(id.Exp.Type, id.Type))
				{
					scope.Errors.Add(new Error("The type of the expression:'" + id.Exp.Type.Name + "' does not correspond with the type of the attribute:  " + Type.Name, Coord));
					return false;
				}

				Type = id.Exp.Type;   // the exp type is a descendant of the defined type of the attribute, so updating it should not harm
				ExpValue = id.Exp.ExpValue;

				foreach (var item in Type.Attributes)
				{
					Attributes.Add(new Identifier(item.Name,item.Exp,item.Type,item.Coord));
				}
				
			}

			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}

		public string Name { get { return id.Name; } private set { id.Name = value; } }
		public Expression Exp { get { return id.Exp; } private set { id.Exp = value; } }
		public Identifier Id { get { return id; } private set { id = value; } }
		public Tuple<bool, Type> IsInherited { get { return isInherited; } private set { isInherited = value; } }
	}
	public class Constant : Expression
	{
		object val;
		public Constant(object val, Type type, Coord c) : base(c)
		{
			this.val = val;
			Type = type;
			ExpValue = this;
		}

		/*
		The simplest expressions are constants. The boolean constants are true and false. Integer constants are
		unsigned strings of digits such as 0, 123, and 007. String constants are sequences of characters enclosed
		in double quotes, such as "This is a string." String constants may be at most 1024 characters long.
		There are other restrictions on strings; see Section 10.
		The constants belong to the basic classes Bool, Int, and String. The value of a constant is an object
		of the appropriate basic class.
		 * */

		public override bool CheckSemantics(Scope scope)
		{
			if (Type.Name == "String")
			{
				if (((string)(((Constant)ExpValue).Val)).Length > 1024)
					scope.Errors.Add(new Error("The string '" + ExpValue + "' has more than 1024 characters, wich is invalid", Coord));
				// See chapter 10 for other restrictions on strings
			}
			ExpValue = this;
			return true; ;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public object Val { get { return val; } set { val = value; } }
	}
	public class Identifier : Expression
	{
		string id;
		Expression exp;
		bool isAttrib;
		bool isParam;
		public Identifier(string name, Expression expression, Type type, Coord c) : base(c)
		{
			id = name;
			exp = expression;
			Type = type;
			isAttrib = false;
			isParam = false;
		}
		public Identifier(string name, Expression exp, Type t, bool isAttrib, bool isParam, Coord c) : base(c)
		{
			id = name;
			this.exp = exp;
			Type = t;
			this.isAttrib = isAttrib;
			this.isParam = isParam;
		}

		public string Name { get { return id; } set { id = value; } }
		public Expression Exp { get { return exp; } set { exp = value; } }
		public override bool CheckSemantics(Scope scope)
		{
			Identifier i = scope.DefVars.Find(e => e.Name == Name);
			if (i == null)
			{
				scope.Errors.Add(new Error("The identifier: " + Name + " isn't defined in this scope", Coord));
				return false;
			}

			exp = i.exp;
			ExpValue = i.ExpValue;
			Type = i.Type;
			isAttrib = i.isAttrib;
			isParam = i.isParam;

			if (exp == null)
				return true;

			if (isAttrib || isParam || Name == "self")
				return true;


			if (!Type.IsDescendant(exp.Type, Type))
			{
				scope.Errors.Add(new Error("The type of the expression body:'" + exp.Type.Name + "' of the identifier: " + id + " does not correspond to the type declared for it:'" + Type.Name + "'", Coord));
				return false;
			}

			return true;
		}
		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}

	}
	public class Assign : Expression
	{
		Identifier left;
		Expression exp;
		public Assign(Identifier id, Expression exp, Coord c) : base(c)
		{
			left = id;
			this.exp = exp;
		}

		public override bool CheckSemantics(Scope scope)
		{
			/*
				The static type of the expression must conform to the declared type of the identifier. The value is the
				value of the expression. The static type of an assignment is the static type of <expr>.
				 it is an error to assign to self
			 */
			Identifier id = scope.DefVars.Find(i => i.Name == left.Name);
			if (id == null)
			{
				scope.Errors.Add(new Error("The identifier: '" + left.Name + "' is not defined in this scope", Coord));
				return false;
			}
			left = id;
			if (left.Name == "self")
			{
				scope.Errors.Add(new Error("Self can't be assigned to", Coord));
				return false;
			}
			if (!left.CheckSemantics(scope))
			{
				//scope.Errors.Add(new Error("The Identifier : " + left.Name + " has errors", Coord));
				return false;
			}
			if (!exp.CheckSemantics(scope))
			{
				//scope.Errors.Add(new Error("The Expression bound to the variable: " + left.Name + " has errors", Coord));
				return false;
			}
			if (exp is Identifier && ((Identifier)exp).Name == "self")
			{
				Type = left.Type;
				ExpValue = left;
				return true;
			}
            if (left.Type.Name == "SELF_TYPE")
                left.Type = scope.EnclosingType;
            if (exp.Type.Name == "SELF_TYPE")
                exp.Type = scope.EnclosingType;

            if (!Type.IsDescendant(exp.Type, left.Type))
			{
				scope.Errors.Add(new Error("The static type of the expression that is assigned: '" + exp.Type.Name + "' must conform to the type declared by the identifier: '" + left.Type.Name + "'", Coord));
			}
			left.Exp = exp;
			left.Type = exp.Type;
			left.ExpValue = exp.ExpValue;
			Type = Exp.Type;
			ExpValue = Exp.ExpValue;
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public Identifier Left { get { return left; } private set { left = value; } }
		public Expression Exp { get { return exp; } private set { exp = value; } }
	}
	public class Dispatch : Expression
	{
		Expression invokerExp;
		Type invokedType;
		string id;
		List<Expression> parameters;

		// For regular method calls like ID(params)
		public Dispatch(string id, List<Expression> parameters, Coord c) : base(c)
		{
			this.id = id;
			this.parameters = parameters;
			invokerExp = new Identifier("self", null, null, c);
		}

		// for atsim(@) type expressions like <invokerExp>@<invokedType>.ID(<exp list i.e params>) e.g: x@B.f()
		public Dispatch(Expression invokerExp, Type invokedType, string id, List<Expression> parameters, Coord c) : base(c)
		{
			this.id = id;
			this.parameters = parameters;
			this.invokerExp = invokerExp;
			this.invokedType = invokedType;
		}

		//To test
		public override bool CheckSemantics(Scope scope)
		{
            /*
			Type checking a dispatch involves several steps.Assume e 0 has static type A. (Recall that this type
		   is not necessarily the same as the type C above.A is the type inferred by the type checker; C is the class
		  of the object computed at runtime, which is potentially any subclass of A.) Class A must have a method
			f
			*/
            if (invokerExp is Identifier && ((Identifier)invokerExp).Name == "self")
            {
                invokerExp = scope.DefVars.Find(e => e.Name == "self");
                //PUT THE ATTRIBUTES OF THE TYPE OF self IN THE SCOPE TO EVAL
            }
            else
            {
                if (!invokerExp.CheckSemantics(new Scope(scope)))
                    return false;
            }
            if (invokedType != null)
			{
                //if (invokerExp.Type.Name == "SELF_TYPE")
                //    invokerExp.Type = scope.EnclosingType;

                Method m0 = invokedType.Methods.Find(e => e.Name == id);
				if (m0 == null)
				{
					scope.Errors.Add(new Error("The type on wich the dispatch was invoked: " + InvokedType.Name + " does not contain a method with that name", Coord));
					return false;
				}
				if (!invokerExp.CheckSemantics(scope)) { return false; }
				if (!Type.IsDescendant(invokerExp.Type, invokedType))
				{
					scope.Errors.Add(new Error(@"the static type to the left of '@' must conform to the type specified to the right of '@'", Coord));
					return false;
				}
				Scope s0 = new Scope(scope);
				for (int i = 0; i < Parameters.Count; i++)
				{
					s0.DefVars.Add(new Identifier(m0.InputParams[i].Item1, (Expression)parameters[i].ExpValue, parameters[i].Type, Coord));
				}
                this.Type = m0.OutputType;
                foreach (var item in invokedType.Attributes)
                {
                    s0.DefVars.Add(new Identifier(item.Name, item.Exp, item.Type, item.Coord));

                }

                //if (m0.Body != null)
                //{
                //    m0.Body.CheckSemantics(s0);
                //    ExpValue = m0.Body.ExpValue;
                //}

                return true;
			}

			if (invokerExp.Type == null)
				invokerExp.Type = scope.EnclosingType;
			if (invokerExp.Type.Name == "SELF_TYPE")
				invokerExp.Type = scope.EnclosingType;

            Method m = invokerExp.Type.Methods.Find(e => e.Name == id);

			if (m == null)
			{
				scope.Errors.Add(new Error("The type of the invoking expression: "+invokerExp.Type.Name+" in the dispatch does not have a method named: " + id, Coord));
				return false;
			}

			// the dispatch and the definition of f must have the same number of arguments
			if (m.InputParams.Count != (parameters.Count))
			{
				scope.Errors.Add(new Error("The number of parameters: "+parameters.Count+" must be the same as the formal definition of the invoked method: " + m.InputParams.Count, Coord));
				return false;
			}

			// the static type of the ith actual parameter must conform to the declared type of the ith formal parameter.
			for (int i = 0; i < Parameters.Count; i++)
			{
				parameters[i].CheckSemantics(scope);
				if (!Type.IsDescendant(parameters[i].Type, m.InputParams[i].Item2))
				{
					if (i == 0)
					{
						scope.Errors.Add(new Error("The type of the 1st input expression:"+ parameters[i].Type.Name+ " does not correspond with the type of the 1st parameter: "+ m.InputParams[i].Item2.Name + " in the formal definition of the method", Coord));
					}
					if (i == 1)
					{
						scope.Errors.Add(new Error("The type of the 2nd input expression:" + parameters[i].Type.Name + " does not correspond with the type of the 2nd parameter: " + m.InputParams[i].Item2.Name + " in the formal definition of the method", Coord));
					}
					if (i == 2)
					{
						scope.Errors.Add(new Error("The type of the 3rd input expression:" + parameters[i].Type.Name + " does not correspond with the type of the 3rd parameter: " + m.InputParams[i].Item2.Name + " in the formal definition of the method", Coord));
					}
					else
					{
						scope.Errors.Add(new Error("The type of the " + (i+1).ToString() + "th input expression:" + parameters[i].Type.Name + " does not correspond with the type of the " + (i + 1).ToString() + "th parameter: " + m.InputParams[i].Item2.Name + " in the formal definition of the method", Coord));
					}
					return false;
				}
			}
			Scope s = new Scope(scope);
			// Bound e0 to self in the scope of the body
			foreach (var item in s.DefVars)
				if (item.Name == "self")
					item.Exp = invokerExp;
			// bind the actual parameters to the scope of the body
			for (int i = 0; i < Parameters.Count; i++)
				s.DefVars.Add(new Identifier(m.InputParams[i].Item1, (Expression)parameters[i].ExpValue, parameters[i].Type, Coord));

            foreach (var item in InvokerExp.Type.Attributes)
                s.DefVars.Add(new Identifier(item.Name,item.Exp,item.Type,item.Coord));

   //         if (m.Body != null)
			//{
			//	m.Body.CheckSemantics(s);
			//	ExpValue = m.Body.ExpValue;
			//}
			//else return true; // only happens with base methods: in_int, in_string,length etc...
			if (m.OutputType.Name == "SELF_TYPE")
				Type = invokerExp.Type;
			else Type = m.OutputType;


			/*
			If f has return type B and B is a class name, then the static type of the dispatch is B. Otherwise, if f
			has return type SELF TYPE, then the static type of the dispatch is A. To see why this is sound, note that
			the self parameter of the method f conforms to type A. Therefore, because f returns SELF TYPE, we can
			infer that the result must also conform to A. Inferring accurate static types for dispatch expressions is
			what justifies including SELF TYPE in the Cool type system.
			*/
			/*
			The other forms of dispatch are:
			<id>(<expr>,...,<expr>)
			<expr>@<type>.id(<expr>,...,<expr>)
			The first form is shorthand for self.<id>(<expr>,...,<expr>).
			The second form provides a way of accessing methods of parent classes that have been hidden by
			redefinitions in child classes. Instead of using the class of the leftmost expression to determine the
			method, the method of the class explicitly specified is used. For example, e@B.f() invokes the method
			f in class B on the object that is the value of e. For this form of dispatch, the static type to the left of
			“@”must conform to the type specified to the right of “@”.
			 */

			// Remeber to check that attributes can't be accesed in a dispatch

			return true;
		}

		public override object Evaluate(Scope scope)
		{
			/*
			 *  Consider the dispatch e 0 .f(e 1 ,...,e n ). To evaluate this expression, the arguments are evaluated in left-
				to-right order, from e 1 to e n . Next, e 0 is evaluated and its class C noted (if e 0 is void a runtime error is
				generated). Finally, the method f in class C is invoked, with the value of e 0 bound to self in the body
				of f and the actual arguments bound to the formals as usual.
			*/
			return ExpValue;
		}

		public Expression InvokerExp { get { return invokerExp; } private set { invokerExp = value; } }
		public Type InvokedType { get { return invokedType; } set { invokedType = value; } }
		public List<Expression> Parameters { get { return parameters; } private set { parameters = value; } }

	}
	public class Conditional : Expression
	{
		Expression ifExp;
		Expression thenExp;
		Expression elseExp;
		/*
		 A conditional has the form
		if <expr> then <expr> else <expr> fi
		The semantics of conditionals is standard. The predicate is evaluated first. If the predicate is true,
		then the then branch is evaluated. If the predicate is false, then the else branch is evaluated. The
		value of the conditional is the value of the evaluated branch.
		The predicate must have static type Bool. The branches may have any static types.

			READ AGAIN THIS SECTION TO GET BACK ON HOW TO CALCULATE THE STATIC TYPE OF A CONDITIONAL STATEMENT

			 */

		public Conditional(Expression ifExp, Expression thenExp, Expression elseExp, Coord c) : base(c)
		{
			this.ifExp = ifExp;
			this.thenExp = thenExp;
			this.elseExp = elseExp;
		}


		public override bool CheckSemantics(Scope scope)
		{
			if (!ifExp.CheckSemantics(scope))
			{
				return false;
			}
			if (ifExp.Type.Name != "Bool")
			{
				scope.Errors.Add(new Error("The condition on the if statement must be of type: Bool", Coord));
				return false;
			}
			if (!thenExp.CheckSemantics(scope))
			{

				return false;
			}
			if (!elseExp.CheckSemantics(scope))
			{
				return false;
			}
            if (thenExp.Type.Name == "SELF_TYPE")
                thenExp.Type = scope.EnclosingType;
            if (elseExp.Type.Name == "SELF_TYPE")
                elseExp.Type = scope.EnclosingType;
            Type = Type.LeastType(thenExp.Type, elseExp.Type);

			//Eval part
			if (ifExp.ExpValue == null)
				return true; // could not eval if expression 'cus there is a value not defined in its expression
			if (((bool)((Constant)ifExp.ExpValue).Val))
				ExpValue = thenExp.ExpValue;
			else ExpValue = elseExp.ExpValue;
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public Expression IfExp { get { return ifExp; } private set { ifExp = value; } }
		public Expression ThenExp { get { return thenExp; } private set { thenExp = value; } }
		public Expression ElseExp { get { return elseExp; } private set { elseExp = value; } }

	}
	public class Loop : Expression
	{
		Expression condition;
		Expression body;
		/*
		 A loop has the form
		while <expr> loop <expr> pool
		The predicate is evaluated before each iteration of the loop. If the predicate is false, the loop terminates
		and void is returned. If the predicate is true, the body of the loop is evaluated and the process repeats.
		The predicate must have static type Bool. The body may have any static type. The static type of a
		loop expression is Object.
			 */
		public Loop(Expression condition, Expression body, Coord c) : base(c)
		{
			this.condition = condition;
			this.body = body;
		}

		//To Test
		public override bool CheckSemantics(Scope scope)
		{
			Type = scope.Types["Object"];
			if (!condition.CheckSemantics(scope))
			{
				return false;
			}
			if (condition.Type.Name != "Bool")
			{
				scope.Errors.Add(new Error("The condition must be of type Bool", Coord));
				return false;
			}
			if (!body.CheckSemantics(scope))
			{
				return false;
			}
			//bool c = (bool)((Constant)(condition.ExpValue)).Val;
			//while (c)
			//{
			//	body.CheckSemantics(scope);
			//	condition.CheckSemantics(scope);
			//	c = (bool)((Constant)(condition.ExpValue)).Val;
			//}
			ExpValue = new Identifier("void", null, scope.Types["Object"], null);
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}

		public Expression Condition { get { return condition; } private set { condition = value; } }
		public Expression Body { get { return body; } private set { body = value; } }
	}
	public class Block : Expression
	{
		List<Expression> exps;



		/// <summary>
		/// A block has the form
		///	{ expr; ... expr; }
		///	The expressions are evaluated in left-to-right order. Every block has at least one expression; the value
		///	of a block is the value of the last expression. The expressions of a block may have any static types. The
		///	static type of a block is the static type of the last expression.
		///	An occasional source of confusion in Cool is the use of semi-colons (“;”). Semi-colons are used
		///	as terminators in lists of expressions (e.g., the block syntax above) and not as expression separators.
		///	Semi-colons also terminate other Cool constructs, see Section 11 for details.
		/// </summary>
		/// <param name="exps"></param>
		/// <param name="c"></param>
		public Block(List<Expression> exps, Coord c) : base(c)
		{
			this.exps = exps;
		}

		//To test
		public override bool CheckSemantics(Scope scope)
		{
			bool coolBlock = true;
			foreach (Expression exp in exps)
				coolBlock = exp.CheckSemantics(scope);
			if (exps[exps.Count - 1].Type == null) { Type = scope.Types["Object"]; }  // To CHANGE TODO TO TEST
			else Type = exps[exps.Count - 1].Type;
			ExpValue = exps[exps.Count - 1].ExpValue;
			return coolBlock;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public List<Expression> Exps { get { return exps; } private set { exps = value; } }
	}
	public class Let : Expression
	{
		/*
		 A let expression has the form
		let <id1> : <type1> [ <- <expr1> ], ..., <idn> : <typen> [ <- <exprn> ] in <expr>
		The optional expressions are initialization; the other expression is the body. A let is evaluated as
		follows. First <expr1> is evaluated and the result bound to <id1>. Then <expr2> is evaluated and the
		result bound to <id2>, and so on, until all of the variables in the let are initialized. (If the initialization
		of <idk> is omitted, the default initialization of type <typek> is used.) Next the body of the let is
		evaluated. The value of the let is the value of the body.
		The let identifiers <id1>,...,<idn> are visible in the body of the let. Furthermore, identifiers
		<id1>,...,<idk> are visible in the initialization of <idm> for any m > k.
		If an identifier is defined multiple times in a let, later bindings hide earlier ones. Identifiers introduced
		by let also hide any definitions for the same names in containing scopes. Every let expression must
		introduce at least one identifier.
		The type of an initialization expression must conform to the declared type of the identifier. The type
		of let is the type of the body.
		The <expr> of a let extends as far (encompasses as many tokens) as the grammar allows.
		It is illegal to bind to self in a let expression
			 */
		List<Identifier> parameters;
		Expression body;
		public Let(List<Identifier> il, Expression body, Coord c) : base(c)
		{
			parameters = il;
			this.body = body;
		}

		public override bool CheckSemantics(Scope scope)
		{
			// Every let expression must introduce at least one identifier.
			if (parameters.Count == 0)
			{
				scope.Errors.Add(new Error("Every let expression must introduce at least one identifier.", Coord));
				return false;
			}
			Scope currentScope = scope;
			for (int i = 0; i < parameters.Count; i++)
			{
				Identifier id = parameters[i];
				// it is an error to bind to self in a let statement
				if (id.Name == "self")
				{
					scope.Errors.Add(new Error("It is illegal to bind to 'self' in a let expression", Coord));
					return false;
				}
				if (id.Exp != null)
				{
					id.Exp.CheckSemantics(currentScope);
					if (!Type.IsDescendant(id.Exp.Type, id.Type))
					{
						scope.Errors.Add(new Error("The type:'" + id.Exp.Type.Name + "' of the expression of the identifier: '" + id.Name + "' does not conform with the type defined by it: '" + id.Type.Name + "'", Coord));
						return false;
					}
					id.ExpValue = id.Exp.ExpValue;
				}

				// Change this to a dictionary later!!!
				// Later bindings hide earlier ones
				var k = currentScope.DefVars.Find(e => e.Name == id.Name);
				if (k != null)
					k = id;
				else currentScope.DefVars.Add(id);
				currentScope = new Scope(currentScope);
			}
			if (!(body.CheckSemantics(currentScope)))
				return false;
			Type = body.Type;
			ExpValue = body.ExpValue;
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public Expression Body { get { return body; } private set { body = value; } }
		public List<Identifier> Parameters { get { return parameters; } private set { parameters = value; } }
	}
	public class Case : Expression
	{
		Expression exp0;
		List<Identifier> ids;
		/*
		 A case expression has the form
		case <expr0> of
		<id1> : <type1> => <expr1>;
		. . .
		<idn> : <typen> => <exprn>;
		esac
		Case expressions provide runtime type tests on objects. First, expr0 is evaluated and its dynamic type
		C noted (if expr0 evaluates to void a run-time error is produced). Next, from among the branches the
		branch with the least type <typek> such that C ≤ <typek> is selected. The identifier <idk> is bound
		to the value of <expr0> and the expression <exprk> is evaluated. The result of the case is the value
		of <exprk>. If no branch can be selected for evaluation, a run-time error is generated. Every case
		expression must have at least one branch.
		For each branch, let T i be the static type of <expri>. The static type of a case expression is
		F
		1≤i≤n T i .
		The identifier id introduced by a branch of a case hides any variable or attribute definition for id visible
		in the containing scope.
		The case expression has no special construct for a “default” or “otherwise” branch. The same affect
		is achieved by including a branch
		x : Object => ...
		because every type is ≤ to Object.
		The case expression provides programmers a way to insert explicit runtime type checks in situa-
		tions where static types inferred by the type checker are too conservative. A typical situation is that
		a programmer writes an expression e and type checking infers that e has static type P. However, the
		programmer may know that, in fact, the dynamic type of e is always C for some C ≤ P. This information
		can be captured using a case expression:
		case e of x : C => ...
		In the branch the variable x is bound to the value of e but has the more specific static type C.
		it is illegal to bind to 'self' in a case
			 */
		public Case(Expression exp0, List<Identifier> ids, Coord c) : base(c)
		{
			this.exp0 = exp0;
			this.ids = ids;
		}

		public override bool CheckSemantics(Scope scope)
		{
			if (!exp0.CheckSemantics(scope))
			{
				return false;
			}
			if (Ids.Count == 0)
			{
				scope.Errors.Add(new Error("Every case statement must have at least one branch", Coord));
				return false;
			}
			if (exp0 is Identifier && ((Identifier)exp0).Name == "void")
			{
				throw new Exception("The base expression in a case statement can't evaluate to void");
			}
			foreach (Identifier id in Ids)
			{
				if (!id.CheckSemantics(scope)) return false;
			}

			Identifier leastTypeId = Type.LeastType(exp0.Type, Ids);

			// it is an error to bind to self in a case statement
			if (leastTypeId.Name == "self")
			{
				scope.Errors.Add(new Error("It is illegal to bind 'self' in a case statement", Coord));
				return false;
			}

			exp0.ExpValue = leastTypeId;
			if (!leastTypeId.Exp.CheckSemantics(scope))
				return false;
			ExpValue = leastTypeId.ExpValue;
			Type = Type.Leasts(ids);

			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}

		public Expression Exp0 { get { return exp0; } private set { exp0 = value; } }
		public List<Identifier> Ids { get { return ids; } private set { ids = value; } }
	}
	public class New : Expression
	{
		/*
		 A new expression has the form
			new <type>
			The value is a fresh object of the appropriate class. If the type is SELF TYPE, then the value is a fresh
			object of the class of self in the current scope. The static type is <type>.
		*/
		public New(Type t, Coord c) : base(c)
		{
			Type = t;
		}

		public override bool CheckSemantics(Scope scope)
		{
			if(Type.Name == "SELF_TYPE")
			{
				Type = scope.EnclosingType;
			}
			if (!scope.Types.ContainsKey(Type.Name))
			{
				scope.Errors.Add(new Error("The type in expression: new " + Type.Name + " isn't defined in this scope",Coord));
				return false;
			}
			Type = scope.Types[Type.Name];
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			throw new NotImplementedException();
		}
	}
	public class Isvoid : Expression
	{
		/*
		 The expression
		 isvoid expr
		 evaluates to true if expr is void and evaluates to false if expr is not void.
			 */
		Expression exp;
		public Isvoid(Expression exp,Coord c) : base(c)
		{
            this.exp = exp;
        }

		public override bool CheckSemantics(Scope scope)
		{
			bool result = exp.CheckSemantics(scope);
			ExpValue = exp.ExpValue;
            Type = scope.Types["Bool"];
			return result;
		}

		public override object Evaluate(Scope scope)
		{
			throw new NotImplementedException();
		}
		public Expression Exp { get { return exp; } private set { exp = value; } }

	}


	public class Parenth : Expression
	{
		Expression exp;
		public Parenth(Expression exp, Coord c) : base(c)
		{
			this.exp = exp;
		}

		public override bool CheckSemantics(Scope scope)
		{
			bool result = exp.CheckSemantics(scope);
			Type = exp.Type;
			ExpValue = exp.ExpValue;
			return result;
		}

		public override object Evaluate(Scope scope)
		{
			return exp.ExpValue;
		}
		public Expression Exp { get { return exp; } private set { exp = value; } }
	}
	public class Tilde : Expression
	{
		Expression exp;
		public Tilde(Expression exp, Coord c) : base(c)
		{
			this.exp = exp;
		}
		//to test
		public override bool CheckSemantics(Scope scope)
		{
			if (!exp.CheckSemantics(scope)) return false;
			if (exp.Type.Name != "Int")
			{
				scope.Errors.Add(new Error("Integer complement must be applied on objects of type Int", Coord));
				return false;
			}
            Type = exp.Type;

            return true;
		}
		//todo
		public override object Evaluate(Scope scope)
		{
			return null;
		}
		public Expression Exp { get { return exp; } private set { exp = value; } }
	}
	public class BinArithExp : Expression
	{
		Op op;
		Expression left;
		Expression right;

		public BinArithExp(Expression l, Expression r, Op op, Coord c) : base(c)
		{
			left = l;
			right = r;
			this.op = op;
            this.Type = left.Type;
		}

		public override bool CheckSemantics(Scope scope)
		{
			if (!left.CheckSemantics(scope) || !right.CheckSemantics(scope))
				return false;
			if (left.Type.Name != "Int" || right.Type.Name != "Int")
			{
				scope.Errors.Add(new Error("Both operands in the arithmetic expression must be of type Int", Coord));
				return false;
			}
			Type = scope.Types["Int"];
			int result = 0;

			//return true 'cus they are variables called maybe from a method that will have value only if the method is called etc...
			if (left.ExpValue == null || right.ExpValue == null)
				return true;
			switch (op)
			{
				case Op.plus:
					result = (int)((Constant)(left.ExpValue)).Val + (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Int"], Coord);
					break;
				case Op.minus:
					result = (int)((Constant)(left.ExpValue)).Val - (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Int"], Coord);
					break;
				case Op.times:
					result = (int)((Constant)(left.ExpValue)).Val * (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Int"], Coord);
					break;
				case Op.div:
					if ((int)((Constant)(right.ExpValue)).Val == 0)
					{
						scope.Errors.Add(new Error("Divide by zero error", Coord));
						return false;
					}
					result = (int)((Constant)(left.ExpValue)).Val / (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Int"], Coord);
					break;
				default:
					scope.Errors.Add(new Error("Invalid operator in arithmetic expression", Coord));
					return false;
#pragma warning disable CS0162 // Unreachable code detected
					break;
#pragma warning restore CS0162 // Unreachable code detected
			}
			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}

		//May change implementation to allow for redefinable operators or flexibility for the language to change in time and add new things

		public Expression Left { get { return left; } private set { left = value; } }
		public Expression Right { get { return right; } private set { right = value; } }

	}

	public class BinCompExp : Expression
	{
		BoolOp op;
		Expression left;
		Expression right;
		public BinCompExp(Expression left, Expression right, BoolOp op, Coord c) : base(c)
		{
			this.left = left;
			this.right = right;
			this.op = op;
            this.Type = left.Type;

        }

        public override bool CheckSemantics(Scope scope)
		{
			if (!left.CheckSemantics(scope) || !right.CheckSemantics(scope))
				return false;
			Type = scope.Types["Bool"];

			bool result;

			// this is for the same reason with binArithExp
			if (left.ExpValue == null || right.ExpValue == null)
				return true;
			switch (op)
			{
				case BoolOp.lesser:
					result = (int)((Constant)(left.ExpValue)).Val < (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Bool"], Coord);
					break;
				case BoolOp.lesserEq:
					result = (int)((Constant)(left.ExpValue)).Val <= (int)((Constant)(right.ExpValue)).Val;
					ExpValue = new Constant(result, scope.Types["Bool"], Coord);
					break;
				case BoolOp.equal:
                    if (left.Type.Name == "Int")
                    {
                        result = (int)((Constant)(left.ExpValue)).Val == (int)((Constant)(right.ExpValue)).Val;
                        ExpValue = new Constant(result, scope.Types["Bool"], Coord);

                    }
                    if (left.Type.Name == "Bool")
                    {
                        result = (bool)((Constant)(left.ExpValue)).Val == (bool)((Constant)(right.ExpValue)).Val;
                        ExpValue = new Constant(result, scope.Types["Bool"], Coord);
                    }

                    if (left.Type.Name == "String")
                    {
                        result = (string)((Constant)(left.ExpValue)).Val == (string)((Constant)(right.ExpValue)).Val;
                        ExpValue = new Constant(result, scope.Types["Bool"], Coord);
                    }

                    break;
			}

			return true;
		}

		public override object Evaluate(Scope scope)
		{
			return ExpValue;
		}
		public Expression Left { get { return left; } private set { left = value; } }
		public Expression Right { get { return right; } private set { right = value; } }
	}
	public class Not : Expression
	{
		Expression exp;
		public Not(Expression exp, Coord c) : base(c)
		{
			this.exp = exp;
            Type = exp.Type;
		}

		public override bool CheckSemantics(Scope scope)
		{
            if (!(exp.CheckSemantics(scope))) {
                return false;
            }

            if (exp.Type.Name != "Bool") {
                return false;
            }
            Type = scope.Types["Bool"];
            return true;
		}

		public override object Evaluate(Scope scope)
		{
			throw new NotImplementedException();
		}
		public Expression Exp { get { return exp; } private set { exp = value; } }

	}

	
	public enum Op
	{
		plus,  // +
		minus, // -
		times, // *
		div,    // /
		unknown
	}
	public enum BoolOp
	{
		lesser,  // <
		lesserEq,// <=
		equal    // =
	}

	#endregion


	public class Coord
	{
		int row, col;
		public Coord(int row, int col)
		{
			this.row = row;
			this.col = col;
		}
		public int Row { get { return row; } private set { row = value; } }
		public int Col { get { return col; } private set { col = value; } }

	}


	public class Error
	{
		string msg;
		Coord c;
		public Error(string msg, Coord c)
		{
			this.msg = msg;
			this.c = c;
		}
		public string Message { get { return msg; } private set { msg = value; } }
		public Coord ErrCoord { get { return c; } private set { c = value; } }

        public override string ToString()
        {
            return "[" + ErrCoord.Row + ":" + ErrCoord.Col + "] -> " + Message;
        }
    }

}
