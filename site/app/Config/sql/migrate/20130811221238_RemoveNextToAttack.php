<?php

class RemoveNextToAttack extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->remove_column('battle_managers', 'next_to_attack');
	}

	public function down() {

	}
}