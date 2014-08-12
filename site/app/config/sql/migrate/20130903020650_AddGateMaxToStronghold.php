<?php

class AddGateMaxToStronghold extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->add_column('strongholds', 'gate_max', 'integer', array('null' => false));
		$this->execute("UPDATE `strongholds` SET gate_max = gate");
	}//strtoupper()

	public function down() {
		$this->remove_column('strongholds', 'gate_max');
	}//down()
}
?>