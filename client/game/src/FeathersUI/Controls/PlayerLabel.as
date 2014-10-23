package src.FeathersUI.Controls {
    import feathers.controls.Label;

    public class PlayerLabel extends Label {
        public function PlayerLabel(playerId: int, playerName: String = null) {
            this.text = playerId.toString();
        }
    }
}
