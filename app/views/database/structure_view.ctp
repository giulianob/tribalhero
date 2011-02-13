<?php
	include 'generated/structure_listing.inc.php';
	$this->set('title_for_layout', $structureName);
	$this->Html->css('style.database.css', null, array('inline' => false));
?>

<div class="span-20 last">
	<div class="span-2">
		<?php echo $this->Html->image('db/buildings/' . $structures[$structureKey]['sprite'] . '.png'); ?>
	</div>
	
	<div class="span-18 last">
		<h2><?php echo $structureName; ?></h2>		
	</div>
	
	<div class="span-20 last">
		<p><?php echo $description; ?></p>	
		
		<?php if (empty($builtBy['name'])) : ?>
		<?php elseif ($converted) : ?>
		<p>Converted from <?php echo $this->Html->link($builtBy['name'] . ' (Level ' . $builtBy['level'] . ')', array('action' => 'view', $builtBy['key'], '#LEVEL_' . $builtBy['level']));?></p>
		<?php else : ?>
		<p>Built by <?php echo $this->Html->link($builtBy['name'] . ' (Level ' . $builtBy['level'] . ')', array('action' => 'view', $builtBy['key'], '#LEVEL_' . $builtBy['level']));?></p>
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
			<div class="span-2">Defense</div>
			<div class="span-13 last">
				<?php echo $this->element('star_ratings', array('min' => $structureStatRanges['defense']['min'], 'max' => $structureStatRanges['defense']['max'], 'value' => $info['defense'] , 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['defense'];?>)</span>
			</div>
		</div>
		<div class="span-20 last">
			<div class="span-2">Range</div>
			<div class="span-3">
				<?php echo $this->element('star_ratings', array('min' => $structureStatRanges['range']['min'], 'max' => $structureStatRanges['range']['max'], 'value' => $info['range'], 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['range'];?>)</span>
			</div>
			<div class="span-2">Stealth</div>
			<div class="span-13 last">
				<?php echo $this->element('star_ratings', array('min' => $structureStatRanges['stealth']['min'], 'max' => $structureStatRanges['stealth']['max'], 'value' => $info['stealth'], 'numberOfStars' => 5)); ?>
				<span class="small">(<?php echo $info['stealth'];?>)</span>
			</div>
		</div>		
		<div class="span-20 last">
			<div class="span-2">Weapon</div>
			<div class="span-3"><?php echo ucwords(strtolower($info['weapon'])); ?></div>
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
	
	<?php if (!empty($info['requirements'])) { ?>
	<div class="span-20 last prepend-top">
		<strong>Requirements</strong>
		<ul class="requirements">
			<?php foreach ($info['requirements'] as $requirement) : ?>
			<li><?php echo $requirement; ?></li>
			<?php endforeach; ?>
		</ul>
	</div>
	<?php } ?>
	
	<?php endforeach; ?>
</div>