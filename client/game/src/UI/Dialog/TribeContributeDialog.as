package src.UI.Dialog
{
    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.City;
    import src.Objects.Effects.Formula;
import src.Objects.Resources;
import src.Objects.StructureObject;
    import src.UI.Components.TradeResourcesPanel;
    import src.UI.GameJPanel;

    public class TribeContributeDialog extends GameJPanel
	{
		private var pnlResource:TradeResourcesPanel;
		private var pnlBottom:JPanel;
		private var btnOk:JButton;

		private var structure: StructureObject;
		private var city: City;
		
		private var onAccept: Function;
		
		public function TribeContributeDialog(parentObj: StructureObject, onAccept: Function):void
		{
			this.onAccept = onAccept;
			this.structure = parentObj;
			this.city = Global.map.cities.get(parentObj.cityId);
			
			createUI();	
			
			title = "Contribute Resources";							
			
			btnOk.addActionListener(sendResources);
		}
		
		public function sendResources(e: Event) : void {						
			if (pnlResource.getResource().total() == 0) {
				InfoDialog.showMessageDialog("Error", "No resources selected to contribute");
				return;
			}		

			var self: TribeContributeDialog = this;
			Global.mapComm.Tribe.contribute(city.id, structure.objectId, pnlResource.getResource(), function(): void {
				if (onAccept != null)
					onAccept(self);
			});
		}	
				
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, function():void{});			

			Global.gameContainer.showFrame(frame);

			return frame;
		}
		
		public function createUI(): void {
			setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.TOP));

            var capacity:int = Formula.contributeCapacity(structure.level);
			pnlResource = new TradeResourcesPanel(structure,new Resources(capacity,capacity,capacity,capacity,capacity));

			pnlBottom = new JPanel();
			pnlBottom.setSize(new IntDimension(200, 10));
			pnlBottom.setLayout(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton("Contribute");

			//component layoution
			append(new JLabel(" ")); //separator
			append(pnlResource);
			append(pnlBottom);

			pnlBottom.append(btnOk);
		}
	}

}
