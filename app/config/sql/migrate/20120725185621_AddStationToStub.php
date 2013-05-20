<?php

class AddStationToStub extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column('troop_stubs', 'station_id', 'integer', array('null' => false, 'unsigned' => true));
        $this->add_column('troop_stubs', 'station_type', 'boolean', array('limit' => 3, 'null' => false, 'unsigned' => true));
		$this->remove_column("troop_stubs", "stationed_city_id");
	}//up()

	public function down() {
		$this->remove_column("troop_stubs", "station_id");
		$this->remove_column("troop_stubs", "station_type");
		$this->add_column('troop_stubs', 'stationed_city_id', 'integer', array('null' => false, 'unsigned' => true));
	}//down()
}
?>