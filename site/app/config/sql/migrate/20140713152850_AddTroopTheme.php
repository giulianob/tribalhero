<?php

class AddTroopTheme extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column("troops", "theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
        $this->add_column("cities", "troop_theme_id", "string", array('default' => 'DEFAULT', 'null' => false, 'limit' => 16));
	}

	public function down() {
        $this->remove_column("troops", "theme_id");
        $this->remove_column("cities", "troop_theme_id");
	}
}