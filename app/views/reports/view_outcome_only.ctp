<?php

$report = array(
    'refreshOnClose' => $refreshOnClose,
    'outcomeOnly' => true);

if (isset($playerOutcome)) {
    $report['playerOutcome'] = $playerOutcome;
}

if (isset($loot)) {
    $report['loot'] = $loot;
}

echo json_encode($report);