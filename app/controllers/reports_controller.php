<?php

class ReportsController extends AppController {
    var $uses = array('Battle');
    var $helpers = array('TimeAdv');

    var $allowedFromGame = array('index_local', 'view_local', 'index_remote', 'view_remote');


    var $troop_states_pst = array(
            'joined the battle',
            'stayed',
            'left the battle',
            'died',
            'retreated',
            'gained new units'
    );

    function beforeFilter() {
        if (!empty($this->params['named'])) {
            $this->params['form'] = $this->params['named'];
        }
        else {
            Configure::write('debug', 0);
        }

        $this->layout = 'ajax';

        parent::beforeFilter();
    }

    function index_local() {
        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $this->params['form']['playerId']
        )));

        $this->Battle =& ClassRegistry::init('Battle');

        $this->paginate = $this->Battle->listInvasionReports(array_keys($cities), true) + array(
                'limit' => 15,
                'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate('Battle');

        $this->set('battle_reports', $reports);
    }

    function view_local() {
        if (empty($this->params['form']['id'])) {
            $this->render(false);
            return;
        }

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $this->params['form']['playerId']
        )));

        $report = $this->Battle->viewInvasionReport(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $reports = $this->Battle->viewBattle($this->params['form']['id']);

        $this->set('troop_states_pst', $this->troop_states_pst);
        $this->set('main_report', $report);
        $this->set('battle_reports', $reports);

        $this->render('view');
    }

    function index_remote() {
        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $this->params['form']['playerId']
        )));

        $options = $this->Battle->listAttackReports(array_keys($cities), true);

        $this->paginate = $options + array(
                'recursive' => '-1',
                'limit' => 15,
                'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate('BattleReportView');

        $this->set('battle_reports', $reports);
    }

    function view_remote() {
        if (empty($this->params['form']['id'])) {
            $this->render(false);
            return;
        }

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $this->params['form']['playerId']
        )));

        $report = $this->Battle->viewAttackReport(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $reports = $this->Battle->viewBattle($report['BattleReportView']['battle_id']);

        // If set, will render only this snapshot instead of the whole battle. This is used to show only the outcome of the battle instead of the whole thing.
        $renderOnlySnapshot = null;

        // If the player is an attacker and hasn't been in the battle for at least X # of rounds then he can only see the outcome
        if ($report['BattleReportView']['is_attacker']) {
            $enterRound = 0;
            $enterSnapshot = null;
            $enterReport = null;
            $foundExit = false;
            $groupId = $report['BattleReportView']['group_id'];

            foreach ($reports as $battle_report) {
                $round = $battle_report['BattleReport']['round'];

                // Loop through each troop that was in this report to find the guy with the groupId specified above
                foreach ($battle_report['BattleReportTroop'] as $snapshot) {
                    if ($snapshot['group_id'] != $groupId) continue;

                    $state = $snapshot['state'];

                    switch($state) {
                        case TROOP_STATE_ENTERING:
                            $enterRound = $round;
                            $enterSnapshot = $snapshot;
                            $enterReport = $battle_report['BattleReport'];
                            break;
                        case TROOP_STATE_EXITING:
                        case TROOP_STATE_DYING:
                        case TROOP_STATE_RETREATING:
                            $foundExit = true;
                            break;
                    }

                    if ($foundExit) {
                        // If player died or didn't last for more than the min rounds then he can only see the outcome report
                        if ($state == TROOP_STATE_DYING || $round - $enterRound < BATTLE_VIEW_MIN_ROUNDS) {
                            // Put the start and exit snapshot of their own troops since that's all they can see.
                            $renderOnlySnapshot = array(
                                    array('BattleReport' => $enterReport, 'snapshot' => $enterSnapshot),
                                    array('BattleReport' => $battle_report['BattleReport'], 'snapshot' => $snapshot)
                            );
                        }

                        break;
                    }
                }

                // We found where the player exited the battle
                if ($foundExit)
                    break;
            }
        }

        $this->set('main_report', $report);
        $this->set('troop_states_pst', $this->troop_states_pst);

        if ($renderOnlySnapshot == null) {
            $this->set('battle_reports', $reports);
            $this->render('view');
        }
        else {
            $this->set('battle_reports', $renderOnlySnapshot);
            $this->render('view_outcome_only');
        }
    }
}