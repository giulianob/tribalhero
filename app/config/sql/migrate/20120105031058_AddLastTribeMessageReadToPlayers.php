<?php

class AddLastTribeMessageReadToPlayers extends Ruckusing_BaseMigration {

	public function up() {
        $table = $this->create_table('message_board_read', array('options' => 'Engine=InnoDB', 'id' => false));        
        $table->column("id", "integer", array('auto_increment' => true, 'primary_key' => true));
		$table->column("player_id", "integer", array('null' => false, 'unsigned' => true));
		$table->column("message_board_thread_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("last_read", "datetime", array('null' => false));
        $table->finish();
		$this->add_index("message_board_read", array('player_id', 'message_board_thread_id'));
		$this->add_index("message_board_read", "last_read");
	}//up()

	public function down() {
		$this->drop_table('message_board_read');
	}//down()
}
?>