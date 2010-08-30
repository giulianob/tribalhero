<div class="span-10">
	<h2>Login</h2>			
		<?php
			if ($session->check('Message.auth')) :
		?>
			<div class="error">		 		
					<?php echo $session->flash('auth'); ?>
			</div>
		<?php
			endif;
		?>
	<?=$form->create('Player', array('action' => 'login'));?>
	<?=$form->input('name', array('label' => 'Enter Your Username'))?>
	<?=$form->input('password', array('label' => 'Enter Your Password', 'type' => 'password'))?>		
	
	<div class="input text">		
	
	</div>
	
	<?=$form->end('Log In');?>
</div>
<div class="span-10 last">
	<div class="float-right round-box">
		<h2>Not a member yet?</h2>
		<p><?=$html->link('Join','register')?> now, it's 100% free!</p>
		<p>Tribal Hero features an immense persistent world where you can<p>
		<ul>
			<li>Grow your tribe from a few buildings to a large city</li>
			<li>Trade with your neighbors and friends</li>
			<li>Wage wars against others or help protect them against other threats</li>
		</ul>
		<p>Best of all, it's completely free to play and there are no downloads required!</p>
		<h2>Still not convinced?</h2>
		<p>Take the <?=$html->link('tour', '/tour')?> to learn more about Tribal Hero.</p>
	</div>
</div>