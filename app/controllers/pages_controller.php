<?php

class PagesController extends AppController {
    var $uses = array();

    var $cacheAction = array(
            'index/' => '1 month'
    );

    function beforeFilter() {
        parent::beforeFilter();
        $this->Auth->allow(array('display', 'facebook', 'index'));
    }

    function index() {
        $this->set('title_for_layout', 'Tribal Hero - a free browser game');
    }

    function display() {
        $path = func_get_args();

        $count = count($path);
        if (!$count) {
            $this->redirect('/');
        }

        $page = $subpage = $title = null;

        if (!empty($path[0])) {
            $page = $path[0];
        }
        if (!empty($path[1])) {
            $subpage = $path[1];
        }
        if (!empty($path[$count - 1])) {
            $title = Inflector::humanize($path[$count - 1]);
        }
        $this->set(compact('page', 'subpage', 'title'));
        $this->render(join('/', $path));
    }

    function facebook() {
        App::import('Vendor', 'facebook/facebook');
        $Player =& ClassRegistry::init('Player');
        $this->layout = 'ajax';
        $GLOBALS['facebook_config']['debug'] = null;
        $facebook = new Facebook($this->fbApiKey, $this->fbSecret);

        $fbid = $facebook->api_client->users_getLoggedInUser();

        if (empty($facebook->fb_params) || !is_numeric($fbid) || empty($fbid))
            $this->redirect('/');

        $loginKey = sha1($fbid + '.' + time() + '.' + rand(0,1000));

        $player = $Player->find('first', array('conditions' => array('facebook_id' => $fbid)));

        if (!$player) {
            //New player
            $user_details = $facebook->api_client->users_getStandardInfo($fbid, array('name', 'proxied_email'));

            $ok = $Player->save(array(
                    'facebook_id' => $fbid,
                    'name' => $user_details[0]['name'],
                    'email_address' => $user_details[0]['proxied_email'],
                    'login_key_date' => date('Y-m-d H:i:s'),
                    'login_key' => $loginKey), false);
        }
        else {
            $ok = $Player->save(array(
                    'id' => $player['Player']['id'],
                    'login_key_date' => date('Y-m-d H:i:s'),
                    'login_key' => $loginKey
                    ), false);
        }

        if (!$ok) {
            $this->render(false);
            echo 'We were unable to log you in. Please refresh the page to try again';
            return;
        }

        $this->set('lsessid', $loginKey);
    }

    function play() {
        // Prevent caching
        header("Cache-Control: no-cache, must-revalidate");
        header("Expires: Sat, 26 Jul 1997 05:00:00 GMT");

		$this->set('title_for_layout', 'Play');
		
        $this->layout = 'game';

        $loginKey = sha1($this->Auth->user('id') + '.' + time() + '.' + rand(0,1000));

        $player =& ClassRegistry::init('Player');

		$playerInfo = $player->findById($this->Auth->user('id'));
		
		if (!empty($playerInfo['Player']['login_key']) && time() - strtotime($playerInfo['Player']['login_key_date']) < 300) {
			$loginKey = $playerInfo['Player']['login_key'];
		} else if (!$player->save(array(
        'id' => $this->Auth->user('id'),
        'login_key_date' => date('Y-m-d H:i:s'),
        'login_key' => $loginKey
        ), false)) {
            $this->Session->setFlash('We were unable to log you into the world. Refresh this page to try again, if the problem persists, contact us.');
            $this->redirect('/');
        }
        
        $this->set('lsessid', $loginKey);        
    }

    function game_popup() {
        Configure::write('debug', 0);
        $this->layout = 'popup';
    }
}
