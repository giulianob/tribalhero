/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 11/29/13
 * Time: 11:52 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Objects.Technologies {
import src.UI.Tooltips.Tooltip;

public interface ITechnologySummarizer {
        function setParameters(name:String, technlogies: Array): void;
        function getName() : *;
        function getSummary() : *;
    }
}
