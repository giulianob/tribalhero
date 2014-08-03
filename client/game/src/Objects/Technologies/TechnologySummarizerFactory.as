package src.Objects.Technologies {
    public class TechnologySummarizerFactory {
        public function TechnologySummarizerFactory() {
        }

        public function CreateSummarizer(key: String, array: Array): ITechnologySummarizer {
            var summarizer: ITechnologySummarizer = null;

            var nonStackingTechnologies: Array = [
                "OVERTIME_TECHNOLOGY",
                "A_CALL_TO_ARMS_TECHNOLOGY",
                "TUNNEL_EXCAVATION_TECHNOLOGY",
                "SENSE_OF_URGENCY_TECHNOLOGY",
                "LAST_STAND_TECHNOLOGY",
                "ARCHERY_RESEARCH_TECHNOLOGY",
                "BOW_RESEARCH_TECHNOLOGY",
                "HIPPOSANDAL_TECHNOLOGY",
                "ATTIC_STORAGE_TECHNOLOGY",
                "FIVE_FOR_FOUR_TECHNOLOGY",
                "FOUR_FOR_THREE_TECHNOLOGY",
                "THREE_FOR_TWO_TECHNOLOGY",
                "TWO_FOR_ONE_TECHNOLOGY",
                "REINVENT_WHEEL_TECHNOLOGY",
                "WEAPON_EXPORT_TECHNOLOGY",
                "SPIKE_BARRICADE_TECHNOLOGY",
                "HANZO_SWORD_TECHNOLOGY",
                "BASIC_TRAINING_TECHNOLOGY"
            ];

            if (key == "SUNDANCE_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(10);
            } else if (key == "GREED_IS_GOOD_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(1, 10);
            } else if (key == "DOUBLE_TIME_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(30);
            } else if (key == "COORDINATED_DEFENSE_TECHNOLOGY" || key == "COORDINATED_ATTACK_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(2, 40);
            } else if (key == "RUSH_ATTACK_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(4);
            } else if (key == "RUSH_DEFENSE_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(8);
            } else if (key == "SENSE_OF_URGENCY_TECHNOLOGY") {
                summarizer = new AdditiveTechnologySummarizer(4, 100);
            } else if (key.indexOf("_ARMOR_RESEARCH_TECHNOLOGY") != -1 ||
                       key.indexOf("_WEAPON_RESEARCH_TECHNOLOGY") != -1) {
                summarizer = new AdditiveTechnologySummarizer(2, 100);
            } else if (key == "HAPPYHOUR_TECHNOLOGY") {
                summarizer = new HappyHourSummarizer();
            } else if (key.indexOf("UPKEEP_REDUCE_") == 0) {
                summarizer = new AdditiveTechnologySummarizer(10, 30);
            } else if (key.indexOf("INSTANT_") == 0) {
                if (key.indexOf("TRAINING_GROUND") != -1) {
                    summarizer = new SpecificPointsPerLevelTechnologySummarizer([15, 15, 15, 20, 25, 30, 35, 40, 45, 50],
                                                                                150);
                } else if (key.indexOf("BARRACK") != -1) {
                    summarizer = new AdditiveTechnologySummarizer(5, 150);
                } else if (key.indexOf("STABLE") != -1) {
                    summarizer = new AdditiveTechnologySummarizer(2, 60);
                } else if (key.indexOf("WORKSHOP") != -1) {
                    summarizer = new AdditiveTechnologySummarizer(1, 30);
                }
            }
            else if (nonStackingTechnologies.indexOf(key) != -1) {
                summarizer = new NoStackingTechnologySummarizer();
            }

            // Incase we miss a tech then just return null
            if (summarizer == null) {
                return null;
            }

            summarizer.setParameters(key, array);

            return summarizer;
        }
    }
}
