<?php

class SplitTables extends Ruckusing_BaseMigration {

    public function up() {

        $this->execute("
            ALTER TABLE `cities`
            ADD COLUMN `value` smallint(5) unsigned   NOT NULL DEFAULT '1' after `labor_production_rate`,
            ADD COLUMN `deleted` int(11)   NOT NULL DEFAULT '0' after `value`,
            ADD KEY `deleted`(`deleted`), COMMENT='';

            ALTER TABLE `players`
            CHANGE `name` `name` varchar(64)  COLLATE latin1_swedish_ci NOT NULL after `id`,
            CHANGE `session_id` `session_id` varchar(128)  COLLATE latin1_swedish_ci NOT NULL after `name`,
            CHANGE `last_login` `last_login` datetime   NOT NULL after `session_id`,
            CHANGE `online` `online` tinyint(1)   NOT NULL after `last_login`,
            CHANGE `created` `created` datetime   NOT NULL after `online`,
            DROP COLUMN `facebook_id`,
            DROP COLUMN `password`,
            DROP COLUMN `email_address`,
            DROP COLUMN `login_key`,
            DROP COLUMN `login_key_date`,
            DROP COLUMN `deleted`,
            DROP COLUMN `banned`,
            DROP COLUMN `admin`,
            DROP COLUMN `reset_key`,
            DROP COLUMN `reset_key_date`,
            DROP KEY `email`,
            DROP KEY `facebook_id`,
            DROP KEY `login_key`,
            ADD KEY `name`(`name`), COMMENT='';

            DROP TABLE `stacktraces`;
        ");
   }

    public function down() {
        throw new Exception("Aint no going back on this one");
    }

}