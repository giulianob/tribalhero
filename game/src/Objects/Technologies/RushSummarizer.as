package src.Objects.Technologies {
    import System.Linq.Enumerable;

    import src.Objects.TechnologyStats;
    import src.Util.StringHelper;

    public class RushSummarizer extends TechnologySummarizer {
        private var _pointsPerLevel: int;

        public function RushSummarizer(pointsPerLevel: int) {
            _pointsPerLevel = pointsPerLevel;
        }

        override public function getSummary():* {
            var bonus: int = Enumerable.from(array).sum(function(tech:TechnologyStats): int {
                return tech.techPrototype.level * _pointsPerLevel;
            });

            return StringHelper.localize(name+"_SUMMARY",bonus);
        }

    }
}