<?php

$report = array(
    'refreshOnClose' => $refreshOnClose,
    'outcomeOnly' => true,
    'playerOutcome' => $playerOutcome);

if (isset($loot)) {
    $report['loot'] = $loot;
}

echo json_encode($report);