<?php

/**
 * @property EmailComponent $Email
 * @property SessionComponent $Session
 */
class AppController extends Controller {

    var $components = array('Auth', 'Security', 'Session', 'DebugKit.Toolbar');
    var $helpers = array('Html', 'Js', 'Form', 'Session', 'Cache');

    function beforeFilter() {
        $this->disableCache();

        $this->Auth->authError = "You have to login to do that!";
        $this->Auth->userModel = 'Player';
        $this->Auth->fields = array('username' => 'name', 'password' => 'password');
        $this->Auth->loginAction = array('admin' => false, 'controller' => 'players', 'action' => 'login');
        $this->Auth->loginRedirect = array('/');
        $this->Auth->autoRedirect = false;
        $this->Auth->authorize = 'controller';
        
        if (isset($this->allowedFromGame) && in_array($this->action, $this->allowedFromGame)) {            
            if (!empty($this->params['named'])) {
                $this->params['form'] = $this->params['named'];
                $this->layout = 'default';                
            } else {
                //$this->params['form'] = $this->params['url'];
                Configure::write('debug', 0);
                $this->layout = 'ajax';
            }
            
            if (array_key_exists('sessionId', $this->params['form']) && array_key_exists('playerId', $this->params['form']) && !empty($this->params['form']['sessionId']) && !empty($this->params['form']['playerId'])) {
                $this->Auth->fields = array('username' => 'id', 'password' => 'session_id');
                
                $playerModel = & ClassRegistry::init('Player');                
                $playerModel->unbindModel(array('hasOne' => array_keys($playerModel->hasOne)));
                $playerModel->unbindModel(array('belongsTo' => array_keys($playerModel->belongsTo)));

                if ($this->Auth->login(array('id' => $this->params['form']['playerId'], 'session_id' => $this->params['form']['sessionId']))) {
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