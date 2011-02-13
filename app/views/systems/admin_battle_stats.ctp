<div class="span-12">
	<h2>Unit Battle Stats</h2>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<th>Type</th>
			<th>Level</th>
			<th>Formation</th>
			<th>Dmg</th>
			<th>Dmg Max</th>
			<th>Dmg Recv</th>
			<th>Hits Dealt</th>
		</tr>
		<?php
		$i = 0;
		foreach ($stats as $stat) {
			$class = null;
			if ($i++ % 2 == 0) {
				$class = ' class="altrow"';
			}
		?>
			<tr<?php echo $class; ?>>
				<td><?php echo $stat['BattleReportObject']['type']; ?>&nbsp;</td>
				<td><?php echo $stat['BattleReportObject']['level']; ?>&nbsp;</td>
				<td><?php echo $stat['BattleReportObject']['formation_type']; ?>&nbsp;</td>
				<td><?php echo $this->Number->precision($stat[0]['damage_average'], 2); ?>&nbsp;</td>
				<td><?php echo $this->Number->precision($stat[0]['damage_max'], 2); ?>&nbsp;</td>
				<td><?php echo $this->Number->precision($stat[0]['damage_taken_average'], 2); ?>&nbsp;</td>
				<td><?php echo $this->Number->precision($stat[0]['hits_dealt_average'], 2); ?>&nbsp;</td>
			</tr>
		<?php } ?>
	</table>
</div>

<div class="span-8 last">
	<div class="span-8 last">
		<h2>Largest By Troops</h2>
		<table cellpadding="0" cellspacing="0">
			<tr>
				<th>City</th>
				<th>Unit Count</th>
			</tr>
			<?php
			$i = 0;
			foreach ($troopSizeStats as $troopSizeStat) {
				$class = null;
				if ($i++ % 2 == 0) {
					$class = ' class="altrow"';
				}
			?>
				<tr<?php echo $class; ?>>
					<td><?php echo $troopSizeStat['City']['name']; ?>&nbsp;</td>
					<td><?php echo $troopSizeStat[0]['troop_count']; ?>&nbsp;</td>
				</tr>
			<?php } ?>
		</table>
	</div>
	
	<div class="span-8 last prepend-top">
		<h2>Largest By Resources</h2>
		<table cellpadding="0" cellspacing="0">
			<tr>
				<th>City</th>
				<th>L</th>
				<th>G</th>
				<th>I</th>
				<th>C</th>
				<th>W</th>
			</tr>
			<?php
			$i = 0;
			foreach ($resourceStats as $resourceStat) {
				$class = null;
				if ($i++ % 2 == 0) {
					$class = ' class="altrow"';
				}
			?>
				<tr<?php echo $class; ?>>
					<td><?php echo $resourceStat['City']['name']; ?></td>
					<td><?php echo $resourceStat['City']['labor']; ?></td>					
					<td><?php echo $resourceStat['City']['gold']; ?></td>					
					<td><?php echo $resourceStat['City']['iron']; ?></td>					
					<td><?php echo $resourceStat['City']['crop']; ?></td>					
					<td><?php echo $resourceStat['City']['wood']; ?></td>					
				</tr>
			<?php } ?>
		</table>
	</div>	
</div>