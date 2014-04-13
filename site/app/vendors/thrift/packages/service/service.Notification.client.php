<?php
/**
 *  @generated
 */
class NotificationClient implements NotificationIf {
  protected $input_ = null;
  protected $output_ = null;

  protected $seqid_ = 0;

  public function __construct($input, $output=null) {
    $this->input_ = $input;
    $this->output_ = $output ? $output : $input;
  }

  public function NewMessage($playerUnreadCount)
  {
    $this->send_NewMessage($playerUnreadCount);
    $this->recv_NewMessage();
  }

  public function send_NewMessage($playerUnreadCount)
  {
    $args = new Notification_NewMessage_args();
    $args->playerUnreadCount = $playerUnreadCount;
    $bin_accel = ($this->output_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_write_binary');
    if ($bin_accel)
    {
      thrift_protocol_write_binary($this->output_, 'NewMessage', TMessageType::CALL, $args, $this->seqid_, $this->output_->isStrictWrite());
    }
    else
    {
      $this->output_->writeMessageBegin('NewMessage', TMessageType::CALL, $this->seqid_);
      $args->write($this->output_);
      $this->output_->writeMessageEnd();
      $this->output_->getTransport()->flush();
    }
  }

  public function recv_NewMessage()
  {
    $bin_accel = ($this->input_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_read_binary');
    if ($bin_accel) $result = thrift_protocol_read_binary($this->input_, 'Notification_NewMessage_result', $this->input_->isStrictRead());
    else
    {
      $rseqid = 0;
      $fname = null;
      $mtype = 0;

      $this->input_->readMessageBegin($fname, $mtype, $rseqid);
      if ($mtype == TMessageType::EXCEPTION) {
        $x = new TApplicationException();
        $x->read($this->input_);
        $this->input_->readMessageEnd();
        throw $x;
      }
      $result = new Notification_NewMessage_result();
      $result->read($this->input_);
      $this->input_->readMessageEnd();
    }
    return;
  }

  public function NewTribeForumPost($tribeId, $unreadCount)
  {
    $this->send_NewTribeForumPost($tribeId, $unreadCount);
    $this->recv_NewTribeForumPost();
  }

  public function send_NewTribeForumPost($tribeId, $unreadCount)
  {
    $args = new Notification_NewTribeForumPost_args();
    $args->tribeId = $tribeId;
    $args->unreadCount = $unreadCount;
    $bin_accel = ($this->output_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_write_binary');
    if ($bin_accel)
    {
      thrift_protocol_write_binary($this->output_, 'NewTribeForumPost', TMessageType::CALL, $args, $this->seqid_, $this->output_->isStrictWrite());
    }
    else
    {
      $this->output_->writeMessageBegin('NewTribeForumPost', TMessageType::CALL, $this->seqid_);
      $args->write($this->output_);
      $this->output_->writeMessageEnd();
      $this->output_->getTransport()->flush();
    }
  }

  public function recv_NewTribeForumPost()
  {
    $bin_accel = ($this->input_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_read_binary');
    if ($bin_accel) $result = thrift_protocol_read_binary($this->input_, 'Notification_NewTribeForumPost_result', $this->input_->isStrictRead());
    else
    {
      $rseqid = 0;
      $fname = null;
      $mtype = 0;

      $this->input_->readMessageBegin($fname, $mtype, $rseqid);
      if ($mtype == TMessageType::EXCEPTION) {
        $x = new TApplicationException();
        $x->read($this->input_);
        $this->input_->readMessageEnd();
        throw $x;
      }
      $result = new Notification_NewTribeForumPost_result();
      $result->read($this->input_);
      $this->input_->readMessageEnd();
    }
    return;
  }

  public function NewBattleReport($playerUnreadCount)
  {
    $this->send_NewBattleReport($playerUnreadCount);
    $this->recv_NewBattleReport();
  }

  public function send_NewBattleReport($playerUnreadCount)
  {
    $args = new Notification_NewBattleReport_args();
    $args->playerUnreadCount = $playerUnreadCount;
    $bin_accel = ($this->output_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_write_binary');
    if ($bin_accel)
    {
      thrift_protocol_write_binary($this->output_, 'NewBattleReport', TMessageType::CALL, $args, $this->seqid_, $this->output_->isStrictWrite());
    }
    else
    {
      $this->output_->writeMessageBegin('NewBattleReport', TMessageType::CALL, $this->seqid_);
      $args->write($this->output_);
      $this->output_->writeMessageEnd();
      $this->output_->getTransport()->flush();
    }
  }

  public function recv_NewBattleReport()
  {
    $bin_accel = ($this->input_ instanceof TProtocol::$TBINARYPROTOCOLACCELERATED) && function_exists('thrift_protocol_read_binary');
    if ($bin_accel) $result = thrift_protocol_read_binary($this->input_, 'Notification_NewBattleReport_result', $this->input_->isStrictRead());
    else
    {
      $rseqid = 0;
      $fname = null;
      $mtype = 0;

      $this->input_->readMessageBegin($fname, $mtype, $rseqid);
      if ($mtype == TMessageType::EXCEPTION) {
        $x = new TApplicationException();
        $x->read($this->input_);
        $this->input_->readMessageEnd();
        throw $x;
      }
      $result = new Notification_NewBattleReport_result();
      $result->read($this->input_);
      $this->input_->readMessageEnd();
    }
    return;
  }

}


?>
