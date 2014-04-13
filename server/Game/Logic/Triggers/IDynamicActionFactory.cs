using System;

namespace Game.Logic.Triggers
{
    public interface IDynamicActionFactory
    {
        IDynamicAction CreateDynamicAction(Type actionType, string nlsDescription);
    }
}
