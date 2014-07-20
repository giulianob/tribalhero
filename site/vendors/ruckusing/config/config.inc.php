<?php

//--------------------------------------------
//Overall file system configuration paths
//--------------------------------------------

//These might already be defined, so wrap them in checks

if (!defined('DS')) {
	define('DS', DIRECTORY_SEPARATOR);
}
	
if (!defined('APP_DIR')) {
	define('APP_DIR', dirname(dirname(RUCKUSING_BASE)) . DS . 'app');
}
	
// DB table where the version info is stored
if(!defined('RUCKUSING_SCHEMA_TBL_NAME')) {
	define('RUCKUSING_SCHEMA_TBL_NAME', 'schema_info');
}

if(!defined('RUCKUSING_TS_SCHEMA_TBL_NAME')) {
	define('RUCKUSING_TS_SCHEMA_TBL_NAME', 'schema_migrations');
}

//Parent of migrations directory.
//Where schema.txt will be placed when 'db:schema' is executed
if(!defined('RUCKUSING_DB_DIR')) {
	define('RUCKUSING_DB_DIR', APP_DIR . DS . 'config' . DS . 'sql');
}

//Where the actual migrations reside
if(!defined('RUCKUSING_MIGRATION_DIR')) {
	define('RUCKUSING_MIGRATION_DIR', APP_DIR . DS . 'config' . DS . 'sql' . DS . 'migrate');
}

?>