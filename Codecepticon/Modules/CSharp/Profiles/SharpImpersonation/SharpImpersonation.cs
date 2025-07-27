using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codecepticon.CommandLine;
using Codecepticon.Modules.CSharp.Profiles.SharpImpersonation.Rewriters;
using Codecepticon.Utils;
using Microsoft.CodeAnalysis;
using BuildEvaluation = Microsoft.Build.Evaluation;

namespace Codecepticon.Modules.CSharp.Profiles.SharpImpersonation
{
    class SharpImpersonation : BaseProfile
    {
        public override string Name { get; } = "SharpImpersonation";

        public override async Task<Solution> Before(Solution solution, Project project)
        {
            if (CommandLineData.CSharp.Rename.CommandLine)
            {
                Logger.Debug("SharpImpersonation: Rewriting command line");
                solution = await RewriteCommandLine(solution, project);
            }

            Logger.Debug("SharpImpersonation: Removing help text");
            solution = await RemoveHelpText(solution, project);
            return solution;
        }

        public override async Task<Solution> After(Solution solution, Project project)
        {
            BuildEvaluation.Project buildProject = VisualStudioManager.GetBuildProject(project);

            if (CommandLineData.CSharp.Rename.Namespaces)
            {
                Logger.Debug("SharpImpersonation: Renaming RootNamespace");
                string rootNamespace = buildProject.GetPropertyValue("RootNamespace");
                string newNamespace = DataCollector.Mapping.Namespaces[rootNamespace];
                buildProject.SetProperty("RootNamespace", newNamespace);
            }

            Logger.Debug("SharpImpersonation: Renaming Misc Properties");
            buildProject.SetProperty("AssemblyName", CommandLineData.Global.NameGenerator.Generate());
            buildProject.SetProperty("Company", CommandLineData.Global.NameGenerator.Generate());
            buildProject.SetProperty("Product", CommandLineData.Global.NameGenerator.Generate());
            buildProject.Save();
            return solution;
        }

        protected async Task<Solution> RewriteCommandLine(Solution solution, Project project)
        {
            Rewriters.CommandLine rewriteCommandLine = new Rewriters.CommandLine();

            project = VisualStudioManager.GetProjectByName(solution, project.Name);
            foreach (Document document in project.Documents)
            {
                SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync();
                
                FileInfo file = new FileInfo(document.FilePath);
                if (file.Name == "Program.cs" || file.Name.Contains("ArgumentParser"))
                {
                    syntaxRoot = rewriteCommandLine.Visit(syntaxRoot);
                }

                solution = solution.WithDocumentSyntaxRoot(document.Id, syntaxRoot);
            }
            return solution;
        }

        protected async Task<Solution> RemoveHelpText(Solution solution, Project project)
        {
            project = VisualStudioManager.GetProjectByName(solution, project.Name);
            
            foreach (Document document in project.Documents)
            {
                FileInfo file = new FileInfo(document.FilePath);
                if (file.Name == "Program.cs")
                {
                    SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync();
                    RemoveHelpText rewriter = new RemoveHelpText();
                    SyntaxNode newSyntaxRoot = rewriter.Visit(syntaxRoot);
                    solution = solution.WithDocumentSyntaxRoot(document.Id, newSyntaxRoot);
                    break;
                }
            }

            return solution;
        }
    }
}