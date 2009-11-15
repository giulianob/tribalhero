using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Data {
    public class TroopStats : ICloneable {
        #region Base Stats
        byte baseAttackRadius = 0;
        public byte BaseAttackRadius {
            get { return baseAttackRadius; }
            set { baseAttackRadius = value; }
        }

        byte baseSpeed = 0;
        public byte BaseSpeed {
            get { return baseSpeed; }
            set { baseSpeed = value; }
        }
        #endregion

        #region Modifiers
        byte modAttackRadius = 0;
        public byte ModAttackRadius {
            get { return modAttackRadius; }
            set { modAttackRadius = value; }
        }

        byte modSpeed = 0;
        public byte ModSpeed {
            get { return modSpeed; }
            set { modSpeed = value; }
        }
        #endregion

        #region Total Getters
        public int TotalAttackRadius {
            get { return modAttackRadius + baseAttackRadius; }
        }

        public int TotalSpeed {
            get { return baseSpeed + modSpeed; }
        }
        #endregion

        #region Constructors
        public TroopStats() {
        }

        public TroopStats(byte attackRadius, byte speed) {
            this.baseAttackRadius = attackRadius;
            this.baseSpeed = speed;
        }
        #endregion

        #region ICloneable Members

        public object Clone() {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
