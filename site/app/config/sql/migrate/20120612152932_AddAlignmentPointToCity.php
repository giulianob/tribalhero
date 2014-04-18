<?php

class AddAlignmentPointToCity extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column('cities', 'alignment_point', "decimal", array('scale' => 2, 'precision' =>10, 'null' => false, 'default' =>50) );
	}//up()

	public function down() {
		$this->remove_column("cities", "alignment_point");
	}//down()
}
?>