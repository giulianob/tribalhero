package src.UI.Dialog {
	import flash.display.MovieClip;
	import flash.display.Sprite;
	import flash.events.Event;
	import src.Constants;
	import src.GameContainer;
	import src.Global;
	import src.Map.*;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.*;
	import src.Objects.Prototypes.UnitPrototype;	
	import src.UI.Components.TroopGridList.TroopGridDragHandler;
	import src.UI.Components.TroopGridList.TroopGridList;
	import src.UI.GameJPanel;	
	import flash.display.DisplayObject;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;


	public class UnitMoveDialog extends GameJPanel {
		
		private var pnlFormations:JPanel;
		private var pnlBottom:JPanel;
		private var btnOk:JButton;
	
		private var city: City;		
		private var tilelists: Array = new Array();
		
		public function UnitMoveDialog(city: City, onAccept: Function)
		{
			createUI();
			title = "Assign Units";
			
			var self: UnitMoveDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );
			
			this.city = city;					
			
			tilelists = new Array();
			
			var troop: Troop = city.troops.getDefaultTroop();
						
			drawTroop(troop, [Formation.Normal, Formation.Garrison]);
		}
		
		public function drawTroop(troop: Troop, formations: Array = null):void
		{												
			tilelists = TroopGridList.getGridList(troop, city.template, formations);
			
			pnlFormations.append(TroopGridList.stackGridLists(tilelists));
						
			var tileListDragDropHandler: TroopGridDragHandler = new TroopGridDragHandler(tilelists);
		}
		
		public function getTroop(): Troop
		{
			var newTroop: Troop = new Troop();
						
			for (var i: int = 0; i < tilelists.length; i++)
			{
				newTroop.add((tilelists[i] as TroopGridList).getFormation());
			}
			
			return newTroop;
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			
			Global.gameContainer.showFrame(frame);
			
			return frame;
		}
		
		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			setLayout(layout0);
			
			pnlFormations = new JPanel();
			pnlFormations.setSize(new IntDimension(400, 10));
			
			pnlBottom = new JPanel();
			pnlBottom.setLocation(new IntPoint(5, 5));
			pnlBottom.setSize(new IntDimension(10, 10));
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			pnlBottom.setLayout(layout1);
			
			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(184, 5));
			btnOk.setSize(new IntDimension(31, 22));
			btnOk.setText("Ok");
			
			//component layoution
			append(pnlFormations);
			append(pnlBottom);
			
			pnlBottom.append(btnOk);
				
		}
	}
	
}