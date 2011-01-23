#region

using System;
using CSScriptLibrary;
using Game.Logic.Conditons;

#endregion

namespace Game.Setup
{
    public class ConditionFactory
    {
        public static object CreateICondition(string name, string condition)
        {
            object assembly =
                    CSScript.LoadCode(
                                      @"using System;
                              using Game.Data;
                              using Game.Battle;

                                    public class " +
                                      name + @"Condition {
                                        public bool Check(" + name +
                                      @" obj) {
                                            return " + condition +
                                      @";
                                        }
                                    }").CreateInstance(name + @"Condition");
            switch(name)
            {
                case "Structure":
                    return assembly.AlignToInterface<IStructureCondition>();
                case "ICombatUnit":
                    return assembly.AlignToInterface<IICombatUnitCondition>();
                case "City":
                    return assembly.AlignToInterface<ICityCondition>();
                case "CombatObject":
                    return assembly.AlignToInterface<ICombatObjectCondition>();
                case "BaseBattleStats":
                    return assembly.AlignToInterface<IBaseBattleStatsCondition>();
            }
            throw new Exception("Type-Condition not supported!");
        }
    }
}