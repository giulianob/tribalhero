<?php

/**
 * @property MessageBoardThread $MessageBoardThread
 * @property MessageBoardPost $MessageBoardPost
 */
class TribeLogsController extends AppController {

    var $helpers = array('Time', 'Text');

    function listing() {
        $playerId = $this->request->data['playerId'];
        $page = array_key_exists('page', $this->request->data) ? intval($this->request->data['page']) : 0;

        $paginationSettings = $this->TribeLog->getListing($playerId, $page);

        if (empty($paginationSettings)) {
            $data = array('success' => false);
            $this->set('data', $data);
            $this->render('/Elements/to_json');
            return;
        }

        $this->paginate = $paginationSettings;
        $this->set('tribelogs', $this->paginate('TribeLog'));

    }
}
