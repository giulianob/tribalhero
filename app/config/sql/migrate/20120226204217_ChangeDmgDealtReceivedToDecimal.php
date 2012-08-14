<?php

class ChangeDmgDealtReceivedToDecimal extends Ruckusing_BaseMigration {

	public function up() {
		$this->change_column("combat_units", "damage_dealt", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_units", "damage_received", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_structures", "damage_dealt", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_structures", "damage_received", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
	}//up()

	public function down() {
		$this->change_column("combat_units", "damage_dealt", "integer", array('null' => false) );
		$this->change_column("combat_units", "damage_received", "integer", array('null' => false) );
		$this->change_column("combat_structures", "damage_dealt", "integer", array('null' => false) );
		$this->change_column("combat_structures", "damage_received", "integer", array('null' => false) );
	}//down()
}
?>