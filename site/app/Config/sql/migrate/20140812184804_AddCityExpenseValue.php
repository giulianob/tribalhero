<?php

class AddCityExpenseValue extends Ruckusing_BaseMigration {

	public function up() {
        $this->add_column('cities', 'expense_value', "decimal", array('scale' => 2, 'precision' => 10, 'null' => false, 'default' => 0));
	}

	public function down() {
		$this->remove_column("cities", "expense_value");
	}
}