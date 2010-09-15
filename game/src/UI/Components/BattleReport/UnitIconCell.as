package src.UI.Components.BattleReport
{
	import org.aswing.*;
	import org.aswing.table.*;
	import src.Objects.Factories.ObjectFactory;

	public class UnitIconCell extends DefaultTextCell
	{

		public function UnitIconCell()
		{
			setHorizontalTextPosition(AsWingConstants.LEFT);
			setIconTextGap(0);
			setHorizontalAlignment(AsWingConstants.CENTER);
		}

		override public function setCellValue(param1: *) : void
		{
			if (ObjectFactory.getClassType(param1.type) == ObjectFactory.TYPE_UNIT) setText(param1.count);
			else setText("");

			setIcon(new AssetIcon(param1.icon));
		}
	}

}

