<?php

class IncreaseStructurePropertyNameLength extends Ruckusing_BaseMigration {

	public function up() {
        $this->change_column("structure_properties_list", "name", "string", array('limit' => 32,'null' => false));
	}//up()

	public function down() {
        $this->change_column("structure_properties_list", "name", "string", array('limit' => 16,'null' => false));
	}//down()
}
?>