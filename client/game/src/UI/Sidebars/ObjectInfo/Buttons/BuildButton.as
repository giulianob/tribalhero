
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.MouseEvent;

    import src.FeathersUI.Controls.ActionButton;
    import src.Global;
    import src.Map.City;
    import src.Map.CityObject;
    import src.Objects.*;
    import src.Objects.Actions.BuildAction;
    import src.Objects.Effects.Formula;
    import src.Objects.Prototypes.*;
    import src.UI.Cursors.*;
    import src.UI.Tooltips.StructureBuildTooltip;
    import src.Util.StringHelper;
    import src.Util.Util;

    import starling.events.Event;

    public class BuildButton extends ActionButton
	{
		private var structPrototype: StructurePrototype;
		private var buildToolTip: StructureBuildTooltip;

		public function BuildButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype)
		{
			super(parentObj, structPrototype.getName());

			this.structPrototype = structPrototype;

			buildToolTip = new StructureBuildTooltip(parentObj as StructureObject, structPrototype);

			addEventListener(Event.TRIGGERED, onTriggered);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			buildToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			buildToolTip.hide();
		}

		public function onTriggered(event: Event):void
		{
			if (isEnabled)
			{
                var city: City = Global.map.cities.get(parentObj.groupId);

                var buildAction: BuildAction = parentAction as BuildAction;
				new BuildStructureCursor(city.defaultTheme, buildAction.type, buildAction.level, buildAction.tilerequirement, parentObj);
			}
		}

		override public function validateButton(): Boolean
		{
			if (structPrototype == null)
			{
				enable();
				return true;
			}

			var city: City = Global.map.cities.get(parentObj.groupId);

			if (city == null)
			{
				return false;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("StructureChangeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.getMissingRequirements(parentObj, effects);

            if (!Formula.cityMaxConcurrentBuildActions(structPrototype.type, city)) {
                missingReqs.push(EffectReqPrototype.asMessage(StringHelper.localize("CONCURRENT_UPGRADE_" + Formula.concurrentBuildUpgrades(city.MainBuilding.level))));
            }

            buildToolTip.missingRequirements = missingReqs;
			buildToolTip.draw();

			if (missingReqs != null && missingReqs.length > 0)
			{
				return false;
			}

			return city.resources.GreaterThanOrEqual(Formula.buildCost(city, structPrototype));
		}
	}
}