<?php

$report = array(
    'refreshOnClose' => $refreshOnClose,
    'outcomeOnly' => false,
    'playerOutcome' => $playerOutcome,
    'battleOutcome' => $battleOutcome,
    'battleEvents' => $battleEvents);

if (!empty($loot)) {
    $report['loot'] = $loot;
}

echo json_encode($report);