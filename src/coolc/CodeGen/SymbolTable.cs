using coolc.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    /// <summary>
    /// In here we will provide a a hierarchy for the clases
    /// </summary>
    public static class SymbolTable
    {
        public static Dictionary<string, ClassNode> Classes { get; set; }
        public static Dictionary<string, int> StringVariables { get; set; }
        //public static Dictionary<string, ClassNode> Symbols { get; set; }
        public static Stack<Dictionary<string, ClassNode>> Symbols { get; set; }
        
        /// <summary>
        /// This method adds the initial clases and methods for each of them
        /// also initialize the Dictionary Clases
        /// </summary>
        public static void Initialize()
        {
            Classes = new Dictionary<string, ClassNode>();
            StringVariables = new Dictionary<string, int>();
            Symbols = new Stack<Dictionary<string, ClassNode>>();
            
            #region Adding Default
            
            #region Declarations
            ClassNode objc;
            ClassNode self;
            ClassNode intc;
            ClassNode boolc;
            ClassNode stringc;
            ClassNode ioc; 
            #endregion

            #region self, int, bool 
            //this classes doesnt have methods

            self = new ClassNode("SELF_TYPE", null);
            Classes.Add(self.Name, self);

            intc = new ClassNode("Int", null);
            Classes.Add(intc.Name, intc);

            boolc = new ClassNode("Bool", null);
            Classes.Add(boolc.Name, boolc);

            #endregion

            #region string
            stringc = new ClassNode("String", self);

            var length = new MethodNode("length", intc);
            stringc.Methods.Add("length", length);

            var concat = new MethodNode("concat", stringc);        //OMFG (O.o)! this? are you f* kidding me?!
            concat.Params.Add("S", new AttributeNode("S", stringc));
            stringc.Methods.Add("concat", concat);

            var substr = new MethodNode("substr", stringc);
            substr.Params.Add("I", new AttributeNode("I", intc));
            substr.Params.Add("L", new AttributeNode("L", intc));
            stringc.Methods.Add("substr", substr);
            Classes.Add(stringc.Name, stringc);
            #endregion

            #region IO
            ioc = new ClassNode("IO", null);
            var out_string = new MethodNode("out_string", self);
            out_string.Params.Add("X", new AttributeNode("X", stringc));
            ioc.Methods.Add("out_string", out_string);

            var out_int = new MethodNode("out_int", self);
            out_int.Params.Add("X", new AttributeNode("X", intc));
            ioc.Methods.Add("out_int", out_int);

            var in_string = new MethodNode("in_string", stringc);
            ioc.Methods.Add("in_string", in_string);

            var in_int = new MethodNode("in_int", intc);
            ioc.Methods.Add("in_int", in_int);
            Classes.Add(ioc.Name, ioc);
            #endregion

            #region Object
            objc = new ClassNode("Object", null);

            var abort = new MethodNode("abort", objc);
            objc.Methods.Add("abort", abort);

            var type_name = new MethodNode("type_name", stringc);
            objc.Methods.Add("type_name", type_name);

            var copy = new MethodNode("copy", self);
            objc.Methods.Add("copy", copy);
            Classes.Add(objc.Name, objc);
            #endregion

            #endregion
        }

    }
}
