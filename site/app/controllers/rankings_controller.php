<?php

class RankingsController extends AppController {

    var $allowedFromGame = array('listing', 'search');

    function ranking() {
        $this->Ranking->batchRanking();
        $this->render(false);
    }
    
    function listing() {
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : null;
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : -1;

        $options = $this->Ranking->getRankingListing($this->params['form']['type'], $id, $page, true);

        if ($options === false) return;

        $this->paginate = $options;
        $data = $this->paginate($this->Ranking);

        $this->set('data', $data);
    }

    function search() {
        $search = array_key_exists('search', $this->params['form']) ? $this->params['form']['search'] : null;

        $options = $this->Ranking->searchRankingListing($this->params['form']['type'], $search);

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
}