<?php

/**
 * @property EmailComponent $Email
 * @property SessionComponent $Session
 */
class AppController extends Controller {

    var $components = array('Auth', 'Security', 'Session', 'DebugKit.Toolbar', 'Email' => array(
            'port' => '25',
            'replyTo' => 'play@tribalhero.com',
            'from' => 'Tribal Hero <play@tribalhero.com>',
            'delivery' => 'smtp',
            'sendAs' => 'html',
            'smtpOptions' => array(
                'port' => 465,
                'timeout' => 15,
                'auth' => true,
                'host' => 'ssl://smtp.gmail.com',
                'username' => 'play@tribalhero.com',
                'password' => 'j8qap6DAgu'
            )
        )
    );
    var $helpers = array('Html', 'Js', 'Form', 'Session', 'Cache');
    var $recaptchaPrivateKey = '6LdYFgYAAAAAAMrRHyqHasNnktIyoTh1fwPKV0Jy';
    var $recaptchaPublicKey = '6LdYFgYAAAAAAEJVxFq049CUy9ml57Ds9hSRlw41';
    var $fbApiKey = 'e0ccfdaba7a3e778380f19ad431bc1c8';
    var $fbSecret = 'af3a84dd6ea5c2351209be0aeff31dec';

    function beforeFilter() {
        $this->disableCache();

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
        $this->Auth->loginRedirect = array('/');
        $this->Auth->autoRedirect = false;
        $this->Auth->authorize = 'controller';

        if (isset($this->allowedFromGame) && in_array($this->action, $this->allowedFromGame)) {

            if (array_key_exists('sessionId', $this->params['form']) && array_key_exists('playerId', $this->params['form']) && !empty($this->params['form']['sessionId']) && !empty($this->params['form']['playerId'])) {
                $playerModel = & ClassRegistry::init('Player');

                $player = $playerModel->find('first', array('conditions' => array('session_id' => $this->params['form']['sessionId'], 'id' => $this->params['form']['playerId'])));

                if (!empty($player)) {
                    $this->Auth->allow($this->action);
                } else {
                    $this->Auth->deny($this->action);
                }
            }
        }
    }

    function isAuthorized() {
	    if ((array_key_exists('admin', $this->params) && $this->params['admin']) && !$this->Auth->user('admin')) {
            return false;
        }
		
        if (isset($this->allowedFromGame) && in_array($this->action, $this->allowedFromGame)) {
            if (!array_key_exists('sessionId', $this->params['form']) || !array_key_exists('playerId', $this->params['form'])) {
                return false;
            }
        }

        return true;
    }

    public static function getCorrectMTime($filePath) {

        $time = filemtime($filePath);

        $isDST = (date('I', $time) == 1);
        $systemDST = (date('I') == 1);

        $adjustment = 0;

        if ($isDST == false && $systemDST == true)
            $adjustment = 3600;

        else if ($isDST == true && $systemDST == false)
            $adjustment = -3600;

        else
            $adjustment = 0;

        return ($time + $adjustment);
    }

}