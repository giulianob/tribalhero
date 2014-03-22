<?php

class AddInvitationIdColumn extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $this->add_column("players", "invitation_tribe_id", "integer", array("unsigned" => true, "null" => false));
    }

//strtoupper()

    public function down() {
        $this->remove_column('players', 'invitation_tribe_id');
    }

//down()
}

