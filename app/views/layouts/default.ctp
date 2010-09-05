<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<?php echo $html->charset(); ?>
	<title>
		<?php echo $title_for_layout; ?> - Tribal Hero
	</title>
	<?php
		//echo $html->meta('icon');
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
	<div class="wrapper">
		<div id="header" class="container prepend-top">
			<div id="login" class="push-18 span-6 last">				
					<cake:nocache>
					<? if ($session->check('Auth.Player.id')) : ?>
						<div>Hello, <strong><?php echo $session->read('Auth.Player.name')?></strong>.</div>
						<?php echo $html->link('Play', '/play')?> |	<?php echo $html->link('Logout', '/players/logout')?>		
					<? else: ?>
						<?php echo $form->create('Player', array('class' => 'small white-label', 'action' => 'login'));?>
						<?php echo $form->input('Player.name', array('error' => false))?>
						<?php echo $form->input('Player.password', array('error' => false))?>
						<span class="float-left">Not a member? <?php echo $this->Html->link("Register now!", array('controller' => 'players', 'action' => 'register'));?></span>
						<?php echo $form->submit('Log In', array('alt' => 'Log In'));?>
						<?php echo $form->end();?>
					<? endif; ?>
					</cake:nocache>				
			</div>							
		</div>
		<div id="content" class="container prepend-top">			
			<?php if ($session->check('Message.flash')) : ?>
				<div class="success">
					<?php echo $session->flash(); ?>
				</div>
			<?php endif; ?>
			
			<div class="span-4">
				<ul id="main-nav">
					<li id="home"><?php echo $html->link('', '/');?></li>					
					<li id="help"><a href="#"></a></li>
					<li id="forums"><a href="#"></a></li>
					<li id="blog"><a href="#"></a></li>
					<li id="register"><?php echo $html->link('', '/players/register');?></li>
				</ul>
			</div>
			<div class="span-20 last">
				<?php echo $content_for_layout; ?>	
			</div>						
		</div>
		<div class="push"></div>		
	</div>
	<div id="footer">&nbsp;</div>
	
	<?php echo $js->writeBuffer(); ?>
</body>
</html>