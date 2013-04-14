<?php

class CitiesController extends AppController {

    var $allowedFromGame = array('autocomplete');

    function autocomplete() {
        $data = array();

        $name = $this->params['form']['name'];

        if (empty($name) || strlen($name) < 3) {
            $this->set(compact('data'));
            $this->render('/elements/to_json');
        }

        $data = array_values($this->City->find('list', array(
                    'fields' => array('name'),
                    'conditions' => array('deleted' => 0, 'name LIKE' => $name . '%'),
                    'order' => 'LENGTH(name) ASC',
                    'limit' => 6
                )));

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

}
