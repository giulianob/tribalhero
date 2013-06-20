<?php

class AddTribeLogs extends Ruckusing_BaseMigration {

	public function up() {
        $table = $this->create_table('tribe_logs', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => true, 'null' => false, 'primary_key' => true));
        $table->column("tribe_id", "integer", array('null' => false, 'unsigned' => true));
		$table->column("created", "datetime", array('null' => false));
        $table->column("type", "integer", array('null' => false));
        $table->column("parameters", "text", array('null' => false));
        $table->finish();

	}//up()

	public function down() {
       $this->drop_table('tribe_logs');
 	}//down()
}
?>