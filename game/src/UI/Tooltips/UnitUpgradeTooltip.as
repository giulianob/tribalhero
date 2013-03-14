
package src.UI.Tooltips {

	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.StructureObject;
	import src.UI.Components.ResourcesPanel;
	import src.UI.Components.UnitStatBox;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.Util;
	import src.Objects.Effects.Formula;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class UnitUpgradeTooltip extends ActionButtonTooltip {
		private var parentObj: StructureObject;
		private var unitPrototype: UnitPrototype;
		private var nextUnitPrototype: UnitPrototype;

		public var missingRequirements: Array;

		private var lblTitle:JLabel;
		private var lblLevel:JLabel;
		private var lblDescription:MultilineLabel;
		private var pnlHeader:JPanel;
		private var statsBox: UnitStatBox;

		private var pnlNextLvl:JPanel;
		private var lblNextLvlTime:JLabel;
		private var nextStatsBox: UnitStatBox;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var pnlResources:JPanel;

		public function UnitUpgradeTooltip(parentObj: StructureObject, unitPrototype: UnitPrototype, nextUnitPrototype: UnitPrototype)
		{

			this.parentObj = parentObj;
			this.unitPrototype = unitPrototype;
			this.nextUnitPrototype = nextUnitPrototype;
		}

		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			else if (pnlHeader == null) createUI();

			var labelMaker: Function = function(text: String, icon: Icon = null) : JLabel {
				var label: JLabel = new JLabel(text, icon);
				GameLookAndFeel.changeClass(label, "Tooltip.text");
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				return label;
			};

			var errorLabelMaker: Function = function(text: String, icon: Icon = null) : JLabel {
				var label: JLabel = new JLabel(text, icon);
				GameLookAndFeel.changeClass(label, "Label.error");
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				return label;
			};

			if (nextUnitPrototype != null)
			{
				ui.append(pnlNextLvl);

				lblNextLvlTime.setText(Util.formatTime(Formula.buildTime(parentObj, nextUnitPrototype.upgradeTime, parentObj.getCorrespondingCityObj().techManager)));

				if (missingRequirements != null && missingRequirements.length > 0)
				{
					pnlRequired.removeAll();
					pnlRequired.setVisible(true);

					pnlRequired.append(lblRequires);

					for each(var req: EffectReqPrototype in missingRequirements)
					pnlRequired.append(errorLabelMaker(RequirementFormula.getMessage(parentObj, req)));
				}
				else
				pnlRequired.setVisible(false);

				var city: City = Global.map.cities.get(parentObj.cityId);
				pnlResources.removeAll();
				pnlResources.append(new ResourcesPanel(Formula.unitUpgradeCost(city, nextUnitPrototype), city));
			}
			else
			ui.remove(pnlNextLvl);
		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL, 7);
			ui.setLayout(layout0);

			var pnlTitleLvl: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 0));
			lblTitle = new JLabel();
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblTitle, "header");

			lblLevel = new JLabel();
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");

			lblNextLvlTime = new JLabel();
			lblNextLvlTime.setIcon(new AssetIcon(new ICON_CLOCK()));
			lblNextLvlTime.setIconTextGap(0);
			lblNextLvlTime.setHorizontalAlignment(AsWingConstants.RIGHT);
			lblNextLvlTime.setConstraints("East");
			GameLookAndFeel.changeClass(lblNextLvlTime, "Tooltip.text");

			lblDescription = new MultilineLabel();
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			pnlNextLvl = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL));

			pnlHeader = new JPanel();
			pnlHeader.setLayout(new BorderLayout(10, 0));

			var lblNextLvl: JLabel = new JLabel("Next Level");
			lblNextLvl.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblNextLvl, "header");

			pnlRequired = new JPanel();
			pnlRequired.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));

			lblRequires = new JLabel();
			lblRequires.setText("Requirements");
			lblRequires.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblRequires, "header");

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(10, 0));

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);

			var city: City = Global.map.cities.get(parentObj.cityId);

			statsBox = UnitStatBox.createFromPrototype(unitPrototype, city);		
			
			//component layoution
			pnlTitleLvl.appendAll(lblTitle, lblLevel);

			ui.append(pnlTitleLvl);
			ui.append(lblDescription);
			ui.append(statsBox);

			if (nextUnitPrototype != null) {
				nextStatsBox = UnitStatBox.createFromPrototype(nextUnitPrototype, city);
				
				ui.append(new JPanel());
				pnlNextLvl.append(pnlHeader);
				pnlNextLvl.append(nextStatsBox);
				pnlNextLvl.append(pnlRequired);
				pnlNextLvl.append(new JPanel());
				pnlNextLvl.append(pnlFooter);

				pnlHeader.append(lblNextLvl);
				pnlHeader.append(lblNextLvlTime);

				pnlFooter.append(pnlResources);
			}
			
			lblTitle.setText("Upgrade " + unitPrototype.getName());
			lblLevel.setText("Level " + unitPrototype.level.toString());
			lblDescription.setText(unitPrototype.getDescription());		
		}	

	}

}

