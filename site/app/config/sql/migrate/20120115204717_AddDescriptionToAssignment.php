<?php

class AddDescriptionToAssignment extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column('assignments', 'description', 'string', array('null' => false, 'default' => '', 'limit' => 250));
	}//up()

	public function down() {
		$this->remove_column('assignments', 'description');
	}//down()
}
?>