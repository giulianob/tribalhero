/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 11/30/13
 * Time: 12:03 AM
 * To change this template use File | Settings | File Templates.
 */
package src.Objects.Technologies {
import System.Linq.Enumerable;

import src.Objects.TechnologyStats;

import src.Util.StringHelper;

public class TechnologySummarizer implements  ITechnologySummarizer{
    protected var name: String;
    protected var array: Array;

    public function getName():* {
        if(array.length>1)
            return StringHelper.localize(name+"_NAME") + " (" + array.length + ")";
        return StringHelper.localize(name+"_NAME");
    }

    public function getSummary():* {
     //   return StringHelper.localize(name+"");
        var max: * = Enumerable.from(array).max(function(tech: TechnologyStats):int {
            return tech.techPrototype.level;
        })
        return StringHelper.localize(name+"_LVL_"+max);
    }

    public function setParameters(name:String, technlogies:Array):void {
        this.name = name;
        this.array = technlogies;
    }
}
}
