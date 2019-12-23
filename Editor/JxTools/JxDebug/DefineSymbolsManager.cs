using System.Linq;
using UnityEditor;

namespace JxDebug
{
    public class DefineSymbolsManager
    {
        public static bool HasDefine( string define, BuildTargetGroup group )
        {
            return new DefineSymbolsAgent(group).Has(define);
        }

        public static void AddDefine( string define, BuildTargetGroup group )
        {
            new DefineSymbolsAgent(group).Add(define);
        }

        public static void RemoveDefine( string define, BuildTargetGroup group )
        {
            new DefineSymbolsAgent(group).Remove(define);
        }

        private class DefineSymbolsAgent
        {
            private string[] defineSymbols;
            private string defineSymbolsString;
            private readonly BuildTargetGroup group;

            public DefineSymbolsAgent( BuildTargetGroup group )
            {
                this.group = group;
                LoadSymbols();
            }

            private void LoadSymbols()
            {
                defineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                defineSymbols = defineSymbolsString.Split(';');
            }

            public bool Has( string define )
            {
                return defineSymbols.Contains(define);
            }

            public void Add( string define )
            {
                if (Has(define))
                {
                    return;
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineSymbolsString + ";" + define);
                LoadSymbols();
            }

            public void Remove( string define )
            {
                if (!Has(define))
                {
                    return;
                }

                var startIndex = defineSymbolsString.IndexOf(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group,
                    defineSymbolsString.Remove(startIndex, define.Length));
                LoadSymbols();
            }
        }
    }
}