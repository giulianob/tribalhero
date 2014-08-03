<?php

class RemoveAIFromReports extends Ruckusing_BaseMigration {

    public function up() {
        $this->execute("ALTER TABLE  `battles` CHANGE  `id`  `id` INT( 10 ) UNSIGNED NOT NULL");
        $this->execute("ALTER TABLE  `battle_reports` CHANGE  `id`  `id` INT( 10 ) UNSIGNED NOT NULL");
        $this->execute("ALTER TABLE  `battle_report_troops` CHANGE  `id`  `id` INT( 10 ) UNSIGNED NOT NULL");
    }

    public function down() {
        
    }

}

