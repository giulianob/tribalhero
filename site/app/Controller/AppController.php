<?php

App::uses('Controller', 'Controller');
App::uses('SessionIdAuthenticate', 'Controller/Component/Auth');

/**
 * @property EmailComponent $Email
 * @property SessionComponent $Session
 * @property ThriftComponent $Thrift
 * @property AuthComponent $Auth
 */
class AppController extends Controller {

    var $components = array(
        'Auth' => array(
            'authenticate' => array('SessionId'),
            'unauthorizedRedirect' => false
        ),
        //'Security',
        'Session',
        'Paginator',
        'DebugKit.Toolbar',
        'Thrift.Thrift' => array(
            'host' => '127.0.0.1',
            'services' => array(
                'Notification' => array('port' => 46000)
            ),

        ),        
    );
    var $helpers = array('Html', 'Js', 'Form', 'Session', 'Cache');

    function beforeFilter() {
        CakeResponse::disableCache();

        $this->layout = 'ajax';
    }

    function isAuthorized() {
        if (array_key_exists('admin', $this->request->params) && $this->request->params['admin']) {
            return $this->Auth->user('rights') >= PLAYER_RIGHTS_ADMIN;
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