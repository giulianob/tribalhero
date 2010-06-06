/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Tooltips {

	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.StructureObject;
	import src.UI.Components.ResourcesPanel;
	import src.UI.Components.UnitStatBox;
	import src.UI.GameLookAndFeel;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TrainTooltip extends Tooltip {
		private var parentObj: StructureObject;
		private var unitPrototype: UnitPrototype;
		public var missingRequirements: Array;

		private var pnlHeader:JPanel;
		private var lblTitle:JLabel;
		private var lblLevel:JLabel;
		private var lblTime:JLabel;
		private var lblDescription:MultilineLabel;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var lblActionCount:JLabel;
		private var pnlResources:JPanel;
		private var statsBox:UnitStatBox;

		public function TrainTooltip(parentObj: StructureObject, unitPrototype: UnitPrototype)
		{
			this.parentObj = parentObj;
			this.unitPrototype = unitPrototype;

			var city: City = Global.map.cities.get(parentObj.cityId);
			statsBox = UnitStatBox.createFromPrototype(unitPrototype, city);

			createUI();

			lblTitle.setText("Train " + unitPrototype.getName());
			lblLevel.setText("Level " + unitPrototype.level);			
			lblDescription.setText(unitPrototype.getDescription());
		}

		public function draw(count: int, max: int) :void
		{
			lblTime.setText(Util.formatTime(Formula.trainTime(parentObj, unitPrototype.trainTime, null)));
			
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

			lblActionCount.setText(count + "/" + max);

			if (missingRequirements != null && missingRequirements.length > 0)
			{
				pnlRequired.removeAll();
				pnlRequired.setVisible(true);
				pnlRequired.append(lblRequires);

				if (missingRequirements != null)
				{
					for each(var req: EffectReqPrototype in missingRequirements)
					pnlRequired.append(errorLabelMaker(RequirementFormula.getMessage(parentObj, req)));
				}
			}
			else {
				pnlRequired.setVisible(false);
			}

			pnlResources.removeAll();
			pnlResources.append(new ResourcesPanel(unitPrototype.trainResources, Global.map.cities.get(parentObj.cityId)));
		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			ui.setLayout(layout0);

			pnlHeader = new JPanel();
			pnlHeader.setLocation(new IntPoint(5, 5));
			pnlHeader.setSize(new IntDimension(200, 17));
			pnlHeader.setLayout(new BorderLayout(20, 0));

			lblTitle = new JLabel();
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTitle.setConstraints("Center");
			GameLookAndFeel.changeClass(lblTitle, "header");

			lblLevel = new JLabel();
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			lblLevel.setConstraints("South");
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");

			lblTime = new JLabel();
			lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
			lblTime.setIconTextGap(0);
			lblTime.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTime.setConstraints("East");
			GameLookAndFeel.changeClass(lblTime, "Tooltip.text");

			lblTime = new JLabel();
			lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
			lblTime.setIconTextGap(0);
			lblTime.setHorizontalAlignment(AsWingConstants.RIGHT);
			lblTime.setConstraints("East");
			GameLookAndFeel.changeClass(lblTime, "Tooltip.text");

			lblDescription = new MultilineLabel();
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			pnlRequired = new JPanel();
			pnlRequired.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));

			lblRequires = new JLabel();
			lblRequires.setText("Requirements");
			lblRequires.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblRequires, "header");

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(10, 0));

			lblActionCount = new JLabel();
			lblActionCount.setConstraints("West");
			lblActionCount.setText("0/1");
			lblActionCount.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblActionCount, "Tooltip.text");

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);

			//component layoution
			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);
			pnlHeader.append(lblLevel);

			pnlFooter.append(lblActionCount);
			pnlFooter.append(pnlResources);

			ui.append(pnlHeader);
			ui.append(lblDescription);
			ui.append(statsBox);
			ui.append(pnlRequired);
			ui.append(pnlFooter);

		}

	}

}

