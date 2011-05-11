<?php

class ReportsController extends AppController {

    var $uses = array('Report', 'Battle');
    var $helpers = array('TimeAdv', 'Time');
    var $allowedFromGame = array('index_local', 'view_local', 'index_remote', 'view_remote');
    var $troop_states_pst = array(
        'joined the battle',
        'stayed',
        'left the battle',
        'died',
        'retreated',
        'gained new units',
        'run out of stamina'
    );

    private function getPlayerId() {
        $playerId = $this->params['form']['playerId'];
        if ($this->Auth->user('admin') && !empty($this->params['form']['playerNameFilter'])) {
            $Player = & ClassRegistry::init('Player');
            $player = $Player->findByName($this->params['form']['playerNameFilter']);
            if ($player)
                $playerId = $player['Player']['id'];
        }

        return $playerId;
    }

    function index_local() {

        $playerId = $this->getPlayerId();

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $playerId
                        )));

        $this->Battle = & ClassRegistry::init('Battle');

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

        $playerId = $this->getPlayerId();

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $playerId
                        )));

        $report = $this->Battle->viewInvasionReport(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $refreshOnClose = false;
        if (!$report['Battle']['read']) {
            $this->Report->markAsRead($this->params['form']['playerId'], true, $this->params['form']['id']);
            $refreshOnClose = true;
        }

        $reports = $this->Battle->viewBattle($this->params['form']['id']);

        $this->set('troop_states_pst', $this->troop_states_pst);
        $this->set('main_report', $report);
        $this->set('battle_reports', $reports);
        $this->set('refresh_on_close', $refreshOnClose);

        $this->render('view');
    }

    function index_remote() {
        $playerId = $this->getPlayerId();

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $playerId
                        )));

        $options = $this->Battle->listAttackReports(array_keys($cities), true);

        $this->paginate = $options + array(
            'recursive' => '-1',
            'limit' => 15,
            'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate($this->Battle->BattleReportView);

        $this->set('battle_reports', $reports);
    }

    function view_remote() {
        if (empty($this->params['form']['id'])) {
            $this->render(false);
            return;
        }

        $playerId = $this->getPlayerId();

        $cities = $this->Battle->City->find('list', array('conditions' => array(
                        'player_id' => $playerId
                        )));

        $report = $this->Battle->viewAttackReport(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $reports = $this->Battle->viewBattle($report['BattleReportView']['battle_id']);

        $refreshOnClose = false;
        if (!$report['BattleReportView']['read']) {
            $this->Report->markAsRead($this->params['form']['playerId'], false, $this->params['form']['id']);
            $refreshOnClose = true;
        }

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
                    if ($snapshot['group_id'] != $groupId)
                        continue;

                    $state = $snapshot['state'];

                    switch ($state) {
                        case TROOP_STATE_ENTERING:
                            $enterRound = $round;
                            $enterSnapshot = $snapshot;
                            $enterReport = $battle_report['BattleReport'];
                            break;
                        case TROOP_STATE_EXITING:
                        case TROOP_STATE_DYING:
                        case TROOP_STATE_RETREATING:
                        case TROOP_STATE_OUT_OF_STAMINA:
                            $foundExit = true;
                            break;
                    }

                    if ($foundExit) {
                        // If player didn't last for more than the min rounds then he can only see the outcome report
                        if ($round - $enterRound < BATTLE_VIEW_MIN_ROUNDS) {
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
        $this->set('refresh_on_close', $refreshOnClose);

        if ($renderOnlySnapshot == null) {
            $this->set('battle_reports', $reports);
            $this->render('view');
        } else {
            $this->set('battle_reports', $renderOnlySnapshot);
            $this->render('view_outcome_only');
        }
    }

}