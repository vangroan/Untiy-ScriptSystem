using System;
using UnityEngine;
using VanGroan.ScriptSystem;


[ExecuteInEditMode]
public class ModDemo : MonoBehaviour
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
            Debug.LogWarning(""Hello, world!"");
        }

        bool HelloFromCode(string name)
        {
            Debug.LogWarningFormat(""Hello from {0}"", name);
            return true;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello world!"");
            Console.ReadLine();
        }
    }
}
";

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.LogWarningFormat("Environment Version {0}", Environment.Version);

        var domain = new ScriptDomain();
        // var references = new string[] { ScriptDomain.GetAssemblyLocation<IModInterface>() };
        var references = new string[] {};
        var result = domain.CompileScript("HelloWorld", Source, references: references);

        if (result.Success)
        {
            var hello = result.Assembly.CreateComponent<MonoBehaviour>("HelloWorld.Hello", new GameObject("Hello Modding"));
            UnityEngine.Debug.Assert(hello != null, "ScriptAssembly didn't return new MonoBehaviour");
        }
        else
        {
            throw new Exception("failed to compile script");
        }
    }
}
