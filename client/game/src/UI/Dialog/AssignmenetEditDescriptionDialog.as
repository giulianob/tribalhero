/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 5/11/14
 * Time: 3:11 PM
 * To change this template use File | Settings | File Templates.
 */
package src.UI.Dialog {
import flash.events.Event;

import org.aswing.AsWingConstants;
import org.aswing.AsWingUtils;
import org.aswing.FlowLayout;
import org.aswing.JButton;
import org.aswing.JFrame;
import org.aswing.JLabel;
import org.aswing.JTextArea;
import org.aswing.SoftBoxLayout;

import src.Global;

import src.UI.GameJPanel;

public class AssignmenetEditDescriptionDialog extends GameJPanel {
    private var assignment: *;
    private var btnOk: JButton;
    private var txtDescription: JTextArea;

    public function AssignmenetEditDescriptionDialog(assignment: *, onChange: Function = null):void
    {
        title = "Edit Assignment Description";

        this.assignment = assignment;

        btnOk = new JButton();
        btnOk.addActionListener(function(e: Event = null): void {
            if (onChange != null)
                onChange(txtDescription.getText());
        });

        createUI();
    }

    public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
    {
        super.showSelf(owner, modal, onClose);
        Global.gameContainer.showFrame(frame);
        return frame;
    }

    public function createUI(): void {
        setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

        btnOk.setText("Edit Description");

        txtDescription = new JTextArea("",5,40);
        txtDescription.setWordWrap(true);
        txtDescription.setMaxChars(250);

        //component layoution
        append(AsWingUtils.createPaneToHold(new JLabel("Enter a description:"), new FlowLayout(AsWingConstants.LEFT)));
        append(AsWingUtils.createPaneToHold(txtDescription, new FlowLayout(AsWingConstants.LEFT)));
        append(AsWingUtils.createPaneToHold(btnOk, new FlowLayout(AsWingConstants.CENTER)));
    }
}
}

