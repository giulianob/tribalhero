package src.UI.Components.ScreenMessages
{
    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.AssetPane;
    import org.aswing.FlowLayout;
    import org.aswing.JPanel;
    import org.aswing.ext.MultilineLabel;

    public class ScreenMessageItem extends JPanel
	{
		public var key: String;
		public var duration: int;

		public function ScreenMessageItem(key: String, message: String, icon: AssetIcon = null, duration: int = 0)
		{			
			var label: MultilineLabel = new MultilineLabel(message, 0, 40);			
			setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));
			
			mouseEnabled = false;
			mouseChildren = false;
						
			if (icon != null) {
				var iconPane: AssetPane = new AssetPane(icon.getAsset(), AssetPane.PREFER_SIZE_BOTH);
				iconPane.setHorizontalAlignment(AsWingConstants.CENTER);
				iconPane.setVerticalAlignment(AsWingConstants.CENTER);
				iconPane.setPreferredWidth(16);
				append(iconPane);
			}
			
			append(label);				

			this.key = key;
			this.duration = duration;
		}

	}

}

