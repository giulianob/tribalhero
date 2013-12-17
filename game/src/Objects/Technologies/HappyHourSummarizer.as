/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 11/29/13
 * Time: 11:50 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Objects.Technologies {
import System.Linq.Enumerable;

import src.Objects.Prototypes.EffectPrototype;
import src.Objects.TechnologyStats;
import src.Util.StringHelper;

public class HappyHourSummarizer extends TechnologySummarizer {

        override public function getSummary():* {
            return StringHelper.localize(name+"_SUMMARY",30+(array.length-1)*8);
        }

    }
}
