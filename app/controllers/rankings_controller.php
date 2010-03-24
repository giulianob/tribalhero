<?php

class RankingsController extends AppController {

    var $allowedFromGame = array('listing');

    function ranking() {
        $this->Ranking->batchRanking();
        $this->render(false);
    }

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

    function listing() {
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : null;
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : -1;

        $options = $this->Ranking->getRankingListing($this->params['form']['type'], $id, $page, true);
        
        if ($options === false) return;

        $this->paginate = $options;
        $data = $this->paginate($this->Ranking);

        $this->set('data', $data);
    }
}