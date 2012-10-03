<?php

class FixMessageBoardIndex extends Ruckusing_BaseMigration {

    public function up() {
        $this->remove_index('message_board_threads', array('sticky', 'last_post_date'), array('name' => 'idx_message_board_threads_sticky_and_last_post_date'));
        $this->add_index('message_board_threads', 'sticky', array('idx_sticky'));
        $this->add_index('message_board_threads', 'last_post_date', array('idx_last_post_date'));
    }

    public function down() {
        
    }

}