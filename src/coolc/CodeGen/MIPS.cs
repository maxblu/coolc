using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using coolc.Visitors;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc.CodeGen
{
    /*
    Notes on code-generation

    First pass for "global declarations"
    Second pass for functions -> does this means classes?

    if no optimization only one reg is needed maybe $t0 ?
     */
    public static class Helpers
    {
        public static string Split(this string[] s, string c)
        {
            var res = "";
            foreach (var item in s)
            {
                res += item + c;
            }
            return res;
        }
        public static string ToReg(this int s)
        {
            return "$t" + s.ToString();
        }

    }
    /// <summary>
    /// This is just an static class for enclose some useful MIPS related stuff 
    /// </summary>
    public static class MIPS
    {
        public static int regCount = 0;
        public static int stringCount = 0;
        public static bool[] tempReg = new bool[10];
        public static bool[] savedReg = new bool[8];
        internal static int intCount = 0;
        public static int ifCount = 0;
        public static int whileCount = 0;

        public static string GetReg() { return (regCount++ % 9).ToReg(); }
        public static string LastReg()
        {
            return ((MIPS.regCount - 1) % 9).ToReg();
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Opcodes
        {
            /// <summary>
            /// Add two numbers
            /// </summary>
            add,
            /// <summary>
            /// Substract two numbers
            /// </summary>
            sub,
            /// <summary>
            /// Multiplies two numbers
            /// </summary>
            mul,
            /// <summary>
            /// Divides two numbers
            /// </summary>
            div,
            /// <summary>
            /// Call a method or subroutine ('jal' is the actual keyword)  
            /// </summary>
            call,
            /// <summary>
            /// Load Word, used to read from memory
            /// </summary>
            lw,
            /// <summary>
            /// Store Word, used to save data to Memory
            /// </summary>
            sw,
            /// <summary>
            /// Jumps
            /// </summary>
            j,
            /// <summary>
            /// Do a syscall, see the Syscall enum for more info
            /// </summary>
            syscall,
            /// <summary>
            /// 
            /// </summary>
            neg,
            /// <summary>
            /// 
            /// </summary>
            not,
            /// <summary>
            /// gets the absolute value of a number
            /// </summary>
            abs,
            /// <summary>
            /// Logic Or
            /// </summary>
            or,
            /// <summary>
            /// Logic Xor
            /// </summary>
            xor,
            /// <summary>
            /// 
            /// </summary>
            seq,
            /// <summary>
            /// Load address
            /// </summary>
            la,
            /// <summary>
            /// jump and link
            /// </summary>
            jal,
            /// <summary>
            /// push to the stack
            /// the real keyword is sw + addiu sp sp 4
            /// </summary>
            push,
            /// <summary>
            /// return from method
            /// return address is in $ra
            /// </summary>
            jr,
            /// <summary>
            /// load inmediate
            /// load an integer in a register
            /// </summary>
            li,
            /// <summary>
            /// usw src1, addr
            /// Store the word in src to the (possibly unaligned) address addr.
            /// </summary>
            usw,
            ///<summary>
            /// Signed lesser or equal
            /// </summary>
            sle,
            /// <summary>
            /// Signed Lesser than
            /// </summary>
            slt,
            /// <summary>
            /// Branch if equal
            /// </summary>
            beq,
            /// <summary>
            /// Branch if not equal
            /// </summary>
            bne,
            /// <summary>
            /// Branch unconditionally
            /// </summary>
            b,
            /// <summary>
            /// Moves from src1 to dest
            /// </summary>
            move,
            subu
        }

        public enum Op
        {
            align,
            ascii,
            asciiz,
            //byte,
            globl,
            word,
            label,
            add,
            addi,
            addiu,
            div,
            divu,
            mul,
            sub,
            and,
            neg,
            nor,
            not,
            or,
            xor,
            li,
            lui,
            seq,
            sge,
            sgt,
            sle,
            sne,
            b,
            beq,
            bge,
            bne,
            j,
            jal,
            jalr,
            jr,
            la,
            lb,
            ld,
            lw,
            sb,
            sd,
            sw,
            move,
            push,
            pop,
            syscall,
            nop,
            initial_data,
            obj_attribs
        }

        public static string Emit(Op opcode, params string[] v)
        {
            string s = "";
            switch (opcode)
            {
                case Op.align:
                    s += "\talign " + v.Split(" ") + "\n";
                    break;
                case Op.ascii:
                    s += "\tascii " + v.Split(" ") + "\n";
                    break;
                case Op.asciiz:
                    s += "\tasciiz " + v.Split(" ") + "\n";
                    break;
                //case Op.byte:
                //        s += "\tbyte " + v.Split(" ") + "\n";
                //    break;
                case Op.globl:
                    s += "\tglobl " + v.Split(" ") + "\n";
                    break;
                case Op.word:
                    s += "\tword " + v.Split(" ") + "\n";
                    break;
                case Op.label:
                    s += "\tlabel " + v.Split(" ") + "\n";
                    break;
                case Op.add:
                    s += "\tadd " + v.Split(" ") + "\n";
                    break;
                case Op.addi:
                    s += "\taddi " + v.Split(" ") + "\n";
                    break;
                case Op.addiu:
                    s += "\taddiu " + v.Split(" ") + "\n";
                    break;
                case Op.div:
                    s += "\tdiv " + v.Split(" ") + "\n";
                    break;
                case Op.divu:
                    s += "\tdivu " + v.Split(" ") + "\n";
                    break;
                case Op.mul:
                    s += "\tmul " + v.Split(" ") + "\n";
                    break;
                case Op.sub:
                    s += "\tsub " + v.Split(" ") + "\n";
                    break;
                case Op.and:
                    s += "\tand " + v.Split(" ") + "\n";
                    break;
                case Op.neg:
                    s += "\tneg " + v.Split(" ") + "\n";
                    break;
                case Op.nor:
                    s += "\tnor " + v.Split(" ") + "\n";
                    break;
                case Op.not:
                    s += "\tnot " + v.Split(" ") + "\n";
                    break;
                case Op.or:
                    s += "\tor " + v.Split(" ") + "\n";
                    break;
                case Op.xor:
                    s += "\txor " + v.Split(" ") + "\n";
                    break;
                case Op.li:
                    s += "\tli " + v.Split(" ") + "\n";
                    break;
                case Op.lui:
                    s += "\tlui " + v.Split(" ") + "\n";
                    break;
                case Op.seq:
                    s += "\tseq " + v.Split(" ") + "\n";
                    break;
                case Op.sge:
                    s += "\tsge " + v.Split(" ") + "\n";
                    break;
                case Op.sgt:
                    s += "\tsgt " + v.Split(" ") + "\n";
                    break;
                case Op.sle:
                    s += "\tsle " + v.Split(" ") + "\n";
                    break;
                case Op.sne:
                    s += "\tsne " + v.Split(" ") + "\n";
                    break;
                case Op.b:
                    s += "\tb " + v.Split(" ") + "\n";
                    break;
                case Op.beq:
                    s += "\tbeq " + v.Split(" ") + "\n";
                    break;
                case Op.bge:
                    s += "\tbge " + v.Split(" ") + "\n";
                    break;
                case Op.bne:
                    s += "\tbne " + v.Split(" ") + "\n";
                    break;
                case Op.j:
                    s += "\tj " + v.Split(" ") + "\n";
                    break;
                case Op.jal:
                    s += "\tjal " + v.Split(" ") + "\n";
                    break;
                case Op.jalr:
                    s += "\tjalr " + v.Split(" ") + "\n";
                    break;
                case Op.jr:
                    s += "\tjr " + v.Split(" ") + "\n";
                    break;
                case Op.la:
                    s += "\tla " + v.Split(" ") + "\n";
                    break;
                case Op.lb:
                    s += "\tlb " + v.Split(" ") + "\n";
                    break;
                case Op.ld:
                    s += "\tld " + v.Split(" ") + "\n";
                    break;
                case Op.lw:
                    s += "\tlw " + v.Split(" ") + "\n";
                    break;
                case Op.sb:
                    s += "\tsb " + v.Split(" ") + "\n";
                    break;
                case Op.sd:
                    s += "\tsd " + v.Split(" ") + "\n";
                    break;
                case Op.sw:
                    s += "\tsw " + v.Split(" ") + "\n";
                    break;
                case Op.move:
                    s += "\tmove " + v.Split(" ") + "\n";
                    break;
                case Op.push:
                    s += "\tpush " + v.Split(" ") + "\n";
                    break;
                case Op.pop:
                    s += "\tpop " + v.Split(" ") + "\n";
                    break;
                case Op.syscall:
                    s += "\tsyscall " + v.Split(" ") + "\n";
                    break;
                case Op.nop:
                    s += "\tnop " + v.Split(" ") + "\n";
                    break;
                case Op.initial_data:
                    s += "\tinitial_data " + v.Split(" ") + "\n";
                    break;
                case Op.obj_attribs:
                    s += "\tobj_attribs " + v.Split(" ") + "\n";
                    break;
                default:
                    break;
            }
            return s;
        }
        
        /// <summary>
        /// Returns a string representing the line or lines 
        /// that execute the op command
        /// </summary>
        /// <param name="opcode">The op that you want to execute</param>
        /// <param name="v">the params that op need</param>
        public static string Emit(Opcodes opcode, params string[] v)
        {
            switch (opcode)
            {
                case MIPS.Opcodes.add:
                    return ("\t" + "add " + v.Split(" ") + "\n");
                case MIPS.Opcodes.sub:
                    return ("\t" + "sub " + v.Split(" ") + "\n");
                case MIPS.Opcodes.mul:
                    return ("\t" + "mul " + v.Split(" ") + "\n");
                case MIPS.Opcodes.div:
                    return ("\t" + "div " + v.Split(" ") + "\n");
                case MIPS.Opcodes.call:
                    return ("\t" + "call " + v.Split(" ") + "\n");
                case MIPS.Opcodes.lw:
                    return ("\t" + "lw " + v.Split(" ") + "\n");
                case MIPS.Opcodes.sw:
                    return ("\t" + "sw " + v.Split(" ") + "\n");
                case MIPS.Opcodes.j:
                    return ("\t" + "j " + v.Split(" ") + "\n");
                case MIPS.Opcodes.syscall:
                    return ("\t" + "syscall " + v.Split(" ") + "\n");
                case MIPS.Opcodes.neg:
                    return ("\t" + "neg " + v.Split(" ") + "\n");
                case MIPS.Opcodes.not:
                    return ("\t" + "not " + v.Split(" ") + "\n");
                case MIPS.Opcodes.abs:
                    return ("\t" + "abs " + v.Split(" ") + "\n");
                case MIPS.Opcodes.or:
                    return ("\t" + "or " + v.Split(" ") + "\n");
                case MIPS.Opcodes.xor:
                    return ("\t" + "xor " + v.Split(" ") + "\n");
                case MIPS.Opcodes.seq:
                    return ("\t" + "seq " + v.Split(" ") + "\n");
                case MIPS.Opcodes.la:
                    return ("\t" + "la " + v.Split(" ") + "\n");
                case MIPS.Opcodes.li:
                    return ("\t" + "li " + v.Split(" ") + "\n");
                case MIPS.Opcodes.push:
                    var s = "";
                    //la idea es mover a la pila y luego actualizar la pila
                    s += ("\t" + "sw " + v.Split(" ") + "0($sp)" + "\n");
                    //s += "\t" + "addu $sp $sp 4\n";// esto puede dar palo verificar no sea addiu or addi
                    return s;
                case MIPS.Opcodes.jal:
                    return ("\t" + "jal " + v.Split(" ") + "\n");
                case MIPS.Opcodes.jr:
                    return "\t" + "jr $ra\t\t\t\t#end of method\n";
                case MIPS.Opcodes.usw:
                    return ("\t" + "usw " + v.Split(" ") + "\t\t\t#Saving\n");
                case MIPS.Opcodes.move:
                    return ("\t" + "move " + v.Split(" ") + "\n");
                case MIPS.Opcodes.sle:
                    return ("\t" + "sle " + v.Split(" ") + "\n");
                case MIPS.Opcodes.slt:
                    return ("\t" + "slt " + v.Split(" ") + "\n");
                case MIPS.Opcodes.beq:
                    return "\t" + "beq " + v.Split(" ") + "\n";
                case MIPS.Opcodes.bne:
                    return "\t" + "bne " + v.Split(" ") + "\n";
                case MIPS.Opcodes.b:
                    return "\t" + "b " + v.Split(" ") + "\n";
                case MIPS.Opcodes.subu:
                    return "\t" + "subu " + v.Split(" ") + "\n";
                default:
                    return "#Bad Opcode: " + opcode.ToString() + "\n";
            }
        }

        internal static string BasicFunctions()
        {
            //string s = "";
            var s = "### ### ### BASIC FUNCTIONS ### ### ###\n";

            s += "out_string:\n";
            s += "\tli $v0, 4\t\t\t# syscall 4 (print_str)\n" +
                "\tlw $a0, 0($sp)\t\t# argument: string\n" +
                "\tsyscall\t\t\t\t# print the string\n" +
                "\tjr $ra\t\t\t\t# jump back\n";

            s += "out_int:\n" +
                "\tli $v0, 1\t\t\t# syscall 1 (print_int)\n" +
                "\tlw $a0, 0($sp)\t\t# argument: string\n" +
                "\tsyscall\t\t\t\t# print the string\n" +
                "\tjr $ra\t\t\t\t# jump back\n";

            s += "length:\n" +
                "\tlw $t0, 0($sp)\n" +
                "\taddiu $t1,$zero,0\n" +
                "\tlengthLoop:\n" +
                    "\t\tlb $t2, ($t0)\n" +
                    "\t\tbeqz $t2, end_length_loop\n" +
                    "\t\taddiu $t0, $t0, 1\n" +
                    "\t\taddiu $t1, $t1, 1\n" +
                    "\t\tj lengthLoop\n" +
                "\tend_length_loop:\n" +
                "\tadd $v0, $zero,$zero\n" +
                "\tadd $v0, $v0, $t1\n" +
                "\tjr $ra\n";

            s += "in_int:\n" +
                    "\tli $v0,5\n" +
                    "\tsyscall\n" +
                    "\tjr $ra\n";

            s += "abort:\n" +
                    "\tli $v0, 10\n" +
                    "\tsyscall\n";

            s += "type_name:\n" +
                    "li $v0, 10\n" +
                    "syscall\n";
            s += "copy:\n" +
                    "li $v0, 10\n" +
                    "syscall\n";
            //s += "length:\n" +
            //        "li $v0, 10\n" +
            //        "syscall\n";
            s += "concat:\n" +
                    "li $v0, 10\n" +
                    "syscall\n";
            s += "substr:\n" +
                    "li $v0, 10\n" +
                    "syscall\n";



            // TODO TO PUT IN .data strBuffer .space 2048
            //s += "in_string:\n" +
            //		"\tla $a0, strBuffer\n" +
            //		"\tli $a1, 2048\n" +
            //		"\tli $v0, 8\n" +
            //		"\tsyscall\n" +
            //		"\tjr $ra\n";

            return s;
        }

        internal static string HelpFunctions()
        {
            var s = "### ### ### HELP FUNCTIONS ### ### ###\n";
            s += "makeroom:\n" +
                "\tlw $a0, 0($sp)\t# argument: int\n" +
                "\tli $v0, 9\t\t# 9 is the sbrk syscall.\n" +
                "\tsyscall\t\t\t# do the syscall.\n" +
                "\tjr $ra";
            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Syscalls
        {
            /// <summary>
            /// this does nothing, its here just to make the other fall in place (code=value)
            /// </summary>
            none,
            /// <summary>
            /// Code: 1, Arg: $a0, res: none
            /// </summary>
            print_int,
            /// <summary>
            /// Code: 2, Arg: $f12, res: none
            /// </summary>
            print_float,
            /// <summary>
            /// Code: 3, Arg: $f12, res: none
            /// </summary>
            print_double,
            /// <summary>
            /// Code: 4, Arg: $a0, res: none
            /// </summary>
            print_string,
            /// <summary>
            /// Code: 5, Arg: none, res: $v0
            /// </summary>
            read_int,
            /// <summary>
            /// Code: 6, Arg: none, res: $f0
            /// </summary>
            read_float,
            /// <summary>
            /// Code: 7, Arg: none, res: $f0
            /// </summary>
            read_double,
            /// <summary>
            /// Code: 8, Arg: $a0 (address) $a1 (length), res: none
            /// </summary>
            read_string,
            /// <summary>
            /// Code: 9, Arg: $a0 (length), res: $v0 address
            /// </summary>
            sbrk,
            /// <summary>
            /// Code: 10, Arg: none, res: none
            /// </summary>
            exit,
            /// <summary>
            /// Do not use this
            /// </summary>
            print_char,
            /// <summary>
            /// Do not use this
            /// </summary>
            read_char,
            /// <summary>
            /// Do not use this
            /// </summary>
            open,
            /// <summary>
            /// Do not use this
            /// </summary>
            read,
            /// <summary>
            /// Do not use this
            /// </summary>
            write,
            /// <summary>
            /// Do not use this
            /// </summary>
            close,
            /// <summary>
            /// Do not use this
            /// </summary>
            exit2
        }
    }
}
