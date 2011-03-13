<?php

class Ranking extends AppModel {
    
    var $name = 'Ranking';

    var $actsAs = array('Linkable', 'Containable');

    var $belongsTo = array(
            'Player' => array(
                            'className' => 'Player',
                            'foreignKey' => 'Player_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            ),
            'City' => array(
                            'className' => 'City',
                            'foreignKey' => 'city_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            )
    );

    var $rankingTypes = array(
            array('name' => 'RANKING_ATTACK_CITY', 'field' => 'attack_point', 'order' => 'desc', 'cityBased' => true),
            array('name' => 'RANKING_DEFENSE_CITY', 'field' => 'defense_point', 'order' => 'desc', 'cityBased' => true),
            array('name' => 'RANKING_LOOT_CITY', 'field' => 'loot_stolen', 'order' => 'desc', 'cityBased' => true),
            array('name' => 'RANKING_ATTACK_PLAYER', 'field' => 'attack_point', 'order' => 'desc', 'cityBased' => false),
            array('name' => 'RANKING_DEFENSE_PLAYER', 'field' => 'defense_point', 'order' => 'desc', 'cityBased' => false),
            array('name' => 'RANKING_LOOT_PLAYER', 'field' => 'loot_stolen', 'order' => 'desc', 'cityBased' => false),
    );

    var $rankingsPerPage = 100;

    /*
     * Returns an array ranking based on the type, and id specified.
     * The id will either be a city_id or player_id depending on the $type provided.
     * If page is set to -1, it will default to the page where the $id specified appears.
    */
    public function getRankingListing($type, $id = null, $page = -1, $returnOptions = false) {

        // Check for invalid type
        if (!is_numeric($type) || $type < 0 || $type >= count($this->rankingTypes)) return false;        

        if (!is_numeric($page)) $page = 0;

        if ($page === -1) {
            $page = intval(($this->getRanking($type, $id) - 1) / $this->rankingsPerPage) + 1;
        }

        if ($this->rankingTypes[$type]['cityBased']) {
            $options = array(
                    'link' => array(
                        'City' => array('fields' => array('City.id', 'City.name')),
                        'Player' => array('fields' => array('Player.id', 'Player.name'))
                    ),
                    'conditions' => array('type' => $type),
                    'page' => $page,
                    'limit' => $this->rankingsPerPage,
                    'fields' => array('Ranking.rank', 'Ranking.value'),
                    'order' => 'Ranking.rank ASC'
            );
        }
        else {
            $options = array(
                    'link' => array('Player'),
                    'conditions' => array('type' => $type),                    
                    'limit' => $this->rankingsPerPage,
                    'page' => $page,
                    'fields' => array('Ranking.rank', 'Ranking.value', 'Player.id', 'Player.name'),
                    'order' => 'Ranking.rank ASC'
            );
        }

        if ($returnOptions) return $options;

        return $this->find('all', $options);
    }

    public function searchRankingListing($type, $search) {        
        if (is_numeric($search))
            $rank = max(1, intval($search));
        else
            $rank = $this->searchRanking($type, $search);
        
        if ($rank === false) return false;

        return $this->getRankingListing($type, null, intval(($rank - 1) / $this->rankingsPerPage) + 1, true);
    }

    /*
     * Searches for the ranking of the $search text in the city and player names
     */
    private function searchRanking($type, $search) {
        if ($this->rankingTypes[$type]['cityBased']) {
            // For city based ranking we allow searching for both cities and player
            $city = $this->City->findByName($search);
            if (!empty($city)) {
                return $this->getCityRanking($type, $city['City']['id']);
            }
        }

        $player = $this->Player->findByName($search);
        if (empty($player))
            return false;

        return $player['Player']['id'];
    }

    /*
     * Returns the ranking of the $type and $id.
     * The $id will either be a city_id or player_id depending on the $type provided.
     * If no ranking is found, it will return 1.
    */
    public function getRanking($type, $id) {
        if ($this->rankingTypes[$type]['cityBased'])
            return $this->getCityRanking($type, $id);
        else
            return $this->getPlayerRanking($type, $id);
    }

    /*
     * Return the ranking of the player specified.
     * If no ranking is found, it will return 1
    */
    public function getPlayerRanking($type, $player_id) {
        if (empty($player_id) || !is_numeric($player_id)) return 1;

        $ranking = $this->find('first', array(
                'contain' => array(),
                'conditions' => array('type' => $type, 'player_id' => $player_id)
        ));

        if (empty($ranking)) return 1;

        return $ranking['Ranking']['rank'];
    }

    /*
     * Return the ranking of the city specified.
     * If no ranking is found, it will return 1
    */
    public function getCityRanking($type, $city_id) {
        if (empty($city_id) || !is_numeric($city_id)) return 1;

        $ranking = $this->find('first', array(
                'contain' => array(),
                'conditions' => array('type' => $type, 'city_id' => $city_id)
        ));

        if (empty($ranking)) return 1;

        return $ranking['Ranking']['rank'];
    }

    /*
     * This is the main function used to batch process on all of the different ranking types
    */
    public function batchRanking() {
        $this->query("TRUNCATE `{$this->table}`");

        for ($i = 0; $i < count($this->rankingTypes); $i++) {
            $type = $this->rankingTypes[$i];
            if ($type['cityBased']) {
                $this->rankCity($i, $type['field'], $type['order']);
            } else {
                $this->rankPlayer($i, $type['field'], $type['order']);
            }
        }
    }

    /*
     * Inserts all of the cities ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
    */
    public function rankCity($type, $field, $order) {
        $cities = $this->City->find('all', array(
                'contain' => array(),
                'order' => $field . ' ' . $order,
                'fields' => array('player_id', 'id', $field)
        ));

        $itemsPerInsert = 500;
        $fields = array('player_id', 'city_id', 'rank', 'type', 'value');

        $cityCount = count($cities);
        $rankings = array();
        for ($i = 0;
        $i < $cityCount;
        ++$i) {
            $city = $cities[$i];

            $rankings[] = '(' . $city['City']['player_id'] . "," . $city['City']['id'] . "," . ($i+1) . "," . $type . "," . $city['City'][$field] . ')';

            if ((($i+1) % $itemsPerInsert) == 0 || $i == count($cities) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }

    /*
     * Inserts all of the player ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
    */
    public function rankPlayer($type, $field, $order) {
        $cities = $this->City->find('all', array(
                'contain' => array(),
                'order' => 'SUM(' . $field . ')  ' . $order,
                'fields' => array('player_id', 'id', 'SUM(' . $field . ') as value'),
                'group' => 'player_id'
        ));

        $itemsPerInsert = 500;
        $fields = array('player_id', 'city_id', 'rank', 'type', 'value');

        $cityCount = count($cities);
        $rankings = array();
        for ($i = 0; $i < $cityCount; ++$i) {
            $city = $cities[$i];

            $rankings[] = '(' . $city['City']['player_id'] . "," . $city['City']['id'] . "," . ($i+1) . "," . $type . "," . $city[0]['value'] . ')';

            if ((($i+1) % $itemsPerInsert) == 0 || $i == count($cities) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }
}