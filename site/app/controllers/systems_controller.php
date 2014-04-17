<?php

class SystemsController extends AppController {

    var $uses = array();
    var $helpers = array('Number');
    var $name = 'Systems';

    function beforeFilter() {
        parent::beforeFilter();

        if (array_key_exists('key', $this->params['url']) && strcmp($this->params['url']['key'], SYSTEM_STATUS_PASS) === 0)
            $this->Auth->allow(array($this->action));
    }

    function admin_status() {
        $SystemVariable = & ClassRegistry::init('SystemVariable');

        $variables = $SystemVariable->find('all');

        $Player = & ClassRegistry::init('Player');

        $onlinePlayers = $Player->find('all', array(
            'contains' => array(),
            'conditions' => array('Player.online' => true)
                ));

        $this->set('onlinePlayers', $onlinePlayers);
        $this->set('variables', $variables);
    }

    function admin_pachube() {
        $SystemVariable = & ClassRegistry::init('SystemVariable');

        $variables = $SystemVariable->find('all');

        $data = array('version' => '1.0.0', 'datastreams' => array());

        foreach ($variables as $variable) {
            if (!is_numeric($variable['SystemVariable']['value'])) continue;
            $data['datastreams'][] = array('id' => $variable['SystemVariable']['name'], 'current_value' => intval($variable['SystemVariable']['value']));
        }        
        
        $this->layout = null;
        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

    function admin_clear_cache() {
        Cache::clear();
        clearCache();

        $this->set('data', array('msg' => 'Cache cleared'));
        $this->render('/elements/to_json');
    }

    function admin_battle_stats() {

        // Unit counts
        $TroopStubList = & ClassRegistry::init('TroopStubList');

        $stats = $TroopStubList->find('all', array(
            'link' => array(
            ),
            'fields' => array(
                'type',
                'SUM(`count`) as count',
            ),
            'group' => array('TroopStubList.type'),
            'order' => array('TroopStubList.type ASC'),
                ));

        $this->set('stats', $stats);

        // Top player by troop size        

        $troopSizeStats = $TroopStubList->find('all', array(
            'link' => array(
                'City' => array(
                    'fields' => array('City.name')
                )
            ),
            'order' => array('troop_count' => 'desc'),
            'fields' => array('SUM(count) as troop_count'),
            'group' => array('TroopStubList.city_id'),
            'limit' => 50
                ));

        $this->set('troopSizeStats', $troopSizeStats);

        // Top player by resources
        $City = & ClassRegistry::init('City');

        $resourceStats = $City->find('all', array(
            'link' => array(),
            'order' => array('resource_total' => 'desc'),
            'fields' => array('name', 'gold', 'wood', 'crop', 'iron', 'labor', '(gold+wood+crop+iron+labor) as resource_total'),
            'limit' => 50
                ));

        $this->set('resourceStats', $resourceStats);
    }

}