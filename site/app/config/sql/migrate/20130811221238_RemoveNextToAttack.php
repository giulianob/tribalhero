<?php

class RemoveNextToAttack extends Ruckusing_BaseMigration {

	public function up() {
		$this->remove_column('battle_managers', 'next_to_attack');
	}

	public function down() {

	}
}