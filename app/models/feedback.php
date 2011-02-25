<?php

class Feedback extends AppModel {

    var $name = 'Feedback';
    var $useTable = false;
    var $validate = array(
        'message' => array(
            'empty' => array(
                'rule' => array('notEmpty'),
                'message' => 'You have to write something to tell us',
                'required' => true,
                'allowEmpty' => false
            ),
            'maxLength' => array(
                'rule' => array('maxLength', 3000),
                'message' => 'Message is too long. Shorten the message and try again.',
            )
        )
    );

}
