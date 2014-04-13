<?php

class AddLastdeletedtribe extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column("players", "last_deleted_tribe", "datetime", array('null' => false, 'default' => '0001-01-01 00:00:00'));
	}

	public function down() {
		$this->remove_column("players", "last_deleted_tribe");
	}
}