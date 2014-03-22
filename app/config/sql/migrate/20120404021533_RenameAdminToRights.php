<?php

class RenameAdminToRights extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $this->change_column('players', 'admin', 'integer', array('unsigned' => true, 'limit' => 5));
        $this->rename_column('players', 'admin', 'rights');
    }

    public function down() {
        
    }

}