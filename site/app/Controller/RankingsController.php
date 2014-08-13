<?php

class RankingsController extends AppController {

    function ranking() {
        $this->Ranking->batchRanking();
        $this->render(false);
    }
    
    function listing() {
        $id = array_key_exists('id', $this->request->data) ? intval($this->request->data['id']) : null;
        $page = array_key_exists('page', $this->request->data) ? intval($this->request->data['page']) : -1;

        $options = $this->Ranking->getRankingListing($this->request->data['type'], $id, $page, true);

        if ($options === false) return;

        $this->paginate = $options;
        $data = $this->paginate($this->Ranking);

        $this->set('data', $data);
    }

    function search() {
        $search = array_key_exists('search', $this->request->data) ? $this->request->data['search'] : null;

        $options = $this->Ranking->searchRankingListing($this->request->data['type'], $search);

        if ($options === false) {
            $this->set('data', array('error' => 'Nothing found with the specified criteria'));
            $this->render('/elements/to_json');
            return;
        }

        $this->paginate = $options['list'];
        $data = $this->paginate($this->Ranking);

        $this->set('searchRank', $options['rank']);
        $this->set('data', $data);
        $this->render('listing');
    }

    function admin_generate($description = null) {
        if (empty($description)) {
            die("Specify a description in the query string");
        }

        $playerByAttackPoints = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, FORMAT(rankings.value, 0) attack_points, 'PLAYER_ATTACK_POINT' type, 'axe' icon,
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
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 4 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playerByDefensePoint = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, FORMAT(rankings.value, 0) defense_points, 'PLAYER_DEFENSE_POINT' type, 'arrowshield' icon,
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
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 5 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playersByLoot = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, FORMAT(rankings.value, 0) loot, 'PLAYER_LOOT' type, 'chest' icon,
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
                            INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 6 AND rankings.rank <= 50
                            LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                            LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                            ORDER BY rankings.rank ASC;";

        $playerByInfluencePoints = "SELECT players.id, players.name player, IF(tribes.name IS NULL,'',tribes.name) tribe, rankings.rank rank, FORMAT(rankings.value, 0) influence_points, 'PLAYER_INFLUENCE_POINTS' type, 'anvil' icon,
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
                                    INNER JOIN rankings ON rankings.player_id = players.id AND rankings.type = 7 AND rankings.rank <= 50
                                    LEFT JOIN tribesmen ON tribesmen.player_id = players.id
                                    LEFT JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC;";

        $playerByVictoryPoints = "SELECT players.id, players.name player, tribes.name tribe, rankings.rank rank, FORMAT(rankings.value, 0) victory_points, 'TRIBE_VICTORY_POINT' type, 'tower' icon,
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
                                    END Tier,
                                    REPLACE('#%% Tribe by Victory Points', '%%', rankings.rank) title,
                                    tribes.name description
                                    FROM players
                                    INNER JOIN tribesmen ON tribesmen.player_id = players.id
                                    INNER JOIN rankings ON rankings.type = 11 AND tribesmen.tribe_id = rankings.tribe_id AND rankings.rank <= 10
                                    INNER JOIN tribes ON tribes.id = tribesmen.tribe_id
                                    ORDER BY rankings.rank ASC";

        $betaParticipants = "SELECT players.id, players.name player, tribes.name tribe, '0' rank, SUM(structures.level) ip, 'BETA_PARTICIPANT' type, 'wing-man' icon, 'Honorary' achievement,
                            3 tier,
                            'Beta Participant' title
                            FROM structures
                            INNER JOIN cities ON cities.id = structures.city_id
                            INNER JOIN players ON cities.player_id = players.id
                            INNER JOIN tribesmen ON tribesmen.player_id = players.id
                            INNER JOIN tribes ON tribes.id = tribesmen.tribe_id
                            GROUP BY players.id HAVING SUM(structures.level) >= 100
                            ORDER BY ip asc";

        // TODO: Most expensive cities
    }    
}