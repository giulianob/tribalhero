package src.Objects.Technologies {
    import System.Linq.Enumerable;

    import src.Objects.TechnologyStats;
    import src.Util.StringHelper;

    public class AdditiveTechnologySummarizer extends NoStackingTechnologySummarizer {
        private var _pointsPerLevel: int;
        private var _cap: int;

        public function AdditiveTechnologySummarizer(pointsPerLevel: int, cap: int = 0) {
            _pointsPerLevel = pointsPerLevel;
            _cap = cap;
        }

        override public function getSummary():* {
            var bonus: int = Enumerable.from(technologies).sum(function(tech:TechnologyStats): int {
                return tech.techPrototype.level * _pointsPerLevel;
            });

            if (_cap > 0) {
                bonus = Math.min(bonus, _cap);
            }

            return StringHelper.localize(name+"_SUMMARY", bonus);
        }

    }
}