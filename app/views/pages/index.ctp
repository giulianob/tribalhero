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
				Tribal Hero is an online multiplayer game which allows you to build your own empire. You will be able to compete against thousands of players in an engaging environment. Best of all, Tribal Hero can be fully enjoyed without having to spend your entire day infront of the computer.
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
			<?php echo $html->link($html->image('screenshots/advanced-city-thumb.png'), '/img/screenshots/advanced-city.png', array('escape' => false, 'title' => 'An advanced military city', 'rel' => 'gallery')); ?>
			<?php echo $html->link($html->image('screenshots/building-tower-thumb.png'), '/img/screenshots/building-tower.png', array('escape' => false, 'title' => 'Build towers around weaker structures to protect them when attacked', 'rel' => 'gallery')); ?>
			<?php echo $html->link($html->image('screenshots/city-overview-thumb.png'), '/img/screenshots/city-overview.png', array('escape' => false, 'title' => 'The overview screen quickly informs you about your city status', 'rel' => 'gallery')); ?>
		</div>		
		<div class="span-10 last prepend-top gallery">
			<?php echo $html->link($html->image('screenshots/military-overview-thumb.png'), '/img/screenshots/military-overview.png', array('escape' => false, 'title' => 'Various types of units can be trained and used to defend or attack other players', 'rel' => 'gallery')); ?>
			<?php echo $html->link($html->image('screenshots/simple-battle-report-thumb.png'), '/img/screenshots/simple-battle-report.png', array('escape' => false, 'title' => 'A battle report will let you know the outcome of battles', 'rel' => 'gallery')); ?>
			<?php echo $html->link($html->image('screenshots/unit-description-thumb.png'), '/img/screenshots/unit-description.png', array('escape' => false, 'title' => 'Each unit provides unique advantages during a battle', 'rel' => 'gallery')); ?>
		</div>		
	</div>
</div>