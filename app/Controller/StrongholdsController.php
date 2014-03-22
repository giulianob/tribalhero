<?php

class StrongholdsController extends AppController {

    var $allowedFromGame = array('autocomplete');

    function autocomplete() {
        $data = array();

        $name = $this->request->data['name'];

        if (empty($name) || strlen($name) < 3) {
            $this->set(compact('data'));
            $this->render('/elements/to_json');
        }

        $data = array_values($this->Stronghold->find('list', array(
            'fields' => array('name'),
            'conditions' => array('name LIKE' => $name . '%', 'state >' => 0),
            'order' => 'LENGTH(name) ASC',
            'limit' => 6
        )));

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

}