<?php

class RemoveDefFromStats extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->remove_column('troop_templates_list', 'defense');
	}//strtoupper()

	public function down() {
		$this->add_column('troop_templates_list', 'defense', 'smallinteger', array('length' => 5, 'unsigned' => true));
	}//down()
}
?>