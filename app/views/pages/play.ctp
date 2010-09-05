<?php
	$this->Html->script('swfobject/swfobject', array('inline' => false));
?>

<style type="text/css">
<!--
body, html {
height: 100%;
width: 100%;
margin: 0;
background-image: none;
}
-->
</style>

<? if (isset($lsessid)) : ?>
<script type="text/javascript">
	var flashvars = {};
	flashvars.hostname = "<?php echo FLASH_DOMAIN?>";
	flashvars.lsessid = "<?php echo $lsessid?>";
	flashvars.sessid = "<?php echo session_id()?>";	
	var params = {};	
	var attributes = {
            id: "Game"
        };
	swfobject.embedSWF("files/Game.swf?version=<?php echo Configure::read('Client.version'); ?> ", "flash-content", "100%", "100%", "10.0.0", "files/expressInstall.swf", flashvars, params, attributes);
</script>
		
<div id="game-box">
	<div id="flash-content">
		<a href="http://www.adobe.com/go/getflashplayer">
			The latest version of Flash player is required to play Tribal Hero. <img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif" alt="Get Adobe Flash player" />
		</a>
	</div>
</div>		
<? endif; ?>