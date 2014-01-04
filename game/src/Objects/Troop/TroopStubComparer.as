package src.Objects.Troop 
{
    import System.Collection.Generic.IEqualityComparer;

    public class TroopStubComparer implements IEqualityComparer
    {                
        public function Equals(x: *, y:*) : Boolean {
            return TroopStub.compareCityIdAndTroopId(x, [y.cityId, y.id]) == 0;
        }              
    }
}