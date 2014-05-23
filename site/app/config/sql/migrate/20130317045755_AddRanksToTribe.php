<?php

class AddRanksToTribe extends Ruckusing_BaseMigration {

	public function up() {
		$json = <<<EOT
[{"Id":0,"Name":"Chief","Permission":1},{"Id":1,"Name":"Elder","Permission":86},{"Id":2,"Name":"Protector","Permission":80},{"Id":3,"Name":"Aggressor","Permission":64},{"Id":4,"Name":"Tribesman","Permission":0}]
EOT;
		$this->add_column("tribes", "ranks", "string", array('limit' => 4096));

        $this->execute("UPDATE `tribes` SET `ranks` = \"{$this->quote_string($json)}\"");
		$this->execute("UPDATE `tribesmen` SET `rank` = 4 WHERE `rank` = 2");

	}//up()

	public function down() {
		$this->remove_column("tribes", "ranks");
	}//down()
}
?>