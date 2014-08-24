<?php

class AddHasJoinedBattleToCombatObjects extends Ruckusing_BaseMigration {

    var $combatObjectTables = array(
        "combat_structures",
        "attack_combat_units",
        "defense_combat_units",
        "barbarian_tribe_combat_units",
        "stronghold_combat_units",
        "stronghold_combat_structures"
    );

	public function up() {
        foreach ($this->combatObjectTables as $table) {
            $this->add_column($table, "is_waiting_to_join_battle", "boolean", array('null' => false, 'default' => false));
            $this->execute("UPDATE `{$table}` SET is_waiting_to_join_battle=0");
        }
	}

	public function down() {
        foreach ($this->combatObjectTables as $table) {
            $this->remove_column($table, "is_waiting_to_join_battle");
        }
    }
}