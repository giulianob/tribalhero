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
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.StructureObject;
	import src.UI.Tooltips.TechnologyTooltip;

	public class TechnologyButton extends ActionButton {
		private var structPrototype: StructurePrototype;
		private var techPrototype: TechnologyPrototype;

		private var techToolTip: TechnologyTooltip;

		public function TechnologyButton(parentObj: StructureObject, structPrototype: StructurePrototype, techPrototype: TechnologyPrototype )
		{
			super(parentObj, techPrototype.getName());

			this.techPrototype = techPrototype;
			this.structPrototype = structPrototype;

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
				Global.mapComm.City.technologyUpgrade(parentObj.cityId, parentObj.objectId, techPrototype.techtype);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);
			if (city == null) {
				trace("TechnologyButton.validateButton: Unknown city");
				disable();
				return false;
			}

			if (!techPrototype)
			{
				trace("TechnologyButton.validateButton: Missing techprototype. Enabling tech button by default.");
				enable();
				return true;
			}

			techToolTip.techPrototype = techPrototype;

			if (parentAction == null)
			{
				trace("TechnologyButton.validateButton: missing parent action");
				techToolTip.draw(currentCount, 999);
				disable();
				return false;
			}

			var techUpgradeAction: TechUpgradeAction = parentAction as TechUpgradeAction;

			if (techPrototype.level >= techUpgradeAction.maxlevel)
			{
				techToolTip.draw(currentCount, parentAction.maxCount);
				disable();
				return false;
			}

			var nextTechPrototype: TechnologyPrototype = TechnologyFactory.getPrototype(techPrototype.techtype, techPrototype.level + 1);
			techToolTip.nextTechPrototype = nextTechPrototype;

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				trace("TechnologyButton.validateButton: Unknown city object");
				disable();
				return false;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);

			techToolTip.missingRequirements = missingReqs;
			techToolTip.draw(currentCount, parentAction.maxCount);

			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}

			if (Global.map.cities.get(parentObj.cityId).resources.GreaterThanOrEqual(nextTechPrototype.resources))
			{
				enable();
				return true;
			}
			else
			{
				disable();
				return false;
			}
		}
	}

}

