package src.UI.Tooltips {
	
	import src.Global;
	import src.Objects.Effects.RequirementFormula;
	import src.Objects.Factories.EffectReqFactory;
	import src.Objects.GameObject;
	import src.Objects.IDisposable;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.ILayout;
	import src.Objects.Prototypes.StructurePrototype;
	import flash.display.MovieClip;
	import src.Objects.StructureObject;
	import src.UI.Components.ResourcesPanel;
	import src.UI.GameLookAndFeel;
	import src.Util.Util;
	import src.Objects.Effects.Formula;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;	
	
	public class BuildTooltip extends Tooltip {
		
		private var structPrototype: StructurePrototype;
		private var parentObj: StructureObject;
		
		public var missingRequirements: Array;
		
		private var pnlHeader:JPanel;
		private var lblTitle:JLabel;
		private var lblTime:JLabel;
		private var lblDescription:JLabel;
		private var pnlRequired:JPanel;
		private var lblRequires:JLabel;
		private var pnlFooter:JPanel;
		private var lblActionCount:JLabel;
		private var pnlResources:JPanel;
		
		public function BuildTooltip(parentObj: StructureObject, structPrototype: StructurePrototype) 
		{
			this.parentObj = parentObj;
			this.structPrototype = structPrototype;
			
			createUI();
		}
		
		public function draw(count: int, max: int) :void
		{						
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
				
			lblTitle.setText("Build " + structPrototype.getName());			
			
			lblTime.setText(Util.formatTime(Formula.buildTime(structPrototype.buildTime, null)));			
			
			lblDescription.setText(structPrototype.getDescription());						
			
			lblActionCount.setText(count + "/" + max);			
					
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
			else
				pnlRequired.setVisible(false);
			
			pnlResources.removeAll();
			pnlResources.append(new ResourcesPanel(structPrototype.buildResources, Global.map.cities.get(parentObj.cityId)));
			
			adjustPosition();
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
			
			lblDescription = new JLabel();
			lblDescription.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");
			
			pnlRequired = new JPanel();
			pnlRequired.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			
			lblRequires = new JLabel();
			lblRequires.setText("Requires");
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
			ui.append(pnlHeader);
			ui.append(lblDescription);
			ui.append(pnlRequired);
			ui.append(pnlFooter);
			
			pnlHeader.append(lblTitle);
			pnlHeader.append(lblTime);					
			
			pnlFooter.append(lblActionCount);
			pnlFooter.append(pnlResources);
		}
	}
	
}
