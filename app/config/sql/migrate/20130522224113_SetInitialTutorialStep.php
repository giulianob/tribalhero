<?php

class SetInitialTutorialStep extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->execute("UPDATE `players` SET tutorial_step = 10");
	}

	public function down() {

	}
}