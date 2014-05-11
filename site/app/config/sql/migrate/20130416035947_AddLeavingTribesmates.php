<?php

class AddLeavingTribesmates extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column("tribes", "leaving_tribesmates", "mediumtext");
        $this->query("UPDATE `tribes` SET leaving_tribesmates = '[]'");
	}

	public function down() {
        $this->remove_column("tribes", "leaving_tribesmates");
	}
}