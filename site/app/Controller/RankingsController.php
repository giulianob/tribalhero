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
}