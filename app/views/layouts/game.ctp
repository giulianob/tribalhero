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
		echo $scripts_for_layout;
	?>	
</head>
<body>
	<div id="game-container">
		<div id="header" class="container prepend-top">
			<div class="span-24 last">
				<div class="float-right">
					<cake:nocache>
						Logged in as, <strong><?php echo $session->read('Auth.Player.name')?></strong>.
						<?php echo $html->link('Logout', '/players/logout')?>
					</cake:nocache>
				</div>
			</div>
		</div>			
		
		<div id="game-content" class="prepend-top">		
			<div>
				<?php echo $content_for_layout; ?>
			</div>			
		</div>

		<div id="footer" class="container prepend-top">
			Copyright &copy; Tribal Hero 2009. All Rights Reserved.
		</div>
	</div>
	<?php echo $cakeDebug; ?>
</body>
</html>