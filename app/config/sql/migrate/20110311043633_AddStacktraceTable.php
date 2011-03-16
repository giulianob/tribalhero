<?php

class AddStacktraceTable extends Ruckusing_BaseMigration {

    public function up() {
        $table = $this->create_table('stacktraces', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => true, 'primary_key' => true));
        $table->column("message", "text");
        $table->column("player_id", "integer");
		$table->column("occurrences", "integer");
        $table->column("player_name", "string", array('limit' => 32));
        $table->column("flash_version", "string", array('limit' => 128));
        $table->column("game_version", "string", array('limit' => 8));
        $table->column("browser_version", "string", array('limit' => 128));
        $table->column("created", "datetime");
		$table->column("updated", "datetime");
        $table->finish();

        $this->add_index('stacktraces', 'player_id');
		$this->add_index('stacktraces', 'updated');
    }

    public function down() {
        $this->drop_table('stacktraces');
    }

}