<?
	//echo $javascript->link('jquery/jquery-1.3.2.min.js', true);
	//echo $javascript->link('jquery/jquery-ui-1.7.1.full.min.js', true);
	
	//echo $html->css('jquery/jquery-ui-1.7.1.custom.css', null, array(), true);	
?>

<?php
	$battleStartTime = strtotime($main_report['BattleReport']['created']);
	foreach($battle_reports as $battle_report):
?>

	<div class="report-box">
		<h2><?=$timeAdv->diff(strtotime($battle_report['BattleReport']['created']) - $battleStartTime);?></h2>		
		<ul class="list">
		<? 	foreach($battle_report['BattleReportTroop'] as $battle_troop):
				if ($battle_troop['state'] == TROOP_STATE_STAYING) continue;										 
		?>
			<li><?=$battle_troop['City']['name']?>(<?=$battle_troop['troop_stub_id']==1?'Local':$battle_troop['troop_stub_id']?>) has <?=$troop_states_pst[$battle_troop['state']]?></li>
		<? 	endforeach; ?>
		</ul>		
		<div class="float-left col45">
			<?=$this->element('battle_report_view', array('is_attack' => false, 'battle_report' => $battle_report))?>					
		</div>
		<div class="float-right col45">
			<?=$this->element('battle_report_view', array('is_attack' => true, 'battle_report' => $battle_report))?>
		</div>
		<div class="clear"> </div>
	</div>
<? endforeach; ?>

<div class="pagination">
	<?php
		echo $paginator->prev('« Previous ', null, null, array('class' => 'disabled'));
		echo $paginator->counter();
		echo $paginator->next(' Next »', null, null, array('class' => 'disabled'));
	?> 	
</div>

<script type="text/javascript">
	$(function() {		
	    $("div.report-tabs").tabs();
	});	 			
</script>