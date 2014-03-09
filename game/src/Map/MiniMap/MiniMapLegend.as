package src.Map.MiniMap
{
import org.aswing.Insets;
import org.aswing.JPanel;
import org.aswing.SoftBoxLayout;
import org.aswing.border.EmptyBorder;

import src.UI.GameJBoxBackground;
import src.UI.GameJFrame;

public class MiniMapLegend
	{
		public static const LEGEND_WIDTH :int = 140;
		private var ui: JPanel = new JPanel();
		private var legendPanel: JPanel = new JPanel();
        private var frame: GameJFrame;

		public function MiniMapLegend()
		{
			ui.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			ui.setPreferredWidth( -1);
			
			legendPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			ui.append(legendPanel);
		}

        public function show(x: int, y: int) : void
		{
            frame = new GameJFrame(null, "", false);

            frame.setContentPane(ui);

            frame.setBackgroundDecorator(null);
            frame.setTitleBar(null);
            frame.setDragable(false);
            frame.setClosable(false);
            frame.setResizable(false);
            frame.show();
			frame.pack();
            align(x, y);
		}
        
        public function align(x: int, y: int): void {
            if (!frame) { return; }
            frame.setLocationXY(x, y);
            frame.repaintAndRevalidate();
        }
		
		public function hide() : void {
			if (frame) {
                frame.dispose();
			}
		}
		
		public function removeAll(): void {
			legendPanel.removeAll();

        }
        public function addPanel(pnl: JPanel) : void
        {
            pnl.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            pnl.setBorder(new EmptyBorder(null, new Insets(5, 5, 5, 5)));

            var outerPnl: JPanel = new JPanel();
            outerPnl.setBackgroundDecorator(new GameJBoxBackground());
            outerPnl.setOpaque(true);
            outerPnl.append(pnl);

            legendPanel.append(outerPnl);
        }

	}

}