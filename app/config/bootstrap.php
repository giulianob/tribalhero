<?php
date_default_timezone_set('UTC');

/**
 * General application defines. These should move later on or at least use Configure class.
 */
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

/**
 * Enable the Dispatcher filters for plugin assets, and CacheHelper.
 */
Configure::write('Dispatcher.filters', array(
    'AssetDispatcher',
    'CacheDispatcher'
));

/**
 * Logging locations
 */
CakeLog::config('debug', array(
    'engine' => 'FileLog',
    'types' => array('notice', 'info', 'debug'),
    'file' => 'debug',
));
CakeLog::config('error', array(
    'engine' => 'FileLog',
    'types' => array('warning', 'error', 'critical', 'alert', 'emergency'),
    'file' => 'error',
));

/**
 * The settings below can be used to set additional paths to models, views and controllers.
 *
 * App::build(array(
 *     'Plugin' => array('/full/path/to/plugins/', '/next/full/path/to/plugins/'),
 *     'Model' =>  array('/full/path/to/models/', '/next/full/path/to/models/'),
 *     'View' => array('/full/path/to/views/', '/next/full/path/to/views/'),
 *     'Controller' => array('/full/path/to/controllers/', '/next/full/path/to/controllers/'),
 *     'Model/Datasource' => array('/full/path/to/datasources/', '/next/full/path/to/datasources/'),
 *     'Model/Behavior' => array('/full/path/to/behaviors/', '/next/full/path/to/behaviors/'),
 *     'Controller/Component' => array('/full/path/to/components/', '/next/full/path/to/components/'),
 *     'View/Helper' => array('/full/path/to/helpers/', '/next/full/path/to/helpers/'),
 *     'Vendor' => array('/full/path/to/vendors/', '/next/full/path/to/vendors/'),
 *     'Console/Command' => array('/full/path/to/shells/', '/next/full/path/to/shells/'),
 *     'Locale' => array('/full/path/to/locale/', '/next/full/path/to/locale/')
 * ));
 *
 */

/**
 * Custom Inflector rules, can be set to correctly pluralize or singularize table, model, controller names or whatever other
 * string is passed to the inflection functions
 *
 * Inflector::rules('singular', array('rules' => array(), 'irregular' => array(), 'uninflected' => array()));
 * Inflector::rules('plural', array('rules' => array(), 'irregular' => array(), 'uninflected' => array()));
 *
 */

/**
 * Plugins need to be loaded manually, you can either load them one by one or all of them in a single call
 * Uncomment one of the lines below, as you need. make sure you read the documentation on CakePlugin to use more
 * advanced ways of loading plugins
 *
 * CakePlugin::loadAll(); // Loads all plugins at once
 * CakePlugin::load('DebugKit'); //Loads a single plugin named DebugKit
 *
 */

App::import('Vendor', 'util');
App::import('Vendor', 'base2n');

CakePlugin::loadAll();