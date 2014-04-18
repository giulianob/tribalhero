package src.Objects.Technologies {
    import System.Linq.Enumerable;

    import src.Objects.TechnologyStats;
    import src.Util.StringHelper;

    public class SpecificPointsPerLevelTechnologySummarizer extends NoStackingTechnologySummarizer {
        private var _pointsPerLevel: Array;
        private var _cap: int;

        public function SpecificPointsPerLevelTechnologySummarizer(pointsPerLevel: Array, cap: int = 0) {
            _pointsPerLevel = pointsPerLevel;
            _cap = cap;
        }

        override public function getSummary():* {
            var bonus: int = Enumerable.from(technologies).sum(function(tech:TechnologyStats): int {
                if (tech.techPrototype.level == 0) {
                    return 0;
                }

                return _pointsPerLevel[tech.techPrototype.level - 1];
            });

            if (_cap > 0) {
                bonus = Math.min(bonus, _cap);
            }

            return StringHelper.localize(name+"_SUMMARY", bonus);
        }

    }
}