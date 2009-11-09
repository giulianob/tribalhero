<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
		<?php echo $html->charset(); ?>
		<title>
			<?php echo $title_for_layout; ?> - Tribal Hero
		</title>
		<?php
			echo $html->meta('icon');
			echo $html->css('blueprint/screen', null, array("media" => "screen, projection"));
			echo $html->css('blueprint/forms', null, array("media" => "screen, projection"));
			echo $html->css('blueprint/print', null, array("media" => "print"));
		?>
		<!--[if lt IE 8]><?php echo $html->css('blueprint/ie', null, array("media" => "screen, projection"));?><![endif]-->	
		<?php
			echo $html->css('style.main');
			echo $html->css('style.popup');
			echo $scripts_for_layout;
		?>		
	</head>
	<body>
		<?php echo $content_for_layout; ?>
	</body>
</html>