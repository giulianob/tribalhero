<?php

class ClearCacheShell extends Shell {

    var $uses = array();

    function main() {
        Cache::clear();

        $cachePaths = array('js', 'css', 'views', 'persistent', 'models', 'assets');
        foreach ($cachePaths AS $config) {
            clearCache(null, $config);
        }

        $this->out('Cache cleared');
    }

}