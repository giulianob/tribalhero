<?php

class FixGateHp extends Ruckusing_BaseMigration {

	public function up() {
        $gateHps = array(10000, 13500, 17300, 21500, 26200, 31300, 37100, 43400, 50300, 58000, 66500, 75800, 86300, 97800, 110500, 124600, 140100, 157200, 176200, 200000);

        for ($i = 0; $i < count($gateHps); $i++) {
            $level = $i + 1;
            $this->execute("UPDATE `strongholds` SET `gate` = {$gateHps[$i]} WHERE `gate` > 0 AND `main_battle_id` = 0 AND `gate_battle_id` = 0 AND `level` = {$level}");
        }
	}

	public function down() {

	}
}