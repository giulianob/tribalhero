<table>
	<tr>
		<th>Time</th>
		<th>Battle Location</th>
		<th>Troop Hometown</th>
		<th>Side</th>
	</tr>
	<? foreach($battle_reports as $battle_report) : ?>
	<tr>
		<td><?=$html->link($battle_report['BattleReport']['created'], array('action' => 'report_view', $battle_report['BattleReport']['id']))?></td>
		<td><?=$battle_report['City']['name']?></td>
		<td><?=$battle_report['TroopCity']['name']?></td>
		<td><?=$battle_report['BattleReportTroopEnter']['is_attacker']?'Attack':'Defense'?></td>
	</tr>
	<? endforeach; ?>
</table>

<div class="pagination">	
	<?php
		echo $paginator->prev('« Previous ', null, null, array('class' => 'disabled'));
		echo $paginator->counter();
		echo $paginator->next(' Next »', null, null, array('class' => 'disabled'));
	?> 	
</div>