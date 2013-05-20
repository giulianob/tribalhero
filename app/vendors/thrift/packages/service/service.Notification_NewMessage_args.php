<?php
/**
 *  @generated
 */
class Notification_NewMessage_args {
  static $_TSPEC;

  public $playerUnreadCount = null;

  public function __construct($vals=null) {
    if (!isset(self::$_TSPEC)) {
      self::$_TSPEC = array(
        1 => array(
          'var' => 'playerUnreadCount',
          'type' => TType::STRUCT,
          'class' => 'PlayerUnreadCount',
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
    return 'Notification_NewMessage_args';
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
          if ($ftype == TType::STRUCT) {
            $this->playerUnreadCount = new PlayerUnreadCount();
            $xfer += $this->playerUnreadCount->read($input);
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
    $xfer += $output->writeStructBegin('Notification_NewMessage_args');
    if ($this->playerUnreadCount !== null) {
      if (!is_object($this->playerUnreadCount)) {
        throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
      }
      $xfer += $output->writeFieldBegin('playerUnreadCount', TType::STRUCT, 1);
      $xfer += $this->playerUnreadCount->write($output);
      $xfer += $output->writeFieldEnd();
    }
    $xfer += $output->writeFieldStop();
    $xfer += $output->writeStructEnd();
    return $xfer;
  }

}


?>
