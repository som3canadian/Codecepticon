using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codecepticon.CommandLine;
using Codecepticon.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codecepticon.Modules.CSharp.Profiles.SharpImpersonation.Rewriters
{
    class CommandLine : CSharpSyntaxRewriter
    {
        protected SyntaxTreeHelper Helper = new SyntaxTreeHelper();

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            string text = node.GetFirstToken().ValueText.Trim();
            
            // Handle exact matches for commands and argument keys
            if (text == "list" || text == "elevated" || text == "wmi" ||
                text == "user" || text == "pid" || text == "binary" || 
                text == "shellcode" || text == "technique")
            {
                return Helper.RewriteCommandLineArg(node, text, "");
            }

            return base.VisitLiteralExpression(node);
        }
    }
}