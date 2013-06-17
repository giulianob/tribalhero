using System;
using System.ComponentModel;
using Game.Data;
using Game.Data.Events;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;

namespace Game.Comm.Channel
{
    public class CityChannel : ICityChannel
    {
        private readonly Util.IChannel channel;

        private readonly Procedure procedure;

        private readonly Formula formula;

        private readonly IRegionManager regionManager;

        public CityChannel(Util.IChannel channel, Procedure procedure, Formula formula, IRegionManager regionManager)
        {
            this.channel = channel;
            this.procedure = procedure;
            this.formula = formula;
            this.regionManager = regionManager;
        }

        public void Register(ICityManager cityManager)
        {
            cityManager.CityAdded += CityAdded;
            cityManager.CityRemoved += CityRemoved;
        }

        private void CityAdded(object sender, EventArgs args)
        {
            Subscribe((City)sender);
        }

        private void CityRemoved(object sender, EventArgs args)
        {
            Unsubscribe((City)sender);
        }

        private void Subscribe(ICity city)
        {
            city.TroopUnitUpdated += TroopManagerTroopUnitUpdated;
            city.TroopUpdated += TroopManagerTroopUpdated;
            city.TroopRemoved += TroopManagerTroopRemoved;
            city.TroopAdded += TroopManagerTroopAdded;
            
            city.ActionRemoved += WorkerActionRemoved;
            city.ActionStarted += WorkerActionAdded;
            city.ActionRescheduled += WorkerActionRescheduled;

            city.ResourcesUpdated += ResourceUpdateEvent;     
                   
            city.UnitTemplateUpdated += UnitTemplateUnitUpdated;      
            
            city.PropertyChanged += CityOnPropertyChanged;

            city.TechnologyCleared += TechnologiesTechnologyCleared;
            city.TechnologyAdded += TechnologiesTechnologyAdded;
            city.TechnologyRemoved += TechnologiesTechnologyRemoved;
            city.TechnologyUpgraded += TechnologiesTechnologyUpgraded;

            city.ObjectAdded += ObjectAdded;
            city.ObjectRemoved += ObjectRemoved;
            city.ObjectUpdated += ObjectUpdated;

            city.ReferenceAdded += ReferenceAdded;
            city.ReferenceRemoved += ReferenceRemoved;

            NewCityUpdate(city);
        }

        private void Unsubscribe(ICity city)
        {
            city.TroopUnitUpdated -= TroopManagerTroopUnitUpdated;
            city.TroopUpdated -= TroopManagerTroopUpdated;
            city.TroopRemoved -= TroopManagerTroopRemoved;
            city.TroopAdded -= TroopManagerTroopAdded;
            
            city.ActionRemoved -= WorkerActionRemoved;
            city.ActionStarted -= WorkerActionAdded;
            city.ActionRescheduled -= WorkerActionRescheduled;

            city.ResourcesUpdated -= ResourceUpdateEvent;       
                 
            city.UnitTemplateUpdated -= UnitTemplateUnitUpdated;      
            
            city.PropertyChanged -= CityOnPropertyChanged;

            city.TechnologyCleared -= TechnologiesTechnologyCleared;
            city.TechnologyAdded -= TechnologiesTechnologyAdded;
            city.TechnologyRemoved -= TechnologiesTechnologyRemoved;
            city.TechnologyUpgraded -= TechnologiesTechnologyUpgraded;

            city.ObjectAdded -= ObjectAdded;
            city.ObjectRemoved -= ObjectRemoved;
            city.ObjectUpdated -= ObjectUpdated;

            city.ReferenceAdded -= ReferenceAdded;
            city.ReferenceRemoved -= ReferenceRemoved;

            if (city.Owner.Session != null)
            {
                channel.Unsubscribe(city.Owner.Session, GetChannelName(city));
            }
        }

        private string GetChannelName(ICity city)
        {
            return "/PLAYER/" + city.Owner.PlayerId;
        }

        private void CityOnPropertyChanged(ICity city, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch(propertyChangedEventArgs.PropertyName)
            {
                case "Radius":
                    RadiusUpdateEvent(city);
                    break;
                case "Battle":
                    if (city.Battle == null)
                    {
                        BattleEnded(city);
                    }
                    else
                    {
                        BattleStarted(city);
                    }
                    break;
                case "HideNewUnits":
                    HideNewUnitsUpdate(city);
                    break;
                case "AttackPoint":
                case "DefensePoint":
                case "AlignmentPoint":
                case "Value":
                    PointUpdate(city);
                    break;
            }
        }

        private void TroopManagerTroopRemoved(ICity city, TroopStubEventArgs args)
        {
            if (!ShouldUpdate(city))
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

            channel.Post(GetChannelName(city), () =>
            {
                var packet = new Packet(Command.TroopRemoved);
                packet.AddUInt32(city.Id);
                packet.AddUInt32(args.Stub.City.Id);
                packet.AddUInt16(args.Stub.TroopId);
                return packet;
            });
        }

