<?php

class AddNearbyCitiesToStrongholds extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column("strongholds", "nearby_cities", 'smallinteger', array('length' => 5, 'unsigned' => true, 'null' => false, 'default' =>0) );
	}//up()

	public function down() {
		$this->remove_column("strongholds", "nearby_cities");
	}//down()
}
?>