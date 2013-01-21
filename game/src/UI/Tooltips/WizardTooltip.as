package src.UI.Tooltips 
{
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.display.Stage;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import flash.utils.Timer;
	import org.aswing.*;
	import org.aswing.event.FrameEvent;
	import org.aswing.event.PopupEvent;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.graphics.Graphics2D;
	import org.aswing.graphics.Pen;
	import org.aswing.graphics.SolidBrush;
	import src.Constants;
	import src.Global;
	import src.UI.Components.ResizeManager;
	import src.UI.GameJFrame;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class WizardTooltip extends Tooltip 
	{
		private var bg: Sprite;
		private var cursor: int;
		private var label: MultilineLabel;
		private var pnlFooter: JPanel;
		private var btnNext: JButton;
		private var btnPrevious: JButton;
		private var btnDone: JButton;		
		private var messages: Array;
		
		public function WizardTooltip(messages: Array) 
		{
			this.messages = messages;
			
			label = new MultilineLabel("", 0, 20);
			bg = new Sprite();
			
			GameLookAndFeel.changeClass(label, "Tooltip.text");
			
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			
			pnlFooter = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, false));
						
			btnNext = new JButton(">");
			btnNext.addActionListener(function(e: Event): void {
				cursor++;
				updateMessage();
			});
			
			btnPrevious = new JButton("<");
			btnPrevious.addActionListener(function(e: Event): void {
				cursor--;
				updateMessage();
			});			
			
			btnDone = new JButton("Ok");
			btnDone.addActionListener(function(e: Event): void {
				hide();
			});
			
			ui.appendAll(label, pnlFooter);
			
			updateMessage();
		}
		
		override protected function mouseInteractive():Boolean 
		{
			return true;
		}
		
		override protected function showFrame(obj: DisplayObject = null):void 
		{
			super.showFrame(obj);
			
			// Set as modal
			var frame: JFrame = ui.getFrame();
			frame.setModal(true);
			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e: PopupEvent): void {
				Global.gameContainer.resizeManager.removeObject(frame);
			});
			
			var g: Graphics2D = new Graphics2D(frame.getModalMC().graphics);
			var bounds: Rectangle = frame.getModalMC().getBounds(frame);
			g.fillRectangle(new SolidBrush(new ASColor(0x000000, 0.6)), bounds.x, bounds.y, bounds.width, bounds.height);			

			Global.gameContainer.resizeManager.addObject(frame.getModalMC(), ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP | ResizeManager.ANCHOR_LEFT | ResizeManager.ANCHOR_BOTTOM);
		}
		
		public function updateMessage(): void {						
			label.setText(messages[cursor]);
			
			// Footer update
			pnlFooter.removeAll();
			
			if (cursor > 0) {
				pnlFooter.append(btnPrevious);
			}
			
			if (cursor == messages.length - 1) {
				pnlFooter.append(btnDone);
			}
			else {
				pnlFooter.append(btnNext);
			}
			
			if (ui && ui.getFrame()) {
				ui.getFrame().pack();
			}
		}
	}

}