<?php

class AddedFirstTimeAttackedToPlayer extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column('players', 'never_attacked', 'boolean', array('default' => 0));
	}

	public function down() {
        $this->remove_column('players', 'never_attacked');
	}
}