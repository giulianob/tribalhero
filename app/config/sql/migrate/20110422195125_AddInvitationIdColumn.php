<?php

class AddInvitationIdColumn extends Ruckusing_BaseMigration {

    public function up() {
        $this->add_column("players", "invitation_tribe_id", "integer", array("unsigned" => true, "null" => false));
    }

//up()

    public function down() {
        $this->remove_column('players', 'invitation_tribe_id');
    }

//down()
}

