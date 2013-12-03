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

public class DietSummarizer extends TechnologySummarizer {

        override public function getSummary():* {
            var bonus: int = Enumerable.from(array).sum(function(tech:TechnologyStats): int {
                return tech.techPrototype.level*10;
            });
            return StringHelper.localize(name+"_SUMMARY",Math.min(30,bonus));
        }

    }
}
