<?php

class AppController extends Controller
{
	var $components = array('Auth', 'Security');
	var $helpers = array('Html', 'Javascript', 'Form');
	
	var $recaptchaPrivateKey = '6LdYFgYAAAAAAMrRHyqHasNnktIyoTh1fwPKV0Jy';
	var $recaptchaPublicKey = '6LdYFgYAAAAAAEJVxFq049CUy9ml57Ds9hSRlw41'; 
	
	function beforeFilter()
	{	
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
	}
}