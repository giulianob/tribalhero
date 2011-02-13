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
	<?php echo $form->create('Player', array('action' => 'login'));?>
	<?php echo $form->input('name', array('label' => 'Enter Your Username'))?>
	<div class="input password">
		<?php echo $form->input('password', array('label' => 'Enter Your Password', 'type' => 'password', 'div' => false))?>		
		<div><?php echo $this->Html->link('Forgot Password?', array('action' => 'forgot'), array('class' => 'small')); ?></div>
	</div>
	<div class="buttons">
		<?php echo $form->button('Log In', array('alt' => 'Login', 'type' => 'submit', 'div' => false));?>	
	</div>
	<?php echo $form->end();?>
</div>
<div class="span-10 last">
	<div class="float-right round-box">
		<h2>Not a member yet?</h2>
		<p><?php echo $html->link('Join','register')?> now, it's 100% free!</p>
		<p>Tribal Hero features an immense persistent world where you can<p>
		<ul>
			<li>Grow your tribe from a few buildings to a large city</li>
			<li>Trade with your neighbors and friends</li>
			<li>Wage wars against others or help protect them against other threats</li>
		</ul>
		<p>Best of all, it's completely free to play and there are no downloads required!</p>
	</div>
</div>