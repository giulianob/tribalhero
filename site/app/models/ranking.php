<?php

class Ranking extends AppModel {

    var $name = 'Ranking';
    var $belongsTo = array(
        'Player',
        'City',
    	'Tribe',
        'Stronghold'
    );
    var $rankingTypes = array(
        array('name' => 'RANKING_ATTACK_CITY', 'field' => 'attack_point', 'order' => 'desc', 'group' => 'city'),
        array('name' => 'RANKING_DEFENSE_CITY', 'field' => 'defense_point', 'order' => 'desc', 'group' => 'city'),
        array('name' => 'RANKING_LOOT_CITY', 'field' => 'loot_stolen', 'order' => 'desc', 'group' => 'city'),
        array('name' => 'RANKING_INFLUENCE_CITY', 'field' => 'value', 'order' => 'desc', 'group' => 'city'),
        array('name' => 'RANKING_EXPENSIVE_CITY', 'field' => 'expense_value', 'order' => 'desc', 'group' => 'city'),

        array('name' => 'RANKING_ATTACK_PLAYER', 'field' => 'attack_point', 'order' => 'desc', 'group' => 'player'),
        array('name' => 'RANKING_DEFENSE_PLAYER', 'field' => 'defense_point', 'order' => 'desc', 'group' => 'player'),
        array('name' => 'RANKING_LOOT_PLAYER', 'field' => 'loot_stolen', 'order' => 'desc', 'group' => 'player'),
        array('name' => 'RANKING_INFLUENCE_PLAYER', 'field' => 'value', 'order' => 'desc', 'group' => 'player'),

        array('name' => 'RANKING_LEVEL_TRIBE', 'field' => 'level', 'order' => 'desc', 'group' => 'tribe'),
        array('name' => 'RANKING_ATTACK_TRIBE', 'field' => 'attack_point', 'order' => 'desc', 'group' => 'tribe'),
        array('name' => 'RANKING_DEFENSE_TRIBE', 'field' => 'defense_point', 'order' => 'desc', 'group' => 'tribe'),
        array('name' => 'RANKING_VICTORY_TRIBE', 'field' => 'victory_point', 'order' => 'desc', 'group' => 'tribe'),
        array('name' => 'RANKING_VICTORY_RATE_TRIBE', 'field' => 'victory_point_rate_sum', 'order' => 'desc', 'group' => 'tribe'),

        array('name' => 'RANKING_LEVEL_STRONGHOLD', 'field' => 'level', 'order' => 'desc', 'group' => 'stronghold'),
        array('name' => 'RANKING_OCCUPIED_STRONGHOLD', 'field' => 'date_occupied', 'order' => 'desc', 'group' => 'stronghold'),
        array('name' => 'RANKING_VICTORY_POINT_RATE', 'field' => 'victory_point_rate', 'order' => 'desc', 'group' => 'stronghold')
    );
    var $rankingsPerPage = 100;

