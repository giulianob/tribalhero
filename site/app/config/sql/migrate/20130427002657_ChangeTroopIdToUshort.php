<?php

class ChangeTroopIdToUshort extends Ruckusing_BaseMigration {

	public function up() {
        $this->change_column("assignments_list", "stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("troop_stubs", "id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("troop_stubs_list", "id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("troop_templates", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("troop_templates_list", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("troops", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("city_defensive_combat_groups", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("defense_combat_units", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
        $this->change_column("battle_report_views", "troop_stub_id", "smallinteger", array('null' => false, 'unsigned' => true, 'limit' => 5));
	}

	public function down() {
        $this->change_column("assignments_list", "stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("troop_stubs", "id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("troop_stubs_list", "id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("troop_templates", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("troop_templates_list", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("troops", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("city_defensive_combat_groups", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("defense_combat_units", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
        $this->change_column("battle_report_views", "troop_stub_id", "boolean", array('null' => false, 'unsigned' => true, 'limit' => 3));
	}
}