<?php

/**
 * @property Battle $Battle
 * @property Report $Report
 * @property City $City
 */
class ReportsController extends AppController {

    var $uses = array('Report', 'Battle', 'City', 'Player');
    var $helpers = array('TimeAdv', 'Time');

    const REPORT_CITY_LOCAL = 1;
    const REPORT_CITY_FOREIGN = 2;
	const REPORT_TRIBE_LOCAL = 3;
	const REPORT_TRIBE_FOREIGN = 4;

    /**
     * Returns the player id of the player making the request
     * or "*" if an admin wants to see all reports.
    */
    private function getPlayerId() {
        $playerId = $this->request->data['playerId'];

        if ($this->Auth->user('rights') >= PLAYER_RIGHTS_ADMIN && !empty($this->request->data['playerNameFilter'])) {
            if ($this->request->data['playerNameFilter'] == '*')
                return '*';

            $player = $this->Player->findByName($this->request->data['playerNameFilter']);
            if ($player) {
                $playerId = $player['Player']['id'];
            }
        }

        return $playerId;
    }

    /**
     * Returns list of city ids
     * or "*" to see all cities
     */
    private function getCities($playerId) {
        if ($playerId === "*") {
            return "*";
        }

        return array_keys($this->City->find('list', array('conditions' => array(
                                'player_id' => $playerId
                                ))));
    }

    /**
     * Get tribe of current player or '*' to see all tribe reports
     */
    private function getTribeId($playerId) {
        if ($playerId === '*') {
            return '*';
        }

        $tribeId = $this->Player->getTribeId($playerId);
        return !$tribeId ? 0 : $tribeId;
    }

    function getReport($playerId, $battleId, $viewType, &$outcomeOnly, &$groupId, &$isUnread, &$loot) {
        switch ($viewType) {
            case ReportsController::REPORT_TRIBE_LOCAL:
                $tribeId = $this->getTribeId($playerId);
                return $this->Battle->viewInvasionReport('Tribe', $tribeId, $battleId, $outcomeOnly, $groupId, $isUnread, $loot);
            case ReportsController::REPORT_TRIBE_FOREIGN:
                $tribeId = $this->getTribeId($playerId);
                return $this->Battle->viewAttackReport('Tribe', $tribeId, $battleId, $outcomeOnly, $groupId, $isUnread, $loot);
            case ReportsController::REPORT_CITY_LOCAL:
                $cityIds = $this->getCities($playerId);
                return $this->Battle->viewInvasionReport('City', $cityIds, $battleId, $outcomeOnly, $groupId, $isUnread, $loot);
            case ReportsController::REPORT_CITY_FOREIGN:
                $cityIds = $this->getCities($playerId);
                return $this->Battle->viewAttackReport('City', $cityIds, $battleId, $outcomeOnly, $groupId, $isUnread, $loot);
            default:
                return false;
        }
    }

    function index_local() {
        $playerId = $this->getPlayerId();

        $viewType = get_value($this->request->data, "viewType");

        if ($viewType === false) {
            $this->render(false);
            return;
        }

        $locationType = get_value($this->request->data, "locationType");
        $locationId = get_value($this->request->data, "locationId");

        switch ($viewType) {
            case ReportsController::REPORT_CITY_LOCAL:
                $ownerIds = $this->getCities($playerId);
                $ownerType = 'City';
                break;
            case ReportsController::REPORT_TRIBE_LOCAL:
                $ownerIds = $this->getTribeId($playerId);
                $ownerType = 'Tribe';
                break;
            default:
                $this->render(false);
                return;
        }

        $this->paginate = $this->Battle->listInvasionReports($viewType, $ownerType, $ownerIds, $locationType, $locationId) + array(
            'limit' => 15,
            'page' => array_key_exists('page', $this->request->data) ? $this->request->data['page'] : 1
        );

        $reports = $this->paginate('Battle');

        // Resolve the name of the battle location
        foreach ($reports as $k => $report) {
            $report['Battle']['location_name'] = $this->Battle->getLocationName($report['Battle']['location_type'], $report['Battle']['location_id']);
            $reports[$k] = $report;
        }

        $this->set('battle_reports', $reports);

        $this->render("index_local");
    }

