<?php

class AddAssignment extends Ruckusing_BaseMigration {

    public function strtoupper() {
        $table = $this->create_table('assignments', array('options' => 'Engine=InnoDB', 'id' => false));        
        $table->column("id", "integer", array('auto_increment' => false, 'primary_key' => true));
		$table->column("tribe_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("x", "integer", array('null' => false, 'unsigned' => true));
        $table->column("y", "integer", array('null' => false, 'unsigned' => true));
        $table->column("attack_time", "datetime", array('null' => false));
        $table->column("mode", "string", array('limit' => 20));
        $table->column("dispatch_count", "integer", array('null' => false, 'unsigned' => true));
        $table->finish();		
		$this->add_index("assignments", "tribe_id");


        $table = $this->create_table('assignments_list', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => false));
        $table->column("city_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $table->finish();
		
		$this->add_index("assignments_list", "id");
    }

    public function down() {
        $this->drop_table('assignments');
        $this->drop_table('assignments_list');
    }
}
?>