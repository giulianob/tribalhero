<?
	$javascript->link('game-actions.js', false);
	$javascript->link('swfobject/swfobject', false);
	$javascript->link('jquery/jquery-1.3.2.min.js', false);
	$javascript->link('thickbox/thickbox-compressed.js', false);
		
	$html->css('thickbox/thickbox.css', null, array(), false);
?>

<? if (isset($lsessid)) : ?>
<script type="text/javascript">
	var flashvars = {};
	flashvars.hostname = "localhost";
	flashvars.lsessid = "<?=$lsessid?>";
	flashvars.sessid = "<?=session_id()?>";
	var params = {};
	params.wmode = "opaque";
	var attributes = {};
	swfobject.embedSWF("files/Game.swf", "flash-content", "976", "640", "10.0.0", false, flashvars, params, attributes);
</script>
		
<div id="game-box">
	<div id="flash-content">
		<a href="http://www.adobe.com/go/getflashplayer">
			The latest version of Flash player is required to play Tribal Hero. <img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif" alt="Get Adobe Flash player" />
		</a>
	</div>
</div>		
<? endif; ?>