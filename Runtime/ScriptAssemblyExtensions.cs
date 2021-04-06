using System;
using UnityEngine;

namespace VanGroan.ScriptSystem
{
    public static class ScriptAssemblyExtensions
    {
        public static Component CreateComponent(this ScriptAssembly assembly, string componentName, GameObject gameObject)
        {
            var ty = assembly.m_assembly.GetType(componentName);
            if (ty != null)
                return gameObject.AddComponent(ty);
            else
                throw new Exception("Assembly does not contain mono behaviour type");
        }

        public static T CreateComponent<T>(this ScriptAssembly assembly, string componentName, GameObject gameObject)
            where T : Component
        {
            var ty = assembly.m_assembly.GetType(componentName);
            var baseType = typeof(T);

            if (ty != null)
            {
                if (baseType == ty || ty.IsSubclassOf(baseType))
                {
                    return gameObject.AddComponent(ty) as T;
                }
                else
                {
                    throw new Exception("Type from assembly does not inherit from UnityEngine.Component");
                }
            }
            else
            {
                throw new Exception("Assembly does not contain mono behaviour type");
            }
        }
    }
}
