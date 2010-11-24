<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<?php echo $html->charset(); ?>
	<title>
		<?php echo $title_for_layout; ?> - Tribal Hero
	</title> 
	<?php
		echo $html->css('style.main');
		echo $scripts_for_layout;
	?>	
</head>
<body>
	<?php echo $content_for_layout; ?>
	
	<?php echo $js->writeBuffer(); ?>
	
	<script type="text/javascript">
		google_analytics_uacct = "UA-17369212-2";
		google_analytics_domain_name = "tribalhero.com";
		var _gaq = _gaq || [];
		_gaq.push(['_setAccount', 'UA-17369212-2'],['_setDomainName', 'tribalhero.com'],['_trackPageview']);
		(function() {var ga = document.createElement('script');
		ga.type = 'text/javascript';
		ga.async = true;
		ga.src = 'http://www.google-analytics.com/ga.js';
		(document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(ga);
		})();
	</script>		
</body>
</html>