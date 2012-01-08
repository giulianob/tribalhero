<?php

class AddOverviewReportFields extends Ruckusing_BaseMigration {

    public function up() {
        $this->add_column("battle_report_views", "battle_report_enter_id", "integer", array('null' => true, 'unsigned' => true));
        $this->add_column("battle_report_views", "battle_report_exit_id", "integer", array('null' => true, 'unsigned' => true));        
    }

    public function down() {
        
    }
}