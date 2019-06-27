using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Diagnostics;
using coolc.AST;
using coolc.Visitors;
using coolc.CodeGen;
using coolc.NewSem;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute

namespace coolc
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO --verbose parameter?

            #region debuging
            ////// To Release comment from HERE ##
            //#if Debug
        //    Fake_Main(new string[] { "../test/semanticerrors/selfmethod.cl" });//TODO
        //}
        //static void Fake_Main(string[] args)
        //{
            //#endif

            // To Release comment til HERE ##
            #endregion

            #region Variables
            double version = 0.1;
            string copyright = "ADB & JLA";
            var verbose = false;
            string usage = "Usage:\n\tcoolc.exe <filename.cl>";
            #endregion

            #region Welcome Message
            ////// ////// Welcome Message   ////// //////
            Console.ForegroundColor = ConsoleColor.Gray;

            if (verbose == true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("A cool compiler :)");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Version: {0}", version);
                Console.WriteLine("Copyright (C) 2017-2018: {0}", copyright);
            }
            #endregion

            #region Open file
            ////// ////// Open file         ////// //////

            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: No arguments given ");
                Console.ForegroundColor = ConsoleColor.Gray;
                //Console.WriteLine(usage);
                Environment.Exit(1);
                return;
            }

            //Console.WriteLine("Opening: {0}\t", args[0]);
            Console.Write("Program: {0,40}\t", args[0]);
            AntlrFileStream input_file;
            try
            {
                input_file = new AntlrFileStream(args[0]);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Opening file ");
                Console.ForegroundColor = ConsoleColor.Gray;
                //Console.WriteLine("");
                //Console.WriteLine(usage);
                Environment.Exit(1);
                return;
            }

            #endregion

            #region Parse
            StreamWriter LEW = new StreamWriter(args[0] + ".lexer.errors.txt");
            StreamWriter PEW = new StreamWriter(args[0] + ".parser.errors.txt");

            ////// ////// Parse             ////// //////
            CoolLexer lexer = new CoolLexer(input_file, LEW, LEW);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            CoolParser parser = new CoolParser(tokens, PEW, PEW);

            Console.ForegroundColor = ConsoleColor.Yellow;
            IParseTree tree = parser.program();

            LEW.Close();
            PEW.Close();

            if (parser.NumberOfSyntaxErrors > 0)
            {
                //Console.WriteLine("{0,20}","[Parsing Error]");
                Console.WriteLine();
                StreamReader LER = new StreamReader(args[0] + ".lexer.errors.txt");
                StreamReader PER = new StreamReader(args[0] + ".parser.errors.txt");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Lexer Errors:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(LER.ReadToEnd());

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parser Errors:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(PER.ReadToEnd());

                LER.Close();
                PER.Close();
                Console.ForegroundColor = ConsoleColor.Gray;

                Environment.Exit(1);
            }
			Console.ForegroundColor = ConsoleColor.Gray;
            #endregion

            #region Check Semantics
            // ////// Check Semantics   ////// //////
            ASTBuilder builder = new ASTBuilder();
            AST.Program p = (AST.Program)builder.Visit(tree);

            if (p.Errors.Count > 0 || p.CheckSemantics().Count > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Semantic errors: ");
                Console.ForegroundColor = ConsoleColor.Yellow;

                foreach (Error e in p.Errors)
                    Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;

                //File.Delete(args[0] + ".lexer.errors.txt");
                //File.Delete(args[0] + ".parser.errors.txt");

                Environment.Exit(1);
                //Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[Cool code]\t");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            //Console.WriteLine();
            //return;
            #endregion

            #region Generate Code
            ////// ////// Code Generation      ////// //////
            // TODO Generate Code

            DotCodeGenerator dcg = new DotCodeGenerator(args[0]);
            dcg.Visit(tree);

            Builder b = new Builder(args[0]);
            try
            {
                b.Compile(tree);
                #endregion

                #region Exit
                ////// ////// Exit              ////// //////
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0,17}", "[Build succeded]");
                Console.ForegroundColor = ConsoleColor.Gray;
                #endregion
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0,17}", "[Build failed]");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            File.Delete(args[0] + ".lexer.errors.txt");
            File.Delete(args[0] + ".parser.errors.txt");

        }
    }
}
