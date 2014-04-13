<div class="row">
	<div class="col-md-15">
	    <h2>Server Status</h2>
	    <p>Current time in UTC is: <strong><?php echo gmdate('n/j/y g:i:s A'); ?></strong></p>
	    
	    <table class="table table-condensed table-striped table-hover">
			<?php
			foreach ($variables as $variable) :
			?>
			<tr>
				<td><?php echo $variable['SystemVariable']['name']; ?> </td>
				<td><?php echo is_numeric($variable['SystemVariable']['value']) ? $this->Number->format($variable['SystemVariable']['value']) : $variable['SystemVariable']['value']; ?> </td>
			</tr>
			<?php endforeach; ?>
		</table>	    
    </div>
</div>

<div class="row">
	<div class="col-md-20">
		Online Players: 
		<?php
		foreach ($onlinePlayers as $player) {
			?>
			<span class="label label-success"><?php echo $player['Player']['name']; ?></span>
			<?php
		}
		?>
	</div>
</div>