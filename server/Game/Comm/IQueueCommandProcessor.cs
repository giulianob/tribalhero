using System;

namespace Game.Comm
{
    public interface IQueueCommandProcessor
    {
        void RegisterCommand(string command, Action<dynamic> func);

        void Execute(string command, dynamic payload);
    }
}