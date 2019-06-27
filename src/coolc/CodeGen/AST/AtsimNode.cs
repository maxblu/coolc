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
    internal class AtsimNode : Node
    {
        private CoolParser.AtsimExpContext context;
        
        public AtsimNode(CoolParser.AtsimExpContext context, Node s) : base(s.Childs)
        {
            this.context = context;
            Expresion = context.expresion(0);
            Params = context.expresion();
            MethodID= context.ID();
            try {
                CallerType = SymbolTable.Classes[context.TYPE().GetText()];
                Method_Node = SymbolTable.Classes[CallerType.Name].Methods[context.ID().GetText()];
            }catch (Exception){/* :( */}
        }

        public CoolParser.ExpresionContext Expresion { get; private set; }
        public CoolParser.ExpresionContext[] Params { get; private set; }
        public ITerminalNode MethodID { get; private set; }
        public ClassNode CallerType { get; private set; }
        public MethodNode Method_Node { get; private set; }

        public Dictionary<string, ClassNode> Symbols { get; private set; }

        public override string GenerateCode()
        {
            try
            {
                CallerType = SymbolTable.Symbols.Peek()[((IdentifierNode)Childs[0]).Name];
            }
            catch (Exception)
            {
                Type = Childs[0].Type;
                CallerType = SymbolTable.Classes[Type];
            } 
            var s = "# Atsim cal\t\t\t"+ CallerType.Name+"."+MethodID.GetText()+"\n";
            int offset = 0;



            foreach (var item in Childs)
            {
                s += item.GenerateCode();
                var reg1 = MIPS.LastReg();//
                s += MIPS.Emit(MIPS.Opcodes.sw, reg1, offset + "($sp)");//push all your sons
                offset += 4;
                //s += MIPS.Emit(MIPS.Opcodes.push, reg1);//push all your sons
            }
            /*Parche*/

            try{
                var t = MIPS.LastReg();
                s += MIPS.Emit(MIPS.Opcodes.lw, t, ((IdentifierNode)Childs[0]).Name);//load the caller
                offset = 8;
                var n = MIPS.GetReg();
                foreach (var item in CallerType.Attributes)// iterate the attributes
                {
                    s += MIPS.Emit(MIPS.Opcodes.lw, n, offset + "(" + t + ")");
                    s += MIPS.Emit(MIPS.Opcodes.usw, n, item.Key);
                    offset += 4;
                }
            }catch (Exception){}
            /* end of Parche*/

            //s += MIPS.Emit(MIPS.Opcodes.sw, MIPS.LastReg().ToReg(), );

            var m = CallerType.GetMethod(MethodID.GetText());
            var callerTypeName = m.ClassName;
            if (callerTypeName!= CallerType.Name)
            {
                //cast
                s += MIPS.Emit(MIPS.Opcodes.lw, MIPS.LastReg(), "0($sp)");
                s += MIPS.Emit(MIPS.Opcodes.move, "$s2", MIPS.LastReg());
                var j = MIPS.LastReg();

                var y = MIPS.GetReg();

                s += MIPS.Emit(MIPS.Opcodes.lw, y, SymbolTable.Classes[CallerType.Name].Size - 4 + "("+j+")");

                s += MIPS.Emit(MIPS.Opcodes.sw, y, "0($sp)");



                //s += MIPS.Emit(MIPS.Opcodes.sw, MIPS.LastReg(), "0($sp)");

            }
            //s += MIPS.Emit(MIPS.Opcodes.move, "$s0", "$ra");
            s += MIPS.Emit(MIPS.Opcodes.jal, callerTypeName+"."+MethodID);// this blows $ra
            //s += MIPS.Emit(MIPS.Opcodes.move, "$ra", "$s0");

            var reg2 = MIPS.GetReg();
            s += MIPS.Emit(MIPS.Opcodes.move, reg2, "$v0\t\t#Method return value");

            if (callerTypeName != CallerType.Name)
                s += MIPS.Emit(MIPS.Opcodes.sw, "$s2", "0($sp)");
            //s += MIPS.Emit(MIPS.Opcodes.subu, "$sp","$sp", "4");

            //SymbolTable.Symbols.Pop();

            return s;// base.GenerateCode();
        }
    }

}