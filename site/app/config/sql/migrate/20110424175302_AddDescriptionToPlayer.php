<?php

class AddDescriptionToPlayer extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column('players', 'description', 'text', array('null' => false, 'default' => ''));
	}

	public function down() {
		$this->remove_column('players', 'description');
	}
}