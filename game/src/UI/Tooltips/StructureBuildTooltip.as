package src.UI.Tooltips {

    import org.aswing.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.City;
    import src.Objects.Effects.Formula;
    import src.Objects.Effects.RequirementFormula;
    import src.Objects.Prototypes.EffectReqPrototype;
    import src.Objects.Prototypes.ILayout;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.StructureObject;
    import src.UI.Components.ResourcesPanel;
    import src.UI.Components.StructureStatBox;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.DateUtil;
    import src.Util.StringHelper;

    public class StructureBuildTooltip extends ActionButtonTooltip {

		private var structPrototype: StructurePrototype;
		private var parentObj: StructureObject;

		public var missingRequirements: Array;

		private var pnlHeader:JPanel;
		private var lblTitle:JLabel;
		private var lblTime:JLabel;
		private var lblDescription:MultilineLabel;
		private var lblLvl:JLabel;
		private var lblLvlDescription:MultilineLabel;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var pnlResources:JPanel;
		private var statsBox: StructureStatBox;
		
		private var city: City;

		public function StructureBuildTooltip(parentObj: StructureObject, structPrototype: StructurePrototype)
		{
			this.parentObj = parentObj;
			this.structPrototype = structPrototype;
			this.city = Global.map.cities.get(parentObj.cityId);			
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

			lblTime.setText(DateUtil.formatTime(Formula.buildTime(parentObj, structPrototype.buildTime, parentObj.getCorrespondingCityObj().techManager)));

			if (structPrototype.layouts.length > 0 || (missingRequirements != null && missingRequirements.length > 0))
			{
				pnlRequired.removeAll();
				pnlRequired.setVisible(true);
				pnlRequired.append(lblRequires);

				for each (var lt: ILayout in structPrototype.layouts)
				pnlRequired.append(labelMaker(lt.toString()));

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
			pnlResources.append(new ResourcesPanel(Formula.buildCost(city, structPrototype), city));
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
			lblDescription.setConstraints("South");
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			lblLvl = new JLabel("Level "  + structPrototype.level);
			lblLvl.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "header");

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

			statsBox = new StructureStatBox(structPrototype.type, structPrototype.level);

			//component layoution
			ui.append(pnlHeader);
			ui.append(lblLvl);
			ui.append(lblLvlDescription);
			ui.append(statsBox);
			ui.append(pnlRequired);
			ui.append(pnlFooter);

			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);
			pnlHeader.append(lblDescription);

			pnlFooter.append(pnlResources);
			
			// text values
			lblTitle.setText("Build " + StringHelper.wordsToUpper(structPrototype.getName()));
			lblDescription.setText(structPrototype.getGeneralDescription());
			lblLvlDescription.setText(structPrototype.getDescription());			
		}
	}

}

