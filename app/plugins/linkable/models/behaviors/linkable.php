<?php
/*
 * LinkableBehavior by GiulianoB
 * Version 1.1
 *
 * More information at:
 * https://bitbucket.org/giulianob/linkable
 *
 * Licensed under The MIT License
 * Redistributions of files must retain the above copyright notice.
 */
 
class LinkableBehavior extends ModelBehavior {
	
	protected $_key = 'link';
	
	protected $_options = array(
		'type' => true, 'table' => true, 'alias' => true,
		'conditions' => true, 'fields' => true, 'reference' => true,
		'class' => true, 'defaults' => true
	);

	protected $_defaults = array('type' => 'LEFT');
	
	public function beforeFind(&$Model, $query) {		
		if (isset($query[$this->_key])) {			
			$optionsDefaults = $this->_defaults + array('reference' => $Model->alias, $this->_key => array());
			$optionsKeys = $this->_options + array($this->_key => true);
			
			// If containable is being used, then let it set the recursive!
			if (empty($query['contain'])) {
				$query = am(array('joins' => array()), $query, array('recursive' => -1));
			} else {				
				$query = am(array('joins' => array()), $query);
			}
			$iterators[] = $query[$this->_key];
			$cont = 0;
			do {			
				$iterator = $iterators[$cont];
				$defaults = $optionsDefaults;
				if (isset($iterator['defaults'])) {
					$defaults = array_merge($defaults, $iterator['defaults']);
					unset($iterator['defaults']);
				}
				$iterations = Set::normalize($iterator);
				foreach ($iterations as $alias => $options) {
					if (is_null($options)) {
						$options = array();
					}
					$options = am($defaults, compact('alias'), $options);
					if (empty($options['alias'])) {
						throw new InvalidArgumentException(sprintf('%s::%s must receive aliased links', get_class($this), __FUNCTION__));
					}					
					if (empty($options['table']) && empty($options['class'])) {
						$options['class'] = $options['alias'];
					} elseif (!empty($options['table']) && empty($options['class'])) {
						$options['class'] = Inflector::classify($options['table']);
					}
					
					// the incoming model to be linked in query
					$_Model =& ClassRegistry::init($options['class']);
					// the already in query model that links to $_Model
					$Reference =& ClassRegistry::init($options['reference']); 	
					$db =& $_Model->getDataSource();
					$associations = $_Model->getAssociated();
					if (isset($Reference->belongsTo[$_Model->alias])) {
						$type = 'hasOne';
						$association = $Reference->belongsTo[$_Model->alias];						
					} else if (isset($associations[$Reference->alias])) {
						$type = $associations[$Reference->alias];
						$association = $_Model->{$type}[$Reference->alias];						
					} else {
						$_Model->bindModel(array('belongsTo' => array($Reference->name => array('className' => $Reference->name))));
						$type = 'belongsTo';
						$association = $_Model->{$type}[$Reference->name];
						$_Model->unbindModel(array('belongsTo' => array($Reference->name)));
					}
					
					if (!isset($options['conditions'])) {
						$options['conditions'] = array();
					} else if (!is_array($options['conditions'])) {
						// Support for string conditions
						$options['conditions'] = array($options['conditions']);
					}
				
					if (isset($options['conditions']['exactly'])) {
						if (is_array($options['conditions']['exactly']))
							$options['conditions'] = reset($options['conditions']['exactly']);
						else
							$options['conditions'] = array($options['conditions']['exactly']);
					} else {
						if ($type === 'belongsTo') {
							$modelKey = $_Model->escapeField($association['foreignKey']);
							$referenceKey = $Reference->escapeField($Reference->primaryKey);
							$options['conditions'][] = "{$referenceKey} = {$modelKey}";
						} elseif ($type === 'hasAndBelongsToMany') {
							if (isset($association['with'])) {
								$Link =& $_Model->{$association['with']};
								if (isset($Link->belongsTo[$_Model->alias])) {
									$modelLink = $Link->escapeField($Link->belongsTo[$_Model->alias]['foreignKey']);
								}
								if (isset($Link->belongsTo[$Reference->alias])) {
									$referenceLink = $Link->escapeField($Link->belongsTo[$Reference->alias]['foreignKey']);
								}								
							} else {
								$Link =& $_Model->{Inflector::classify($association['joinTable'])};
							}
							if (empty($modelLink)) {
								$modelLink = $Link->escapeField(Inflector::underscore($_Model->alias) . '_id');
							}
							if (empty($referenceLink)) {
								$referenceLink = $Link->escapeField(Inflector::underscore($Reference->alias) . '_id');
							}
							$referenceKey = $Reference->escapeField();
							$query['joins'][] = array(
								'alias' => $Link->alias,
								'table' => $Link->getDataSource()->fullTableName($Link),
								'conditions' => "{$referenceLink} = {$referenceKey}",
								'type' => 'LEFT'
							);
							$modelKey = $_Model->escapeField();
							$options['conditions'][] = "{$modelLink} = {$modelKey}";						
						} else {
							$referenceKey = $Reference->escapeField($association['foreignKey']);
							$modelKey = $_Model->escapeField($_Model->primaryKey);
							$options['conditions'][] = "{$modelKey} = {$referenceKey}";
						}					
					}

                    if (!empty($association['conditions'])) {
                        if (is_array($association['conditions'])) {
                            $options['conditions'] = array_combine($association['conditions'], $options['conditions']);
                        }
                        else {
                            $options['conditions'][] = $association['conditions'];
                        }
                    }
						
					if (empty($options['table'])) {
						$options['table'] = $db->fullTableName($_Model, true);
					}
					
					// Decide whether we should mess with the fields or not
					// If this query is a COUNT query then we just leave it alone
					if (!isset($query['fields']) || is_array($query['fields']) || strpos($query['fields'], 'COUNT(*)') === FALSE) {
						if (!empty($options['fields'])) {
							if ($options['fields'] === true && !empty($association['fields'])) {
								$options['fields'] = $db->fields($_Model, null, $association['fields']);
							} elseif ($options['fields'] === true) {
								$options['fields'] = $db->fields($_Model);
							} else {
								$options['fields'] = $db->fields($_Model, null, $options['fields']);	
							}
							
							if (is_array($query['fields']))						
								$query['fields'] = array_merge($query['fields'], $options['fields']);						
							else						
								$query['fields'] = array_merge($db->fields($Model), $options['fields']);						
						}
						else if (!isset($options['fields']) || (isset($options['fields']) && !is_array($options['fields'])))
						{
							if (!empty($association['fields'])) {
									$options['fields'] = $db->fields($_Model, null, $association['fields']);
							} else {
									$options['fields'] = $db->fields($_Model);
							}
							
							if (is_array($query['fields'])) {
								$query['fields'] = array_merge($query['fields'], $options['fields']);
							} else {
								// If user didn't specify any fields then select all fields by default (just as find would)
								$query['fields'] = array_merge($db->fields($Model), $options['fields']);
							}
						}
					}
					
					$options[$this->_key] = am($options[$this->_key], array_diff_key($options, $optionsKeys));
					$options = array_intersect_key($options, $optionsKeys);
					if (!empty($options[$this->_key])) {
						$iterators[] = $options[$this->_key] + array('defaults' => array_merge($defaults, array('reference' => $options['class'])));
					}
					
					$query['joins'][] = array_intersect_key($options, array('type' => true, 'alias' => true, 'table' => true, 'conditions' => true));					
				}
				$cont++;
				$notDone = isset($iterators[$cont]);
			} while ($notDone);
		}		
		
		unset($query['link']);
		
		return $query;
	}
}