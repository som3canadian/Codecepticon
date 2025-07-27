using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codecepticon.Modules.CSharp.Profiles.SharpImpersonation.Rewriters
{
    class RemoveHelpText : CSharpSyntaxRewriter
    {
        protected SyntaxTreeHelper Helper = new SyntaxTreeHelper();

        protected List<string> HelpFunctions = new List<string>
        {
            "banner",
            "help",
            "showhelp",
            "usage",
            "showusage",
            "printhelp"
        };

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            BlockSyntax block = RewriteBlock(node);
            return block ?? base.VisitBlock(node);
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            string text = node.GetFirstToken().ValueText;
            
            if (text.Contains("SharpImpersonation") && 
                (text.Contains("Usage:") || text.Contains("Examples:") || text.Contains("Help:")))
            {
                return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, 
                    SyntaxFactory.Literal(""));
            }

            return base.VisitLiteralExpression(node);
        }

        protected BlockSyntax RewriteBlock(BlockSyntax node)
        {
            SyntaxNode pMethod = Helper.FindParentOfType(node, SyntaxKind.MethodDeclaration);
            if (pMethod == null)
            {
                return null;
            }

            string functionName = pMethod.ChildTokens().LastOrDefault().ValueText.ToLower();
            if (String.IsNullOrEmpty(functionName))
            {
                return null;
            }

            if (!HelpFunctions.Contains(functionName))
            {
                return null;
            }
            return SyntaxFactory.Block(null);
        }
    }
}