    function index_remote() {
        $playerId = $this->getPlayerId();

        $viewType = get_value($this->request->data, "viewType");

        if ($viewType === false) {
            $this->render(false);
            return;
        }

        $locationType = get_value($this->request->data, "locationType");
        $locationId = get_value($this->request->data, "locationId");

        switch ($viewType) {
            case ReportsController::REPORT_CITY_FOREIGN:
                $ownerIds = $this->getCities($playerId);
                $ownerType = 'City';
                break;
            case ReportsController::REPORT_TRIBE_FOREIGN:
                $ownerIds = $this->getTribeId($playerId);
                $ownerType = 'Tribe';
                break;
            default:
                $this->render(false);
                return;
        }

        $options = $this->Battle->listAttackReports($viewType, $ownerType, $ownerIds, $locationType, $locationId);

        $this->paginate = $options + array(
            'recursive' => '-1',
            'limit' => 15,
            'page' => array_key_exists('page', $this->request->data) ? $this->request->data['page'] : 1
        );

        $reports = $this->paginate($this->Battle);

        // Resolve the name of the battle location and owner
        foreach ($reports as $k => $report) {
            $report['BattleReportView']['owner_name'] = $this->Battle->getLocationName($report['BattleReportView']['owner_type'], $report['BattleReportView']['owner_id']);
            $report['Battle']['location_name'] = $this->Battle->getLocationName($report['Battle']['location_type'], $report['Battle']['location_id']);
            $reports[$k] = $report;
        }

        $this->set('battle_reports', $reports);
    }

    function mark_all_as_read() {
        $this->Report->markAllAsRead($this->request->data['playerId']);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function view_report() {
        $viewType = get_value($this->request->data, 'viewType');
        $reportId = get_value($this->request->data, 'id');

        if ($viewType === false || $reportId === false) {
            $this->render(false);
            return;
        }

        $playerId = $this->getPlayerId();

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;
        $loot = null;

        $report = $this->getReport($playerId, $reportId, $viewType, $outcomeOnly, $groupId, $isUnread, $loot);
        if ($report === false) {
            $this->render(false);
            return;
        }

        // Figure out if the player has read this report before
        $refreshOnClose = false;
        if ($groupId > 0 && $isUnread && $this->request->data['playerId'] == $playerId) {
            $this->Report->markAsRead($playerId, $viewType == ReportsController::REPORT_CITY_LOCAL, $reportId);
            $refreshOnClose = true;
        }

        // Only show outcome if we have the group
        $playerOutcome = null;
        if ($groupId) {
            // Get outcome for the player
            $playerOutcome = $this->Battle->viewGroupOutcome($report, $groupId);
        }

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
        $viewType = get_value($this->request->data, "viewType");
        $playerId = $this->getPlayerId();

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;
        $loot = null;

        $report = $this->getReport($playerId, $this->request->data['id'], $viewType, $outcomeOnly, $groupId, $isUnread, $loot);
        if ($report === false) {
            $this->render(false);
            return;
        }

        $battleEvents = $this->Battle->viewBattleEvents($report, get_value($this->request->data, 'page', 0));

        $this->set(compact('battleEvents'));
        $this->render('view_more_events');
    }

    function view_snapshot() {
        $viewType = get_value($this->request->data, "viewType");
        $playerId = $this->getPlayerId();

        $outcomeOnly = false;
        $groupId = 0;
        $isUnread = false;
        $loot = null;

        $report = $this->getReport($playerId, $this->request->data['id'], $viewType, $outcomeOnly, $groupId, $isUnread, $loot);

        if ($report === false || $outcomeOnly) {
            $this->render(false);
            return;
        }

        $battleReport = $this->Battle->viewSnapshot($report['Battle']['id'], $this->request->data['reportId']);

        if ($battleReport === false) {
            $this->render(false);
            return;
        }

        $this->set('battleStartTime', $report['Battle']['created']);
        
        $this->set(compact('battleReport'));
    }

}