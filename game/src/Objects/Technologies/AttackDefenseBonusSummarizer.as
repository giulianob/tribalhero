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

public class AttackDefenseBonusSummarizer extends TechnologySummarizer {
        private var effectCode:int;

        public function AttackDefenseBonusSummarizer(effectCode: int) {
            this.effectCode=effectCode;
        }
        override public function getSummary():* {
            var bonus: int = Enumerable.from(array).where(function(tech:TechnologyStats):Boolean {
                return Enumerable.from(tech.techPrototype.effects).any(function(effect:EffectPrototype):Boolean{
                    return effect.effectCode==effectCode;
                })
            }).sum(function(tech:TechnologyStats): int {
                return tech.techPrototype.level * 2;
            });
            return StringHelper.localize(name+"_SUMMARY",Math.min(100,bonus));
        }

    }
}
