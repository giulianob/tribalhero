<?php

class AddOverviewReportFields extends Ruckusing_BaseMigration {

    public function up() {
        $this->add_column("battle_report_views", "battle_report_enter_id", "integer", array('null' => true, 'unsigned' => true));
        $this->add_column("battle_report_views", "battle_report_exit_id", "integer", array('null' => true, 'unsigned' => true));

        $this->query("UPDATE battle_report_views 
                    INNER JOIN battle_reports ON battle_reports.battle_id = battle_report_views.battle_id
                    INNER JOIN battle_report_troops ON battle_report_troops.group_id = battle_report_views.group_id AND battle_report_troops.battle_report_id = battle_reports.id AND battle_report_troops.state in (0)
                    SET battle_report_views.battle_report_enter_id = battle_reports.id"
        );
        
        $this->query("UPDATE battle_report_views 
                    INNER JOIN battle_reports ON battle_reports.battle_id = battle_report_views.battle_id
                    INNER JOIN battle_report_troops ON battle_report_troops.group_id = battle_report_views.group_id AND battle_report_troops.battle_report_id = battle_reports.id AND battle_report_troops.state in (6,4,3,2)
                    SET battle_report_views.battle_report_exit_id = battle_reports.id");
    }

    public function down() {
        $this->remove_column('battle_report_views', 'battle_report_enter_id');
        $this->remove_column('battle_report_views', 'battle_report_exit_id');
    }

}