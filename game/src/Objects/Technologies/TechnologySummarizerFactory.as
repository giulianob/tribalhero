/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 11/30/13
 * Time: 12:46 AM
 * To change this template use File | Settings | File Templates.
 */
package src.Objects.Technologies {
import src.Objects.Prototypes.EffectPrototype;


public class TechnologySummarizerFactory {
        public function TechnologySummarizerFactory() {
        }

        public function CreateSummarizer(key: String, array: Array) : ITechnologySummarizer {
            var summarizer:ITechnologySummarizer;
            if(key.indexOf("WEAPON_RESEARCH_TECHNOLOGY")!=-1) {
                summarizer=new AttackDefenseBonusSummarizer(EffectPrototype.EFFECT_ATTACK_BONUS);
            } else if(key.indexOf("ARMOR_RESEARCH_TECHNOLOGY")!=-1) {
                summarizer=new AttackDefenseBonusSummarizer(EffectPrototype.EFFECT_DEFENSE_BONUS);
            } else if(key.indexOf("SUNDANCE")!=-1) {
                summarizer=new SundanceSummarizer();
            } else if(key.indexOf("GREED")!=-1) {
                summarizer=new GreedIsGoodSummarizer();
            } else if(key.indexOf("DOUBLE_TIME")!=-1) {
                summarizer=new DoubleTimeSummarizer();
            } else if(key.indexOf("RUSH")!=-1) {
                summarizer=new RushSummarizer();
            } else if(key.indexOf("URGENCY")!=-1) {
                summarizer=new SenseOfUrgencySummarizer();
            } else if(key.indexOf("HAPPYHOUR")!=-1) {
                summarizer=new HappyHourSummarizer();
            } else if(key.indexOf("UPKEEP_REDUCE")!=-1) {
                summarizer=new DietSummarizer();
            } else if(key.indexOf("INSTANT")!=-1) {
                if(key.indexOf("TRAINING_GROUND")!=-1) {
                    summarizer = new InstantTrainSummarizer(5,150);
                } else if(key.indexOf("BARRACK")!=-1) {
                    summarizer = new InstantTrainSummarizer(5,150);
                } else if(key.indexOf("STABLE")!=-1) {
                    summarizer = new InstantTrainSummarizer(2,150);
                } else if(key.indexOf("WORKSHOP")!=-1) {
                    summarizer = new InstantTrainSummarizer(1,150);
                } else {
                    summarizer=new TechnologySummarizer();
                }
            } else {
                summarizer=new TechnologySummarizer();
            }
            summarizer.setParameters(key,array);
            return summarizer;

        }
    }
}
