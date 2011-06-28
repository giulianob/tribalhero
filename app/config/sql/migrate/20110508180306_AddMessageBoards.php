<?php

class AddMessageBoards extends Ruckusing_BaseMigration {

    public function up() {
        $table = $this->create_table('message_board_threads', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => true, 'primary_key' => true));
        $table->column("tribe_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("created", "datetime", array('null' => false));
        $table->column("player_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("last_post_date", "datetime", array('null' => false));
        $table->column("last_post_player_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("message_board_post_count", "integer", array('null' => false));
        $table->column("subject", "string", array('null' => false, 'length' => 150));
        $table->column("message", "text", array('null' => false));
        $table->column("deleted", "boolean", array('null' => false));
        $table->column("sticky", "boolean", array('null' => false));
        $table->finish();

        $this->add_index('message_board_threads', array('tribe_id', 'deleted'));
        $this->add_index('message_board_threads', array('sticky', 'last_post_date'));

        $table = $this->create_table('message_board_posts', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => true, 'primary_key' => true));
        $table->column("message_board_thread_id", "integer", array('null' => false));
        $table->column("player_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("created", "datetime", array('null' => false));
        $table->column("message", "text", array('null' => false));
        $table->column("deleted", "boolean", array('null' => false));
        $table->finish();

        $this->add_index('message_board_posts', array('message_board_thread_id', 'created'));
    }

    public function down() {
        $this->drop_table('message_board_threads');
        $this->drop_table('message_board_posts');
    }

}