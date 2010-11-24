<?php
	include 'generated/technology_listing.inc.php';
	$this->set('title_for_layout', $techName);
	$this->Html->css('style.database.css', null, array('inline' => false));
?>

<div class="span-20 last">	
	<div class="span-20 last">
		<h2><?php echo $techName; ?></h2>		
	</div>
	
	<div class="span-20 last">	
		<p>Researched by <?php echo $this->Html->link($trainedBy['name'] . ' (Level ' . $trainedBy['level'] . ')', array('action' => 'view', $trainedBy['key'], '#LEVEL_' . $trainedBy['level']));?></p>
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
			<p><?php echo $info['description']; ?></p>
		</div>
				
		<!-- Cost/Time -->
		<div class="span-20 last cost">	
			<div class="time"><?php echo $this->Html->image('db/icons/basic/clock_16.png', array('alt' => 'Time to build/upgrade'));?> <?php echo $this->element('format_time', array('time' => $info['time'])); ?></div>
			
			<div><?php echo $info['labor']; ?> <?php echo $this->Html->image('db/icons/resources/Labor.png', array('alt' => 'Labor cost'));?></div>
			
			<div><?php echo $info['gold']; ?> <?php echo $this->Html->image('db/icons/resources/Gold.png', array('alt' => 'Gold cost'));?></div>			
			
			<div><?php echo $info['wood']; ?>  <?php echo $this->Html->image('db/icons/resources/Wood.png', array('alt' => 'Wood cost'));?></div>			
			
			<div><?php echo $info['crop']; ?>  <?php echo $this->Html->image('db/icons/resources/Crop.png', array('alt' => 'Crop cost'));?></div>			
			
			<div><?php echo $info['iron']; ?>  <?php echo $this->Html->image('db/icons/resources/Iron.png', array('alt' => 'Iron cost'));?></div>			
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