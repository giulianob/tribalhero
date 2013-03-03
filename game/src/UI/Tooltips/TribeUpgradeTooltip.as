
package src.UI.Tooltips {

	import src.Util.StringHelper;
	import mx.events.ResourceEvent;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Resources;
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

	public class TribeUpgradeTooltip extends ActionButtonTooltip {
		private var level: int;
		private var hasNextLevel: Boolean;
		
		private var lblTitle:JLabel;
		private var lblLevel:JLabel;
		private var lblDescription:MultilineLabel;
		private var lblNextLevelDescription:MultilineLabel;		
		
		private var pnlNextLvl:JPanel;
		private var pnlFooter:JPanel;
		private var pnlResources:JPanel;
		
		private var cost: Resources;

		public function TribeUpgradeTooltip(level: int, cost: Resources)
		{
			this.level = level;
			this.cost = cost;
			var nextLvl: String = getDescription(level + 1);;
			this.hasNextLevel = nextLvl && nextLvl != "";
		}
		
		private function getDescription(level: int): String {
			return StringHelper.localize("TRIBE_LVL_" + level);
		}

		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			else if (pnlFooter == null) createUI();

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

			if (hasNextLevel)
			{
				ui.append(pnlNextLvl);

				pnlResources.removeAll();
				pnlResources.append(new ResourcesPanel(Formula.getTribeUpgradeCost(level), cost, true, false));
			}
			else {
				ui.remove(pnlNextLvl);
			}
			
			
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

			lblDescription = new MultilineLabel("", 0, 30);
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");
			
			lblNextLevelDescription = new MultilineLabel("", 0, 30);
			GameLookAndFeel.changeClass(lblNextLevelDescription, "Tooltip.text");			

			pnlNextLvl = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL));

			var lblNextLvl: JLabel = new JLabel("Next Level");
			lblNextLvl.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblNextLvl, "header");

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(10, 0));

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);
		
			//component layoution
			pnlTitleLvl.appendAll(lblTitle, lblLevel);

			ui.append(pnlTitleLvl);
			ui.append(lblDescription);

			if (hasNextLevel) {				
				
				lblNextLevelDescription.setText(getDescription(level + 1));
				
				ui.append(new JPanel());
				pnlNextLvl.append(lblNextLvl);				
				pnlNextLvl.append(lblNextLevelDescription);
				pnlNextLvl.append(new JPanel());
				pnlNextLvl.append(pnlFooter);

				pnlFooter.append(pnlResources);
			}
			
			lblTitle.setText("Upgrade Tribe");
			lblLevel.setText("Level " + level.toString());
			lblDescription.setText(getDescription(level));					
		}	

	}

}

