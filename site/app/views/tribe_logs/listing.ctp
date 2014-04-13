<?php

$paging = $paginator->params('TribeLog');

$data = array();

foreach ($tribelogs as $tribelog) {

    $data[] = array(
            'created' => strtotime($tribelog['TribeLog']['created']),
            'type' => $tribelog['TribeLog']['type'],
            'parameters' => $tribelog['TribeLog']['parameters'],
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'tribelogs' => $data);

echo $this->Js->object($results);
