
package src.UI.Tooltips {

	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.ILayout;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.StructureObject;
	import src.UI.Components.ResourcesPanel;
	import src.UI.Components.StructureStatBox;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.Util;
	import src.Objects.Effects.Formula;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class StructureChangeTooltip extends ActionButtonTooltip {

		private var nextStructPrototype: StructurePrototype;
		private var parentObj: StructureObject;

		public var missingRequirements: Array;

		private var pnlHeader:JPanel;
		private var lblTitle:JLabel;
		private var lblTime:JLabel;
		private var lblDescription:MultilineLabel;
		private var lblLvlDescription:MultilineLabel;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var pnlResources:JPanel;

		public function StructureChangeTooltip(parentObj: StructureObject, nextStructPrototype: StructurePrototype)
		{
			this.parentObj = parentObj;
			this.nextStructPrototype = nextStructPrototype;
		}

		override public function draw() :void
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

			lblTitle.setText("Convert to " + nextStructPrototype.getName());

			lblTime.setText(Util.formatTime(Formula.buildTime(parentObj, nextStructPrototype.buildTime, parentObj.getCorrespondingCityObj().techManager)));

			lblDescription.setText(nextStructPrototype.getGeneralDescription());

			lblLvlDescription.setText(nextStructPrototype.getDescription());

			if (nextStructPrototype.layouts.length > 0 || (missingRequirements != null && missingRequirements.length > 0))
			{
				pnlRequired.removeAll();
				pnlRequired.setVisible(true);
				pnlRequired.append(lblRequires);

				for each (var lt: ILayout in nextStructPrototype.layouts)
				pnlRequired.append(labelMaker(lt.toString()));

				if (missingRequirements != null)
				{
					for each(var req: EffectReqPrototype in missingRequirements)
					pnlRequired.append(errorLabelMaker(RequirementFormula.getMessage(parentObj, req)));
				}
			}
			else
			pnlRequired.setVisible(false);

			var city: City = Global.map.cities.get(parentObj.cityId);

			pnlResources.removeAll();
			pnlResources.append(new ResourcesPanel(Formula.buildCost(city, nextStructPrototype), city));
		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(3);
			ui.setLayout(layout0);

			pnlHeader = new JPanel();
			pnlHeader.setLocation(new IntPoint(5, 5));
			pnlHeader.setSize(new IntDimension(200, 17));
			pnlHeader.setLayout(new BorderLayout(10, 0));

			lblTitle = new JLabel();
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTitle.setConstraints("Center");
			GameLookAndFeel.changeClass(lblTitle, "header");

			lblTime = new JLabel();
			lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
			lblTime.setIconTextGap(0);
			lblTime.setHorizontalAlignment(AsWingConstants.RIGHT);
			lblTime.setConstraints("East");
			GameLookAndFeel.changeClass(lblTime, "Tooltip.text");

			lblDescription = new MultilineLabel();
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			lblLvlDescription = new MultilineLabel();
			GameLookAndFeel.changeClass(lblLvlDescription, "Tooltip.text");

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

			var statsBox: StructureStatBox = new StructureStatBox(nextStructPrototype.type, nextStructPrototype.level);

			//component layoution
			ui.append(pnlHeader);
			ui.append(lblDescription);
			ui.append(lblLvlDescription);
			ui.append(statsBox);
			ui.append(pnlRequired);
			ui.append(pnlFooter);

			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);

			pnlFooter.append(pnlResources);
		}

	}

}

