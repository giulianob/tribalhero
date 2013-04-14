﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic.Conditons;

namespace Game.Logic.Triggers.Events
{
    class StructureConvertEvent : ICityEvent 
    {
        private readonly IStructure structure;
        private readonly int type;
        private readonly byte level;

        public StructureConvertEvent(IStructure structure, int type, byte level)
        {
            this.structure = structure;
            this.type = type;
            this.level = level;
        }

        #region Implementation of ICityEvent

        public IGameObject GameObject
        {
            get
            {
                return structure;
            }
        }

        public dynamic Parameters
        {
            get
            {
                return new {type, level};
            }
        }

        #endregion
    }
}
