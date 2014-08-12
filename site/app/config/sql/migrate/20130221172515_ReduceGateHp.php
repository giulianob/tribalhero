<?php

class ReduceGateHp extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column('strongholds', 'bonus_days', "decimal", array('scale' => 2, 'precision' =>10, 'null' => false, 'default' =>0) );
	}

	public function down() {
		$this->remove_column("strongholds", "bonus_days");
	}
}
?>