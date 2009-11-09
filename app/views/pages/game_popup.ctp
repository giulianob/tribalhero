<?
	echo $javascript->link('jquery/jquery-1.3.2.min.js', true);
	echo $javascript->link('jquery/jquery-ui-1.7.1.full.min.js', true);
	
	echo $html->css('jquery/jquery-ui-1.7.1.custom.css', null, array(), true);	
?>

<div>

<div id="tabs">
	<ul>
		<li><a href="messages">Mail</a></li>
		<li><a href="events">Events</a></li> 
		<li><a href="reports">Battle Reports</a></li>
	</ul>
</div>

</div>

<script type="text/javascript">
	$(function() {		
		$('#tabs').tabs({
		    load: function(event, ui) {
		        $('a', ui.panel).click(function() {
		        	$('#tabs').data('disabled.tabs', []);
		            $(ui.panel).load(this.href);
		            return false;
		        });
	    	}
		});
		
		$("#tabs").tabs();
		
		$("#tabs ul.ui-tabs-nav a").click(function(){
            if ($(this).parent().is("li.ui-tabs-selected"))
            {
            	$("#tabs").tabs("load", $("#tabs ul.ui-tabs-nav a").index(this));
        	}
        }); 
	    
	    $('#tabs').bind('tabsselect', function(event, ui)
	    {
		        $('a', ui.panel).click(function() {
		            $(ui.panel).load(this.href);
		            return false;
		        });
	    });		
	    	    
	});
	 			

</script>