        private void ReferenceAdded(ICity city, ActionReferenceArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.ReferenceAdd);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt16(args.ReferenceStub.ReferenceId);
                    packet.AddUInt32(args.ReferenceStub.WorkerObject.WorkerId);
                    packet.AddUInt32(args.ReferenceStub.Action.ActionId);
                    return packet;
                });
        }

        private void ReferenceRemoved(ICity city, ActionReferenceArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city),
                         () => 
                             {
                                 var packet = new Packet(Command.ReferenceRemove);
                                 packet.AddUInt32(city.Id);
                                 packet.AddUInt16(args.ReferenceStub.ReferenceId);

                                 return packet;
                             });
        }

        private void ResourceUpdateEvent(ICity city, EventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
            {
                var packet = new Packet(Command.CityResourcesUpdate);
                packet.AddUInt32(city.Id);
                PacketHelper.AddToPacket(city.Resource, packet);
                return packet;
            });
        }

        private void RadiusUpdateEvent(ICity city)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            regionManager.ObjectUpdateEvent(city.MainBuilding, city.X, city.Y);

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityRadiusUpdate);
                    packet.AddUInt32(city.Id);
                    packet.AddByte(city.Radius);
                    return packet;
                });
        }

        private void PointUpdate(ICity city)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
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

        private void HideNewUnitsUpdate(ICity city)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityHideNewUnitsUpdate);
                    packet.AddUInt32(city.Id);
                    packet.AddByte(city.HideNewUnits ? (byte)1 : (byte)0);
                    return packet;
                });
        }

        private void ObjectAdded(ICity city, GameObjectArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            RecalculateValue(city, args.Object);

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityObjectAdd);
                    packet.AddUInt16(Region.GetRegionIndex(args.Object));
                    PacketHelper.AddToPacket(args.Object, packet);
                    return packet;
                });
        }

        private void ObjectRemoved(ICity city, GameObjectArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            RecalculateValue(city, args.Object);

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityObjectRemove);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(args.Object.ObjectId);
                    return packet;
                });
        }

        private void ObjectUpdated(ICity city, GameObjectArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            RecalculateValue(city, args.Object);
            
            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityObjectUpdate);
                    packet.AddUInt16(Region.GetRegionIndex(args.Object));
                    PacketHelper.AddToPacket(args.Object, packet);
                    return packet;
                });
        }

        private void UnitTemplateUnitUpdated(ICity city, EventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.UnitTemplateUpgraded);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(city.Template, packet);
                    return packet;
                });
        }

        private void BattleStarted(ICity city)
        {
            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityBattleStarted);
                    packet.AddUInt32(city.Id);
                    return packet;
                });
        }

        private void BattleEnded(ICity city)
        {
            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityBattleEnded);
                    packet.AddUInt32(city.Id);
                    return packet;
                });
        }

        private void WorkerActionRescheduled(ICity city, ActionWorkerEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            var passiveAction = args.Stub as PassiveAction;
            if (passiveAction != null && !passiveAction.IsVisible)
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.ActionRescheduled);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(args.Stub, packet, true);
                    return packet;
                });
        }

        private void WorkerActionAdded(ICity city, ActionWorkerEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            var passiveAction = args.Stub as PassiveAction;
            if (passiveAction != null && !passiveAction.IsVisible)
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.ActionStarted);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(args.Stub, packet, true);
                    return packet;
                });
        }

        private void WorkerActionRemoved(ICity city, ActionWorkerEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            var passiveAction = args.Stub as PassiveAction;
            if (passiveAction != null && !passiveAction.IsVisible)
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.ActionCompleted);
                    packet.AddInt32((int)args.State);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(args.Stub, packet, true);
                    return packet;
                });
        }

        private void TechnologiesTechnologyUpgraded(ICity city, TechnologyEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TechUpgraded);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(args.Technology.OwnerLocation == EffectLocation.City ? 0 : args.Technology.OwnerId);
                    packet.AddUInt32(args.Technology.Type);
                    packet.AddByte(args.Technology.Level);
                    return packet;
                });
        }

        private void TechnologiesTechnologyRemoved(ICity city, TechnologyEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TechRemoved);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(args.Technology.OwnerLocation == EffectLocation.City ? 0 : args.Technology.OwnerId);
                    packet.AddUInt32(args.Technology.Type);
                    packet.AddByte(args.Technology.Level);
                    return packet;
                });
        }

        private void TechnologiesTechnologyCleared(ICity city, TechnologyEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TechCleared);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(args.TechnologyManager.OwnerLocation == EffectLocation.City ? 0 : args.TechnologyManager.OwnerId);
                    return packet;
                });
        }

        private void TechnologiesTechnologyAdded(ICity city, TechnologyEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TechAdded);
                    packet.AddUInt32(city.Id);
                    packet.AddUInt32(args.Technology.OwnerLocation == EffectLocation.City ? 0 : args.Technology.OwnerId);
                    packet.AddUInt32(args.Technology.Type);
                    packet.AddByte(args.Technology.Level);
                    return packet;
                });
        }
        
        private void TroopManagerTroopUpdated(ICity city, TroopStubEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TroopUpdated);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(args.Stub, packet);
                    return packet;
                });
        }

        private void TroopManagerTroopUnitUpdated(ICity city, TroopStubEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            RecalculateUpkeep(city);
        }

        private void TroopManagerTroopAdded(ICity city, TroopStubEventArgs args)
        {
            if (!ShouldUpdate(city))
            {
                return;
            }

            RecalculateUpkeep(city);

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.TroopAdded);
                    packet.AddUInt32(city.Id);
                    PacketHelper.AddToPacket(args.Stub, packet);
                    return packet;
                });
        }

        private bool ShouldUpdate(ICity city)
        {
            return Global.Current.FireEvents && city.Deleted == City.DeletedState.NotDeleted;
        }

        private void NewCityUpdate(ICity city)
        {
            if (city.Owner.Session == null)
            {
                return;
            }

            channel.Post(GetChannelName(city), () =>
                {
                    var packet = new Packet(Command.CityNewUpdate);
                    PacketHelper.AddToPacket(city, packet);
                    return packet;
                });
        }

        private void RecalculateValue(ICity city, IGameObject gameObject)
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

        private void RecalculateUpkeep(ICity city)
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
