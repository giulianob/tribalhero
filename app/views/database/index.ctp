<?php
	include 'structure_listing.inc.php';
	include 'unit_listing.inc.php';
?>

<div class="span-20 last">
	<div class="span-3"><?php echo $this->Html->link('Game Database', array('action' => 'index'))?></div>		
	<div class="span-3"><?php echo $this->Html->link('Building Tree', array('action' => 'tree'))?></div>
	<div class="span-14 last"> </div>
</div>

<div class="span-20 last prepend-top append-bottom">
	<h2>Structures</h2>

	<div class="span-20 last">
		<?php 
		$i = 0;
		$f = true;
		foreach ($structures as $key => $info) :
			if ($i % 5 == 0) echo (!$f ? '</div>' : '') . '<div class="span-20 last '. (!$f ? 'prepend-top' : '') .'">';
			$f = false;
			$i = ($i + 1) % 5;
		?>
		<div class="span-4 prepend-top<?php echo ($i == 0 ? ' last' : ''); ?>">
			<div class="span-4 last" style="height: 75px">
				<?php echo $this->Html->link($this->Html->image('db/buildings/' . $info['sprite'] . '.png', array('class' => 'center-block')), array('action' => 'view', $key), array('escape' => false)); 
				?>
			</div>
			<div class="span-4 last text-center"><strong><?php echo $this->Html->link($info['name'], array('action' => 'view', $key)); ?></strong></div>
		</div>
		<?php 
			endforeach; 
			if ($i != 0) echo '<div class="prepend-top span-' . (4 * (5 - $i)) . ' last"> </div>';
		?>	
		</div>
	</div>
	
</div>


<div class="span-20 last prepend-top append-bottom">
	<h2>Units</h2>

	<div class="span-20 last">
		<?php 
		$i = 0;
		$f = true;
		foreach ($units as $key => $info) :
			if ($i % 5 == 0) echo (!$f ? '</div>' : '') . '<div class="span-20 last '. (!$f ? 'prepend-top' : '') .'">';
			$f = false;
			$i = ($i + 1) % 5;
		?>
		<div class="span-4 prepend-top<?php echo ($i == 0 ? ' last' : ''); ?>">
			<div class="span-4 last" style="height: 40px">
				<?php echo $this->Html->link($this->Html->image('db/units/' . $info['sprite'] . '.png', array('class' => 'center-block')), array('action' => 'view', $key), array('escape' => false)); 
				?>
			</div>
			<div class="span-4 last text-center"><strong><?php echo $this->Html->link($info['name'], array('action' => 'view', $key)); ?></strong></div>
		</div>
		<?php 
			endforeach; 
			if ($i != 0) echo '<div class="prepend-top span-' . (4 * (5 - $i)) . ' last"> </div>';
		?>	
		</div>
	</div>
	
</div>
<!-- Effing IE7 -->
<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p>