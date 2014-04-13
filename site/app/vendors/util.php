<?php

function set_default_options($defaultOptions, $input) {
    if (!is_array($input))
        return $defaultOptions;
    
    foreach ($input as $key => $value) {
        if (isset($defaultOptions[$key]) && $value && is_array($value))
            $defaultOptions[$key] = set_default_options($defaultOptions[$key], $input[$key]);
        else
            $defaultOptions[$key] = $value;
    }

    return $defaultOptions;
}

function check($array, $paths = null) {
    if (!is_array($paths)) {
        $paths = func_get_args();
        array_shift($paths);
    }

    foreach ($paths as $path) {
        if (!Set::check($array, $path))
            return false;
    }

    return true;
}

function get_value($array, $path, $defaultValue = FALSE) {    
    if (!$array)
        return $defaultValue;
    
    if (!is_array($path))
        $path = explode('.', $path);

    foreach ($path as $i => $key) {
        if (is_numeric($key) && intval($key) > 0 || $key === '0')
            $key = intval($key);

        if ($i === count($path) - 1) {
            if (is_array($array) && array_key_exists($key, $array))
                return $array[$key];
            else
                break;
        }

        if (!is_array($array) || !array_key_exists($key, $array))
            break;

        $array = & $array[$key];
    }

    return $defaultValue;
}