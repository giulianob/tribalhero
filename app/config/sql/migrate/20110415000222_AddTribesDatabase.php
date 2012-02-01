<?php

class AddTribesDatabase extends Ruckusing_BaseMigration {

    public function up() {
        $table = $this->create_table('tribesmen', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("player_id", "integer", array('auto_increment' => false, 'primary_key' => true, 'unsigned' => true));
        $table->column("tribe_id", "integer", array('null' => false, 'unsigned' => true));
        $table->column("join_date", "datetime", array('null' => false));
        $table->column("rank", "boolean", array('limit' => 3, 'unsigned' => true));
        $table->column("crop", "integer", array('null' => false));
        $table->column("gold", "integer", array('null' => false));
        $table->column("iron", "integer", array('null' => false));
        $table->column("wood", "integer", array('null' => false));
        $table->finish();



        $table = $this->create_table('tribes', array('options' => 'Engine=InnoDB', 'id' => false));
        $table->column("id", "integer", array('auto_increment' => false, 'primary_key' => true, 'unsigned' => true));
        $table->column("name", "string", array('length' => 20));
        $table->column("desc", "text", array('null' => false, 'default' => ''));
        $table->column("level", "boolean", array('limit' => 3, 'unsigned' => true));
        $table->column("owner_id", "integer", array('null' => false, 'unsigned' => true));
        $table->finish();
    }

    public function down() {
        $this->drop_table('tribesmen');
        $this->drop_table('tribes');
    }

}