    /**
     * Returns an array ranking based on the type, and id specified.
     * The id will either be a city_id or player_id depending on the $type provided.
     * If page is set to -1, it will default to the page where the $id specified appears.
     * @param int $type Type index
     * @param int $id City/player id to search for
     * @param int $page Page to return
     * @param bool $returnOptions If set to true, will return conditions instead of performing a find
     * @return array Results or conditions depending on $returnOptions flag
     */
    public function getRankingListing($type, $id = null, $page = -1, $returnOptions = false) {

        // Check for invalid type
        if (!is_numeric($type) || $type < 0 || $type >= count($this->rankingTypes))
            return false;

        if (!is_numeric($page))
            $page = 0;

        // Auto find the correct page based on player's ranking
        if ($page === -1) {
            $page = intval(($this->getRanking($type, $id) - 1) / $this->rankingsPerPage) + 1;
        }

        if ($this->rankingTypes[$type]['group']=='city') {
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
        } else if ($this->rankingTypes[$type]['group']=='player') {
            $options = array(
                'link' => array('Player'),
                'conditions' => array('type' => $type),
                'limit' => $this->rankingsPerPage,
                'page' => $page,
                'fields' => array('Ranking.rank', 'Ranking.value', 'Player.id', 'Player.name'),
                'order' => 'Ranking.rank ASC'
            );
        } else if ($this->rankingTypes[$type]['group']=='tribe') {
             $options = array(
                'link' => array('Tribe'),
                'conditions' => array('type' => $type),
                'limit' => $this->rankingsPerPage,
                'page' => $page,
                'fields' => array('Ranking.rank', 'Ranking.value', 'Tribe.id', 'Tribe.name'),
                'order' => 'Ranking.rank ASC'
            );       
        } else if ($this->rankingTypes[$type]['group']=='stronghold') {
             $options = array(
                'link' => array(
                    'Stronghold' => array('fields' => array('Stronghold.id', 'Stronghold.name')),
                    'Tribe' => array('fields' => array('Tribe.id', 'Tribe.name'))
                ),
                'conditions' => array('type' => $type),
                'limit' => $this->rankingsPerPage,
                'page' => $page,
                'fields' => array('Ranking.rank', 'Ranking.value'),
                'order' => 'Ranking.rank ASC'
            );       
        }

        if ($returnOptions)
            return $options;

        return $this->find('all', $options);
    }

    /**
     * Returns the full page of items containing the record found by the search param
     * @param int $type Type index
     * @param mixed $search If numeric, will search for that rank, otherwise will search for player/city name
     * @return mixed Ranking page found
     */
    public function searchRankingListing($type, $search) {
        if (is_numeric($search))
            $rank = max(1, intval($search));
        else
            $rank = $this->searchRanking($type, $search);

        if ($rank === false)
            return false;

        return array('rank' => $rank, 'list' => $this->getRankingListing($type, null, intval(($rank - 1) / $this->rankingsPerPage) + 1, true));
    }

    /**
     * Searches for the ranking of the $search text in the city/player names
     * @param int $type Type index
     * @param string $search Search criteria to look for in city/player names
     * @return int
     */
    private function searchRanking($type, $search) {
        if (!isset($this->rankingTypes[$type]))
            return false;

        if ($this->rankingTypes[$type]['group']=='city') {
            // For city based ranking we allow searching for both cities and player
            $city = $this->City->findByName($search);
            if (!empty($city)) {
                return $this->getCityRanking($type, $city['City']['id']);
            }
        } else if($this->rankingTypes[$type]['group']=='player') {

	        $player = $this->Player->findByName($search);
	        if (empty($player))
	            return false;
	
	        return $this->getPlayerRanking($type, $player['Player']['id']);
        } else if($this->rankingTypes[$type]['group']=='tribe') {

	        $tribe = $this->Tribe->findByName($search);
	        if (empty($tribe))
	            return false;
	
	        return $tribe['Tribe']['id'];
        } else if($this->rankingTypes[$type]['group']=='stronghold') {

	        $stronghold = $this->Stronghold->findByName($search);
	        if (empty($stronghold))
	            return false;

            return $this->getStrongholdRanking($type, $stronghold['Stronghold']['id']);
        }

        return false;
    }

    /**
     * Returns the ranking of the $type and $id.
     * The $id will either be a city_id or player_id depending on the $type provided.
     * If no ranking is found, it will return 1.
     * @param int $type Type index
     * @param int $id City/player id
     * @return mixed
     */
    public function getRanking($type, $id) {
        if ($this->rankingTypes[$type]['group']=='city')
            return $this->getCityRanking($type, $id);
        else if ($this->rankingTypes[$type]['group']=='player')
            return $this->getPlayerRanking($type, $id);
        else if ($this->rankingTypes[$type]['group']=='tribe')
            return $this->getTribeRanking($type, $id);
        else if ($this->rankingTypes[$type]['group']=='stronghold')
            return $this->getStrongholdRanking($type, $id);
    }

