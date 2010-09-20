<?php
                // Generated by DatabaseGenerator program on 9/20/2010 12:33:37 AM
	            $structureKey = 'FARM_STRUCTURE';
	            $structureName = 'Farm';
	            $description = 'Uses the city\'s laborers to grow and produce crops.  Ability to build stable at higher level.';
	
	            $converted = '0';
	            $builtBy = array('name' => 'Town Center', 'key' => 'TOWNCENTER_STRUCTURE', 'level' => '1');
	
	            // Levels array should contain:
	            $levels = array(
		            array('description' => 'Allows a maximum of 20 laborers to work in the farm.', 'time' => 300, 'gold' => 0, 'crop' => 70, 'iron' => 0, 'labor' => 0, 'wood' => 80, 'hp' => 800, 'defense' => 100, 'range' => 14, 'stealth' => 14, 'weapon' => 'BARRICADE', 'maxLabor' => 20, 'requirements' => array('',
)),
array('description' => 'Increases the farm to support a maximum of 45 laborers.', 'time' => 550, 'gold' => 0, 'crop' => 150, 'iron' => 0, 'labor' => 0, 'wood' => 180, 'hp' => 1000, 'defense' => 125, 'range' => 14, 'stealth' => 14, 'weapon' => 'BARRICADE', 'maxLabor' => 45, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 70 laborers.', 'time' => 1000, 'gold' => 0, 'crop' => 220, 'iron' => 0, 'labor' => 0, 'wood' => 300, 'hp' => 1240, 'defense' => 155, 'range' => 14, 'stealth' => 14, 'weapon' => 'BARRICADE', 'maxLabor' => 70, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 95 laborers.', 'time' => 1820, 'gold' => 0, 'crop' => 280, 'iron' => 0, 'labor' => 0, 'wood' => 360, 'hp' => 1560, 'defense' => 195, 'range' => 15, 'stealth' => 15, 'weapon' => 'BARRICADE', 'maxLabor' => 95, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 120 laborers.  Allows to build Stable.', 'time' => 3310, 'gold' => 0, 'crop' => 440, 'iron' => 0, 'labor' => 0, 'wood' => 590, 'hp' => 1920, 'defense' => 240, 'range' => 15, 'stealth' => 15, 'weapon' => 'BARRICADE', 'maxLabor' => 120, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 145 laborers.', 'time' => 6040, 'gold' => 0, 'crop' => 700, 'iron' => 0, 'labor' => 0, 'wood' => 970, 'hp' => 2400, 'defense' => 300, 'range' => 15, 'stealth' => 15, 'weapon' => 'BARRICADE', 'maxLabor' => 145, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 170 laborers.', 'time' => 11000, 'gold' => 0, 'crop' => 1110, 'iron' => 0, 'labor' => 0, 'wood' => 1600, 'hp' => 3000, 'defense' => 375, 'range' => 16, 'stealth' => 16, 'weapon' => 'BARRICADE', 'maxLabor' => 170, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 195 laborers.', 'time' => 20050, 'gold' => 0, 'crop' => 1760, 'iron' => 0, 'labor' => 0, 'wood' => 2640, 'hp' => 3720, 'defense' => 465, 'range' => 16, 'stealth' => 16, 'weapon' => 'BARRICADE', 'maxLabor' => 195, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 215 laborers.', 'time' => 36530, 'gold' => 0, 'crop' => 2790, 'iron' => 0, 'labor' => 0, 'wood' => 4350, 'hp' => 4640, 'defense' => 580, 'range' => 16, 'stealth' => 16, 'weapon' => 'BARRICADE', 'maxLabor' => 215, 'requirements' => array()),
array('description' => 'Increases the farm to support a maximum of 240 laborers.', 'time' => 66570, 'gold' => 0, 'crop' => 4430, 'iron' => 0, 'labor' => 0, 'wood' => 7170, 'hp' => 5760, 'defense' => 720, 'range' => 17, 'stealth' => 17, 'weapon' => 'BARRICADE', 'maxLabor' => 240, 'requirements' => array()),

	            );

                include 'structure_view.ctp';
            