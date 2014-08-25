package src.UI.Tooltips {

    import org.aswing.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Prototypes.*;
    import src.UI.Components.*;
    import src.UI.LookAndFeel.*;
    import src.Util.*;

    public class TrainTooltip extends ActionButtonTooltip {
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
		private var pnlResources:JPanel;
		private var statsBox:UnitStatBox;

		public function TrainTooltip(parentObj: StructureObject, unitPrototype: UnitPrototype)
		{
			this.parentObj = parentObj;
			this.unitPrototype = unitPrototype;
		}

		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			else if (pnlHeader == null) createUI();
			
			var structure: StructureObject = parentObj as StructureObject;
			var city: City = Global.map.cities.get(structure.cityId);
			var trainTime: int = Formula.trainTime(structure.level, 1, unitPrototype);
						
			lblTime.setText(DateUtil.formatTime(trainTime));
			
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
			pnlResources.append(new ResourcesPanel(Formula.unitTrainCost(city, unitPrototype), city));
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
			lblTime.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_CLOCK")));
			lblTime.setIconTextGap(0);
			lblTime.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTime.setConstraints("East");
			GameLookAndFeel.changeClass(lblTime, "Tooltip.text");

			lblTime = new JLabel();
			lblTime.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_CLOCK")));
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

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);

			var city: City = Global.map.cities.get(parentObj.cityId);
			statsBox = UnitStatBox.createFromPrototype(unitPrototype, city);			
			
			//component layoution
			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);
			pnlHeader.append(lblLevel);

			pnlFooter.append(pnlResources);

			ui.append(pnlHeader);
			ui.append(lblDescription);
			ui.append(statsBox);
			ui.append(pnlRequired);
			ui.append(pnlFooter);

			lblTitle.setText("Train " + unitPrototype.getName());
			lblLevel.setText("Level " + unitPrototype.level);			
			lblDescription.setText(unitPrototype.getDescription());			
		}

	}

}

