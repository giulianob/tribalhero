<?php

class AddAtkDefPointToTribe extends Ruckusing_BaseMigration {

	public function strtoupper() {
    	$this->add_column("tribes", "attack_point", "integer", array('null' => false));
        $this->add_column("tribes", "defense_point", "integer", array('null' => false));
   
	}//strtoupper()

	public function down() {
        $this->remove_column("tribes", "attack_point");
        $this->remove_column("tribes", "defense_point");
	}//down()
}
?>