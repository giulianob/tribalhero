<?php

class ChangeAtkHpTodecimal extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->change_column("troop_templates_list", "max_hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("troop_templates_list", "attack", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_units", "left_over_hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_structures", "max_hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_structures", "hp", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
		$this->change_column("combat_structures", "attack", "decimal", array('scale' => 2, 'precision' =>10, 'null' => false) );
	}//strtoupper()

	public function down() {
		$this->change_column("troop_templates_list", "max_hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("troop_templates_list", "attack",'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("combat_units", "left_over_hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("combat_structures", "hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("combat_structures", "max_hp", 'smallinteger', array('length' => 5, 'unsigned' => true) );
		$this->change_column("combat_structures", "attack", 'smallinteger', array('length' => 5, 'unsigned' => true) );
	}//down()
}
?>