<?php

/**
 * @property Battle $Battle
 * @property Report $Report
 */
class ReportsController extends AppController {

    var $uses = array('Report', 'Battle', 'City');
    var $helpers = array('TimeAdv', 'Time');
    var $allowedFromGame = array('index_local', 'view_snapshot', 'view_report', 'view_more_events', 'index_remote', 'mark_all_as_read');

    /**
     * Returns list of city ids
     * @return mixed Null if user is admin and can see all cities, otherwise, list of city ids 
     */
    private function getCities() {
        $playerId = $this->params['form']['playerId'];
        if ($this->Auth->user('rights') >= PLAYER_RIGHTS_ADMIN && !empty($this->params['form']['playerNameFilter'])) {
            if ($this->params['form']['playerNameFilter'] == '*')
                return null;

            $Player = & ClassRegistry::init('Player');
            $player = $Player->findByName($this->params['form']['playerNameFilter']);
            if ($player)
                $playerId = $player['Player']['id'];
        }

        return array_keys($this->City->find('list', array('conditions' => array(
                                'player_id' => $playerId
                                ))));
    }

    function index_local() {
        $cityIds = $this->getCities();

        $this->Battle = & ClassRegistry::init('Battle');

        $this->paginate = $this->Battle->listInvasionReports($cityIds, true) + array(
            'limit' => 15,
            'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate('Battle');

        // Resolve the name of the battle location
        foreach ($reports as $k => $report) {
            $report['Battle']['location_name'] = $this->Battle->getLocationName($report['Battle']['location_type'], $report['Battle']['location_id']);
            $reports[$k] = $report;
        }

        $this->set('battle_reports', $reports);
    }

    function mark_all_as_read() {
        $this->Report->markAllAsRead($this->params['form']['playerId']);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function view_report() {
        if (empty($this->params['form']['id'])) {
            $this->render(false);
            return;
        }

        $cityIds = $this->getCities();
        $isLocal = filter_var(get_value($this->params['form'], 'isLocal', false), FILTER_VALIDATE_BOOLEAN, FILTER_NULL_ON_FAILURE);

        if ($isLocal === null) {
            $this->render(false);
        }

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;
        $loot = null;

        if (!$isLocal) {
            $report = $this->Battle->viewAttackReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }
        else {
            $report = $this->Battle->viewInvasionReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }

        // If report not found, then this guy is trying to get something they do not have access to
        if ($report === false) {
            $this->render(false);
            return;
        }

        // Figure out if the player has read this report before
        $refreshOnClose = false;
        if (!is_null($cityIds) && $isUnread) {
            $this->Report->markAsRead($this->params['form']['playerId'], $isLocal, $this->params['form']['id']);
            $refreshOnClose = true;
        }

        // Get outcome for the player
        $playerOutcome = $this->Battle->viewGroupOutcome($report, $groupId);

        // If it's outcome only, then we only show the player outcome
        if ($outcomeOnly) {
            $this->set(compact('playerOutcome', 'refreshOnClose'));
            $this->render('view_outcome_only');
            return;
        }

        // Get battle outcome and initial page of events
        $battleOutcome = $this->Battle->viewBattleOutcome($report);
        $battleEvents = $this->Battle->viewBattleEvents($report, 0);

        $this->set(compact('playerOutcome', 'battleOutcome', 'battleEvents', 'refreshOnClose', 'loot'));
    }

    function view_more_events() {
        if (empty($this->params['form']['id'])) {
            $this->render(false);
            return;
        }

        $cityIds = $this->getCities();
        $isLocal = filter_var(get_value($this->params['form'], 'isLocal', false), FILTER_VALIDATE_BOOLEAN, FILTER_NULL_ON_FAILURE);

        if ($isLocal === null) {
            $this->render(false);
        }

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;

        if (!$isLocal) {
            $report = $this->Battle->viewAttackReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }
        else {
            $report = $this->Battle->viewInvasionReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }

        if ($report === false || $outcomeOnly) {
            $this->render(false);
            return;
        }

        $battleEvents = $this->Battle->viewBattleEvents($report, get_value($this->params['form'], 'page', 0));

        $this->set(compact('battleEvents'));
        $this->render('view_more_events');
    }

    function index_remote() {
        $cityIds = $this->getCities();

        $options = $this->Battle->listAttackReports($cityIds, true);

        $this->paginate = $options + array(
            'recursive' => '-1',
            'limit' => 15,
            'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate($this->Battle->BattleReportView);

        // Resolve the name of the battle location
        foreach ($reports as $k => $report) {
            $report['Battle']['location_name'] = $this->Battle->getLocationName($report['Battle']['location_type'], $report['Battle']['location_id']);
            $reports[$k] = $report;
        }

        $this->set('battle_reports', $reports);
    }

    function view_snapshot() {
        if (empty($this->params['form']['id']) || empty($this->params['form']['reportId'])) {
            $this->render(false);
            return;
        }

        $cityIds = $this->getCities();
        $isLocal = filter_var(get_value($this->params['form'], 'isLocal', false), FILTER_VALIDATE_BOOLEAN, FILTER_NULL_ON_FAILURE);

        if ($isLocal === null) {
            $this->render(false);
        }

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;

        if (!$isLocal) {
            $report = $this->Battle->viewAttackReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }
        else {
            $report = $this->Battle->viewInvasionReport($cityIds, $this->params['form']['id'], $outcomeOnly, $groupId, $isUnread, $loot);
        }

        if ($report === false || $outcomeOnly) {
            $this->render(false);
            return;
        }

        $battleReport = $this->Battle->viewSnapshot($report['Battle']['id'], $this->params['form']['reportId']);

        if ($battleReport === false) {
            $this->render(false);
            return;
        }

        $this->set('battleStartTime', $report['Battle']['created']);
        
        $this->set(compact('battleReport'));
    }

}