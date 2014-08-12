<?php

class RenameTribeColumns extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $this->rename_column('tribes', 'id', 'player_id');
        $this->remove_column('tribes', 'owner_id');
    }

    public function down() {
        $this->rename_column('tribes', 'player_id', 'id');
        $this->add_column('tribes', 'owner_id', "integer", array('null' => false, 'unsigned' => true));
        $this->execute("UPDATE `tribes` SET owner_id = id");
    }

}