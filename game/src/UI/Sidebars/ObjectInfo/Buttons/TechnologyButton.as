/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Sidebars.ObjectInfo.Buttons {
	import src.Global;
	import src.Map.CityObject;
	import src.Objects.Actions.ActionButton;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Map.City;
	import src.Objects.Actions.TechUpgradeAction;
	import src.Objects.Factories.TechnologyFactory;
	import src.Objects.Prototypes.*;
	import src.Util.Util;
	import src.Objects.StructureObject;
	import src.UI.Tooltips.TechnologyTooltip;

	public class TechnologyButton extends ActionButton {
		private var techPrototype: TechnologyPrototype;

		private var techToolTip: TechnologyTooltip;

        public function TechnologyButton(parentObj:StructureObject, techPrototype: TechnologyPrototype) {
			super(parentObj, techPrototype.getName());

			this.techPrototype = techPrototype;

			techToolTip = new TechnologyTooltip(parentObj, techPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			techToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			techToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				Global.mapComm.City.technologyUpgrade(parentObj.groupId, parentObj.objectId, techPrototype.techtype);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			if (city == null) {
				Util.log("TechnologyButton.validateButton: Unknown city");
				return false;
			}

			if (!techPrototype)
			{
				Util.log("TechnologyButton.validateButton: Missing techprototype. Enabling tech button by default.");
				enable();
				return true;
			}

			techToolTip.techPrototype = techPrototype;

			if (parentAction == null)
			{
				Util.log("TechnologyButton.validateButton: missing parent action");
				techToolTip.draw();
				return false;
			}

			var techUpgradeAction: TechUpgradeAction = parentAction as TechUpgradeAction;

			if (techUpgradeAction == null || techPrototype.level >= techUpgradeAction.maxlevel)
			{
				techToolTip.draw();
				return false;
			}

			var nextTechPrototype: TechnologyPrototype = TechnologyFactory.getPrototype(techPrototype.techtype, techPrototype.level + 1);
			techToolTip.nextTechPrototype = nextTechPrototype;

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("TechnologyButton.validateButton: Unknown city object");
				return false;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.getMissingRequirements(parentObj, effects);

			techToolTip.missingRequirements = missingReqs;
			techToolTip.draw();

			if (missingReqs != null && missingReqs.length > 0)
			{
				return false;
			}

			return Global.map.cities.get(parentObj.groupId).resources.GreaterThanOrEqual(nextTechPrototype.resources);
		}
	}

}

