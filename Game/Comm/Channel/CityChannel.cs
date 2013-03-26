using System;
using System.ComponentModel;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;

namespace Game.Comm.Channel
{
    public class CityChannel
    {
        private readonly ICity city;

        private readonly Util.Channel channel;

        private readonly Procedure procedure;

        private readonly Formula formula;

        private readonly IRegionManager regionManager;

        private readonly string channelName;

        public CityChannel(ICity city, Util.Channel channel, Procedure procedure, Formula formula, IRegionManager regionManager)
        {
            this.city = city;
            this.channel = channel;
            this.procedure = procedure;
            this.formula = formula;
            this.regionManager = regionManager;

            channelName = "/CITY/" + city.Id;
        }

        public void Subscribe()
        {
            city.Troops.TroopUnitUpdated += TroopManagerTroopUnitUpdated;
            city.Troops.TroopUpdated += TroopManagerTroopUpdated;
            city.Troops.TroopRemoved += TroopManagerTroopRemoved;
            city.Troops.TroopAdded += TroopManagerTroopAdded;
            
            city.Template.UnitUpdated += UnitTemplateUnitUpdated;

            city.Worker.ActionRemoved += WorkerActionRemoved;
            city.Worker.ActionStarted += WorkerActionAdded;
            city.Worker.ActionRescheduled += WorkerActionRescheduled;

            city.Resource.ResourcesUpdate += ResourceUpdateEvent;          
  
            city.PropertyChanged += CityOnPropertyChanged;
        }

        private void CityOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch(propertyChangedEventArgs.PropertyName)
            {
                case "Radius":
                    RadiusUpdateEvent();
                    break;
                case "Battle":
                    if (city.Battle == null)
                    {
                        BattleEnded();
                    }
                    else
                    {
                        BattleStarted();
                    }
                    break;
                case "HideNewUnits":
                    HideNewUnitsUpdate();
                    break;
                case "AttackPoint":
                case "DefensePoint":
                case "AlignmentPoint":
                case "Value":
                    PointUpdate();
                    break;
            }
        }

        public void Unsubscribe()
        {
            city.Troops.TroopUnitUpdated -= TroopManagerTroopUnitUpdated;
            city.Troops.TroopUpdated -= TroopManagerTroopUpdated;
            city.Troops.TroopRemoved -= TroopManagerTroopRemoved;
            city.Troops.TroopAdded -= TroopManagerTroopAdded;
            
            city.Template.UnitUpdated -= UnitTemplateUnitUpdated;

            city.Worker.ActionRemoved -= WorkerActionRemoved;
            city.Worker.ActionStarted -= WorkerActionAdded;
            city.Worker.ActionRescheduled -= WorkerActionRescheduled;

            city.Resource.ResourcesUpdate -= ResourceUpdateEvent;                        
        }

        private void TroopManagerTroopRemoved(ITroopStub stub)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            bool doUpdate = city.IsUpdating;
            if (!doUpdate)
            {
                city.BeginUpdate();
            }
            city.Resource.Crop.Upkeep = procedure.UpkeepForCity(city);
            if (!doUpdate)
            {
                city.EndUpdate();
            }

