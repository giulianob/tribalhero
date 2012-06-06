<?php

class AddResourcesToTribes extends Ruckusing_BaseMigration {

    public function up() {
        $this->add_column("tribes", "crop", "integer", array('null' => false));
        $this->add_column("tribes", "gold", "integer", array('null' => false));
        $this->add_column("tribes", "iron", "integer", array('null' => false));
        $this->add_column("tribes", "wood", "integer", array('null' => false));
    }

    public function down() {
        $this->remove_column("tribes", "crop");
        $this->remove_column("tribes", "gold");
        $this->remove_column("tribes", "iron");
        $this->remove_column("tribes", "wood");
    }

}