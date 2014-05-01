using System;
using System.Collections.Generic;
using Game.Comm;
using Game.Data.Tribe;
using Game.Util.Locking;
using Persistance;

namespace Game.Data
{
    public interface IPlayer : ILockable, IPersistableObject
    {
        Session Session { get; set; }

        string Name { get; set; }

        uint PlayerId { get; }

        uint TutorialStep { get; set; }

        string PlayerHash { get; }

        DateTime Created { get; }

        DateTime LastLogin { get; set; }

        string SessionId { get; set; }

        PlayerRights Rights { get; set; }

        PlayerChatState ChatState { get; }

        bool IsIdle { get; }

        string Description { get; set; }

        ITribesman Tribesman { get; set; }

        DateTime LastDeletedTribe { get; set;  }

        uint TribeRequest { get; set; }

        int AttackPoint { get; }

        int DefensePoint { get; }

        int Value { get; }

        bool IsLoggedIn { get; }

        DateTime Muted { get; set; }

        bool Banned { get; set; }

        bool IsInTribe { get; }

        bool NeverAttacked { get; set; }

        void Add(ICity city);

        int GetCityCount(bool includeDeleted = false);

        IEnumerable<ICity> GetCityList(bool includeDeleted = false);

        ICity GetCity(uint id);

        void SendSystemMessage(IPlayer from, String subject, String message);

        void TribeUpdate();

        AchievementList Achievements { get; }
        bool SoundMuted { get; set; }

        DateTime? HasTwoFactorAuthenticated { get; set; }

        string TwoFactorSecretKey { get; set; }
    }
}