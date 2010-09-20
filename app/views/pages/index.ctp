<?php
$this->Html->css('colorbox/colorbox', null, array('inline' => false));		
$this->Html->script('jquery/jquery-1.4.2.min', array('inline' => false));		
$this->Html->script('colorbox/jquery.colorbox-min', array('inline' => false));	

$code = <<<JS
$(document).ready(function(){			
	$("a[rel='gallery']").colorbox({transition:"fade",width:"80%"});
});
JS;

$this->Js->buffer($code);
	
?>

<div class="span-20 last">
	<div class="span-10">
		<div class="span-10 last">
			<h2>Welcome</h2>
			<p>
				Tribal Hero is an online multiplayer game which allows you to build your own empire. You will be able to compete against thousands of players in an engaging environment. Best of all, Tribal Hero can be fully enjoyed without having to spend your entire day in front of the computer.
			</p>
			<p>
				Just <?php echo $html->link('sign up', '/players/register');?> and try it!
			</p>
		</div>

		<div class="span-10 last">
			<h2>Features</h2>
			<ul>
				<li>Innovative map system that allows you to seamlessly navigate the entire world.</li>
				<li>Strategically build your city, make friends, and wage wars.</li>
				<li>Watch every step of a battle as it happens.</li>
				<li>Play directly from your browser without any downloads.</li>
				<li>It's free!</li>		
			<ul>
		</div>
	</div>
	
	<div class="span-10 last">
		<h2>Screenshots</h2>	
		<div class="span-10 last gallery">			
			<?php 
			$desc = 'An advanced military city';
			echo $html->link($html->image('screenshots/advanced-city-thumb.png', array('alt' => $desc)), '/img/screenshots/advanced-city.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
			
			<?php 
			$desc = 'Build towers around weaker structures to protect them when attacked';
			echo $html->link($html->image('screenshots/building-tower-thumb.png', array('alt' => $desc)), '/img/screenshots/building-tower.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
			
			<?php 
			$desc = 'The overview screen quickly informs you about your city status';
			echo $html->link($html->image('screenshots/city-overview-thumb.png', array('alt' => $desc)), '/img/screenshots/city-overview.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
		</div>		
		<div class="span-10 last prepend-top gallery">
			<?php 
			$desc = 'Various types of units can be trained and used to defend or attack other players';
			echo $html->link($html->image('screenshots/military-overview-thumb.png', array('alt' => $desc)), '/img/screenshots/military-overview.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
			
			<?php 
			$desc = 'Watch battles as they unfold real time';
			echo $html->link($html->image('screenshots/battle-viewer-thumb.png', array('alt' => $desc)), '/img/screenshots/battle-viewer.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
			
			<?php 
			$desc = 'A battle report will let you know the outcome of battles';
			echo $html->link($html->image('screenshots/simple-battle-report-thumb.png', array('alt' => $desc)), '/img/screenshots/simple-battle-report.png', array('escape' => false, 'title' => $desc, 'rel' => 'gallery')); ?>
		</div>		
	</div>
</div>