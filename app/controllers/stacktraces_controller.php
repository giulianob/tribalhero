<?php

class StacktracesController extends AppController {

    var $name = 'Stacktraces';

    function beforeFilter() {
        parent::beforeFilter();

        $this->Auth->allow(array('game_submit'));
    }

    function admin_index() {
        $this->set('stacktraces', $this->paginate());
    }

    function game_submit() {

        $this->data = $this->params['form'];

        if (count($this->data) != 6) {
            debug("Bad data count");
            return;
        }

        // Ignore duplicate messages
        $sameMessage = $this->Stacktrace->find('first', array(
                    'conditions' => array(
                        'player_id' => $this->data['playerId'],
                        'message' => $this->data['stacktrace'],
                    ),
                    'link' => array(),
                ));

        if (empty($sameMessage)) {
            $this->Stacktrace->save(array('Stacktrace' => array(
                    'message' => $this->data['stacktrace'],
                    'player_id' => $this->data['playerId'],
                    'player_name' => $this->data['playerName'],
                    'flash_version' => $this->data['flashVersion'],
                    'game_version' => $this->data['gameVersion'],
                    'browser_version' => $this->data['browserVersion'],
					'occurrences' => 1,
                    )));
        } else {
			$this->Stacktrace->save(array('Stacktrace' => array(
				'id' => $sameMessage['Stacktrace']['id'],
				'occurrences' => $sameMessage['Stacktrace']['occurrences'] + 1,				
			)));
		}

        $this->layout = 'ajax';
        $this->set('data', array('success' => true));
        $this->render('/elements/to_json');
    }

}
