<?php

/**
 * @property MessageBoardThread $MessageBoardThread
 * @property MessageBoardPost $MessageBoardPost
 */
class TribeLogsController extends AppController {

    var $helpers = array('Time', 'Text');
    var $allowedFromGame = array('listing');

    function listing() {
        $playerId = $this->params['form']['playerId'];
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : 0;

        $this->paginate = $this->TribeLog->getListing($playerId, $page);

        if (empty($this->paginate)) {
            $data = array('success' => false);
            $this->set('data', $data);
            $this->render('/elements/to_json');
            return;
        }

        $this->set('tribelogs', $this->paginate('TribeLog'));

    }
}
