<?php

class ReportsController extends AppController {
    var $uses = array('Battle');
    var $helpers = array('TimeAdv');

    var $allowedFromGame = array('index_local', 'view_local', 'index_remote', 'view_remote');

    //temp until i figure out how to put this in bootstrap
    var	$troop_states_pst = array(
            'joined the battle',
            'stayed',
            'left the battle',
            'died',
            'retreated',
            'gained new units'
    );

    var $object_types = array(
            101 => 'Swordsman',
            102 => 'Hoplite',
            103 => 'Archer',
            201 => 'Calvary',
            202 => 'Heavy Calvary',
            302 => 'Helepolis',
            2000 => 'Town',
            2102 => 'Refinery',
            2106 => 'Farm',
            2107 => 'Lumbermill',
            2108 => 'Advanced Farm',
            2109 => 'Advanced Lumbermill',
            2110 => 'Foundry',
            2201 => 'Barrack',
            2202 => 'Stable',
            2203 => 'Workshop',
            2301 => 'Academy',
            2302 => 'Armory',
            2303 => 'Blacksmith',
            2304 => 'Cannonsmith',
            2305 => 'Gunsmith',
            2306 => 'Whitesmith',
            2402 => 'Tower',
            2403 => 'Cannon Tower',
            2501 => 'Distribution Center',
            2502 => 'Market',
            2503 => 'Embassy',
            2504 => 'Shop',
            2000 => 'Town',
    );

    var $formations = array(
            1 => "Normal",
            2 => "Attack",
            3 => "Defense",
            4 => "Scout",
            5 => "Garrison",
            6 => "Structure",
            7 => "Local",
            11 => "Captured",
            12 => "Wounded",
            13 => "Killed"
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

        $report = $this->Battle->viewInvasionBattle(array_keys($cities), $this->params['form']['id']);        

        if ($report === false) {
            $this->render(false);
            return;
        }

        $options = $this->Battle->viewInvasionReport($this->params['form']['id'], true);
        
        $this->paginate = $options + array(
                'limit' => 15,
                'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate('BattleReport');
        
        $this->set('formations', $this->formations);
        $this->set('troop_states_pst', $this->troop_states_pst);
        $this->set('main_report', $report);
        $this->set('battle_reports', $reports);
        $this->set('object_types', $this->object_types);

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

        $reports = $this->paginate('BattleReport');

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
        $report = $this->Battle->viewAttackBattle(array_keys($cities), $this->params['form']['id']);

        if ($report === false) {
            $this->render(false);
            return;
        }

        $options = $this->Battle->viewAttackReport($report, true);

        $this->paginate = $options + array(
                'limit' => 15,
                'page' => array_key_exists('page', $this->params['form']) ? $this->params['form']['page'] : 1
        );

        $reports = $this->paginate('BattleReport');        
        $this->set('formations', $this->formations);
        $this->set('troop_states_pst', $this->troop_states_pst);
        $this->set('main_report', $report);
        $this->set('battle_reports', $reports);
        $this->set('object_types', $this->object_types);
        $this->render('view');
    }
}