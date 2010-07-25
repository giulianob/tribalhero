<?php
class PlayersController extends AppController {


    var $name = 'Players';

    function beforeFilter() {
        parent::beforeFilter();
        
        $this->Auth->allow(array('register', 'login', 'logout'));

        $this->Security->disabledFields = array('recaptcha_challenge_field', 'recaptcha_response_field');
    }

    function register() {
        App::import('Vendor', 'Recaptcha', array('file' => 'recaptcha'.DS.'recaptchalib.php'));
        $this->set('recaptchaPublicKey', $this->recaptchaPublicKey);

        if (!empty($this->data)) {
            $resp = recaptcha_check_answer($this->recaptchaPrivateKey,
                    $_SERVER["REMOTE_ADDR"],
                    $this->params['form']['recaptcha_challenge_field'],
                    $this->params['form']['recaptcha_response_field']);

            $this->Player->set($this->data);
            if ($this->Player->validates()) {
                if ($resp->is_valid) {
                    if (!empty($this->data['Player']['password_once']))
                        $this->data['Player']['password'] = $this->Auth->password($this->data['Player']['password_once']);

                    if ($this->Player->save($this->data, false)) {
                        $this->Session->setFlash('Your account has been created! Check your e-mail for the activation code. Remember to check your spam folder if you don\'t see an e-mail from us.');
                        $this->redirect($this->Auth->logout(array('controller' => 'players', 'action' => 'login')));
                    }
                }
                else {
                    $this->set('error', $resp->error);
                }
            }
        }

        unset($this->data['Player']['password_once']);
        unset($this->data['Player']['password_twice']);
    }

    function login() {
        if (!empty($this->data)) {
            if ($this->Auth->login($this->data['Player'])) {
                $url = $this->Auth->redirect();
                $redirect = Router::parse($url);

                if ($redirect['controller'] == 'players' && ($redirect['action'] == 'login' || $redirect['action'] == 'register'))
                    $this->redirect('/');
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
        $this->Session->setFlash("You have been logged out");
        $this->redirect($this->Auth->logout());
    }
}
