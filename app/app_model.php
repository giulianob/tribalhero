<?php

class AppModel extends Model {

    var $recursive = -1;
    var $actsAs = array('Containable', 'Linkable.Linkable');
    var $cacheQueries = true;

    //Lazy loading
    function __get($assoc) {
        foreach ($this->__associations as $type) {
            if (array_key_exists($assoc, $this->{$type}))
                parent::__constructLinkedModel($assoc, $this->{$type}[$assoc]['className']);

            parent::__generateAssociation($type);
        }

        return $this->{$assoc};
    }

    function __isset($assoc) {
        if (isset($this->{$assoc})) {
            return true;
        } else {
            //Try loading a model
            foreach ($this->__associations as $type) {
                if (array_key_exists($assoc, $this->{$type})) {
                    parent::__constructLinkedModel($assoc, $this->{$type}[$assoc]['className']);
                }

                parent::__generateAssociation($type);
            }

            return isset($this->{$assoc});
        }
    }

    function __constructLinkedModel($assoc, $className = null) {

        foreach ($this->__associations as $type) {
            if (array_key_exists($assoc, $this->{$type})) {
                return;
            }
        }

        parent::__constructLinkedModel($assoc, $className);
    }

    /**
     * Validation rule to match one field to another
     * @param <type> $field
     * @param <type> $matchField
     * @return <type>
     */
    function match($field, $matchField) {
        $val = array_pop($field);
        return empty($val) || strcmp($val, $this->data[$this->name][$matchField]) == 0;
    }

}
