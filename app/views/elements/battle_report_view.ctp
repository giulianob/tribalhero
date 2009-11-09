<p class="strong"><?=$is_attack?'Attack':'Defense'?></p>
<div class="report-tabs">
	<ul>
	<? 	foreach($battle_report['BattleReportTroop'] as $battle_troop):
			if ($battle_troop['is_attacker'] != $is_attack) continue;					  
	?>			
		<li><a href="#a<?=$is_attack?'1':'0'?>r<?=$battle_report['BattleReport']['round']?>t<?=$battle_report['BattleReport']['turn']?>t<?=$battle_troop['group_id']?>"><?=$battle_troop['City']['name']?>(<?=$battle_troop['troop_stub_id']==1?'Local':$battle_troop['troop_stub_id']?>)</a></li>			
	<? 	endforeach; ?>
	</ul>
	<? foreach($battle_report['BattleReportTroop'] as $battle_troop):
			if ($battle_troop['is_attacker'] != $is_attack) continue;
	?>
			<div id="a<?=$is_attack?'1':'0'?>r<?=$battle_report['BattleReport']['round']?>t<?=$battle_report['BattleReport']['turn']?>t<?=$battle_troop['group_id']?>">
				
				<? if ($is_attack) : ?>
				<!-- Resources -->
				<table class="borderless">
					<tr>
						<td><?=$battle_troop['gold']?> G</td>
						<td><?=$battle_troop['crop']?> C</td>
						<td><?=$battle_troop['iron']?> I</td>
						<td><?=$battle_troop['wood']?> W</td>
					</tr>
				</table>
				<? endif; ?>
				
				<!-- Objects -->
				<table>
					<tr>
						<th>Type</th>
						<th>Level</th>
						<th>Count</th>
						<th>HP</th>						
						<th>Dmg Taken</th>
						<th>Dmg Dealt</th>
					</tr>											
			<?
				unset($lastFormation);						
				foreach($battle_troop['BattleReportObject'] as $battle_object):
					if (!isset($lastFormation) || $lastFormation != $battle_object['formation_type']) :					
						$lastFormation = $battle_object['formation_type'];		 
			?>
					<tr>
						<td colspan="6" class="header"><?=$formations[$battle_object['formation_type']]?></td>
					</tr>
			<?		endif; ?>
					<tr class="<?=$battle_object['hp'] == 0 ? 'dead' : ''?>">
						<td><?=isset($object_types[$battle_object['type']]) ? $object_types[$battle_object['type']] : $battle_object['type'];?></td>
						<td><?=$battle_object['level'];?></td>
						<td><?=$battle_object['count'];?></td>
						<td><?=$battle_object['hp'];?></td>						
						<td><?=$battle_object['damage_taken']?></td>
						<td><?=$battle_object['damage_dealt']?></td>
					</tr>						
			<? 	endforeach; ?>
				</table>
			</div>
	<?	endforeach; ?>
</div>