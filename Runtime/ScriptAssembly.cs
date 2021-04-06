using System;

namespace VanGroan.ScriptSystem
{
    public class ScriptAssembly
    {
        internal readonly System.Reflection.Assembly m_assembly;

        public ScriptAssembly(System.Reflection.Assembly assembly)
        {
            m_assembly = assembly;
        }
    }
}
