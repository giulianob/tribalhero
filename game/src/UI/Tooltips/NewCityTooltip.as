package src.UI.Tooltips {

    import src.Util.DateUtil;
    import src.Util.StringHelper;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.ILayout;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.StructureObject;
	import src.UI.Components.NewCityResourcesPanel;
	import src.UI.Components.ResourcesPanel;
	import src.UI.Components.StructureStatBox;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.StringHelper;
	import src.Util.Util;
	import src.Objects.Effects.Formula;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class NewCityTooltip extends ActionButtonTooltip {

		private var structPrototype: StructurePrototype;

		public var missingRequirements: Array;

		private var pnlHeader:JPanel;
		private var lblTitle:JLabel;
		private var lblTime:JLabel;
		private var lblDescription:MultilineLabel;
		private var pnlFooter:JPanel;		
		private var pnlResources:JPanel;
		
		private var city: City;
		
		public function NewCityTooltip(structPrototype: StructurePrototype)
		{
			this.structPrototype = structPrototype;
			this.city = Global.gameContainer.selectedCity;
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

			lblTime.setText(DateUtil.formatTime(Formula.buildTime(city, structPrototype.buildTime, city.techManager)));

			pnlResources.removeAll();
			
			pnlResources.append(new NewCityResourcesPanel());
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

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(10, 0));

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);

			//component layoution
			ui.append(pnlHeader);
			ui.append(pnlFooter);

			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);
			pnlHeader.append(lblDescription);
			
			pnlFooter.append(pnlResources);
			
			// text values
			lblTitle.setText("Build New City");
			lblDescription.setText(StringHelper.localize("NEW_CITY_DESC"));
		}
	}

}

