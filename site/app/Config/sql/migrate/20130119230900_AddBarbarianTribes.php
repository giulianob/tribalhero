<?php

class AddBarbarianTribes extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->execute("
        CREATE TABLE IF NOT EXISTS `barbarian_tribes` (
            `id` int(11) unsigned NOT NULL,
            `x` int(11) unsigned NOT NULL,
            `y` int(11) unsigned NOT NULL,
            `level` tinyint(3) unsigned NOT NULL,
            `camp_remains` tinyint(3) unsigned NOT NULL,
            `resource_crop` int(11) NOT NULL,
            `resource_wood` int(11) NOT NULL,
            `resource_iron` int(11) NOT NULL,
            `resource_gold` int(11) NOT NULL,
            `in_world` tinyint(1) NOT NULL,
            `state` tinyint(3) unsigned NOT NULL,
            `state_parameters` text NOT NULL,
            `created` datetime NOT NULL,
            `last_attacked` datetime NOT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;
        ");
	}

	public function down() {

	}
}