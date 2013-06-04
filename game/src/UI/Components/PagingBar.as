package src.UI.Components 
{	
	import org.aswing.*;
	
	public class PagingBar extends JPanel
	{
		public var page: int = 1;
		public var totalPages: int = 0;
		private var pnlPaging:JPanel;
		private var btnPrevious:JLabelButton;
		private var btnFirst:JLabelButton;
		private var lblPages:JLabel;
		private var btnNext:JLabelButton;
		private var btnLast:JLabelButton;
		private var btnRefresh:JLabelButton;
		
		private var firstTitle: *;
		private var previousTitle: *;
		private var nextTitle: *;
		private var lastTitle: *;
		private var refreshTitle: *;
		
		private var loadPage: Function;
		
		private var includeCaption: Boolean;
		
		public function PagingBar(loadPage: Function, includeCaption: Boolean = true, firstTitle: * = "<< First", previousTitle: * = "<< Previous", nextTitle: * = "Next >", lastTitle: * = "Last >>", refreshTitle: * = "Refresh")
		{
			if (firstTitle === true)
				firstTitle = "<< First";
				
			if (previousTitle === true)
				previousTitle = "<< Previous";
			
			if (nextTitle === true)
				nextTitle = "Next >";
				
			if (lastTitle === true)
				lastTitle = "Last >>";
				
			this.loadPage = loadPage;
			this.includeCaption = includeCaption;
			this.firstTitle = firstTitle;
			this.previousTitle = previousTitle;
			this.nextTitle = nextTitle;
			this.lastTitle = lastTitle;
			this.refreshTitle = refreshTitle;
			
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
			
			btnLast.addActionListener(function() : void{
				loadPage(totalPages);
			});						
			
			btnRefresh.addActionListener(function() : void {
				loadPage(page);
			});
		}
		
		public function setData(data: * ) : void {
			page = data.page;
			totalPages = data.pages;
			btnFirst.setEnabled(page > 1);
			btnPrevious.setEnabled(page > 1);
			btnNext.setEnabled(page < data.pages);
			btnLast.setEnabled(page < data.pages);
			lblPages.setText(data.page + " of " + data.pages);
		}
		
		public function refreshPage(page: int = int.MIN_VALUE): void {
			if (page != int.MIN_VALUE)
				loadPage(page);
			else
				loadPage(this.page);
		}
		
		private function createUI() : void {
			btnFirst = new JLabelButton(firstTitle);
			btnPrevious = new JLabelButton(previousTitle);
			btnNext = new JLabelButton(nextTitle);
			btnLast = new JLabelButton(lastTitle);
			btnRefresh = new JLabelButton(refreshTitle);
			
			btnFirst.setEnabled(false);
			btnPrevious.setEnabled(false);
			btnNext.setEnabled(false);
			btnLast.setEnabled(false);			

			lblPages = new JLabel();

			if (firstTitle)
				append(btnFirst);
				
			if (previousTitle)
				append(btnPrevious);
				
			if (includeCaption)
				append(lblPages);
				
			if (nextTitle)
				append(btnNext);
				
			if (lastTitle)
				append(btnLast);
				
			if (refreshTitle)
				append(btnRefresh);				
		}		
		
	}

}