<?php

class ChangeBaseAtkHpToDecimal extends Ruckusing_BaseMigration {

	public function up() {
		$this->change_column("structures", "hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("battle_report_objects", "hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("battle_report_objects", "damage_taken", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("battle_report_objects", "damage_dealt", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
	}//up()

	public function down() {
		$this->change_column("structures", "hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("battle_report_objects", "hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("battle_report_objects", "damage_taken", "integer", array('null' => false) );
		$this->change_column("battle_report_objects", "damage_dealt", "integer", array('null' => false) );
	}//down()
}
?>
