<?php
/**
 *  @generated
 */
class Notification_NewTribeForumPost_args {
  static $_TSPEC;

  public $tribeId = null;
  public $unreadCount = null;

  public function __construct($vals=null) {
    if (!isset(self::$_TSPEC)) {
      self::$_TSPEC = array(
        1 => array(
          'var' => 'tribeId',
          'type' => TType::I32,
          ),
        2 => array(
          'var' => 'unreadCount',
          'type' => TType::I32,
          ),
        );
    }
    if (is_array($vals)) {
      if (isset($vals['tribeId'])) {
        $this->tribeId = $vals['tribeId'];
      }
      if (isset($vals['unreadCount'])) {
        $this->unreadCount = $vals['unreadCount'];
      }
    }
  }

  public function getName() {
    return 'Notification_NewTribeForumPost_args';
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
          if ($ftype == TType::I32) {
            $xfer += $input->readI32($this->tribeId);
          } else {
            $xfer += $input->skip($ftype);
          }
          break;
        case 2:
          if ($ftype == TType::I32) {
            $xfer += $input->readI32($this->unreadCount);
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
    $xfer += $output->writeStructBegin('Notification_NewTribeForumPost_args');
    if ($this->tribeId !== null) {
      $xfer += $output->writeFieldBegin('tribeId', TType::I32, 1);
      $xfer += $output->writeI32($this->tribeId);
      $xfer += $output->writeFieldEnd();
    }
    if ($this->unreadCount !== null) {
      $xfer += $output->writeFieldBegin('unreadCount', TType::I32, 2);
      $xfer += $output->writeI32($this->unreadCount);
      $xfer += $output->writeFieldEnd();
    }
    $xfer += $output->writeFieldStop();
    $xfer += $output->writeStructEnd();
    return $xfer;
  }

}


?>
