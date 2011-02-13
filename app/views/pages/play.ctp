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

<?php
if (isset($lsessid)) {
    
    $swfMtime = AppController::getCorrectMTime(WWW_ROOT . DS . 'files' . DS . 'Game.swf');
?>
    <script type="text/javascript">
        var flashvars = {};
        flashvars.hostname = "<?php echo FLASH_DOMAIN; ?>";
        flashvars.lsessid = "<?php echo $lsessid; ?>";
        flashvars.sessid = "<?php echo session_id(); ?>";
        flashvars.siteVersion = "<?php echo $swfMtime ?>";
        var params = {};
        var attributes = {
            id: "Game"
        };
        swfobject.embedSWF("files/Game.swf?mtime=<?php echo $swfMtime; ?>", "flash-content", "100%", "100%", "10.0.0", "js/swfobject/expressInstall.swf", flashvars, params, attributes);
    </script>

    <div id="game-box">
        <div id="flash-content">
            <a href="http://www.adobe.com/go/getflashplayer">
                                                			The latest version of Flash player is required to play Tribal Hero. <img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif" alt="Get Adobe Flash player" />
            </a>
        </div>
    </div>
<?php
}
?>