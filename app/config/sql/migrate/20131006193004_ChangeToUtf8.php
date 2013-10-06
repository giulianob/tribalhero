<?php

class ChangeToUtf8 extends Ruckusing_BaseMigration {

	public function up() {
		$this->query("ALTER DATABASE CHARACTER SET utf8;");
	}

	public function down() {
		throw new Exception("Cannot go back down");
	}
}