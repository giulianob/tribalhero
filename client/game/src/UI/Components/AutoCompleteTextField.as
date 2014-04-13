package src.UI.Components 
{
    import flash.events.Event;
    import flash.events.KeyboardEvent;
    import flash.events.TimerEvent;
    import flash.ui.Keyboard;
    import flash.utils.Timer;

    import org.aswing.AsWingManager;
    import org.aswing.JMenuItem;
    import org.aswing.JPopupMenu;
    import org.aswing.JTextField;
    import org.aswing.event.AWEvent;

    public class AutoCompleteTextField extends JTextField
	{		
		private var menuDropdown: JPopupMenu = new JPopupMenu();	
		private var previousText: String = "";
		private var autoCompleteFunction: Function;
		private var refreshTimer: Timer = new Timer(250, 1);
		
		private var previousResults: Object = new Object();
		
		private var ssi : int = 0;
        private var sei : int = 0;
		
		public function AutoCompleteTextField(autoCompleteFunction: Function)
		{
			this.autoCompleteFunction = autoCompleteFunction;
			
			addEventListener(KeyboardEvent.KEY_UP, onKeyUp);
			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event): void {
				menuDropdown.setVisible(false);
				previousResults = new Object();
			});
			
			refreshTimer.addEventListener(TimerEvent.TIMER, callAutoComplete);
			
			menuDropdown.getSelectionModel().addStateListener(function(e: AWEvent): void {
				onMenuItemClick(e);
			});
			
			addEventListener(AWEvent.FOCUS_LOST, function(e: Event): void {
				if (menuDropdown) {
					menuDropdown.setVisible(false);
				}
			});
			
			var self: AutoCompleteTextField = this;
			addEventListener(AWEvent.FOCUS_GAINED, function(e: Event): void {
				if (getLength() >= 3 && menuDropdown && menuDropdown.getComponentCount()) {
					menuDropdown.show(self, 0, self.getHeight());
				}
			});
			
			// Fix flash trying to put cursor at the beginning/end when up/down is pressed
			addEventListener(KeyboardEvent.KEY_DOWN, function(e: KeyboardEvent): void {
				if (e.keyCode == Keyboard.UP || e.keyCode == Keyboard.DOWN) {
					AsWingManager.callNextFrame(function(): void {
						self.setSelection(getLength(), getLength());
					});
					return;
				}				
			});
        }
		
		private function onKeyUp(e: KeyboardEvent = null): void {			
			// Ignore menu selection changes
			if (e.keyCode == Keyboard.UP || e.keyCode == Keyboard.DOWN) {				
				return;
			}
			
			if (getText() == previousText) {
				return;			
			}
			
			previousText = getText();			
			
			if (getLength() < 3) {
				menuDropdown.setVisible(false);
				return;
			}				
			
			refreshTimer.reset();
			
			if (previousResults[getText()])
				setResults(previousResults[getText()], null);
			else				
				refreshTimer.start();			
		}
		
		private function callAutoComplete(e: Event = null): void {
			autoCompleteFunction(getText(), setResults);
		}
		
		private function onMenuItemClick(e: AWEvent = null): void {
			var menuItem: JMenuItem = e.target as JMenuItem;
			setText(menuItem.getText());
			setSelection(getLength(), getLength());
			
			if (menuDropdown.getComponentCount() == 1) {
				menuDropdown.removeAll();
			}
		}
		
		public function setResults(data: *, originalName: String) : void {
			if (!isVisible() || !isFocusOwner())
				return;
			
			if (originalName != null)
				previousResults[originalName] = data;
			
			menuDropdown.setVisible(false);
			menuDropdown.removeAll();
			
			if (data.length == 0)
				return;
			
			// Don't show if only a single result and this is it
			if (data.length == 1 && getText().toLocaleLowerCase().localeCompare(data[0].toLocaleLowerCase()) == 0)
				return;
				
			for each (var name: String in data) {
				var menuItem: JMenuItem = menuDropdown.addMenuItem(name);
				menuItem.addActionListener(onMenuItemClick);
			}		
			
			menuDropdown.setPreferredWidth(this.getWidth());
			menuDropdown.show(this, 0, this.getHeight());
		}
	}

}