<?php

class ReportsController extends AppController {
    var $uses = array('Battle');
    var $helpers = array('TimeAdv');

    var $allowedFromGame = array('index_local', 'view_local', 'index_remote', 'view_remote');

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

        //main report data to find the beginning/end of the battle reports we need
        $report = $this->Battle->viewAttackReport(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $reports = $this->Battle->viewBattle($report['BattleReportView']['battle_id']);

        $this->set('main_report', $report);
        $this->set('battle_reports', $reports);
        $this->render('view');
    }
}