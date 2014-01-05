/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 11/29/13
 * Time: 11:50 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Objects.Technologies {
import System.Linq.Enumerable;

import src.Objects.TechnologyStats;
import src.Util.StringHelper;

public class InstantTrainSummarizer extends TechnologySummarizer {
        private var unitPerLevel:int;
        private var max: int;

        public function InstantTrainSummarizer(unitPerLevel: int, max:int) {
            this.unitPerLevel = unitPerLevel;
            this.max = max;
        }
        override public function getSummary():* {
            var bonus: int = Enumerable.from(array).sum(function(tech:TechnologyStats): int {
                return tech.techPrototype.level*unitPerLevel;
            });
            return StringHelper.localize(name+"_SUMMARY",Math.min(max,bonus));
        }

}
}