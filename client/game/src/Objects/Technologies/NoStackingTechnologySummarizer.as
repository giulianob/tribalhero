package src.Objects.Technologies {
    import System.Linq.Enumerable;

    import src.Objects.TechnologyStats;

    import src.Util.StringHelper;

    public class NoStackingTechnologySummarizer implements ITechnologySummarizer{
        protected var name: String;
        protected var technologies: Array;

        public function getName():* {
            return StringHelper.localize(name+"_NAME");
        }

        public function getSummary():* {
            // If a summary exists then the tech shouldnt be using this summarizer class.
            // As a safety, we just return a generic message instead of showing the wrong value to the user.
            if (StringHelper.localize(name + "_SUMMARY") != "") {
                return StringHelper.localize("NO_TECH_SUMMARY");
            }

            var max: * = Enumerable.from(technologies).max(function(tech: TechnologyStats):int {
                return tech.techPrototype.level;
            });

            return StringHelper.localize(name+"_LVL_"+max);
        }

        public function setParameters(name:String, technlogies:Array):void {
            this.name = name;
            this.technologies = technlogies;
        }
    }
}
