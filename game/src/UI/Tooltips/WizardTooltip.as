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
			
			resize();
		}
	}

}