<?php

class AddBarbarianTribeBattle extends Ruckusing_BaseMigration {

	public function up() {
        $table = $this->create_table('barbarian_tribe_combat_groups', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('unsigned' => true, 'primary_key' => true));
        $table->column("battle_id", "integer", array('unsigned' => true, 'primary_key' => true));
        $table->column("barbarian_tribe_id", "integer", array('unsigned' => true));
        $table->finish();

        $this->execute("
            CREATE TABLE IF NOT EXISTS `barbarian_tribe_combat_units` (
              `id` int(10) unsigned NOT NULL,
              `battle_id` int(10) unsigned NOT NULL,
              `barbarian_tribe_id` int(10) unsigned NOT NULL,
              `last_round` int(10) unsigned NOT NULL,
              `rounds_participated` int(10) NOT NULL,
              `damage_dealt` decimal(10,2) NOT NULL,
              `damage_received` decimal(10,2) NOT NULL,
              `group_id` int(10) unsigned NOT NULL,
              `level` tinyint(3) unsigned NOT NULL,
              `type` smallint(5) unsigned NOT NULL,
              `count` smallint(5) unsigned NOT NULL,
              `left_over_hp` decimal(10,2) NOT NULL,
              `damage_min_dealt` smallint(5) unsigned NOT NULL,
              `damage_max_dealt` smallint(5) unsigned NOT NULL,
              `damage_min_received` smallint(5) unsigned NOT NULL,
              `damage_max_received` smallint(5) unsigned NOT NULL,
              `hits_dealt` smallint(5) unsigned NOT NULL,
              `hits_dealt_by_unit` int(10) unsigned NOT NULL,
              `hits_received` smallint(5) unsigned NOT NULL,
              PRIMARY KEY (`battle_id`,`id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;
        ");

	}

	public function down() {

	}
}