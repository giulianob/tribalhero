<?php

class ReduceGateHp extends Ruckusing_BaseMigration {

	public function up() {
        $gateHps = array(5000, 6000, 7000, 8000, 9500, 11000, 12500, 14500, 16500, 18500, 21000, 23500, 26000, 29000, 32000, 35000, 38500, 42000, 46000, 50000);

        for ($i = 0; $i < count($gateHps); $i++) {
            $level = $i + 1;
            $this->execute("UPDATE `strongholds` SET `gate` = {$gateHps[$i]} WHERE `gate` > {$gateHps[$i]} AND `main_battle_id` = 0 AND `gate_battle_id` = 0 AND `level` = {$level}");
        }
		
		$this->add_column('strongholds', 'bonus_days', "decimal", array('scale' => 2, 'precision' =>10, 'null' => false, 'default' =>0) );
	}

	public function down() {
		$this->remove_column("strongholds", "bonus_days");
	}
}
?>