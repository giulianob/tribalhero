<?php

class AddTribeToRanking extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->add_column("rankings", "tribe_id", "integer", array('null' => false, 'unsigned' => true));
	}//strtoupper()

	public function down() {
        $this->remove_column("rankings", "tribe_id");
	}//down()
}
?>