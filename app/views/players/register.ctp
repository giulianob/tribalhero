<?php 
 // Hides weird iframe in Chrome/Safari which breaks layout 
?>
<style type="text/css">
	iframe { display: none; }
	div.input iframe { display: block; }
</style>

<div class="span-10">
	<h2>Join the battle today!</h2>
	<?php echo $form->create('Player', array('action' => 'register'));?>
	<?php echo $form->input('name', array('label' => 'Choose a player name'))?>
	<?php echo $form->input('email_address', array('label' => 'Email address (has to be valid)'))?>
	<?php echo $form->input('password_once', array('label' => 'Choose a password', 'type' => 'password'))?>
	<?php echo $form->input('password_twice', array('label' => 'Retype password', 'type' => 'password'))?>
	
	<div class="input text">		
		<?php echo recaptcha_get_html($recaptchaPublicKey, isset($error) ? $error : null)?>
	</div>
	<?php echo $form->input('tos', array('label' => 'I accept the Terms of Service and the game rules', 'type' => 'checkbox'))?>
	<?php echo $form->end('Register');?>
</div>
<div class="span-10 last">
	<div class="round-box">
		<h2>Why join our world?</h2>
		<p>Tribal Hero features an immense persistent world where you can<p>
		<ul>
			<li>Grow your tribe from a few structures to a large city</li>
			<li>Trade with your neighbors and friends</li>
			<li>Wage wars against your foes or protect your friends from their enemies.</li>
		</ul>
		<p>Best of all, it's completely free to play and there are no downloads required!</p>
		<h2>Still not convinced?</h2>
		<p>Take the <?php echo $html->link('tour', '/tour')?> to learn more about Tribal Hero.</p>
	</div>
</div>