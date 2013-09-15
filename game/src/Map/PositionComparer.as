package src.Map {
    import System.Collection.Generic.IEqualityComparer;

    public class PositionComparer implements IEqualityComparer {
        public function Equals(x: *, y: *): Boolean {
            var pos1: Position = x as Position;
            var pos2: Position = y as Position;

            if (!pos1 || !pos2) {
                return false;
            }

            return pos1.equals(pos2);
        }
    }
}
