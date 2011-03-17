<?php

class AddCityValue extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column('cities', 'value', 'smallinteger', array('unsigned' => true, 'null' => false));
	}

	public function down() {
            $this->remove_column('cities', 'value');
	}
}