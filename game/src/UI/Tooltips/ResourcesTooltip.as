﻿package src.UI.Tooltips
{

	import fl.lang.Locale;
	import flash.events.Event;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Map.City;
	import src.Objects.LazyValue;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.BinaryList.BinaryList;
	import src.Util.BinaryList.BinaryListEvent;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class ResourcesTooltip extends Tooltip
	{

		private var city: City;

		public function ResourcesTooltip(city: City)
		{
			createUI();

			this.city = city;

			ui.addEventListener(Event.ADDED_TO_STAGE, addedToStage);

			ui.addEventListener(Event.REMOVED_FROM_STAGE, removedFromStage);
		}

		private function addedToStage(e: Event) : void
		{
			city.addEventListener(City.RESOURCES_UPDATE, draw);
			city.troops.addEventListener(BinaryListEvent.CHANGED, draw);
			draw();
		}

		private function removedFromStage(e: Event) : void
		{
			city.troops.removeEventListener(BinaryListEvent.CHANGED, draw);
			city.removeEventListener(City.RESOURCES_UPDATE, draw);			
		}

		private function draw(e: Event = null) : void
		{
			ui.removeAll();
						
			var unitCounts: * = city.troops.getUnitTotalsByStatus();

			ui.append(resourceLabelMaker("Gold", city.resources.gold, new AssetIcon(new ICON_GOLD()), false, true));
			ui.append(resourceLabelMaker("Wood", city.resources.wood, new AssetIcon(new ICON_WOOD())));
			ui.append(resourceLabelMaker("Crop", city.resources.crop, new AssetIcon(new ICON_CROP()), true, true));
			ui.append(resourceLabelMaker("Iron", city.resources.iron, new AssetIcon(new ICON_IRON())));
			ui.append(simpleLabelMaker("Laborers", StringUtil.substitute(Locale.loadString("CITY_OVERVIEW_LABORERS_LABEL"), city.resources.labor.getValue().toString(), city.getBusyLaborCount().toString()), false, "", new AssetIcon(new ICON_LABOR())));
			ui.append(simpleLabelMaker("Influence Points", city.value.toString(), false, "", new AssetIcon(new ICON_UPGRADE())));
			ui.append(simpleLabelMaker("Units", StringUtil.substitute(Locale.loadString("CITY_OVERVIEW_UNITS_STATUS_LABEL"), unitCounts.idle, unitCounts.onTheMove), false, "", new AssetIcon(new ICON_SINGLE_SWORD())));
			ui.append(simpleLabelMaker("Upkeep", (city.resources.crop.getHourlyUpkeep() * -1).toString(), true, "crop", new AssetIcon(new ICON_CROP())));			
		}

		private function simpleLabelMaker(name: String, value: String, hourly: Boolean = false, unit: String = "", icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(name + ": " + value + (unit?" "+unit:"") + (hourly?" per hour":""), icon);

			GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");

			label.setIconTextGap(0);			
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);

			return label;
		}

		private function resourceLabelMaker(name: String, resource: LazyValue, icon: Icon = null, includeLimit: Boolean = true, includeRate: Boolean = true) : JLabel {
			var value: int = resource.getValue();
			
			var netRate: int = resource.getHourlyRate() - resource.getHourlyUpkeep();

			var label: JLabel = new JLabel(
				(name != "" ? (name + ": ") : "") + 
				value + 
				(includeLimit ? "/" + resource.getLimit() : "") + 
				(includeRate ? 
					" (" + (netRate > 0 ? "+" : "") + netRate + (resource.getHourlyUpkeep() > 0 ? " net per hour":" per hour") + 
					")" : 
					""
				)
				, icon);

			GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");

			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);

			return label;
		}

		public function createUI(): void
		{
			var layout0:GridLayout = new GridLayout();
			layout0.setRows(4);
			layout0.setColumns(2);
			layout0.setHgap(20);
			layout0.setVgap(10);
			ui.setLayout(layout0);
		}

	}

}
