﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorzCore.DataTypes;
using ColorzCore.Lexer;
using ColorzCore.Parser;
using ColorzCore.Parser.AST;
using ColorzCore.IO;
using System.IO;

namespace ColorzCore.Preprocessor.Directives
{
    class IncludeExternalDirective : IDirective
    {
        public int MinParams { get { return 1; } }
        public int? MaxParams { get { return null; } }
        public bool RequireInclusion { get { return true; } }

        public IncludeFileSearcher FileSearcher { get; set; }

        public Maybe<ILineNode> Execute(EAParser parse, Token self, IList<IParamNode> parameters, MergeableGenerator<Token> tokens)
        {
            Maybe<string> validFile = FileSearcher.FindFile(Path.GetDirectoryName(self.FileName), IOUtility.GetToolFileName(parameters[0].ToString()));

            if (validFile.IsNothing)
            {
                parse.Error(parameters[0].MyLocation, "Tool " + parameters[0].ToString() + " not found.");
                return new Nothing<ILineNode>();
            }
            //TODO: abstract out all this running stuff into a method so I don't have code duplication with inctext

            //from http://stackoverflow.com/a/206347/1644720
            // Start the child process.
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.RedirectStandardError = true;
            // Redirect the output stream of the child process.
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(self.FileName);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = validFile.FromJust;
            StringBuilder argumentBuilder = new StringBuilder();
            for (int i = 1; i < parameters.Count; i++)
            {
                if(parameters[i].Type == ParamType.ATOM)
                {
                    parameters[i] = ((IAtomNode)parameters[i]).Simplify();
                }
                argumentBuilder.Append(parameters[i].PrettyPrint());
                argumentBuilder.Append(' ');
            }
            argumentBuilder.Append("--to-stdout");
            p.StartInfo.Arguments = argumentBuilder.ToString();
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            MemoryStream outputBytes = new MemoryStream();
            MemoryStream errorStream = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(outputBytes);
            p.StandardError.BaseStream.CopyTo(errorStream);
            p.WaitForExit();

            byte[] output = outputBytes.GetBuffer().Take((int)outputBytes.Length).ToArray();
            if (errorStream.Length > 0)
            {
                parse.Error(self.Location, Encoding.ASCII.GetString(errorStream.GetBuffer().Take((int)errorStream.Length).ToArray()));
            }
            else if (output.Length >= 7 && Encoding.ASCII.GetString(output.Take(7).ToArray()) == "ERROR: ")
            {
                parse.Error(self.Location, Encoding.ASCII.GetString(output.Skip(7).ToArray()));
            }
            return new Just<ILineNode>(new DataNode(parse.CurrentOffset, output));
        }
    }
}
