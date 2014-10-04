<p>Follow the link below to reset your password<p>
<p>
	<?php 
	$url = $this->Html->url(array('controller' => 'players', 'action' => 'reset', $reset_key), true);
	echo $this->Html->link($url, $url);
	?>
</p>