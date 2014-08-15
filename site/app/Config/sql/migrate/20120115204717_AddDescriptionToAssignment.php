<?php

class AddDescriptionToAssignment extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column('assignments', 'description', 'string', array('null' => false, 'default' => '', 'limit' => 250));
	}//strtoupper()

	public function down() {
		$this->remove_column('assignments', 'description');
	}//down()
}
?>