package src.UI.Tooltips 
{
    import flash.events.Event;
    import flash.events.MouseEvent;

    import org.aswing.AsWingConstants;
    import org.aswing.Component;
    import org.aswing.Icon;
    import org.aswing.JLabel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.ext.MultilineLabel;

    import src.Objects.Actions.ActionButton;
    import src.Objects.Effects.RequirementFormula;
    import src.Objects.GameObject;
    import src.Objects.Prototypes.EffectReqPrototype;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class SimpleRequirementTooltip extends ActionButtonTooltip
	{
		private var button: ActionButton;
		private var requirements: Array;
		private var text: String;
		
		public function SimpleRequirementTooltip(button: ActionButton, tooltip: String = "", missingRequirements: Array = null)
		{		
			this.text = tooltip;
			this.button = button;
			this.requirements = missingRequirements;
			button.addEventListener(MouseEvent.MOUSE_MOVE, onRollOver);
			button.addEventListener(MouseEvent.ROLL_OUT, onRollOut);
			button.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		public function setRequirements(missingRequirements: Array): void {
			this.requirements = missingRequirements;			
		}
		
		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			
			ui.removeAll();
			ui.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
				
			var messageLabel: Component;
			if (text.length < 40) {
				messageLabel = new JLabel(text);
			} else {
				messageLabel = new MultilineLabel(text, 0, 20);
			}
			GameLookAndFeel.changeClass(messageLabel, "Tooltip.text");			
			ui.append(messageLabel);			
			
			
			var errorLabelMaker: Function = function(text: String, icon: Icon = null) : JLabel {
				var label: JLabel = new JLabel(text, icon);
				GameLookAndFeel.changeClass(label, "Label.error");
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				return label;
			};						
			
			if (requirements) {
				for each(var req: EffectReqPrototype in requirements) {
					ui.append(errorLabelMaker(RequirementFormula.getMessage(button.parentObj as GameObject, req)));
				}
			}
		}
		
		private function parentHidden(e: Event): void {
			onRollOut(e);
			button.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		public function setText(tooltip: String) : void {
			this.hide();
			this.text = tooltip;			
		}
		
		private function onRollOver(e: Event):void {
			show(button);
		}
		
		private function onRollOut(e: Event):void {
			hide();
		}
	}
	
}