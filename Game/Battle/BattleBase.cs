using System.Collections.Generic;

namespace Game.Battle {
    public class BattleBase {
        public delegate void OnBattle(CombatList atk, CombatList def);

        public event OnBattle EnterBattle;
        public event OnBattle ExitBattle;

        public delegate void OnRound(CombatList atk, CombatList def, uint round, int stamina);

        public event OnRound EnterRound;

        public delegate void OnTurn(CombatList atk, CombatList def, int turn);

        public event OnTurn EnterTurn;
        public event OnTurn ExitTurn;

        public delegate void OnReinforce(IEnumerable<CombatObject> list);

        public event OnReinforce ReinforceAttacker;
        public event OnReinforce ReinforceDefender;
        public event OnReinforce WithdrawAttacker;
        public event OnReinforce WithdrawDefender;

        public delegate void OnUnitUpdate(CombatObject obj);

        public event OnUnitUpdate UnitAdded;
        public event OnUnitUpdate UnitRemoved;
        public event OnUnitUpdate UnitUpdated;

        public event OnUnitUpdate SkippedAttacker;

        public delegate void OnAttack(CombatObject source, CombatObject target, ushort damage);

        public event OnAttack ActionAttacked;

        public void EventEnterBattle(CombatList atk, CombatList def) {
            if (EnterBattle != null)
                EnterBattle(atk, def);
        }

        public void EventExitBattle(CombatList atk, CombatList def) {
            if (ExitBattle != null)
                ExitBattle(atk, def);
        }

        public void EventEnterRound(CombatList atk, CombatList def, uint round, int stamina) {
            if (EnterRound != null)
                EnterRound(atk, def, round, stamina);
        }

        public void EventEnterTurn(CombatList atk, CombatList def, int turn) {
            if (EnterTurn != null)
                EnterTurn(atk, def, turn);
        }

        public void EventExitTurn(CombatList atk, CombatList def, int turn) {
            if (ExitTurn != null)
                ExitTurn(atk, def, turn);
        }

        public void EventReinforceAttacker(IEnumerable<CombatObject> list) {
            if (ReinforceAttacker != null)
                ReinforceAttacker(list);
        }

        public void EventReinforceDefender(IEnumerable<CombatObject> list) {
            if (ReinforceDefender != null)
                ReinforceDefender(list);
        }

        public void EventWithdrawAttacker(IEnumerable<CombatObject> list) {
            if (WithdrawAttacker != null)
                WithdrawAttacker(list);
        }

        public void EventWithdrawDefender(IEnumerable<CombatObject> list) {
            if (WithdrawDefender != null)
                WithdrawDefender(list);
        }

        public void EventUnitRemoved(CombatObject obj) {
            if (UnitRemoved != null)
                UnitRemoved(obj);
        }

        public void EventUnitAdded(CombatObject obj) {
            if (UnitAdded != null)
                UnitAdded(obj);
        }

        public void EventUnitUpdated(CombatObject obj) {
            if (UnitUpdated != null)
                UnitUpdated(obj);
        }

        public void EventActionAttacked(CombatObject source, CombatObject target, ushort dmg) {
            if (ActionAttacked != null)
                ActionAttacked(source, target, dmg);
        }

        public void EventSkippedAttacker(CombatObject source) {
            if (SkippedAttacker != null)
                SkippedAttacker(source);
        }
    }
}