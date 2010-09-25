<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<?php echo $html->charset(); ?>
	<title><?php echo $title_for_layout . ($this->name == 'Pages' && $this->action == 'index' ? '' : ' - Tribal Hero'); ?></title>
	<?php
		echo $html->meta('description', 'Tribal Hero is a free multiplayer game which allows you to build your own empire.');
		echo $html->css('blueprint/screen', null, array("media" => "screen, projection"));
	?>
	<!--[if lt IE 8]><?php echo $html->css('blueprint/ie', null, array("media" => "screen, projection"));?><![endif]-->	
	<?php			
		
		echo $html->css('style.main');
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
			<cake:nocache>
			<?php if ($session->check('Message.flash')) : ?>				
				<div class="success">
					<?php echo $session->flash(); ?>
				</div>				
			<?php endif; ?>
			</cake:nocache>
			
			<div class="span-4">
				<ul id="main-nav">
					<li id="home"><?php echo $html->link('', '/');?></li>					
					<li id="help"><?php echo $html->link('', '/database/');?></li>
					<li id="forums"><a href="http://forums.tribalhero.com"></a></li>
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

	<?php
		if (isset($use_jquery) && $use_jquery == true) :
	?>
		<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>
		<script>!window.jQuery && document.write('<script src="<?php echo $this->Html->url('/js/jquery-1.4.2.min.js');?>"><\/script>')</script>	
	<? endif; ?>
	
	<?php 
		echo $scripts_for_layout;
		echo $js->writeBuffer(); 
	?>
	
	<!--[if IE 6]>
	<script type="text/javascript"> 
		/*Load jQuery if not already loaded*/ if(typeof jQuery == 'undefined'){ document.write("<script type=\"text/javascript\"   src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js\"></"+"script>"); var __noconflict = true; } 
		var IE6UPDATE_OPTIONS = {
			icons_path: "<?php echo $this->Html->url('/js/ie6update/images/'); ?>"
		}
	</script>
		<?php echo $this->Html->script('ie6update/ie6update.js'); ?>
	<![endif]-->
	
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