package src.UI.Components 
{	
	import org.aswing.*;
	
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class PagingBar extends JPanel
	{
		public var page: int = 1;
		private var pnlPaging:JPanel;
		private var btnPrevious:JLabelButton;
		private var btnFirst:JLabelButton;
		private var lblPages:JLabel;
		private var btnNext:JLabelButton;
		
		private var loadPage: Function;
		
		public function PagingBar(loadPage: Function) 
		{
			this.loadPage = loadPage;
			
			createUI();
			
			// Paging buttons
			btnFirst.addActionListener(function() : void {
				loadPage(1);
			});

			btnNext.addActionListener(function() : void {
				loadPage(page + 1);
			});

			btnPrevious.addActionListener(function() : void{
				loadPage(page - 1);
			});			
		}
		
		public function setData(data: * ) : void {
			page = data.page;
			btnFirst.setVisible(page > 1);
			btnPrevious.setVisible(page > 1);
			btnNext.setVisible(page < data.pages);
			lblPages.setText(data.page + " of " + data.pages);
		}
		
		private function createUI() : void {
			btnFirst = new JLabelButton("<< Newest");
			btnPrevious = new JLabelButton("< Newer");
			btnNext = new JLabelButton("Older >");

			lblPages = new JLabel();
			
			append(btnFirst);
			append(btnPrevious);
			append(lblPages);
			append(btnNext);
		}
		
	}

}