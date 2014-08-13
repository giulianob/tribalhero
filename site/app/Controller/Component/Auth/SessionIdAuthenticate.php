<?php

class SessionIdAuthenticate {
    public function getUser($request) {
        $playerId = $request->data['playerId'];
        $sessionId = $request->data['sessionId'];

        if (empty($playerId) || empty($sessionId)) {
            return false;
        }

        $player = ClassRegistry::init('Player')->find('first', array(
			'conditions' => array('id' => $playerId, 'session_id' => $sessionId)
		));

		if (empty($player)) {
		    return false;
		}

        return $player['Player'];
    }

    public function authenticate(CakeRequest $request, CakeResponse $response) {
        return false;
    }

    public function unauthenticated(CakeRequest $request, CakeResponse $response) {
        throw new ForbiddenException();
	}
}
