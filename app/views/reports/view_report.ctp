<?php

$report = array(
    'refreshOnClose' => $refreshOnClose,
    'outcomeOnly' => false,
    'battleOutcome' => $battleOutcome,
    'battleEvents' => $battleEvents);

if (!empty($playerOutcome)) {
    $report['playerOutcome'] = $playerOutcome;
}

if (!empty($loot)) {
    $report['loot'] = $loot;
}

echo json_encode($report);