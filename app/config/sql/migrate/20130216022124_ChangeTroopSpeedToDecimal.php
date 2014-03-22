<?php

class ChangeTroopSpeedToDecimal extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->change_column("troops", "speed", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
	}//strtoupper()

	public function down() {
		$this->change_column("troops", "speed", 'smallinteger', array('length' => 3, 'unsigned' => true) );
	}//down()
}
?>