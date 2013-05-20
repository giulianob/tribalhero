<?php

class AddAchievements extends Ruckusing_BaseMigration {

	public function up() {
        $table = $this->create_table('achievements', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("player_id", "integer", array('null' => false, 'unsigned' => true, 'primary_key' => true));
        $table->finish();

        $table = $this->create_table('achievements_list', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("player_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("id", "integer", array('null' => false));
        $table->column("type", "string", array('null' => false, 'limit' => 64));
        $table->column("icon", "string", array('null' => false, 'limit' => 32));
        $table->column("tier", "boolean", array('null' => false, 'unsigned' => true));
        $table->column("title", "string", array('null' => false, 'limit' => 128));
        $table->column("description", "string", array('null' => false, 'limit' => 128));
        $table->finish();

        $this->add_index("achievements_list", "player_id");
	}

	public function down() {
        $this->drop_table('achievements');
        $this->drop_table('achievements_list');
	}
}