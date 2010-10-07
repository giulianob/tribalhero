<?php
class PlayersController extends AppController {

	var $components = array('Email' => array(
		'port'=>'25', 
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
	));

    var $name = 'Players';

    function beforeFilter() {
        parent::beforeFilter();
        
        $this->Auth->allow(array('register', 'login', 'logout', 'forgot', 'reset'));

        $this->Security->disabledFields = array('recaptcha_challenge_field', 'recaptcha_response_field');
    }

    function register() {
		$this->set('title_for_layout', 'Register New Account');
	
        App::import('Vendor', 'Recaptcha', array('file' => 'recaptcha'.DS.'recaptchalib.php'));
        $this->set('recaptchaPublicKey', $this->recaptchaPublicKey);

        if (!empty($this->data)) {
			$verifiedCaptcha = $this->Session->read('verified_captcha') === true;
			
			if (!$verifiedCaptcha) {
				$resp = recaptcha_check_answer($this->recaptchaPrivateKey,
                    $_SERVER["REMOTE_ADDR"],
                    $this->params['form']['recaptcha_challenge_field'],
                    $this->params['form']['recaptcha_response_field']);
			}

			$fields = array('name', 'password_once', 'password_twice', 'email_address', 'last_login');
			
            $this->Player->set($this->data);
			$this->data['Player']['last_login'] = date("Y-m-d H:i:s");
			
			if ($verifiedCaptcha || $resp->is_valid) {
				if ($this->Player->validates(array('fieldList' => $fields))) {    
					$this->Session->delete('verified_captcha');
				
                    if (!empty($this->data['Player']['password_once']))
                        $this->data['Player']['password'] = $this->Auth->password($this->data['Player']['password_once']);
					
                    if ($this->Player->save($this->data, false, $fields)) {
                        $this->Session->setFlash('Your account has been created! Login below to start playing.', 'default', array('class' => 'success'));
                        $this->redirect($this->Auth->logout(array('controller' => 'players', 'action' => 'login')));
                    }
                } else {
					$this->Session->write('verified_captcha', true);
				}
            }
			else {
				$this->set('error', $resp->error);
			}			
        }

        unset($this->data['Player']['password_once']);
        unset($this->data['Player']['password_twice']);
    }

	function reset($reset_key = null) {
		if (empty($this->data) && empty($reset_key)) {
			$this->Session->setFlash("Invalid reset code specified. Request a new one below.", 'default', array('class' => 'error'));
			$this->redirect(array('action' => 'forgot'));
		}		
		
		$reset_key = empty($this->data) ? $reset_key : $this->data['Player']['reset_key'];			
		
		// Validate key
		$player = $this->Player->find('first', array(
			'conditions' => array('Player.reset_key' => $reset_key),
			'contain' => array()
		));

		if (empty($reset_key) || empty($player) || empty($player['Player']['reset_key']) || time() - strtotime($player['Player']['reset_key_date']) > 24 * 3600) {
			$this->Session->setFlash("The specified reset code has expired. Request a new one below and use it within 24 hours.", 'default', array('class' => 'error'));
			$this->redirect(array('action' => 'forgot'));			
		}
		
		if (!empty($this->data)) {
			$fields = array('id', 'password_once', 'password_twice', 'reset_key', 'password');
			$this->data['Player']['reset_key'] = null;
			$this->data['Player']['id'] = $player['Player']['id'];
			
			$this->Player->set($this->data);
			if ($this->Player->validates(array('fieldList' => $fields))) {
				if (!empty($this->data['Player']['password_once']))
					$this->data['Player']['password'] = $this->Auth->password($this->data['Player']['password_once']);			
					
				if ($this->Player->save($this->data, false, $fields)) {
					$this->Session->setFlash("Your password has been reset. Login below to start playing again", 'default', array('class' => 'success'));
					$this->redirect(array('action' => 'login'));
				}
			}
		}
		
		$this->data['Player']['reset_key'] = $reset_key;
	}
	
	function forgot() {
		$this->set('title_for_layout', 'Forgot Password');
	
		if (!empty($this->data)) {
			$player = $this->Player->find('first', array(
				'conditions' => array('name' => $this->data['Player']['name']),
				'contain' => array()
			));
			
			if (empty($player)) {
				$this->Session->setFlash("A player with the specified name was not found.", 'default', array('class' => 'error'));
				$this->redirect(array($this->here));			
			} else if (!empty($player['Player']['reset_key']) && time() - strtotime($player['Player']['reset_key_date']) < 3600) {
				$this->Session->setFlash("A reset e-mail has already been request for this account in the past hour. We only allow requesting one reset per hour.", 'default', array('class' => 'error'));
				$this->redirect(array($this->here));
			}
			else {
				$resetKey = sha1($player['Player']['name'] . rand() . time());

				if (!$this->Player->save(array(
					'id' => $player['Player']['id'],
					'reset_key' => $resetKey,
					'reset_key_date' => date('Y-m-d H:i:s')
				))) {
					$this->Session->setFlash("There was an error generating your reset request. Contact us for help.", 'default', array('class' => 'error'));				
					$this->redirect(array($this->here));	
				}
				
				$this->Email->to = '<' . $player['Player']['email_address'] . '>';
				$this->Email->subject = 'Password Reset Request';				
				$this->Email->template = 'reset_password';
				$this->set('reset_key', $resetKey);
				$this->Email->send();
				
				$this->Session->setFlash("A reset message has been sent to the e-mail address associated with this account.", 'default', array('class' => 'success'));				
				$this->redirect(array($this->here));				
			}			
		}				
	}
	
    function login() {
		$this->set('title_for_layout', 'Login');
	
        if (!empty($this->data)) {
            if ($this->Auth->login($this->data['Player'])) {
                $url = $this->Auth->redirect();
                $redirect = Router::parse($url);

                if ($redirect['controller'] == 'players' && ($redirect['action'] == 'login' || $redirect['action'] == 'register'))
                    $this->redirect('/play');
                else
                    $this->redirect($url);
            }
            else {
                if (isset($this->data['Player']['name']))
                    $this->Session->write('Player.name', $this->data['Player']['name']);

                $this->redirect($this->Auth->login());
            }
        }
        else {
            $this->data['Player']['name'] = $this->Session->read('Player.name');
            $this->Session->delete('Player.name');
        }

        unset($this->data['Player']['password']);

    }

    function logout() {
        $this->Session->setFlash("You have been logged out", 'default', array('class' => 'success'));
        $this->redirect($this->Auth->logout());
    }
}
