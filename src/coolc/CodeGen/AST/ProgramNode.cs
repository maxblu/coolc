#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
using System;
using coolc.Visitors;
using System.Collections.Generic;


namespace coolc.CodeGen
{
    public class ProgramNode : Node
    {
        private CoolParser.ProgramContext context;
        //private Node Childs;
        
        public ProgramNode(CoolParser.ProgramContext context, Node s) : base(s.Childs)
        {
            
            this.context = context;
            //this.Childs = s;
        }
        public override string GenerateCode()
        {
            var s = "# coolc-output" + ":\n";//todo add the calss name :(

            s += ".data\n";
            s += "self:\t.word\t0\t#self\n";
            s += GiveMeTheData();

            s += ".text\n";
            s += base.GenerateCode();

            s += MIPS.BasicFunctions();
            s += MIPS.HelpFunctions();

            return s;
        }

        //private new string GiveMeTheData()
        //{
        //    var s = "";

        //    foreach (var i in Childs)
        //    {
        //        s += GiveMeTheData(i);// add the strings :) that need to be stored at the heap
        //    }
        //    return s;
        //}
        //private string GiveMeTheData(Node item)
        //{
        //    /*
        //     * TODO
        //     * This is not the best way
        //     * stock in the stack... use the $fp
        //     */
        //    var s = "";
        //    foreach (var i in item.Childs)
        //    {
        //        s += GiveMeTheData(i);
        //    }
        //    try{s += "\tstr" + ((StringNode)item).I + ":\t.asciiz\t" + ((StringNode)item).V + "\n";}catch (Exception){}

        //    return s;
        //}
    }
}