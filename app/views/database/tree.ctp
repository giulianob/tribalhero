<div class="span-20 last">
	<div class="span-3"><?php echo $this->Html->link('Game Database', array('action' => 'index'))?></div>		
	<div class="span-3"><?php echo $this->Html->link('Building Tree', array('action' => 'tree'))?></div>
	<div class="span-14 last"> </div>
</div>

<div class="span-20 last prepend-top">
	<div class="span-15">
		<h2>Building Tree</h2>		
	</div>
	<div class="span-5 last">
		<?php echo $this->Html->link('View Larger', '/img/tree/game-tree-large.png', array('class' => 'float-right icon action-zoom', 'target' => '_blank')); ?>
	</div>
	<div class="span-20 last">
		<?php echo $this->Html->image('tree/game-tree.png', array('alt' => 'TribalHero Tree', 'class' => 'float-left')); ?>
	</div>
</div>