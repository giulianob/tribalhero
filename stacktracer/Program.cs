using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;

namespace stacktracer
{
    class Program
    {
        private const string regPOI = @"\bclass\b|\bfunction\b|\breturn\b|[""'/{}]";
        private const string regFun = @"\bfunction\b\s*((?:[gs]et\s+)?\w*)\s*\(";
        private const string regCls = @"class\s+(\w+)[\s{]";
        private const string regStr = @"([""'/]).*?(?<!\\)\1";

        private static Regex rePOI = new Regex(regPOI, RegexOptions.Multiline & RegexOptions.IgnoreCase);
        private static Regex reFun = new Regex(regFun, RegexOptions.Multiline & RegexOptions.IgnoreCase);
        private static Regex reCls = new Regex(regCls, RegexOptions.Multiline & RegexOptions.IgnoreCase);
        private static Regex reStr = new Regex(regStr, RegexOptions.Multiline & RegexOptions.IgnoreCase);

        private static string path;
        private static bool outputOnly = true;

        class StackTraceItem
        {
            public string Name { get; set; }
            public int Depth { get; set; }
            public int Anon { get; set; }
        }

        static void Main(string[] args)
        {
            var p = new OptionSet
                    {{"output-only=", v => outputOnly = bool.Parse(v)}, {"path=", v => path = v}};

            p.Parse(Environment.GetCommandLineArgs());

            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                
            } else
            {
                ProcessFile(path);
            }

            Console.ReadKey();
        }

        static void ProcessFile(string filePath)
        {
            var body = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, filePath));

            var stack = new Stack();
            StackTraceItem lastf;
            int depth = 0;
            int retvar = 0;
            string klass = "";
            bool alreadyReturned = false;

            Match match = rePOI.Match(body);

            while (match.Success)
            {
                var poi = match.Groups[0];
                int pos = match.Index;
                var endPos = match.Index + match.Length;

                string line;
                switch (poi.Value)
                {
                    // Strings
                    case "\"":
                    case "/":
                        var strm = reStr.Match(body, pos);
                        Regex strReg = new Regex(@"[=(,]\s*$");
                        if (strm.Success && (poi.Value != "/" || strReg.Match(body, pos).Success))
                            endPos = strm.Index + strm.Length;
                        Console.Out.WriteLine("String:" + strm.Value);
                        break;
                    // Class
                    case "class":
                        klass = reCls.Match(body, pos).Groups[1].Value;
                        Console.Out.WriteLine("Class: " + klass);
                        break;
                    // Function
                    case "function":
                        var fnameMatch = reFun.Match(body, pos);
                        var fname = string.Empty;
                        // Regular functions
                        if (fnameMatch.Groups.Count > 1 && fnameMatch.Groups[1].Value != string.Empty)
                        {
                            fname = klass + "." + fnameMatch.Groups[1].Value;
                            Console.Out.WriteLine("Function: " + fname);
                        }
                        // Anonymous functions
                        else
                        {
                            lastf = (StackTraceItem)stack.Peek();
                            lastf.Anon += 1;
                            fname = lastf.Name + ".anon" + lastf.Anon;
                            Console.Out.WriteLine("Anonymous Function: " + fname);
                        }

                        stack.Push(new StackTraceItem
                                   {
                                           Name = fname,
                                           Depth = depth,
                                           Anon = 0,
                                   });

                        var brace = body.IndexOf('{', pos) + 1;
                        var enterFuncLine = string.Empty;
                        body = body.Substring(0, brace) + enterFuncLine + body.Substring(brace);
                        depth += 1;                        
                        endPos = brace + enterFuncLine.Length;
                        break;
                    // Opening brackets
                    case "{":
                        depth += 1;
                        break;
                    // Return statements
                    case "return":
                        lastf = (StackTraceItem)stack.Peek();
                        var semicolon = body.IndexOf(';', pos);
                        Regex retReg = new Regex(@"return\s*;");
                        var matchResult = retReg.Match(body, pos);
                        if (matchResult.Value != string.Empty)
                        {
                            //RETURN WITHOUT VALUE HERE
                            line = string.Format("domystuffhere(\"{0}\");\r\nreturn;\r\n", lastf.Name);
                        }
                        else
                        {
                            retvar += 1;
                            var tmpRetValue = body.Substring(pos + 6, semicolon - (pos + 6));
                            //RETURN WITH VALUE HERE
                            line = string.Format("var __ret{1}__: * ={0};\r\ndomystuffhere(\"{2}\");\r\nreturn __ret{1}__;\r\n", tmpRetValue, retvar, lastf.Name);
                        }

                        body = body.Substring(0, pos) + line + body.Substring(semicolon + 1);
                        endPos = pos + line.Length;
                        alreadyReturned = true;
                        break;
                        // Function end
                    case "}":
                        depth -= 1;
                        if (stack.Count > 0 && ((StackTraceItem)stack.Peek()).Depth == depth)
                        {
                            lastf = (StackTraceItem)stack.Pop();
                            if (!alreadyReturned)
                            {
                                line = string.Format("domystuffhere(\"{0}\");\r\n", lastf.Name);
                                body = body.Substring(0, pos) + line + body.Substring(pos);
                                endPos += line.Length;
                            }
                            alreadyReturned = false;
                        }                        
                        break;
                }

                pos = endPos;
                match = rePOI.Match(body, pos);
            }

            Console.Out.WriteLine(body);
        }
    }
}
