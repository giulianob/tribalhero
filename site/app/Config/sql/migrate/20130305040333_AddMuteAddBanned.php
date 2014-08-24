<?php

class AddMuteAddBanned extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->add_column("players", "muted", "datetime", array('null' => false, 'default' => '0001-01-01 00:00:00'));
        $this->add_column("players", "banned", "boolean", array('default' => 0));
	}

	public function down() {
        $this->remove_column("players", "muted");
        $this->remove_column("players", "banned");
		
	}
}
