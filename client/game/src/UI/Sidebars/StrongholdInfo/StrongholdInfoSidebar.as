package src.UI.Sidebars.StrongholdInfo {
    import flash.display.DisplayObject;
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.Position;
    import src.Objects.*;
    import src.Objects.Factories.StrongholdFactory;
    import src.Objects.Stronghold.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Flows.StoreFlow;
    import src.UI.Sidebars.ObjectInfo.Buttons.*;
    import src.UI.Sidebars.StrongholdInfo.Buttons.*;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StrongholdInfoSidebar extends GameJSidebar
	{
		//UI
		private var pnlStats:Form;
		private var pnlGroups:JPanel;

        private var themeLink: JLabelButton;
        private var themeDropdown: JPopupMenu;

		private var stronghold: Stronghold;

		public function StrongholdInfoSidebar(stronghold: Stronghold)
		{
			this.stronghold = stronghold;

			stronghold.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			createUI();
			update();
		}

		public function onObjectUpdate(e: Event):void
		{
			update();
		}

		public function update():void
		{

			clear();		
			
			addStatRow(t("STR_NAME"), new StrongholdLabel(stronghold.id, Constants.session.tribe.isInTribe(stronghold.tribeId)));
						
			if (stronghold.tribeId == 0) {
				addStatRow(t("STR_TRIBE"), t("STR_NOT_OCCUPIED"));
			} else {
				addStatRow(t("STR_TRIBE"), new TribeLabel(stronghold.tribeId));
			}
			
			addStatRow(t("STR_LEVEL"), stronghold.level.toString());
            addStatRow(t("STR_GATEMAX"), stronghold.gateMax.toString(), null, t("STRONGHOLD_GATE_MAX_HP_TOOLTIP"));

			if (Constants.session.tribe.isInTribe(stronghold.tribeId)) {
				pnlGroups.append(new ViewStrongholdButton(stronghold));
				pnlGroups.append(new SendReinforcementButton(stronghold, new Location(Location.STRONGHOLD, stronghold.id)));

                if (Constants.session.tribe.hasRight(Tribe.SET_THEME)) {
                    themeDropdown = new JPopupMenu();

                    for each (var theme: String in Constants.session.themesPurchased) {
                        var newSprite: String = StrongholdFactory.getSpriteName(theme);
                        if (!FlashAssets.doesSpriteExist(newSprite)) {
                            continue;
                        }

                        themeDropdown.append(createThemeMenuItem(theme));
                    }

                    themeDropdown.append(new JSeparator());

                    themeLink = new JLabelButton("", null, AsWingConstants.LEFT);
                    themeLink.addActionListener(function (e: Event): void {
                        if (!themeDropdown.isVisible()) {
                            themeDropdown.show(themeLink, 0, themeLink.getHeight());
                        }
                        else {
                            themeDropdown.setVisible(false);
                        }
                    });

                    themeLink.setText(t("THEME_" + stronghold.theme));
                    addStatRow(t("OBJECT_INFO_SIDEBAR_THEME_LABEL"), themeLink);

                    var storeLink: JLabelButton = new JLabelButton(t("OBJECT_INFO_SIDEBAR_BUY_MORE_THEMES"), null, AsWingConstants.LEFT);
                    themeDropdown.append(storeLink);

                    storeLink.addActionListener(function (e: Event): void {
                        new StoreFlow().showStore();
                    });
                }

			} else {
				pnlGroups.append(new SendAttackButton(stronghold,new Location(Location.STRONGHOLD, stronghold.id)));
			}


			var buttons: Array = [];

			//Special Case Buttons
			switch(stronghold.state.getStateType())
			{
				case SimpleGameObject.STATE_BATTLE:
					pnlGroups.append(new ViewBattleButton(stronghold));
				break;
			}		
		}

        private function createThemeMenuItem(theme: String): JMenuItem {
            var sprite: DisplayObject = StrongholdFactory.getSprite(theme);
            Util.resizeSprite(sprite, 85, 85);

            var menuItem: JMenuItem = new JMenuItem(t("THEME_" + theme), new AssetIcon(sprite));

            var capturedTheme: String = theme;
            menuItem.addActionListener(function(e: Event): void {
                Global.mapComm.Stronghold.setTheme(stronghold.id, capturedTheme);
            });

            return menuItem
        }

        private function addStatRow(title: String, textOrComponent: *, icon: Icon = null, tooltip: String = null) : Component {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: Component;
			if (textOrComponent is String) {
				var label: JLabel = new JLabel(textOrComponent as String);
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				label.setHorizontalTextPosition(AsWingConstants.LEFT);
				label.setName("value");
				label.setIcon(icon);
				rowValue = label;
			} 
			else {
				rowValue = textOrComponent as Component;
            }

            if (tooltip) {
                new SimpleTooltip(rowTitle, tooltip);
            }

			pnlStats.addRow(rowTitle, rowValue);

			return rowValue;
		}

		private function clear():void
		{
			pnlGroups.removeAll();
			pnlStats.removeAll();
		}

		public function dispose():void
		{
			stronghold.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			pnlGroups.setBorder(new EmptyBorder(null, new Insets(0, 0, 20, 0)));


			//component layoution
			append(pnlStats);
			append(pnlGroups);			
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Position = stronghold.primaryPosition.toPosition();
			frame.getTitleBar().setText("Stronghold (" + pt.x + "," + pt.y + ")");

			frame.show();
			return frame;
		}
	}

}

