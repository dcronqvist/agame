using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using AGame;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.DebugTools;

namespace AGame.Engine.Assets.Scripting
{
    internal class ScriptCompiler
    {
        public byte[] Compile(string code, out string[] errorMsgs)
        {
            var sourceCode = code;

            using (var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode).Emit(peStream);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    List<string> errors = new List<string>();

                    foreach (var diagnostic in failures)
                    {
                        errors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()} at {sourceCode.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)}");
                    }

                    errorMsgs = errors.ToArray();
                    return null;
                }

                peStream.Seek(0, SeekOrigin.Begin);

                errorMsgs = new string[0];
                return peStream.ToArray();
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
            };

            Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
                .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            return CSharpCompilation.Create(null,
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }
}