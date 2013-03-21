<?php

class AddRanksToTribe extends Ruckusing_BaseMigration {

	public function up() {
		$json = <<<EOT
[{"Id":0,"Name":"Chief","Permission":"All"},{"Id":1,"Name":"Elder","Permission":"AssignmentCreate, Repair, Kick, Invite"},{"Id":2,"Name":"Protector","Permission":"AssignmentCreate, Repair"},{"Id":3,"Name":"Aggressor","Permission":"AssignmentCreate"},{"Id":4,"Name":"Tribesman","Permission":"None"}]
EOT;
		$this->add_column("tribes", "ranks", "string", array('limit' => 4096, 'default' =>$json));
		
		$this->execute("UPDATE `tribesmen` SET `rank` = 4 WHERE `rank` = 2");
	}//up()

	public function down() {
		$this->remove_column("tribes", "ranks");
	}//down()
}
?>