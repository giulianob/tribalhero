<?php

class AddSoundMutedOption extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column("players", "sound_muted", "boolean", array('default' => 0));
	}
	
	public function down() {
		$this->remove_column("players", "sound_muted");
	}
}