    /**
     * Return the ranking of the tribe specified.
     * @param int $type Type index
     * @param int $tribe_id Tribe id to find
     * @return int Tribe's rank or 1 if not found
     */
    public function getTribeRanking($type, $tribe_id) {
        if (empty($tribe_id) || !is_numeric($tribe_id))
            return 1;

        $ranking = $this->find('first', array(
                    'contain' => array(),
                    'conditions' => array('type' => $type, 'tribe_id' => $tribe_id)
                ));

        if (empty($ranking))
            return 1;

        return $ranking['Ranking']['rank'];
    }
    
    /**
     * Return the ranking of the player specified.
     * @param int $type Type index
     * @param int $player_id Player id to find
     * @return int Player's rank or 1 if not found
     */
    public function getPlayerRanking($type, $player_id) {
        if (empty($player_id) || !is_numeric($player_id))
            return 1;

        $ranking = $this->find('first', array(
                    'contain' => array(),
                    'conditions' => array('type' => $type, 'player_id' => $player_id)
                ));

        if (empty($ranking))
            return 1;

        return $ranking['Ranking']['rank'];
    }

    /**
     * Return the ranking of the city specified.
     * @param int $type Type index
     * @param int $city_id City id to find
     * @return int City rank or 1 if not found
     */
    public function getCityRanking($type, $city_id) {
        if (empty($city_id) || !is_numeric($city_id))
            return 1;

        $ranking = $this->find('first', array(
                    'contain' => array(),
                    'conditions' => array('type' => $type, 'city_id' => $city_id)
                ));

        if (empty($ranking))
            return 1;

        return $ranking['Ranking']['rank'];
    }

    /**
     * Return the ranking of the stronghold specified.
     * @param int $type Type index
     * @param int $stronghold_id Stronghold id to find
     * @return int Stronghold rank or 1 if not found
     */
    public function getStrongholdRanking($type, $stronghold_id) {
        if (empty($stronghold_id) || !is_numeric($stronghold_id))
            return 1;

        $ranking = $this->find('first', array(
                    'contain' => array(),
                    'conditions' => array('type' => $type, 'stronghold_id' => $stronghold_id)
                ));

        if (empty($ranking))
            return 1;

        return $ranking['Ranking']['rank'];
    }

    /**
     * This is the main function used to batch process on all of the different ranking types.
     * This should only be called from the Cake shell
     */
    public function batchRanking() {
        $this->query("TRUNCATE `{$this->table}`");

        for ($i = 0; $i < count($this->rankingTypes); $i++) {
            $type = $this->rankingTypes[$i];
            if ($type['group']=='city') {
                $this->rankCity($i, $type['field'], $type['order']);
            } else if($type['group']=='player') {
                $this->rankPlayer($i, $type['field'], $type['order']);
            } else if($type['group']=='tribe') {
            	$this->rankTribe($i, $type['field'], $type['order']);
            } else if($type['group']=='stronghold') {
                $this->rankStronghold($i, $type['field'], $type['order']);
            }
        }
    }

