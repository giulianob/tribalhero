<style type="text/css">

div.cost div { display:inline;  margin-right: 10px; }

div.cost div.time { margin-right: 25px; }

ul.requirements { margin-bottom: 0; }
	
</style>

<?php
	// Generated values
	/*
	$structureKey = '#STRUCTURE#';
	$structureName = '#STRUCTURE_NAME#';
	$description = '#DESCRIPTION#';
	
	$converted = '#CONVERTED#';
	$builtBy = array('name' => '#BUILT_BY_NAME#', 'key' => '#BUILT_BY#', 'level' => '#BUILT_BY_LEVEL#');
	
	// Levels array should contain:
	// description, time, gold, crop, iron, labor, wood, hp, defense, range, stealth, weapon, maxLabor
	$levels = array(
		#LEVELS#
	);
	*/
	
		$structureStatRanges = array(
			"defense" => array('min' => 0, 'max' => 12500),
			"stealth" => array('min' => 0, 'max' => 20),
			"range" => array('min' => 0, 'max' => 15)
		);	
	
	// Test data
	$structureKey = 'FARM_STRUCTURE';
	$structureName = 'Farm';
	$description = 'Test description';
	
	$converted = '0';
	$builtBy = array('name' => 'Town Center', 'key' => '#BUILT_BY#', 'level' => '1');
	
	// Levels array should contain:
	// description, time, gold, crop, iron, labor, wood, hp, defense, range, stealth, weapon, maxLabor, requirements
	$levels = array(
		array('description' => 'Level 1 description', 'time' => 66, 'gold' => 30, 'crop' => 40, 'iron' => 50, 'labor' => 0, 'wood' => 30, 'hp' => 6300, 'defense' => 30, 'range' => 5, 'stealth' => 3, 'weapon' => 'Bow', 'maxLabor' => 20,
			'requirements' => array('Does not already have a farm')
		),
		array('description' => 'Level 2 description', 'time' => 66, 'gold' => 30, 'crop' => 40, 'iron' => 50, 'labor' => 0, 'wood' => 30, 'hp' => 6300, 'defense' => 30, 'range' => 5, 'stealth' => 3, 'weapon' => 'Bow', 'maxLabor' => 20, 'requirements' => array()),
		array('description' => 'Level 2 description', 'time' => 66, 'gold' => 30, 'crop' => 40, 'iron' => 50, 'labor' => 0, 'wood' => 30, 'hp' => 6300, 'defense' => 30, 'range' => 5, 'stealth' => 3, 'weapon' => 'Bow', 'maxLabor' => 20, 'requirements' => array()),
	);	
?>

<div class="span-20 last">
	<div class="span-2">
		<?php echo $this->Html->image('db/buildings/FARM_STRUCTURE.png'); ?>
	</div>
	
	<div class="span-18 last">
		<h2><?php echo $structureName; ?></h2>
	</div>
	
	<div class="span-20 last">
		<p><?php echo $description; ?></p>	
		
		<?php if (empty($builtBy['name'])) : ?>
		<?php elseif ($converted) : ?>
		<p>Converted from <?php echo $this->Html->link($builtBy['name'] . ' (Level' . $builtBy['level'] . ')', array('action' => 'view', $builtBy['key'], '#LEVEL_' . $builtBy['level']));?></p>
		<?php else : ?>
		<p>Built by <?php echo $this->Html->link($builtBy['name'] . ' (Level' . $builtBy['level'] . ')', array('action' => 'view', 'TOWNCENTER_STRUCTURE', '#LEVEL_1'));?></p>
		<?php endif; ?>
	</div>
	
	<?php 
	$level = 0;
	foreach ($levels as $info) : ?>
	<?php
		$level++;
	?>
	<a name="LEVEL_<?php echo $level; ?>"></a>
	<div class="span-20 last prepend-top">
		<div class="span-20 last <?php echo ($level != 1 ? 'prepend-top' : ''); ?>">
			<h3 class="no-margin">Level <?php echo $level;?></h3>
			<!-- Description -->
			<p><?php echo $info['description']; ?> </p>
		</div>
		
		<!-- Cost/Time -->
		<div class="span-20 last cost">	
			<div class="time"><?php echo $this->Html->image('db/icons/basic/clock_16.png');?> <?php echo $this->element('format_time', array('time' => $info['time'])); ?></div>
			
			<div><?php echo $info['labor']; ?> <?php echo $this->Html->image('db/icons/resources/Labor.png');?></div>
			
			<div><?php echo $info['gold']; ?> <?php echo $this->Html->image('db/icons/resources/Gold.png');?></div>			
			
			<div><?php echo $info['wood']; ?>  <?php echo $this->Html->image('db/icons/resources/Wood.png');?></div>			
			
			<div><?php echo $info['crop']; ?>  <?php echo $this->Html->image('db/icons/resources/Crop.png');?></div>			
			
			<div><?php echo $info['iron']; ?>  <?php echo $this->Html->image('db/icons/resources/Iron.png');?></div>			
		</div>
		
		<!-- Stats -->
		<div class="span-20 last prepend-top">
			<div class="span-2">HP</div>
			<div class="span-3"><?php echo $info['hp']; ?> </div>
			<div class="span-2">Defense</div>
			<div class="span-13 last"><?php echo $this->element('star_ratings', array('min' => $structureStatRanges['defense']['min'], 'max' => $structureStatRanges['defense']['max'], 'value' => $info['defense'] , 'numberOfStars' => 5)); ?></div>
		</div>
		<div class="span-20 last">
			<div class="span-2">Range</div>
			<div class="span-3"><?php echo $this->element('star_ratings', array('min' => $structureStatRanges['range']['min'], 'max' => $structureStatRanges['range']['max'], 'value' => $info['range'], 'numberOfStars' => 5)); ?></div>
			<div class="span-2">Stealth</div>
			<div class="span-13 last"><?php echo $this->element('star_ratings', array('min' => $structureStatRanges['stealth']['min'], 'max' => $structureStatRanges['stealth']['max'], 'value' => $info['stealth'], 'numberOfStars' => 5)); ?></div>
		</div>		
		<div class="span-20 last">
			<div class="span-2">Weapon</div>
			<div class="span-3"><?php echo $info['weapon']; ?></div>
			<?php 
			if ($info['maxLabor'] > 0) :
			?>
			<div class="span-2">Laborers</div>
			<div class="span-13 last"><?php echo $info['maxLabor']; ?></div>
			<?php else: ?>
			<div class="span-15 last"> </div>
			<?php endif; ?>
		</div>		
	</div>
	
	<? if (!empty($info['requirements'])) : ?>
	<div class="span-20 last prepend-top">
		<strong>Requirements</strong>
		<ul class="requirements">
			<?php foreach ($info['requirements'] as $requirement) : ?>
			<li><?php echo $requirement; ?></li>
			<?php endforeach; ?>
		</ul>
	</div>
	<?php endif; ?>
	
	<?php endforeach; ?>
</div>