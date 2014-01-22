package src.Objects.Technologies {
    import src.Objects.Prototypes.EffectPrototype;


    public class TechnologySummarizerFactory {
        public function TechnologySummarizerFactory() {
        }

        public function CreateSummarizer(key: String, array: Array): ITechnologySummarizer {
            var summarizer: ITechnologySummarizer;
            if (key == "WEAPON_RESEARCH_TECHNOLOGY") {
                summarizer = new AttackDefenseBonusSummarizer(EffectPrototype.EFFECT_ATTACK_BONUS);
            } else if (key == "ARMOR_RESEARCH_TECHNOLOGY") {
                summarizer = new AttackDefenseBonusSummarizer(EffectPrototype.EFFECT_DEFENSE_BONUS);
            } else if (key == "SUNDANCE_TECHNOLOGY") {
                summarizer = new SundanceSummarizer();
            } else if (key == "GREED_IS_GOOD_TECHNOLOGY") {
                summarizer = new GreedIsGoodSummarizer();
            } else if (key == "DOUBLE_TIME_TECHNOLOGY") {
                summarizer = new DoubleTimeSummarizer();
            } else if (key == "RUSH_ATTACK_TECHNOLOGY") {
                summarizer = new RushSummarizer(4);
            } else if (key == "RUSH_DEFENSE_TECHNOLOGY") {
                summarizer = new RushSummarizer(8);
            } else if (key == "SENSE_OF_URGENCY_TECHNOLOGY") {
                summarizer = new SenseOfUrgencySummarizer();
            } else if (key == "HAPPYHOUR_TECHNOLOGY") {
                summarizer = new HappyHourSummarizer();
            } else if (key.indexOf("UPKEEP_REDUCE_") == 0) {
                summarizer = new DietSummarizer();
            } else if (key.indexOf("INSTANT_") == 0) {
                if (key.indexOf("TRAINING_GROUND") != -1) {
                    summarizer = new InstantTrainSummarizer(5, 150);
                } else if (key.indexOf("BARRACK") != -1) {
                    summarizer = new InstantTrainSummarizer(5, 150);
                } else if (key.indexOf("STABLE") != -1) {
                    summarizer = new InstantTrainSummarizer(2, 60);
                } else if (key.indexOf("WORKSHOP") != -1) {
                    summarizer = new InstantTrainSummarizer(1, 30);
                } else {
                    summarizer = new TechnologySummarizer();
                }
            } else {
                summarizer = new TechnologySummarizer();
            }
            summarizer.setParameters(key, array);
            return summarizer;

        }
    }
}