    /**
     * Inserts all of the cities ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
     */
    public function rankCity($type, $field, $order) {
        $cities = $this->City->find('all', array(
                    'contain' => array(),
                    'conditions' => array('City.deleted' => 0),
                    'order' => array($field . ' ' . $order, 'City.player_id ASC'),
                    'fields' => array('player_id', 'id', $field)
                ));

        $itemsPerInsert = 500;
        $fields = array('player_id', 'city_id', 'rank', 'type', 'value');

        $cityCount = count($cities);
        $rankings = array();
        for ($i = 0; $i < $cityCount; ++$i) {
            $city = $cities[$i];

            $rankings[] = '(' . $city['City']['player_id'] . "," . $city['City']['id'] . "," . ($i + 1) . "," . $type . "," . $city['City'][$field] . ')';

            if ((($i + 1) % $itemsPerInsert) == 0 || $i == count($cities) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }

    /**
     * Inserts all of the player ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
     */
    public function rankPlayer($type, $field, $order) {
        $cities = $this->City->find('all', array(
                    'contain' => array(),
                    'conditions' => array('City.deleted' => 0),
                    'order' => array('SUM(' . $field . ')  ' . $order, 'player_id ASC'),
                    'fields' => array('player_id', 'id', 'SUM(' . $field . ') as value'),
                    'group' => 'player_id HAVING COUNT(`id`) > 0'
                ));

        $itemsPerInsert = 500;
        $fields = array('player_id', 'city_id', 'rank', 'type', 'value');

        $cityCount = count($cities);
        $rankings = array();
        for ($i = 0; $i < $cityCount; ++$i) {
            $city = $cities[$i];

            $rankings[] = '(' . $city['City']['player_id'] . ",0," . ($i + 1) . "," . $type . "," . $city[0]['value'] . ')';

            if ((($i + 1) % $itemsPerInsert) == 0 || $i == count($cities) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }
     /**
     * Inserts all of the tribe ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
     */
    public function rankTribe($type, $field, $order) {
        if($field == "victory_point_rate_sum") {
            $tribes = $this->Tribe->find('all', array(
                        'contain' => array(),
                        'conditions' => array('Tribe.deleted' => 0),
                        'order' => array($field . ' ' . $order, 'Tribe.id ASC'),
                        'fields' => array('Tribe.id', 'SUM(Stronghold.victory_point_rate) as victory_point_rate_sum'),
                        'link' => array('Stronghold' => array('fields' => array())),
                        'group' => array('Tribe.id')
                    ));

            foreach ($tribes as $k => $tribe) {
                $tribes[$k]['Tribe']['victory_point_rate_sum'] = empty($tribe[0]['victory_point_rate_sum']) ? 0 : $tribe[0]['victory_point_rate_sum'];
                unset($tribes[$k][0]);
            }
        } else {
            $tribes = $this->Tribe->find('all', array(
                        'contain' => array(),
                        'conditions' => array('Tribe.deleted' => 0),
                        'order' => array($field . ' ' . $order, 'id ASC'),
                        'fields' => array('id', $field ),
                    ));
        }

        $itemsPerInsert = 500;
        $fields = array( 'tribe_id', 'player_id', 'city_id', 'rank', 'type', 'value');

        $tribeCount = count($tribes);
        $rankings = array();

        for ($i = 0; $i < $tribeCount; ++$i) {
            $tribe = $tribes[$i];
            $rankings[] = '(' . $tribe['Tribe']['id'] . ",0,0," . ($i + 1) . "," . $type . "," . $tribe['Tribe'][$field] . ')';

            if ((($i + 1) % $itemsPerInsert) == 0 || $i == count($tribes) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }
    
    /**
     * Inserts all of the stronghold ranking into the rankings table
     * @param type int The ranking type
     * @param field string The field used by this ranking to aggregate on
     * @param order string The order of ranking (asc or desc)
     */
    public function rankStronghold($type, $field, $order) {
        $strongholds = $this->Stronghold->find('all', array(
                    'contain' => array(),
                    'conditions' => array('state >' => 0),
                    'order' => array($field . ' ' . $order, 'id ASC'),
                    'fields' => array('id', 'tribe_id', $field ),
                ));

        $itemsPerInsert = 500;
        $fields = array( 'stronghold_id', 'tribe_id','rank', 'type', 'value');

        $strongholdCount = count($strongholds);
        $rankings = array();

        for ($i = 0; $i < $strongholdCount; ++$i) {
            $stronghold = $strongholds[$i];
            if($field=="date_occupied") {
                $rankings[] = "(" . $stronghold['Stronghold']['id'] . "," . $stronghold['Stronghold']['tribe_id'] . ",". ($i + 1) . "," . $type. "," . "UNIX_TIMESTAMP('" .$stronghold['Stronghold'][$field]. "'))";
            } else {
                $rankings[] = '(' . $stronghold['Stronghold']['id'] . "," . $stronghold['Stronghold']['tribe_id'] . ",". ($i + 1) . "," . $type . "," .$stronghold['Stronghold'][$field] . ')';
            }
            if ((($i + 1) % $itemsPerInsert) == 0 || $i == count($strongholds) - 1) {
                $this->getDataSource()->insertMulti($this->table, $fields, $rankings);
                $rankings = array();
            }
        }
    }
}