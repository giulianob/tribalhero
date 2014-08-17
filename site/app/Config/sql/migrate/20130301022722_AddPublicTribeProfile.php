<?php

class AddPublicTribeProfile extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->add_column("tribes", "public_desc", "text", array('null' => false, 'default' => ''));
	}

	public function down() {
		$this->remove_column("tribes","public_desc");
	}
}