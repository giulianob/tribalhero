<?php

class AddTribeId extends Ruckusing_BaseMigration {

	public function strtoupper() {
        $this->remove_index('tribes', 'player_id', array('name' => 'PRIMARY'));
        $this->rename_column('tribes', 'player_id', 'owner_player_id');

        $this->add_column('tribes', 'id', 'integer', array('null' => false, 'unsigned' => true));
        $this->execute("UPDATE `tribes` SET id = owner_player_id");
        $this->execute("ALTER TABLE `tribes` ADD PRIMARY KEY(`id`)");
	}

	public function down() {

	}
}