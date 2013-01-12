package src.UI.LookAndFeel
{

	import org.aswing.plaf.basic.BasicSplitPaneUI;
	import org.aswing.plaf.basic.splitpane.Divider;

	public class GameSplitPaneUI extends BasicSplitPaneUI{

		override protected function createDivider():Divider{
			return new GameSplitPaneDivider(sp);
		}
	}
}
