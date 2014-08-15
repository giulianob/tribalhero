<?php

class SystemsController extends AppController {

    var $uses = array('Ranking');
    var $helpers = array('Number');
    var $name = 'Systems';

    function beforeFilter() {
        parent::beforeFilter();

        $this->layout = 'default';

        if (array_key_exists('key', $this->request->query) && strcmp($this->request->query['key'], SYSTEM_STATUS_PASS) === 0) {
            $this->Auth->allow(array($this->request->action));
        }
    }

    function admin_generate_rankings() {
        if (empty($this->request->query['description'])) {
            die("Specify a description in the query string");
        }

        $description = $this->request->query['description'];

        $playerByAttackPoints = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value, 'PLAYER_ATTACK_POINT' type, 'axe' icon,
                                    CASE
                                        WHEN rankings.rank = 1 THEN 'Gold'
                                        WHEN rankings.rank = 2 THEN 'Silver'
                                        WHEN rankings.rank > 10 THEN 'Honorary'
                                        WHEN rankings.rank >= 3 THEN 'Bronze'
                                    END achievement,
                                    CASE WHEN rankings.rank = 1 THEN 0
                                        WHEN rankings.rank = 2 THEN 1
                                        WHEN rankings.rank > 10 THEN 3
                                        WHEN rankings.rank >= 3 THEN 2
                                    END tier,
                                    REPLACE('#%% Player by Attack Points', '%%', rankings.rank) title
                                    FROM players
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 5 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playerByDefensePoint = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value, 'PLAYER_DEFENSE_POINT' type, 'arrowshield' icon,
                                    CASE
                                        WHEN rankings.rank = 1 THEN 'Gold'
                                        WHEN rankings.rank = 2 THEN 'Silver'
                                        WHEN rankings.rank > 10 THEN 'Honorary'
                                        WHEN rankings.rank >= 3 THEN 'Bronze'
                                    END achievement,
                                    CASE WHEN rankings.rank = 1 THEN 0
                                        WHEN rankings.rank = 2 THEN 1
                                        WHEN rankings.rank > 10 THEN 3
                                        WHEN rankings.rank >= 3 THEN 2
                                    END tier,
                                    REPLACE('#%% Player by Defense Points', '%%', rankings.rank) title
                                    FROM players
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 6 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playersByLoot = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value, 'PLAYER_LOOT' type, 'chest' icon,
                            CASE
                                WHEN rankings.rank = 1 THEN 'Gold'
                                WHEN rankings.rank = 2 THEN 'Silver'
                                WHEN rankings.rank > 10 THEN 'Honorary'
                                WHEN rankings.rank >= 3 THEN 'Bronze'
                            END achievement,
                            CASE WHEN rankings.rank = 1 THEN 0
                                WHEN rankings.rank = 2 THEN 1
                                WHEN rankings.rank > 10 THEN 3
                                WHEN rankings.rank >= 3 THEN 2
                            END tier,
                            REPLACE('#%% Player by Loot', '%%', rankings.rank) title
                            FROM players
                            INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 7 AND rankings.rank <= 50
                            LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                            LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                            ORDER BY rankings.rank ASC;";

        $playerByInfluencePoints = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value, 'PLAYER_INFLUENCE_POINTS' type, 'anvil' icon,
                                    CASE
                                        WHEN rankings.rank = 1 THEN 'Gold'
                                        WHEN rankings.rank = 2 THEN 'Silver'
                                        WHEN rankings.rank > 10 THEN 'Honorary'
                                        WHEN rankings.rank >= 3 THEN 'Bronze'
                                    END achievement,
                                    CASE WHEN rankings.rank = 1 THEN 0
                                        WHEN rankings.rank = 2 THEN 1
                                        WHEN rankings.rank > 10 THEN 3
                                        WHEN rankings.rank >= 3 THEN 2
                                    END tier,
                                    REPLACE('#%% Player by Influence Points', '%%', rankings.rank) title
                                    FROM players
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 8 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playerByVictoryPoints = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value, 'TRIBE_VICTORY_POINT' type, 'tower' icon,
                                    CASE rankings.rank
                                        WHEN 1 THEN 'Gold'
                                        WHEN 2 THEN 'Silver'
                                        WHEN 3 THEN 'Bronze'
                                        ELSE  'Honorary'
                                    END achievement,
                                    CASE rankings.rank
                                        WHEN 1 THEN 0
                                        WHEN 2 THEN 1
                                        WHEN 3 THEN 2
                                        ELSE 3
                                    END tier,
                                    REPLACE('#%% Tribe by Victory Points', '%%', rankings.rank) title,
                                    REPLACE('%%', '%%', tribes.name) description
                                    FROM players
                                    INNER JOIN tribesmen ON tribesmen.player_id = players.id
                                    INNER JOIN rankings ON rankings.type = 12 AND tribesmen.tribe_id = rankings.tribe_id AND rankings.rank <= 10
                                    INNER JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC";

        $betaParticipants = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, '0' as `rankings.value`, SUM(structures.level) ip, 'BETA_PARTICIPANT' type, 'wing-man' icon, 'Honorary' achievement,
                            3 tier,
                            'Beta Participant' title
                            FROM structures
                            INNER JOIN cities ON cities.id = structures.city_id
                            INNER JOIN players ON cities.player_id = players.id
                            LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                            LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                            GROUP BY players.id HAVING SUM(structures.level) >= 100
                            ORDER BY ip asc";

        $mostExpensiveCities = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, rankings.value,  'MOST_EXPENSIVE_CITY' type, 'chalice' icon,
                                CASE
                                    WHEN rankings.rank = 1 THEN 'Gold'
                                    WHEN rankings.rank = 2 THEN 'Silver'
                                    WHEN rankings.rank > 10 THEN 'Honorary'
                                    WHEN rankings.rank >= 3 THEN 'Bronze'
                                END achievement,
                                CASE WHEN rankings.rank = 1 THEN 0
                                    WHEN rankings.rank = 2 THEN 1
                                    WHEN rankings.rank > 10 THEN 3
                                    WHEN rankings.rank >= 3 THEN 2
                                END tier,
                                REPLACE('#%% Most Expensive City', '%%', rankings.rank) title,
                                REPLACE('%%', '%%', cities.name) description
                                FROM players
                                INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 4 AND rankings.rank <= 50
                                INNER JOIN cities ON cities.id = rankings.city_id
                                LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                ORDER BY rankings.rank ASC;";

        $db = $this->Ranking->getDataSource();

        echo "<h3>SQL</h3>";
        echo "<pre>-- sql statements\n";
        echo "-- player by attack points\n";
        $results = $db->fetchAll($playerByAttackPoints);
        $this->generateRankingsResults($results, $description);

        echo "-- player by defense points\n";
        $results = $db->fetchAll($playerByDefensePoint);
        $this->generateRankingsResults($results, $description);

        echo "-- player by loot\n";
        $results = $db->fetchAll($playersByLoot);
        $this->generateRankingsResults($results, $description);

        echo "-- player by influence points\n";
        $results = $db->fetchAll($playerByInfluencePoints);
        $this->generateRankingsResults($results, $description);

        echo "-- player by victory points\n";
        $results = $db->fetchAll($playerByVictoryPoints);
        $this->generateRankingsResults($results, $description);

        echo "-- beta participants\n";
        $results = $db->fetchAll($betaParticipants);
        $this->generateRankingsResults($results, $description);

        echo "-- most expensive cities\n";
        $results = $db->fetchAll($mostExpensiveCities);
        $this->generateRankingsResults($results, $description);

        echo "-- end of SQL</pre>";

        echo "<h3>Wikia</h3><pre>";

        $results = $db->fetchAll($playerByAttackPoints);
        $this->generateWikiTable($results, 'Attack Points');

        $results = $db->fetchAll($playerByDefensePoint);
        $this->generateWikiTable($results, 'Defense Points');

        $results = $db->fetchAll($playersByLoot);
        $this->generateWikiTable($results, 'Loot Stolen');

        $results = $db->fetchAll($playerByInfluencePoints);
        $this->generateWikiTable($results, 'Influence Points');

        $results = $db->fetchAll($playerByVictoryPoints);
        $this->generateWikiTable($results, 'Victory Points');

        $results = $db->fetchAll($mostExpensiveCities);
        $this->generateWikiTable($results, 'Expensive City');


        echo "</pre>";
        $this->render(false);
    }

