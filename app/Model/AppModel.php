<?php
App::uses('Model', 'Model');

class AppModel extends Model {

    var $recursive = -1;
    var $actsAs = array('Containable', 'Linkable.Linkable');
    var $cacheQueries = false;

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

    /**
     * Returns SQL time string from sql time
     * @param type $time
     * @return type 
     */
    function toSqlTime($time) {
        return date("Y-m-d H:i:s", $time);
    }
    
   /**
     * Override of deleteDependent to support faster dependency deletion.
     * To use just set 'dependentBatchDelete' => true in the $hasMany variable.
     * Related records will then be removed using deleteAll instead of deleting 1 by 1.
     * The only side effect is that no callbacks will be issued on the deleted records, so anything dependent on those records
     * will not be deleted.
     * 
     * This should be moved to a behavior at some point in time.
     * 
     * @param type $id
     * @param type $cascade 
     */
    function _deleteDependent($id, $cascade) {
        parent::_deleteDependent($id, $cascade);

        if (!empty($this->__backAssociation)) {
            $savedAssociatons = $this->__backAssociation;
            $this->__backAssociation = array();
        }

        // Support to do batch delete on children instead of letting cake select 1 at a time. This is for performance.
        foreach (array_merge($this->hasMany, $this->hasOne) as $assoc => $data) {
            if (array_key_exists('dependentBatchDelete', $data) && $data['dependentBatchDelete'] === true && $cascade === true) {
                $model = & $this->{$assoc};
                $conditions = array($model->escapeField($data['foreignKey']) => $id);
                if ($data['conditions'])
                    $conditions = array_merge((array) $data['conditions'], $conditions);

                $model->recursive = -1;

                $model->deleteAll($conditions, false, false);
            }
        }

        if (isset($savedAssociatons)) {
            $this->__backAssociation = $savedAssociatons;
        }
    }
    
}
