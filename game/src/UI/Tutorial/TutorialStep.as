package src.UI.Tutorial 
{	
	import fl.lang.Locale;
	import flash.display.DisplayObject;
	import org.aswing.geom.IntPoint;
	import org.aswing.plaf.basic.adjuster.PopupSliderThumbIcon;
	import src.Map.Map;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Tooltips.TextTooltip;
	import src.UI.Tooltips.Tooltip;
	import src.UI.Tooltips.WizardTooltip;
	public class TutorialStep 
	{
		public var resume: Function;
		public var map: Map;
		
		private var messageTooltip: Tooltip = new Tooltip();
		private var messageId: String;
		
		public function execute(): void {
			
		}				
		
		protected function showMessageAtObject(target: DisplayObject, messageId: String): void {
			if (messageId == this.messageId) {
				this.messageTooltip.getUI().getFrame().toFront();
				return;
			}
			
			hideAllMessages();
			
			var text: String = Locale.loadString(messageId);
			this.messageTooltip = new TextTooltip(text == null ? "[" + messageId + "]" : text);
			this.messageTooltip.show(target);		
			
			this.messageId = messageId;			
		}
		
		protected function showWizardAtPosition(position: IntPoint, wizardId: String, messageIds: Array): void {
			wizardId += "_WIZARD";
			
			if (wizardId == this.messageId) {
				return;
			}
			
			hideAllMessages();
			
			var messages: Array = new Array();
			
			for each (var messageId: String in messageIds) {
				var text: String = Locale.loadString("TUTORIAL_" + messageId);
				messages.push(text == null ? "[" + messageId + "]" : text);
			}
			
			var wizardTooltip: WizardTooltip = new WizardTooltip(messages);			
			this.messageTooltip = wizardTooltip;
			this.messageTooltip.showFixed(position);
			this.messageId = wizardId;			
		}
		
		protected function showMessageAtPosition(position: IntPoint, messageId: String): void {
			if (messageId == this.messageId) {
				return;
			}
			
			hideAllMessages();			
			
			var text: String = Locale.loadString(messageId);
			this.messageTooltip = new TextTooltip(text == null ? "[" + messageId + "]" : text);
			this.messageTooltip.showFixed(position);	
			this.messageId = messageId;
		}
		
		public function hideAllMessages(): void {
			if (messageTooltip) {
				messageTooltip.hide();
				messageTooltip = null;
				messageId = "";
			}
		}
		
		protected function complete(): void {
			hideAllMessages();
			resume();
		}		
	}
}