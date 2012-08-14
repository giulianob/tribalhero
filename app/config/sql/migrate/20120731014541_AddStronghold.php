<?php

class AddStronghold extends Ruckusing_BaseMigration {

	public function up() {
        $table = $this->create_table('strongholds', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => false, 'primary_key' => true, 'unsigned' => true));
        $table->column("name", "string", array('length' => 20));
        $table->column("level", "boolean", array('limit' => 3, 'unsigned' => true));
        $table->column("tribe_id", "integer", array('null' => true, 'unsigned' => true));
		$table->column("state", "boolean", array('null' => false, 'limit' => 3, 'unsigned' => true));
		$table->column("gate", "integer", array('null' => false));
        $table->column("x", "integer", array('null' => false, 'unsigned' => true));
        $table->column("y", "integer", array('null' => false, 'unsigned' => true));
        $table->finish();
	}//up()

	public function down() {
        $this->drop_table('strongholds');
	}//down()
}
?>