<?php

class AddNlsDescToPassive extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column('passive_actions', 'nls_description', 'string', array('limit' => 16));
	}

	public function down() {
		$this->remove_column('passive_actions', 'nls_description');
	}
}