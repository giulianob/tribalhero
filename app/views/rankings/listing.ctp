<?php

$paging = $paginator->params('Ranking');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'rankings' => array());

// Check if this is a city or a player based ranking. For player ranking, we skip the City field since it doesnt exist
$isCity = count($data) > 0 && array_key_exists('City', $data[0]);

foreach ($data as $rank) {
    if ($isCity) {
        $results['rankings'][] = array(
                'rank' => $rank['Ranking']['rank'],
                'value' => $rank['Ranking']['value'],
                'cityId' => $rank['City']['id'],
                'cityName' => $rank['City']['name'],
                'playerId' => $rank['Player']['id'],
                'playerName' => $rank['Player']['name']
        );
    } else {
        $results['rankings'][] = array(
                'rank' => $rank['Ranking']['rank'],
                'value' => $rank['Ranking']['value'],
                'playerId' => $rank['Player']['id'],
                'playerName' => $rank['Player']['name']
        );
    }
}

echo $this->Js->object($results);