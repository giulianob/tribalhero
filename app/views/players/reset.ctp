<div class="span-20 last">
	<h2>Reset Password</h2>					
	<?php echo $this->Form->create('Player', array('action' => 'reset'));?>
	<?php echo $this->Form->hidden('reset_key');?>
	<?php echo $this->Form->input('password_once', array('label' => 'Choose a new password', 'type' => 'password'))?>
	<?php echo $this->Form->input('password_twice', array('label' => 'Retype new password', 'type' => 'password'))?>
	<div class="buttons">
		<?php echo $form->button('Submit', array('alt' => 'Submit', 'type' => 'submit', 'div' => false));?>	
	</div>
	<?php echo $this->Form->end();?>
</div>