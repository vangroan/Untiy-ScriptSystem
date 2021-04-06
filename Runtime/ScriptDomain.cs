using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityEngine;

namespace VanGroan.ScriptSystem
{
    public class ScriptDomain
    {
        const string Source = @"
using System;
using UnityEngine;

namespace HelloWorld
{
    public class Hello : MonoBehaviour
    {
        void Start()
        {
            Debug.Log(""Hello, world!"");
        }
    }
}
";

        public void SayHello()
        {
            var tree = CSharpSyntaxTree.ParseText(Source);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences(
                                                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            // Semantic Analysis
            var model = compilation.GetSemanticModel(tree);
        }

        public ScriptDomain.Result CompileScript(string assemblyName, string script, string[] references = null)
        {
            return CompileScripts(assemblyName, new string[] { script }, references: references);
        }

        public ScriptDomain.Result CompileScripts(string assemblyName, string[] scripts, string[] references = null)
        {
            using (var ms = new MemoryStream())
            {
                var trees = scripts.Select(s => SyntaxFactory.ParseSyntaxTree(SourceText.From(s))).ToArray();
                var compilation = CSharpCompilation.Create(assemblyName)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(
                        // mscorlib.dll
                        GetMscorlib(),
                        // UnityEngine
                        GetUnityEngine()
                    )
                    .AddSyntaxTrees(trees);
                
                if (references != null)
                {
                    compilation = compilation.AddReferences(references.Select(s => MetadataReference.CreateFromFile(s)));
                }

                var result = compilation.Emit(ms);

                if (HandleCompileResult(result))
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    var assembly = LoadAssembly(ms);
                    return new Result
                    {
                        Success = true,
                        Assembly = new ScriptAssembly(assembly),
                    };
                }
                else
                {
                    return new Result
                    {
                        Success = false,
                        Assembly = null,
                    };
                }
            }
        }

        protected System.Reflection.Assembly LoadAssembly(MemoryStream ms)
        {
            var assembly = Assembly.Load(ms.ToArray());
            // TODO: Do we need different loaders depending on framework, standard or core?
#if NET46
            // Different in full .Net framework
            // var assembly = Assembly.Load(ms.ToArray());
#elif NET5
            // TODO: This is apparently more sandboxed and worth pursuing, but requires newer versions. (.Net Standard?)
            // var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
#endif
            return assembly;
        }

        private bool HandleCompileResult(Microsoft.CodeAnalysis.Emit.EmitResult result)
        {
            if (result.Success)
            {
                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning(string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.ToString())
                ));
                return false;
            }
        }

        private MetadataReference GetMscorlib()
        {
            UnityEngine.Debug.LogWarningFormat("Environment Version (ScriptDomain) {0}", Environment.Version);
            return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        }

        private MetadataReference GetUnityEngine()
        {
            return MetadataReference.CreateFromFile(typeof(MonoBehaviour).Assembly.Location);
        }

        /// <summary>
        ///     Utility for determining the DLL file location of a loaded assembly using a value.
        /// </summary>
        public static string GetAssemblyLocation<T>()
        {
            return typeof(T).Assembly.Location;
        }

        public sealed class Result
        {
            public bool Success { get; internal set; }
            public ScriptAssembly Assembly { get; internal set; }
        }
    }
}
