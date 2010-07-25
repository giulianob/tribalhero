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
	<div id="container">
		<div id="header" class="container prepend-top">
			<div class="span-24 last">
				<div class="float-right">
					<cake:nocache>
					<? if ($session->check('Auth.Player.id')) : ?>
						Logged in as, <strong><?php echo $session->read('Auth.Player.name')?></strong>.
						<?php echo $html->link('Play', '/play')?> |	<?php echo $html->link('Logout', '/players/logout')?>		
					<? else: ?>
						<?php echo $form->create('Player', array('class' => 'small inline', 'action' => 'login'));?>
						<?php echo $form->input('Player.name', array('error' => false))?>
						<?php echo $form->input('Player.password', array('error' => false))?>
						<?php echo $form->button('Log In', array('type' => 'submit', 'alt' => 'Log In'));?>
						<?php echo $form->end();?>
					<? endif; ?>
					</cake:nocache>
				</div>
			</div>			
			<div id="logo" class="span-8"><?php echo $html->image('logo.png')?></div>
			<div class="span-16 last">
				<ul id="main-nav">
					<li id="home"><?php echo $html->link('', '/');?></li>
					<li id="tour"><a href="#"></a></li>
					<li id="help"><a href="#"></a></li>
					<li id="community"><a href="#"></a></li>
				</ul>
			</div>			
		</div>
		<div id="content" class="container prepend-top">			
			<?php if ($session->check('Message.flash')) : ?>
				<div class="success">
					<?php $session->flash(); ?>
				</div>
			<?php endif; ?>
			
			<?php echo $content_for_layout; ?>			
		</div>
		<div id="footer" class="container prepend-top">
			Copyright &copy; Tribal Hero <?php echo date("Y");?>. All Rights Reserved.
		</div>
	</div>	
</body>
</html>