<?php

class AddAdminColumn extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $this->add_column('players', 'admin', 'boolean', array('default' => 0));
    }

    public function down() {
        $this->remove_column('players', 'admin');
    }
}