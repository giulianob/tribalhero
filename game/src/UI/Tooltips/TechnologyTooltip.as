
package src.UI.Tooltips {
	import src.Global;
	import src.Objects.Effects.Formula;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.StructureObject;
	import src.UI.Components.ResourcesPanel;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TechnologyTooltip extends ActionButtonTooltip {

		private var parentObj: StructureObject;

		public var techPrototype: TechnologyPrototype;
		public var nextTechPrototype: TechnologyPrototype;

		public var missingRequirements: Array;

		private var lblTitle:JLabel;
		private var lblLevel:JLabel;
		private var lblDescription:MultilineLabel;
		private var pnlHeader:JPanel;

		private var pnlNextLvl:JPanel;
		private var lblNextLvlTime:JLabel;
		private var lblNextLvlDescription:MultilineLabel;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var pnlResources:JPanel;

		public function TechnologyTooltip(parentObj: StructureObject, techPrototype: TechnologyPrototype, nextTechPrototype: TechnologyPrototype = null) {
			this.parentObj = parentObj;
			this.techPrototype = techPrototype;
			this.nextTechPrototype = nextTechPrototype;
		}

		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			else if (pnlHeader == null) createUI();
			
			lblTitle.setText(techPrototype.getName());

			if (techPrototype.level == 0) {
				lblDescription.setText("You have not trained this technology yet");
			}
			else {
				lblLevel.setText("Level " + techPrototype.level.toString());
				lblDescription.setText(techPrototype.getDescription());
			}

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

			if (nextTechPrototype != null)
			{
				ui.append(pnlNextLvl);

				lblNextLvlDescription.setText(nextTechPrototype.getDescription());
				lblNextLvlTime.setText(Util.formatTime(Formula.buildTime(parentObj, nextTechPrototype.time, parentObj.getCorrespondingCityObj().techManager)));

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

				pnlResources.removeAll();
				pnlResources.append(new ResourcesPanel(nextTechPrototype.resources, Global.map.cities.get(parentObj.cityId)));
			}
			else
			ui.remove(pnlNextLvl);

		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL, 3);
			ui.setLayout(layout0);

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

			lblDescription = new MultilineLabel("", 0, 20);
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			pnlNextLvl = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL));

			pnlHeader = new JPanel();
			pnlHeader.setLayout(new BorderLayout(10, 0));

			var lblNextLvl: JLabel = new JLabel("Next Level");
			lblNextLvl.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblNextLvl, "header");

			lblNextLvlDescription = new MultilineLabel("", 0, 20);	
			GameLookAndFeel.changeClass(lblNextLvlDescription, "Tooltip.text");

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

			//component layoution
			ui.append(lblTitle);

			if (techPrototype.level == 0) {
				ui.append(lblDescription);
			} else {
				ui.append(lblLevel);
				ui.append(lblDescription);
			}

			pnlNextLvl.append(pnlHeader);
			pnlNextLvl.append(lblNextLvlDescription);
			pnlNextLvl.append(pnlRequired);
			pnlNextLvl.append(pnlFooter);

			pnlHeader.append(lblNextLvl);
			pnlHeader.append(lblNextLvlTime);

			pnlFooter.append(pnlResources);
		}
	}
}

