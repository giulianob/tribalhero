package src.Objects.Factories {
    import src.Map.Map;
    import src.Objects.Actions.*;
    import src.Objects.Effects.EffectReqManager;
    import src.Objects.Prototypes.EffectPrototype;
    import src.Objects.Prototypes.Worker;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    /**
	 * ...
	 * @author Default
	 */
	public class WorkerFactory {

		private static var map: Map;
		private static var workers: BinaryList;

		public static function init(_map: Map, data: XML):void
		{
			map = _map;

			workers = new BinaryList(Worker.sortOnType, Worker.compare);

			for each (var workerNode: XML in data.Workers.*)
			{
				var worker: Worker = new Worker();

				worker.type = workerNode.@type;

				for each (var actionNode: XML in workerNode.*)
				{
					var effectReqId: int = actionNode.@effectreq;
					var effectReq: EffectReqManager = null;
					var effectReqInherit: int = EffectPrototype.INHERIT_UPWARD;
					var options: int = actionNode.@option;

					var action: Action = null;

					if (effectReqId.toString() != '' && effectReqId > 0)
					{
						effectReq = EffectReqFactory.getEffectRequirements(effectReqId);
					}

					if (actionNode.@effectreqinherit.toString() != '')
					{
						switch(actionNode.@effectreqinherit.toLowerCase())
						{
							case "upward":
								effectReqInherit = EffectPrototype.INHERIT_UPWARD;
							break;
							case "invisible":
								effectReqInherit = EffectPrototype.INHERIT_INVISIBLE;
							break;
							case "self":
								effectReqInherit = EffectPrototype.INHERIT_SELF;
							break;
							default:
							break;
						}
					}

					switch(String(actionNode.name()))
					{
						case "RoadBuild":
							action = new BuildRoadAction();
						break;
						case "RoadDestroy":
							action = new DestroyRoadAction();
						break;
						case "ForestCampBuild":
							action = new ForestCampBuildAction(actionNode.@type);
						break;
						case "StructureBuild":
							action = new BuildAction(actionNode.@type, actionNode.@tilerequirement, actionNode.@level);
						break;
						case "StructureChange":
							action = new StructureChangeAction(actionNode.@type, actionNode.@level);
						break;
						case "StructureDowngrade":
							action = new StructureUserDowngradeAction();
						break;
						case "TechnologyUpgrade":
							action = new TechUpgradeAction(actionNode.@type, actionNode.@maxlevel);
							worker.addTechUpgradeAction(action as TechUpgradeAction);
						break;
						case "StructureUpgrade":
							action = new StructureUpgradeAction();
						break;
						case "TrainUnit":
							action = new TrainAction(actionNode.@type);
						break;
						case "UnitUpgrade":
							action = new UnitUpgradeAction(actionNode.@type, actionNode.@maxlevel);
						break;
						case "ResourceSend":
							action = new SendResourcesAction();
						break;
						case "ResourceBuy":
							action = new MarketAction("buy");
						break;
						case "ResourceSell":
							action = new MarketAction("sell");
						break;
						case "LaborMove":
							action = new LaborMoveAction();
						break;
						case "ForestCampRemove":
							action = new ForestCampRemoveAction();
						break;
						case "StructureSelfDestroy":
							action = new StructureSelfDestroyAction();
						break;
						case "ResourceGather":
							action = new ResourceGatherAction();
						break;
						case "TribeContribute":
							action = new TribeContributeAction();
						break;
						default:
							Util.log("Unknown action '" + actionNode.name() + "' in worker '" + worker.type.toString() + "'");
							action = new DefaultAction(actionNode.@command);
					}

					if (action != null)
					{
						action.options = options;
						action.effectReq = effectReq;
						action.effectReqInherit = effectReqInherit;
						action.index = actionNode.@index;
						worker.addAction(action as IAction);
					}
				}

				workers.add(worker);
			}

			workers.sort();
		}

		public static function getPrototype(type: int): Worker
		{
			return workers.get(type);
		}
	}

}

