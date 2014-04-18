<?php

class AddStoreTables extends Ruckusing_BaseMigration {
	public function up() {
		$this->add_column("battle_report_objects", "theme_id", "string", array('null' => true, 'limit' => 16));
		$this->add_column("combat_structures", "theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
		$this->add_column("structures", "theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
		$this->add_column("cities", "default_theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
		$this->add_column("cities", "wall_theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));

		// Set defautl theme for structures
		$this->query("UPDATE `battle_report_objects` SET theme_id='DEFAULT' WHERE type >= 1000"); 
	}

	public function down() {
		$this->remove_column("battle_report_objects", "theme_id");
		$this->remove_column("combat_structures", "theme_id");
		$this->remove_column("structures", "theme_id");
		$this->remove_column("cities", "default_theme_id");
		$this->remove_column("cities", "wall_theme_id");
	}
}