<?php

class AddCobblestoneRoads extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column("cities", "road_theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
	}

	public function down() {
		$this->remove_column("cities", "road_theme_id");
	}
}