#region

using System.Collections.Generic;

#endregion

namespace Game.Battle
{
    public abstract class BattleBase
    {
        #region Delegates

        public delegate void OnAttack(CombatObject source, CombatObject target, ushort damage);

        public delegate void OnBattle(CombatList atk, CombatList def);

        public delegate void OnReinforce(IEnumerable<CombatObject> list);

        public delegate void OnRound(CombatList atk, CombatList def, uint round);

        public delegate void OnTurn(CombatList atk, CombatList def, int turn);

        public delegate void OnUnitUpdate(CombatObject obj);

        #endregion

        public event OnBattle EnterBattle;
        public event OnBattle ExitBattle;

        public event OnRound EnterRound;

        public event OnTurn EnterTurn;
        public event OnTurn ExitTurn;

        public event OnReinforce ReinforceAttacker;
        public event OnReinforce ReinforceDefender;
        public event OnReinforce WithdrawAttacker;
        public event OnReinforce WithdrawDefender;

        public event OnUnitUpdate UnitAdded;
        public event OnUnitUpdate UnitRemoved;
        public event OnUnitUpdate UnitUpdated;

        public event OnUnitUpdate SkippedAttacker;

        public event OnAttack ActionAttacked;

        public void EventEnterBattle(CombatList atk, CombatList def)
        {
            if (EnterBattle != null)
                EnterBattle(atk, def);
        }

        public void EventExitBattle(CombatList atk, CombatList def)
        {
            if (ExitBattle != null)
                ExitBattle(atk, def);
        }

        public void EventEnterRound(CombatList atk, CombatList def, uint round)
        {
            if (EnterRound != null)
                EnterRound(atk, def, round);
        }

        public void EventEnterTurn(CombatList atk, CombatList def, int turn)
        {
            if (EnterTurn != null)
                EnterTurn(atk, def, turn);
        }

        public void EventExitTurn(CombatList atk, CombatList def, int turn)
        {
            if (ExitTurn != null)
                ExitTurn(atk, def, turn);
        }

        public void EventReinforceAttacker(IEnumerable<CombatObject> list)
        {
            if (ReinforceAttacker != null)
                ReinforceAttacker(list);
        }

        public void EventReinforceDefender(IEnumerable<CombatObject> list)
        {
            if (ReinforceDefender != null)
                ReinforceDefender(list);
        }

        public void EventWithdrawAttacker(IEnumerable<CombatObject> list)
        {
            if (WithdrawAttacker != null)
                WithdrawAttacker(list);
        }

        public void EventWithdrawDefender(IEnumerable<CombatObject> list)
        {
            if (WithdrawDefender != null)
                WithdrawDefender(list);
        }

        public void EventUnitRemoved(CombatObject obj)
        {
            if (UnitRemoved != null)
                UnitRemoved(obj);
        }

        public void EventUnitAdded(CombatObject obj)
        {
            if (UnitAdded != null)
                UnitAdded(obj);
        }

        public void EventUnitUpdated(CombatObject obj)
        {
            if (UnitUpdated != null)
                UnitUpdated(obj);
        }

        public void EventActionAttacked(CombatObject source, CombatObject target, ushort dmg)
        {
            if (ActionAttacked != null)
                ActionAttacked(source, target, dmg);
        }

        public void EventSkippedAttacker(CombatObject source)
        {
            if (SkippedAttacker != null)
                SkippedAttacker(source);
        }
    }
}