<?php
/**
 * @property Property $Property
 * @property PropertyHistory $PropertyHistory
 */
class RankingShell extends Shell {

    var $uses = array('City', 'Ranking');

    function main() {
        $timeStart = microtime(true);
        $this->Ranking->batchRanking();
        $this->out("Script took: " . (microtime(true) - $timeStart) . " seconds");
    }
}