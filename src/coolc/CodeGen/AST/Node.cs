#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
using System;
using System.Collections.Generic;

namespace coolc.CodeGen
{
    public abstract class Node
    {
        public List<Node> Childs { get; set; }
        public Node(List<Node> sons)
        {
            Childs = sons;
            if (Childs.Count == 0)
                Type = "Object";
            else
                Type = Childs[Childs.Count - 1].Type;
        }
        public virtual string GenerateCode()
        {
            var s = "";
            foreach (var item in Childs)
                s += item.GenerateCode();
            return s;
        }

        public virtual string GiveMeTheData()
        {
            var s = "";
            foreach (var item in Childs)
                s += item.GiveMeTheData();
            return s;
        }

        public virtual bool CheckSemantic()
        {
            foreach (var item in Childs)
                if (!item.CheckSemantic())
                    return false;
            return true;
        }

        public virtual string Type { get; set; }
    }

}