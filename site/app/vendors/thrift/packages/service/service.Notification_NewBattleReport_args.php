<?php
/**
 *  @generated
 */
class Notification_NewBattleReport_args {
  static $_TSPEC;

  public $playerUnreadCount = null;

  public function __construct($vals=null) {
    if (!isset(self::$_TSPEC)) {
      self::$_TSPEC = array(
        1 => array(
          'var' => 'playerUnreadCount',
          'type' => TType::LST,
          'etype' => TType::STRUCT,
          'elem' => array(
            'type' => TType::STRUCT,
            'class' => 'PlayerUnreadCount',
            ),
          ),
        );
    }
    if (is_array($vals)) {
      if (isset($vals['playerUnreadCount'])) {
        $this->playerUnreadCount = $vals['playerUnreadCount'];
      }
    }
  }

  public function getName() {
    return 'Notification_NewBattleReport_args';
  }

  public function read($input)
  {
    $xfer = 0;
    $fname = null;
    $ftype = 0;
    $fid = 0;
    $xfer += $input->readStructBegin($fname);
    while (true)
    {
      $xfer += $input->readFieldBegin($fname, $ftype, $fid);
      if ($ftype == TType::STOP) {
        break;
      }
      switch ($fid)
      {
        case 1:
          if ($ftype == TType::LST) {
            $this->playerUnreadCount = array();
            $_size0 = 0;
            $_etype3 = 0;
            $xfer += $input->readListBegin($_etype3, $_size0);
            for ($_i4 = 0; $_i4 < $_size0; ++$_i4)
            {
              $elem5 = null;
              $elem5 = new PlayerUnreadCount();
              $xfer += $elem5->read($input);
              $this->playerUnreadCount []= $elem5;
            }
            $xfer += $input->readListEnd();
          } else {
            $xfer += $input->skip($ftype);
          }
          break;
        default:
          $xfer += $input->skip($ftype);
          break;
      }
      $xfer += $input->readFieldEnd();
    }
    $xfer += $input->readStructEnd();
    return $xfer;
  }

  public function write($output) {
    $xfer = 0;
    $xfer += $output->writeStructBegin('Notification_NewBattleReport_args');
    if ($this->playerUnreadCount !== null) {
      if (!is_array($this->playerUnreadCount)) {
        throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
      }
      $xfer += $output->writeFieldBegin('playerUnreadCount', TType::LST, 1);
      {
        $output->writeListBegin(TType::STRUCT, count($this->playerUnreadCount));
        {
          foreach ($this->playerUnreadCount as $iter6)
          {
            $xfer += $iter6->write($output);
          }
        }
        $output->writeListEnd();
      }
      $xfer += $output->writeFieldEnd();
    }
    $xfer += $output->writeFieldStop();
    $xfer += $output->writeStructEnd();
    return $xfer;
  }

}


?>
