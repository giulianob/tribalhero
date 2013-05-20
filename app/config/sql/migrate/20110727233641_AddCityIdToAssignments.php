<?php

class AddCityIdToAssignments extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column("assignments", "city_id", "integer", array('auto_increment' => false, 'unsigned' => true));
		$this->add_index("assignments", "city_id", array('name' => 'idx_city_id'));
	}

	public function down() {
		$this->remove_index("assignments", "city_id", array('name' => 'idx_city_id'));
		$this->remove_column("assignments", "city_id");
	}
}