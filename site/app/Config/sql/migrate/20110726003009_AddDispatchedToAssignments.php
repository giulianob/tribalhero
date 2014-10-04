<?php

class AddDispatchedToAssignments extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column("assignments_list", "dispatched", "boolean", array('null' => false, 'unsigned' => true));
	}

	public function down() {
		$this->remove_column("assignments_list", "dispatched");
	}
}