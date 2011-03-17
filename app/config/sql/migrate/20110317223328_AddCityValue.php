<?php

class AddCityValue extends Ruckusing_BaseMigration {

    public function up() {
        $this->add_column('cities', 'value', 'smallinteger', array('unsigned' => true, 'null' => false, 'default' => 1));
    }

    public function down() {
        $this->remove_column('cities', 'value');
    }

}