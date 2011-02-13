<?php
	include 'generated/unit_listing.inc.php';
	$this->set('title_for_layout', $unitName);
	$this->Html->css('style.database.css', null, array('inline' => false));
?>

<div class="span-20 last">
	<div class="span-1">
		<?php echo $this->Html->image('db/units/' . $units[$unitKey]['sprite'] . '.png'); ?>
	</div>
	
	<div class="span-19 last">
		<h2><?php echo $unitName; ?></h2>		
	</div>
	
	<div class="span-20 last">	
		<p><?php echo $description; ?></p>	
		<p>Trained by <?php echo $this->Html->link($trainedBy['name'] . ' (Level ' . $trainedBy['level'] . ')', array('action' => 'view', $trainedBy['key'], '#LEVEL_' . $trainedBy['level']));?></p>
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
		</div>
		
		<!-- Cost/Time -->
		<div class="span-20 last cost">	
			<div class="time"><?php echo $this->Html->image('db/icons/props/clock.png', array('alt' => 'Time to build/upgrade'));?> <?php echo $this->element('format_time', array('time' => $info['time'])); ?></div>
			
			<div><?php echo $info['labor']; ?> <?php echo $this->Html->image('db/icons/resources/Labor.png', array('alt' => 'Labor cost'));?></div>
			
			<div><?php echo $info['gold']; ?> <?php echo $this->Html->image('db/icons/resources/Gold.png', array('alt' => 'Gold cost'));?></div>			
			
			<div><?php echo $info['wood']; ?>  <?php echo $this->Html->image('db/icons/resources/Wood.png', array('alt' => 'Wood cost'));?></div>			
			
			<div><?php echo $info['crop']; ?>  <?php echo $this->Html->image('db/icons/resources/Crop.png', array('alt' => 'Crop cost'));?></div>			
			
			<div><?php echo $info['iron']; ?>  <?php echo $this->Html->image('db/icons/resources/Iron.png', array('alt' => 'Iron cost'));?></div>			
		</div>
		
		<!-- Stats -->
		<div class="span-20 last prepend-top">
			<div class="span-2">HP</div>
			<div class="span-3"><?php echo $info['hp']; ?> </div>
			<div class="span-2">Carry</div>
			<div class="span-13 last"><?php echo $info['carry'];?></div>
		</div>
		<div class="span-20 last">
			<div class="span-2">Armor</div>
			<div class="span-3"><?php echo ucwords(strtolower($info['armor'])); ?></div>
			<div class="span-2">Unit Class</div>
			<div class="span-13 last"><?php echo ucwords(strtolower($info['unitClass'])); ?></div>
		</div>			
		<div class="span-20 last">
			<div class="span-2">Weapon</div>
			<div class="span-3"><?php echo ucwords(strtolower($info['weapon'])); ?></div>
			<div class="span-2">Class</div>
			<div class="span-13 last"><?php echo ucwords(strtolower($info['weaponClass'])); ?></div>
		</div>					
		<div class="span-20 last prepend-top">
			<div class="span-2">Attack</div>
			<div class="span-3">
				<?php echo $this->element('star_ratings', array('min' => $unitStatRanges['attack']['min'], 'max' => $unitStatRanges['attack']['max'], 'value' => $info['attack'] , 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo ($info['attack'] / $info['upkeep']);?>)</span>			
			</div>
			<div class="span-2">Defense</div>
			<div class="span-13 last">
				<?php echo $this->element('star_ratings', array('min' => $unitStatRanges['defense']['min'], 'max' => $unitStatRanges['defense']['max'], 'value' => $info['defense'] , 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo ($info['defense'] / $info['upkeep']);?>)</span>
			</div>
		</div>		
		<div class="span-20 last">
			<div class="span-2">Range</div>
			<div class="span-3">
				<?php echo $this->element('star_ratings', array('min' => $unitStatRanges['range']['min'], 'max' => $unitStatRanges['range']['max'], 'value' => $info['range'], 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['range'];?>)</span>
			</div>
			<div class="span-2">Stealth</div>
			<div class="span-13 last">
				<?php echo $this->element('star_ratings', array('min' => $unitStatRanges['stealth']['min'], 'max' => $unitStatRanges['stealth']['max'], 'value' => $info['stealth'], 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['stealth'];?>)</span>
			</div>
		</div>		
		<div class="span-20 last">
			<div class="span-2">Speed</div>
			<div class="span-3">
				<?php echo $this->element('star_ratings', array('min' => $unitStatRanges['speed']['min'], 'max' => $unitStatRanges['speed']['max'], 'value' => $info['speed'], 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['speed'];?>)</span>
			</div>
			<div class="span-2">Upkeep</div>
			<div class="span-13 last">
				<?php echo $this->Html->image('db/icons/resources/Crop.png', array('alt' => 'Upkeep')) . $info['upkeep'];?> per hour
			</div>			
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