    private function generateWikiTable($results, $rankingName) {
        echo "
== $rankingName ==
{| border='1' cellpadding='0' cellspacing='0' class='article-table article-table-selected' style='width: 100%;'
! scope='col'|Player
! scope='col'|Tribe
! scope='col'|Rank
! scope='col'|{$rankingName}
! scope='col'|Achievement\n";

        foreach ($results as $result) {
            $playerName = $result['players']['player'];
            $tribeName = $result[0]['tribe'];
            $rank = $result['rankings']['rank'];
            $value = floor($result['rankings']['value']);
            $achievement = $result[0]['achievement'];

            echo "|-
| {$playerName}||{$tribeName}||{$rank}||{$value}||{$achievement}\n";
        }

        echo "|}\n\n";
    }

    private function generateRankingsResults($results, $description) {
        App::uses('Sanitize', 'Utility');
        foreach ($results as $result) {
            $title = Sanitize::escape($result[0]['title']);

            $descriptionSuffix = !empty($result[0]['description']) ? (': '.$result[0]['description']) : '';
            $achievementDescription = Sanitize::escape($description . $descriptionSuffix);

            $sql = "\tINSERT INTO `achievements` (`player_id`, `type`, `icon`, `tier`, `title`, `description`, `created`) VALUES
                    ({$result['players']['id']},
                    '{$result[0]['type']}',
                    '{$result[0]['icon']}',
                    {$result[0]['tier']},
                    '$title',
                    '$achievementDescription',
                    UTC_TIMESTAMP());";

            echo $sql . "\n";
        }
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
        $this->render('/Elements/to_json');
    }

    function admin_clear_cache() {
        Cache::clear();
        clearCache();

        $this->set('data', array('msg' => 'Cache cleared'));
        $this->render('/Elements/to_json');
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