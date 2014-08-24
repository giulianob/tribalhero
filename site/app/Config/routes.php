<?php

	Router::connect('/', array('controller' => 'pages', 'action' => 'index'));
	Router::connect('/play', array('controller' => 'pages', 'action' => 'play'));
	Router::connect('/facebook', array('controller' => 'pages', 'action' => 'facebook'));	
        Router::connect('/feedback', array('controller' => 'pages', 'action' => 'feedback'));
	Router::connect('/pages/*', array('controller' => 'pages', 'action' => 'display'));

	CakePlugin::routes();
	require CAKE . 'Config' . DS . 'routes.php';	