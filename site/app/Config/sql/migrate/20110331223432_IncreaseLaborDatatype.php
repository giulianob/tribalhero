<?php

class IncreaseLaborDatatype extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $this->change_column('structures', 'labor', 'smallinteger', array('length' => 5, 'unsigned' => true));
    }

    public function down() {
        $this->change_column('structures', 'labor', 'tinyinteger', array('length' => 3, 'unsigned' => true));
    }

}