            channel.Post(channelName, () =>
            {
                var packet = new Packet(Command.TroopRemoved);
                packet.AddUInt32(city.Id);
                packet.AddUInt32(stub.City.Id);
                packet.AddByte(stub.TroopId);
                return packet;
            });
        }

        private void ResourceUpdateEvent()
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
            {
                var packet = new Packet(Command.CityResourcesUpdate);
                packet.AddUInt32(city.Id);
                PacketHelper.AddToPacket(city.Resource, packet);
                return packet;
            });
        }

        private void RadiusUpdateEvent()
        {
            if (!ShouldUpdate())
            {
                return;
            }

            regionManager.ObjectUpdateEvent(city.MainBuilding, city.X, city.Y);

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityRadiusUpdate);
                    packet.AddUInt32(city.Id);
                    packet.AddByte(city.Radius);
                    return packet;
                });
        }

        private void PointUpdate()
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityPointUpdate);
                    packet.AddUInt32(city.Id);
                    packet.AddInt32(city.AttackPoint);
                    packet.AddInt32(city.DefensePoint);
                    packet.AddUInt16(city.Value);
                    packet.AddFloat((float)city.AlignmentPoint);
                    return packet;
                });
        }

        private void HideNewUnitsUpdate()
        {
            if (!Global.FireEvents || city.Deleted != City.DeletedState.NotDeleted)
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityHideNewUnitsUpdate);
                    packet.AddUInt32(city.Id);
                    packet.AddByte(city.HideNewUnits ? (byte)1 : (byte)0);
                    return packet;
                });
        }

        private void NewCityUpdate()
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post("/PLAYER/" + city.Owner.PlayerId, () =>
                {
                    var packet = new Packet(Command.CityNewUpdate);
                    PacketHelper.AddToPacket(city, packet);
                    return packet;
                });
        }

        private void ObjAddEvent(IGameObject obj)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            RecalculateValue(obj);

            var structure = obj as IStructure;
            if (structure != null)
            {
                structure.Technologies.TechnologyCleared += TechnologiesTechnologyCleared;
                structure.Technologies.TechnologyAdded += TechnologiesTechnologyAdded;
                structure.Technologies.TechnologyRemoved += TechnologiesTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += TechnologiesTechnologyUpgraded;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityObjectAdd);
                    packet.AddUInt16(Region.GetRegionIndex(obj));
                    PacketHelper.AddToPacket(obj, packet);
                    return packet;
                });
        }

        private void ObjRemoveEvent(IGameObject obj)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            RecalculateValue(obj);

            var structure = obj as IStructure;
            if (structure != null)
            {
                structure.Technologies.TechnologyCleared -= TechnologiesTechnologyCleared;
                structure.Technologies.TechnologyAdded -= TechnologiesTechnologyAdded;
                structure.Technologies.TechnologyRemoved -= TechnologiesTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded -= TechnologiesTechnologyUpgraded;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityObjectRemove);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(obj.ObjectId);
                    return packet;
                });
        }

        private void ObjUpdateEvent(IGameObject sender, uint origX, uint origY)
        {
            if (!Global.FireEvents || city.Deleted != City.DeletedState.NotDeleted)
            {
                return;
            }

            RecalculateValue(sender);
            
            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityObjectUpdate);
                    packet.AddUInt16(Region.GetRegionIndex(sender));
                    PacketHelper.AddToPacket(sender, packet);
                    return packet;
                });
        }

        private void UnitTemplateUnitUpdated(UnitTemplate sender)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.UnitTemplateUpgraded);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(sender, packet);
                    return packet;
                });
        }

        private void BattleStarted()
        {
            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityBattleStarted);
                    packet.AddUInt32(city.Id);
                    return packet;
                });
        }

        private void BattleEnded()
        {
            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.CityBattleEnded);
                    packet.AddUInt32(city.Id);
                    return packet;
                });
        }

        private void WorkerActionRescheduled(GameAction stub, ActionState state)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.ActionRescheduled);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(stub, packet, true);
                    return packet;
                });
        }

        private void WorkerActionAdded(GameAction stub, ActionState state)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.ActionStarted);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(stub, packet, true);
                    return packet;
                });
        }

        private void WorkerActionRemoved(GameAction stub, ActionState state)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.ActionCompleted);
                    packet.AddInt32((int)state);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(stub, packet, true);
                    return packet;
                });
        }

        private void TechnologiesTechnologyUpgraded(Technology tech)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TechUpgraded);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
                    packet.AddUInt32(tech.Type);
                    packet.AddByte(tech.Level);
                    return packet;
                });
        }

        private void TechnologiesTechnologyRemoved(Technology tech)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TechRemoved);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
                    packet.AddUInt32(tech.Type);
                    packet.AddByte(tech.Level);
                    return packet;
                });
        }

        private void TechnologiesTechnologyCleared(ITechnologyManager manager)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TechCleared);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(manager.OwnerLocation == EffectLocation.City ? 0 : manager.OwnerId);
                    return packet;
                });
        }

        private void TechnologiesTechnologyAdded(Technology tech)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TechAdded);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
                    packet.AddUInt32(tech.Type);
                    packet.AddByte(tech.Level);
                    return packet;
                });
        }
        
        private void TroopManagerTroopUpdated(ITroopStub stub)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TroopUpdated);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(stub, packet);
                    return packet;
                });
        }

        private void TroopManagerTroopUnitUpdated(ITroopStub stub)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            RecalculateUpkeep();
        }

        private void TroopManagerTroopAdded(ITroopStub stub)
        {
            if (!ShouldUpdate())
            {
                return;
            }

            RecalculateUpkeep();

            channel.Post(channelName, () =>
                {
                    var packet = new Packet(Command.TroopAdded);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(stub, packet);
                    return packet;
                });
        }

        private bool ShouldUpdate()
        {
            return Global.FireEvents && city.Deleted == City.DeletedState.NotDeleted;
        }

        private void RecalculateValue(IGameObject gameObject)
        {
            if (!(gameObject is IStructure))
            {
                return;
            }

            bool doUpdate = city.IsUpdating;
            if (!doUpdate)
            {
                city.BeginUpdate();
            }

            city.Value = formula.CalculateCityValue(city);

            if (!doUpdate)
            {
                city.EndUpdate();
            }
        }

        private void RecalculateUpkeep()
        {
            bool doUpdate = city.IsUpdating;
            if (!doUpdate)
            {
                city.BeginUpdate();
            }
            city.Resource.Crop.Upkeep = procedure.UpkeepForCity(city);
            if (!doUpdate)
            {
                city.EndUpdate();
            }
        }
    }
}
