<?php

class AddGateMaxToStronghold extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column('strongholds', 'gate_max', 'integer', array('null' => false));
		$this->execute("UPDATE `strongholds` SET gate_max = gate");
	}//up()

	public function down() {
		$this->remove_column('strongholds', 'gate_max');
	}//down()
}
?>