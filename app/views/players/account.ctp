<div class="span-20 last">
	<h2>Account Settings</h2>					
	
	<?php echo $form->create('Player', array('action' => 'account'));?>
	<?php echo $form->input('current_password', array('label' => 'Enter Your Current Password', 'type' => 'password'))?>
	<?php echo $form->input('email_address', array('label' => 'Email Address'))?>
	<em>Only fill out the following fields to change your password</em>
	<?php echo $form->input('password_once', array('label' => 'Choose a password', 'type' => 'password'))?>
	<?php echo $form->input('password_twice', array('label' => 'Retype password', 'type' => 'password'))?>
	<div class="buttons">
		<?php echo $form->button('Save', array('alt' => 'Save', 'type' => 'submit', 'div' => false));?>	
	</div>	
	<?php echo $form->end();?>
</div>