<?php

class AddTribeToRanking extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column("rankings", "tribe_id", "integer", array('null' => false, 'unsigned' => true));
	}//up()

	public function down() {
        $this->remove_column("rankings", "tribe_id");
	}//down()
}
?>