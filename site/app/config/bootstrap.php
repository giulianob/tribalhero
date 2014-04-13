<?php
date_default_timezone_set('UTC');

define('SYSTEM_STATUS_PASS', 'Jf43JfmasDL10G3ga4Dac93');
	
define('TROOP_STATE_ENTERING', 0);
define('TROOP_STATE_STAYING', 1);
define('TROOP_STATE_EXITING', 2);
define('TROOP_STATE_DYING', 3);
define('TROOP_STATE_RETREATING', 4);
define('TROOP_STATE_REINFORCED', 5);
define('TROOP_STATE_OUT_OF_STAMINA', 6);

define('BATTLE_VIEW_MIN_ROUND', 5);

define('PLAYER_RIGHTS_BASIC', 0);
define('PLAYER_RIGHTS_MODERATOR', 1);
define('PLAYER_RIGHTS_ADMIN', 2);
define('PLAYER_RIGHTS_BUREAUCRAT', 3);

define('FLASH_DOMAIN', '*.tribalhero.com');

App::import('Vendor', 'util');