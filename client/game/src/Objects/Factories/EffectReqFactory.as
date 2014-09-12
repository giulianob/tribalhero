package src.Objects.Factories {
    import src.Objects.Effects.EffectReqManager;
    import src.Objects.Prototypes.EffectReqPrototype;
    import src.Util.BinaryList.*;

	public class EffectReqFactory {
		
		private static var effectManagers: BinaryList;

        public static function init(data: XML): void {
			effectManagers = new BinaryList(EffectReqManager.sortOnId, EffectReqManager.compareId);
			
			var effectManager: EffectReqManager;
			
			for each (var effectReqNode: XML in data.EffectRequirements.*)
			{				
				effectManager = effectManagers.get(effectReqNode.@id);				
				
				if (effectManager == null)
				{
					if (effectManager)
						effectManager.effectReqs.sort(EffectReqPrototype.sortOnMethod);
					effectManager = new EffectReqManager(effectReqNode.@id);					
					effectManagers.add(effectManager);					
				}
				
				var effectReq: EffectReqPrototype = new EffectReqPrototype();
				
				effectReq.method = effectReqNode.@method;
				effectReq.param1 = effectReqNode.@param1;
				effectReq.param2 = effectReqNode.@param2;
				effectReq.param3 = effectReqNode.@param3;
				effectReq.param4 = effectReqNode.@param4;
				effectReq.param5 = effectReqNode.@param5;
				effectReq.description = effectReqNode.@description;
				
				effectManager.effectReqs.push(effectReq);
			}
			
			if (effectManager)
				effectManager.effectReqs.sort(EffectReqPrototype.sortOnMethod);			
		}
		
		public static function getEffectRequirements(id: int): EffectReqManager
		{
			return effectManagers.get(id);
		}

	}
	
}