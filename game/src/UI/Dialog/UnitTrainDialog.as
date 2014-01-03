package src.UI.Dialog {

import System.Collection.Generic.IEnumerable;
import System.Linq.Enumerable;

import flash.events.*;

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.ext.*;
import org.aswing.geom.*;

import src.*;
import src.Map.*;
import src.Objects.*;
import src.Objects.Effects.*;
import src.Objects.Factories.ObjectFactory;
import src.Objects.Prototypes.*;
import src.Objects.Troop.Formation;
import src.Objects.Troop.TroopStub;
import src.Objects.Troop.Unit;
import src.UI.*;
import src.UI.Components.*;
import src.Util.*;

public class UnitTrainDialog extends GameJPanel {

		private var structure:StructureObject;
		private var city: City;
		private var txtTitle:JLabel;
		private var panel4:JPanel;
		private var sldAmount:JAdjuster;
		private var pnlUpkeepMsg: JPanel;
		private var panel8:JPanel;
		private var pnlResources: JPanel;
		private var lblTime: JLabel;
		private var lblUpkeep: JLabel;
		private var btnOk:JButton;
		private var unitPrototype: UnitPrototype;
        private var lblUpkeepMsg: MultilineLabel;
        private var pnlTime: JPanel;

		public function UnitTrainDialog(structure: StructureObject, unitPrototype: UnitPrototype, onAccept: Function):void {
			this.unitPrototype = unitPrototype;
			this.structure = structure;
			this.city = Global.map.cities.get(structure.cityId);		

			createUI();

			title = "Train " + unitPrototype.getName();

			txtTitle.setText("How many units would you like to train?");

			sldAmount.setMinimum(1);
			sldAmount.setValues(1, 0, 1, city.resources.Div(Formula.unitTrainCost(city, unitPrototype)));

			sldAmount.addStateListener(updateResources);
			sldAmount.addStateListener(updateTime);

			var self: UnitTrainDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			updateResources();
			updateTime();
		}

        private function getInstantTimeCount() : int {
            var effectForStructureType:Array =
                    Enumerable.from(city.techManager.getEffects(EffectPrototype.EFFECT_UNIT_TRAIN_INSTANT_TIME, EffectPrototype.INHERIT_ALL)).where(function(effect:EffectPrototype): Boolean{
                        return (int)(effect.param1) == structure.type;
                    }).toArray();

            if (effectForStructureType.length==0)
                return 0;

            var units:IEnumerable = Enumerable.from(city.troops.getMyStubs()).selectMany(function(stub: TroopStub):Array {
                return Enumerable.from(stub).selectMany(function(formation:Formation):Array{
                    return formation.toArray();
                }).toArray();
            })

            var current:int = units.sum(function(unit: Unit):int {
                return ObjectFactory.isType(effectForStructureType[0].param2, unit.type) ? unit.count : 0;
            });

            var threshold: int = Math.min(effectForStructureType.sum(function(effect:EffectPrototype) :int {
                return effect.param3;
            }),effectForStructureType[0].param4);

            if (current >= threshold)
                return 0;

            return Math.max(threshold - current, 0);
        }

		private function updateTime(e: Event = null) : void {
            var instantTimeCount: int = getInstantTimeCount();
            var count: int = sldAmount.getValue();
			var trainTime: int = Formula.trainTime(structure.level, count-instantTimeCount, unitPrototype);
            pnlTime.removeAll();
            if(instantTimeCount==0){ // no instant trained
                lblTime.setText(DateUtil.formatTime(trainTime));
                pnlTime.append(lblTime);
            } else if(instantTimeCount>=count) {  // all instant trained
                var instantMessageAll: JLabel = new JLabel("All "+ Math.min(instantTimeCount,count) + " unit(s) will be trained immediately.");
                instantMessageAll.setHorizontalAlignment(AsWingConstants.LEFT);
                pnlTime.append(instantMessageAll);
            } else { // mix
                var instantMessage: JLabel = new JLabel("First "+ Math.min(instantTimeCount,count) + " unit(s) will be trained immediately.");
                instantMessage.setHorizontalAlignment(AsWingConstants.LEFT);
                pnlTime.append(instantMessage);

                var instantMessage2: JLabel = new JLabel("Next " + Math.max(count-instantTimeCount,0) + " unit(s) will be trained in:");
                instantMessage2.setHorizontalAlignment(AsWingConstants.LEFT);
                pnlTime.append(instantMessage2);

                lblTime.setText(DateUtil.formatTime(trainTime));
                pnlTime.append(lblTime);
            }
		}

		private function updateResources(e: Event = null) : void {			
			var totalUpkeep: Number = Formula.getUpkeepWithReductions(unitPrototype.upkeep * sldAmount.getValue(),  unitPrototype.type, city);
			lblUpkeep.setText(StringHelper.localize("STR_PER_HOUR", -(Math.ceil(totalUpkeep / Constants.secondsPerUnit))));

			var cityResources: LazyResources = city.resources;
			pnlUpkeepMsg.setVisible((cityResources.crop.getUpkeep() + Math.ceil(totalUpkeep)) > cityResources.crop.getRate());

			var cost: Resources = Formula.unitTrainCost(city, unitPrototype);

			pnlResources.removeAll();
			pnlResources.append(new SimpleResourcesPanel(cost.multiplyByUnit(sldAmount.getValue())));

			if (getFrame() != null) {
				getFrame().pack();
			}
		}

		public function getAmount(): JAdjuster
		{
			return sldAmount;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI(): void
		{
			setPreferredWidth(350);
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5);
			setLayout(layout0);

			txtTitle = new JLabel();
			txtTitle.setMinimumWidth(220);
			txtTitle.setHorizontalAlignment(AsWingConstants.LEFT);

			panel4 = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			panel4.setLayout(layout1);

			sldAmount = new JAdjuster();
			sldAmount.setColumns(5);

			panel8 = new JPanel();
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.CENTER);
			panel8.setLayout(layout2);

			btnOk = new JButton();
			btnOk.setSize(new IntDimension(22, 22));
			btnOk.setText("Ok");

			var pnlCost: JPanel = new JPanel(new BorderLayout(5, 5));

			pnlResources = new JPanel();
			pnlResources.setConstraints("North");

 			lblTime = new JLabel("", new AssetIcon(new ICON_CLOCK()));
			lblTime.setConstraints("West");
			new SimpleTooltip(lblTime, "Time to train units");
            lblTime.setHorizontalAlignment(AsWingConstants.LEFT);

            pnlTime = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

			lblUpkeep = new JLabel("", new AssetIcon(new ICON_CROP()));
			lblUpkeep.setConstraints("East");
			new SimpleTooltip(lblUpkeep, "Upkeep");

			pnlUpkeepMsg = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 3));
			pnlUpkeepMsg.setBorder(new LineBorder(null, new ASColor(0xff0000), 1, 10));
			lblUpkeepMsg = new MultilineLabel("The upkeep required to train this many units may exceed your city's crop production rate. Your units will starve and die out if there is not enough crop.", 0, 28);
			pnlUpkeepMsg.setVisible(false);

			//component layoution
			pnlUpkeepMsg.append(new AssetPane(new ICON_ALERT()));
			pnlUpkeepMsg.append(lblUpkeepMsg);

			panel4.append(sldAmount);

			panel8.append(btnOk);

			pnlCost.append(pnlResources);
			pnlCost.append(pnlTime);
			pnlCost.append(lblUpkeep);

			append(txtTitle);
			append(panel4);
			append(pnlCost);
			append(pnlUpkeepMsg);
			append(panel8);
		}
	}

}

