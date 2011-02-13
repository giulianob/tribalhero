<div class="span-20 last">
    <dl><?php $i = 0;
$class = ' class="altrow"'; ?>

		<?php
		foreach ($variables as $variable) :
		?>
		<dt<?php if ($i % 2 == 0)
            echo $class; ?>><?php echo $variable['SystemVariable']['name']; ?></dt>
        <dd<?php if ($i++ % 2 == 0)
                echo $class; ?>>
                <?php echo is_numeric($variable['SystemVariable']['value']) ? $this->Number->format($variable['SystemVariable']['value']) : $variable['SystemVariable']['value']; ?>
            &nbsp;
        </dd>
		<?php endforeach; ?>
		
    </dl>
</div>

<div class="span-20 last prepend-top">
	Online Players: 
	<?php
	$f = 0;	
	foreach ($onlinePlayers as $player) {
		echo ($f != 0 ? ', ' : '') . $player['Player']['name'];
		$f = 1;
	}
	?>
</div>