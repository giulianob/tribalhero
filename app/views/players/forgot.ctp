<div class="span-20 last">
	<h2>Forgot Password</h2>					
	
	<p>Fill out the following form to receive an e-mail with information on how to reset your password.</p>
	<?php echo $form->create('Player', array('action' => 'forgot'));?>
	<?php echo $form->input('name', array('label' => 'Enter Your Username'))?>
	<div class="buttons">
		<?php echo $form->button('Submit', array('alt' => 'Submit', 'type' => 'submit', 'div' => false));?>	
	</div>	
	<?php echo $form->end();?>
</div>