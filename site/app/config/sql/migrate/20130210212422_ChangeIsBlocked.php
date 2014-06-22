<?php

class ChangeIsBlocked extends Ruckusing_BaseMigration {

	public function up() {
        $this->change_column('structures', 'is_blocked', 'integer', array('length' => 5, 'unsigned' => true));
        $this->change_column('troops', 'is_blocked', 'integer', array('length' => 5, 'unsigned' => true));
	}

	public function down() {

	}
}