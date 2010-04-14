<?php

class AppController extends Controller {
    var $components = array('Auth', 'Security');
    var $helpers = array('Html', 'Javascript', 'Form');

    var $recaptchaPrivateKey = '6LdYFgYAAAAAAMrRHyqHasNnktIyoTh1fwPKV0Jy';
    var $recaptchaPublicKey = '6LdYFgYAAAAAAEJVxFq049CUy9ml57Ds9hSRlw41';

    var $fbApiKey = 'e0ccfdaba7a3e778380f19ad431bc1c8';
    var $fbSecret = 'af3a84dd6ea5c2351209be0aeff31dec';

    function beforeFilter() {
        /*
		if (Configure::read('debug') == 0){
            @ob_start ('ob_gzhandler');
            header('Content-type: text/html; charset: UTF-8');
            header('Cache-Control: must-revalidate');
            $offset = -1;
            $ExpStr = "Expires: " .gmdate('D, d M Y H:i:s',time() + $offset) . ' GMT';
        	header($ExpStr);
        }
        */

        $this->Auth->authError = "You have to login to do that!";
        $this->Auth->userModel = 'Player';
        $this->Auth->fields = array('username' => 'name', 'password' => 'password');
        $this->Auth->loginAction = array('admin' => false, 'controller' => 'players', 'action' => 'login');
        $this->Auth->loginRedirect = '/';
        $this->Auth->autoRedirect = false;
        $this->Auth->authorize = 'controller';

        if (isset($this->allowedFromGame) && in_array($this->action, $this->allowedFromGame)) {

            if (array_key_exists('sessionId', $this->params['form']) && array_key_exists('playerId', $this->params['form'])) {
                $playerModel =& ClassRegistry::init('Player');

                $player = $playerModel->find('first', array('conditions' => array('session_id' => $this->params['form']['sessionId'], 'id' => $this->params['form']['playerId'])));

                if (!empty($player)) {
                    $this->Auth->allow($this->action);
                } else
                    $this->Auth->deny($this->action);
            }
        }
    }

    function isAuthorized() {        
        if (isset($this->allowedFromGame) && in_array($this->action, $this->allowedFromGame)) {
            if (!array_key_exists('sessionId', $this->params['form']) || !array_key_exists('playerId', $this->params['form'])) {                
                $this->Auth->deny($this->action);
            }
        }
    }
}