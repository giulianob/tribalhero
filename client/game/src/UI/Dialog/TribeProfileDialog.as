package src.UI.Dialog
{
    import System.Collection.Generic.IGrouping;
    import System.Linq.Enumerable;
    import System.Linq.Option.Option;

    import com.adobe.serialization.json.JSONDecoder;

    import fl.lang.*;

    import flash.events.*;
    import flash.utils.*;

    import mx.utils.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.event.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;
    import org.aswing.table.*;

    import src.*;
    import src.Comm.GameURLLoader;
    import src.Objects.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Process.*;
    import src.Objects.Stronghold.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.BattleReport.*;
    import src.UI.Components.TableCells.*;
    import src.UI.Components.Tribe.*;
    import src.UI.LookAndFeel.*;
    import src.UI.Tooltips.*;
    import src.Util.*;

    public class TribeProfileDialog extends GameJPanel
    {
        private var profileData: * ;

        private var assignmentInfoDialog: AssignmentInfoDialog;

        private var pnlHeader: JPanel;
        private var pnlInfoContainer: JPanel;

        private var messageBoard: MessageBoard;

        private var pnlTabs: JTabbedPane;
        private var pnlInfoTabs: JTabbedPane;

        private var pnlStrongholds:JPanel;
        private var remoteReports:RemoteReportList;
        private var localReports:LocalReportList;
        private var strongholdTab:JPanel;
        private var messageBoardTab:JPanel;
        private var pnlOngoingAttacks:JPanel;

        private var logTab: JPanel;
        private var logLoader: GameURLLoader;
        private var txtArea : JPanel;
        private var tribeLogPagingBar: PagingBar;
        private var tribeLogTab: Container;

        public function TribeProfileDialog(profileData: *)
        {
            this.profileData = profileData;

            logLoader = new GameURLLoader();
            logLoader.addEventListener(Event.COMPLETE, onReceiveLogs);
            createUI();

            pnlTabs.addStateListener(function (e: InteractiveEvent): void {
                if (pnlTabs.getSelectedComponent() == strongholdTab) {
                    localReports.loadInitially();
                    remoteReports.loadInitially();
                }
                else if (pnlTabs.getSelectedComponent() == messageBoardTab) {
                    messageBoard.loadInitially();
                }
            });
        }

        private function dispose():void {
        }

        public function update(): void {
            Global.mapComm.Tribe.viewTribeProfile(profileData.tribeId, function(newProfileData: *): void {
                if (!newProfileData) {
                    return;
                }

                profileData = newProfileData;
                createInfoTab();
                createStrongholdList();
                createOngoingAttacksList();

                // Reopen info dialog
                if (assignmentInfoDialog != null && assignmentInfoDialog.getFrame()) {
                    assignmentInfoDialog.getFrame().dispose();

                    var hasAssignment: Option = Enumerable.from(profileData.assignments).firstOrNone(function(assignment: *) {
                        return assignmentInfoDialog.assignment.id == assignment.id;
                    });

                    if (hasAssignment.isSome) {
                        showAssignmentInfo(hasAssignment.value);
                    }
                }
            });
        }

        public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
        {
            super.showSelf(owner, modal, onClose, dispose);
            Global.gameContainer.closeAllFramesByType(TribeProfileDialog);
            Global.gameContainer.showFrame(frame);
            return frame;
        }

        private function createUI():void {
            setPreferredSize(new IntDimension(Math.min(1025, Constants.screenW - GameJImagePanelBackground.getFrameWidth()) , Math.max(375, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));
            title = "Tribe Profile - " + profileData.tribeName;
            setLayout(new BorderLayout(10, 10));

            // Header panel
            pnlHeader = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            pnlHeader.setConstraints("North");

            // Tab panel
            pnlTabs = new JTabbedPane();
            pnlTabs.setPreferredSize(new IntDimension(375, 350));
            pnlTabs.setConstraints("Center");

            // Append tabs
            pnlTabs.appendTab(createInfoTab(), "Info");

            messageBoardTab = createMessageBoardTab();
            pnlTabs.appendTab(messageBoardTab, "Message Board");

            strongholdTab = createStrongholdTab();
            pnlTabs.appendTab(strongholdTab, StringHelper.localize("STR_STRONGHOLDS"));

            // Append main panels
            appendAll(pnlHeader, pnlTabs);

        }

        private function createMessageBoardTab(): JPanel {
            messageBoard = new MessageBoard();
            return messageBoard;
        }

        private function simpleLabelMaker(text: String, tooltip:String, icon:Icon = null):JLabel
        {
            var label:JLabel = new JLabel(text, icon);

            label.setIconTextGap(0);
            label.setHorizontalTextPosition(AsWingConstants.RIGHT);
            label.setHorizontalAlignment(AsWingConstants.LEFT);

            new SimpleTooltip(label, tooltip);

            return label;
        }

        private function createOngoingAttackItem(stronghold : * ): JPanel {
            var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,0));
            var pnlTop: JPanel = new JPanel(new BorderLayout(5, 0));
            var pnlBottom: JPanel = new JPanel(new BorderLayout(5, 0));

            var lblName : StrongholdLabel = new StrongholdLabel(stronghold.id, false, stronghold.name);
            lblName.setHorizontalAlignment(AsWingConstants.LEFT);
            lblName.setVerticalAlignment(AsWingConstants.TOP);

            var pnlNameStatus: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));
            pnlNameStatus.setPreferredHeight(25);
            pnlNameStatus.append(lblName);

            var grid: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
            grid.append(simpleLabelMaker(StringHelper.localize("STR_LEVEL_VALUE", stronghold.lvl), StringHelper.localize("STR_LEVEL"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_UPGRADE"))));

            if (stronghold.tribeId == 0) {
                grid.append(simpleLabelMaker(StringHelper.localize("STR_NEUTRAL"), StringHelper.localize("STR_NEUTRAL"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD"))));
            }
            else {
                var tribeLabel:TribeLabel = new TribeLabel(stronghold.tribeId, stronghold.tribeName);
                tribeLabel.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD")));
                grid.append(tribeLabel);
            }

            pnlTop.setPreferredHeight(25);
            pnlTop.append(pnlNameStatus, "Center");

            pnlBottom.append(grid, "Center");

            pnl.setPreferredWidth(400);
            pnl.appendAll(pnlTop, pnlBottom);

            return pnl;
        }

        private function createStrongholdItem(stronghold : * ): JPanel {
            var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,0));
            var pnlTop: JPanel = new JPanel(new BorderLayout(5, 0));
            var pnlBottom: JPanel = new JPanel(new BorderLayout(5, 0));

            var lblName : StrongholdLabel = new StrongholdLabel(stronghold.id, stronghold.name);
            lblName.setHorizontalAlignment(AsWingConstants.LEFT);
            lblName.setVerticalAlignment(AsWingConstants.TOP);

            var pnlNameStatus: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));
            pnlNameStatus.setPreferredHeight(25);
            pnlNameStatus.append(lblName);
            pnlNameStatus.append(Stronghold.getBattleStateString(stronghold, 2, 30));

            var grid: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
            grid.append(simpleLabelMaker(StringHelper.localize("STR_LEVEL_VALUE", stronghold.lvl), StringHelper.localize("STR_LEVEL"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_UPGRADE"))));
            grid.append(simpleLabelMaker(StringHelper.localize("STR_PER_HOUR_RATE", Util.roundNumber(stronghold.victoryPointRate)), StringHelper.localize("STR_VP_RATE"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR"))));
            var timediff :int = Global.map.getServerTime() - stronghold.dateOccupied;
            grid.append(simpleLabelMaker(DateUtil.niceDays(timediff), StringHelper.localize("STR_DAYS_OCCUPIED"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD"))));

            var lblTroop: JLabel = new JLabel(StringHelper.localize("STR_UPKEEP_COUNT", stronghold.upkeep));
            lblTroop.setHorizontalAlignment(AsWingConstants.RIGHT);

            var pnlGate: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 0, AsWingConstants.RIGHT));
            pnlGate.setPreferredHeight(25);
            var lblGate: JLabel = new JLabel(StringHelper.localize("STR_GATE"), null, AsWingConstants.LEFT);
            lblGate.setVerticalAlignment(AsWingConstants.TOP);
            pnlGate.append(lblGate);

            if (stronghold.battleState == Stronghold.BATTLE_STATE_NONE && stronghold.gate < stronghold.gateMax && Constants.session.tribe.hasRight(Tribe.REPAIR)) {
                var btnGateRepair: JLabelButton = new JLabelButton(Stronghold.gateToString(stronghold.gateMax, stronghold.gate), null, AsWingConstants.LEFT);
                btnGateRepair.useHandCursor = true;
                btnGateRepair.addEventListener(MouseEvent.CLICK, function(e: Event): void {
                    Global.mapComm.Stronghold.repairStrongholdGate(stronghold.id, function(): void {
                        update();
                    });
                });
                var tooltip: SimpleTooltip = new SimpleTooltip(btnGateRepair, StringHelper.localize("STRONGHOLD_REPAIR_GATE_ACTION"));
                tooltip.append(new ResourcesPanel(Formula.getGateRepairCost(stronghold.gateMax, stronghold.gate), profileData.resources, true, false));
                btnGateRepair.setVerticalAlignment(AsWingConstants.TOP);
                pnlGate.append(btnGateRepair);
            }
            else {
                var lblGateHealth: JLabel = new JLabel(Stronghold.gateToString(stronghold.gateMax, stronghold.gate), null, AsWingConstants.LEFT);
                lblGateHealth.setVerticalAlignment(AsWingConstants.TOP);
                pnlGate.append(lblGateHealth);
            }

            pnlTop.setPreferredHeight(25);
            pnlTop.append(pnlNameStatus, "Center");
            pnlTop.append(pnlGate, "East");

            pnlBottom.append(grid, "Center");
            pnlBottom.append(lblTroop, "East");

            pnl.setPreferredWidth(400);
            pnl.appendAll(pnlTop, pnlBottom);

            return pnl;
        }

        private function createStrongholdTab(): JPanel {
            // Strongholds List
            var pnl: JPanel = createStrongholdList();
            var tabTroops: JTabbedPane = new JTabbedPane();
            tabTroops.appendTab(Util.createTopAlignedScrollPane(pnl), StringHelper.localize("STR_STRONGHOLDS_UNDER_COMMAND"));

            //  Ongoing Attack Tab
            pnl = createOngoingAttacksList();
            var tabOnGoing: JTabbedPane = new JTabbedPane();
            tabOnGoing.appendTab(Util.createTopAlignedScrollPane(pnl), StringHelper.localize("STR_ONGOING_ATTACKS"));
            tabOnGoing.setPreferredHeight(150);

            // Report Tab
            pnl = createReportsPanels();
            var tabReports: JTabbedPane = new JTabbedPane();
            tabReports.appendTab(pnl, StringHelper.localize("STR_REPORTS"));

            // Troop + Ongoing
            var pnlLeft : JPanel = new JPanel(new BorderLayout(10,10));
            tabTroops.setConstraints("Center");
            tabOnGoing.setConstraints("South");
            pnlLeft.appendAll(tabTroops, tabOnGoing);

            // Main tab
            pnl = new JPanel(new BorderLayout(10, 10));
            pnlLeft.setConstraints("West");
            pnlLeft.setPreferredWidth(Math.max(150, getPreferredWidth() / 2.5));
            tabReports.setConstraints("Center");
            pnl.appendAll(pnlLeft, tabReports);

            return pnl;
        }

        private function createOngoingAttacksList(): JPanel
        {
            if (!pnlOngoingAttacks) {
                pnlOngoingAttacks = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 15));
            }
            else {
                pnlOngoingAttacks.removeAll();
            }

            for each (var stronghold: * in profileData.openStrongholds) {
                pnlOngoingAttacks.append(createOngoingAttackItem(stronghold));
            }

            if (profileData.openStrongholds.length == 0) {
                pnlOngoingAttacks.append(new JLabel(StringHelper.localize("TRIBE_NO_STRONGHOLDS_OPEN"), null, AsWingConstants.LEFT));
            }

            return pnlOngoingAttacks;
        }

        private function createReportsPanels(): JPanel
        {
            var localReportBorder:TitledBorder = new TitledBorder(null, "Invasion Reports", 1, AsWingConstants.LEFT, 0, 10);
            localReportBorder.setBeveled(true);

            localReports = new LocalReportList(BattleReportViewer.REPORT_TRIBE_LOCAL, [BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_ATTACK_TRIBES], null);
            localReports.setBorder(localReportBorder);

            var remoteReportBorder:TitledBorder = new TitledBorder(null, "Foreign Reports", 1, AsWingConstants.LEFT, 0, 10);
            remoteReportBorder.setColor(new ASColor(0x0, 1));
            remoteReportBorder.setBeveled(true);

            remoteReports = new RemoteReportList(BattleReportViewer.REPORT_TRIBE_FOREIGN, [BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_DEFENSE_TRIBES], null);
            remoteReports.setBorder(remoteReportBorder);

            var pnl: JPanel = new JPanel(new GridLayout(2, 1, 5));
            pnl.appendAll(localReports, remoteReports);

            return pnl;
        }

        private function createStrongholdList(): JPanel
        {
            if (!pnlStrongholds) {
                pnlStrongholds = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 15));
            }
            else {
                pnlStrongholds.removeAll();
            }

            for each (var stronghold: * in profileData.strongholds) {
                pnlStrongholds.append(createStrongholdItem(stronghold));
            }

            if (profileData.strongholds.length == 0) {
                pnlStrongholds.append(new JLabel(StringHelper.localize("TRIBE_NO_STRONGHOLDS_OWNED"), null, AsWingConstants.LEFT));
            }

            return pnlStrongholds;
        }

        private function createIncomingPanelItem(incoming: *): JPanel {
            var pnlContainer: JPanel = new JPanel(new BorderLayout());
            var pnlHeader: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));

            pnlHeader.append(new RichLabel(RichLabel.getHtmlForLocation(incoming.source), 1));
            pnlHeader.setConstraints("Center");

            var lblCountdown: CountDownLabel = new CountDownLabel(incoming.endTime, StringHelper.localize("STR_BATTLE_IN_PROGRESS"));
            lblCountdown.setConstraints("East");

            pnlContainer.appendAll(pnlHeader, lblCountdown);
            return pnlContainer;
        }

        private function createAssignmentItem(assignment: *): JPanel {
            var pnlContainer: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));

            var pnlName: RichLabel = new RichLabel(StringUtil.substitute(Locale.loadString(assignment.isAttack?"ASSIGNMENT_ATK":"ASSIGNMENT_DEF"),RichLabel.getHtmlForLocation(assignment.target)));
            var pnlStats: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.RIGHT, 5, AsWingConstants.RIGHT));
            pnlStats.appendAll(
                    new JLabel(assignment.troopCount, new AssetIcon(assignment.isAttack ? SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD") : SpriteFactory.getFlashSprite("ICON_SHIELD")), AsWingConstants.RIGHT),
                    new CountDownLabel(assignment.endTime, "Troops Dispatched")
            );
            pnlStats.setConstraints("East");

            var pnlHeader: JPanel = new JPanel(new BorderLayout(10));
            pnlHeader.appendAll(pnlName, pnlStats);

            var pnlBottom: JPanel = new JPanel(new BorderLayout(5));

            var btnJoin: JLabelButton = new JLabelButton("Join", null, AsWingConstants.RIGHT);
            btnJoin.setConstraints("East");

            var btnDetails: JLabelButton = new JLabelButton("Details", null, AsWingConstants.LEFT);

            var lblDescription: JLabel = new JLabel(StringHelper.truncate(assignment.description,75), null, AsWingConstants.LEFT);
            lblDescription.setConstraints("Center");
            if (assignment.description != "")
                new SimpleTooltip(lblDescription, assignment.description);

            var pnlButtons: JPanel = new JPanel(new FlowLayout());

            if (Constants.session.tribe.hasRight(Tribe.ASSIGNMENT)) {
                var btnEdit: JLabelButton = new JLabelButton("Edit Description", null, AsWingConstants.LEFT);

                btnEdit.addActionListener(function(e:Event): void {
                    var edit: AssignmentEditDescriptionDialog = new AssignmentEditDescriptionDialog(assignment, function():void {
                        update();
                    });
                    edit.show();
                });

                pnlButtons.appendAll(btnEdit, btnDetails, btnJoin);
            } else {
                pnlButtons.appendAll(btnDetails, btnJoin);
            }
            pnlButtons.setConstraints("East");

            pnlBottom.appendAll(lblDescription, pnlButtons);

            pnlContainer.appendAll(pnlHeader, pnlBottom);

            btnDetails.addActionListener(function(e: Event): void {
                showAssignmentInfo(assignment);
            });

            btnJoin.addActionListener(function(e: Event): void {
                var join: AssignmentJoinProcess = new AssignmentJoinProcess(Global.gameContainer.selectedCity, assignment);
                join.execute().then(function(value: *): void { update(); });
            });

            return pnlContainer;
        }

        private function showAssignmentInfo(assignment: *): void {
            assignmentInfoDialog = new AssignmentInfoDialog(assignment, function (): void {
                update();
            });

            assignmentInfoDialog.show();
        }

        private function createAssignmentTab() : Container {
            var btnCreate: JButton = new JButton("Create");

            var pnlFooter: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT));
            pnlFooter.setConstraints("South");

            // Set up tab
            var pnlAssignmentHolder: JPanel = new JPanel(new BorderLayout(0, 5));
            var pnlAssignments: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

            for each (var assignment: * in profileData.assignments) {
                pnlAssignments.append(createAssignmentItem(assignment));
            }

            var scrollAssignment: JScrollPane = new JScrollPane(new JViewport(pnlAssignments, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
            (scrollAssignment.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
            scrollAssignment.setConstraints("Center");

            pnlAssignmentHolder.appendAll(scrollAssignment, pnlFooter);

            if (Constants.session.tribe.hasRight(Tribe.ASSIGNMENT)) {
                pnlFooter.append(btnCreate);
            }

            var menu : JPopupMenu = new JPopupMenu();
            menu.addMenuItem("Offensive Assignment").addActionListener(function(e: Event): void {
                var assignmentCreate: AtkAssignmentCreateProcess = new AtkAssignmentCreateProcess(Global.gameContainer.selectedCity);
                assignmentCreate.execute();
            });
            menu.addMenuItem("Defensive Assignment").addActionListener(function(e: Event): void {
                var assignmentCreate: DefAssignmentCreateProcess = new DefAssignmentCreateProcess(Global.gameContainer.selectedCity);
                assignmentCreate.execute();
            });

            btnCreate.addActionListener(function(e: Event): void {
                menu.show(btnCreate, 0, btnCreate.height);
            });

            new SimpleTooltip(btnCreate, "An assignment is an organized attack/defense used by the tribe to dispatch troops automatically so they all reach the target at the same time regardless of the distance/speed of each troop. NOTE: Troops must have at least 40 upkeep to be part of an assignment.");

            return pnlAssignmentHolder;
        }

        private function onReceiveLogs(e: Event): void {
            var data: Object;
            try
            {
                data = logLoader.getDataAsObject();
            }
            catch (e: Error) {
                InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
                return;
            }

            if (data.error != null) {
                InfoDialog.showMessageDialog("Info", data.error);
                return;
            }
            txtArea.removeAll();
            tribeLogPagingBar.setData(data);

            for each(var log:* in data.tribelogs) {
                var panel: JPanel = new JPanel(new SoftBoxLayout());
                var params: * = new JSONDecoder(log.parameters).getValue();
                var date:JLabel = new JLabel(DateUtil.niceShort(log.created));
                date.setVerticalAlignment(AsWingConstants.TOP);
                panel.append(date);

                var icon:AssetIcon;
                switch((int)(log.type)) {
                    case 1: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_UPGRADE")); break;
                    case 2: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD")); break;
                    case 3: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_LABOR")); break;
                    case 4: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR")); break;
                    case 5: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_UNFRIEND")); break;
                    case 6: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_UNFRIEND")); break;
                    case 7: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD")); break;
                    case 8: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD")); break;
                    case 9: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD")); break;
                    default: icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR")); break;
                }
                var iconLabel: JLabel = new JLabel("",icon);
                iconLabel.setVerticalAlignment(AsWingConstants.TOP);
                panel.append(iconLabel);
                panel.append(new RichLabel(StringHelper.localize("TRIBE_LOG_"+log.type,params),0,40));
                txtArea.append(panel);
            }
        }

        private function createLogTab() : Container {
            if(!logTab) {
                logTab = new JPanel(new BorderLayout());
                txtArea = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
                txtArea.setConstraints("Center");
                txtArea.setBackground(ASColor.BLUE);

                tribeLogPagingBar = new PagingBar(function(page: int = 0): void {
                    Global.mapComm.Tribe.logListing(logLoader, page);
                });
                tribeLogPagingBar.setConstraints("South");

                logTab.appendAll(Util.createTopAlignedScrollPane(txtArea), tribeLogPagingBar);
            }

            return logTab;
        }

        private function createMembersTab() : Container {
            var modelMembers: VectorListModel = new VectorListModel(profileData.members);
            var tableMembers: JTable = new JTable(new PropertyTableModel(
                    modelMembers,
                    ["Player", "Rank", "Last Seen", ""],
                    [".", "rank", "date", "."],
                    [null, new TribeRankTranslator(Constants.session.tribe.ranks), null]
            ));
            tableMembers.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
                tableMembers.getCellEditor().cancelCellEditing();
            });
            tableMembers.setRowSelectionAllowed(false);
            tableMembers.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
            tableMembers.getColumnAt(0).setPreferredWidth(135);
            tableMembers.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(PlayerLabelCell));
            tableMembers.getColumnAt(1).setPreferredWidth(100);
            tableMembers.getColumnAt(2).setPreferredWidth(100);
            tableMembers.getColumnAt(3).setCellFactory(new GeneralTableCellFactory(TribeMemberActionCell));
            tableMembers.getColumnAt(3).setPreferredWidth(70);

            var scrollMembers: JScrollPane = new JScrollPane(tableMembers, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);

            return scrollMembers;
        }

        private function createIncomingSection(grouping: IGrouping):* {
            var pnlHeader: JPanel = new JPanel(new BorderLayout());
            var pnlCounter: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS,5));
            var pnlGroup: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

            //  Detail panel
            for each(var item:* in grouping) {
                pnlGroup.append(createIncomingPanelItem(item));
            }
            pnlGroup.setVisible(false);

            //  Section Panel
            var lblTarget: RichLabel = new RichLabel(RichLabel.getHtmlForLocation(grouping.first().target), 1);
            lblTarget.setConstraints("Center");

            var lblAttackers: JLabel = new JLabel(grouping.count().toString(), new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD")));

            var lblCountdown: CountDownLabel = new CountDownLabel(grouping.first().endTime, StringHelper.localize("STR_BATTLE_IN_PROGRESS"));
            lblCountdown.setConstraints("East");

            var btnExpand: JLabel = new JLabel("", new AssetIcon(SpriteFactory.getFlashSprite("ICON_EXPAND")), AsWingConstants.LEFT);
            btnExpand.useHandCursor = true;
            btnExpand.buttonMode = true;
            btnExpand.addEventListener(MouseEvent.CLICK, function (e: Event): void {
                if (pnlGroup.isVisible()) {
                    pnlGroup.setVisible(false);
                    btnExpand.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_EXPAND")));
                    pnlCounter.setVisible(true);
                }
                else {
                    pnlGroup.setVisible(true);
                    btnExpand.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_COLLAPSE")));
                    pnlCounter.setVisible(false);
                }
            });
            btnExpand.setConstraints("West");

            pnlCounter.appendAll(lblAttackers, lblCountdown);
            pnlCounter.setConstraints("East");
            pnlHeader.appendAll(btnExpand, lblTarget, pnlCounter);

            return { label: pnlHeader, panel: pnlGroup };
        }

        private function createIncomingAttackTab(): Container {
            var pnlIncomingAttacks: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

            profileData.incomingAttacks.sortOn("endTime", Array.NUMERIC);

            for each(var grouping: IGrouping in Enumerable.from(profileData.incomingAttacks).groupBy(function(incoming:*):String {
                if (incoming.target.hasOwnProperty("cityId")) {
                    return "City::" + incoming.target.cityId;
                }
                else if (incoming.target.hasOwnProperty("strongholdId")) {
                    return "Stronghold::" + incoming.target.strongholdId;
                }

                throw new Error("Unknown target");
            } ).orderBy(function(grouping:IGrouping):int {
                        return grouping.min(function(item:*):int {
                            return item.endTime;
                        });
                    })) {
                var group:* = createIncomingSection(grouping);

                pnlIncomingAttacks.appendAll(group.label, group.panel);
            }

            var scrollIncomingAttacks: JScrollPane = new JScrollPane(new JViewport(pnlIncomingAttacks, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
            (scrollIncomingAttacks.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);

            return scrollIncomingAttacks;
        }

        private function createInfoTab(): Container {
            // Clear out container if it already exists instead of recreating it
            if (!pnlInfoContainer)
                pnlInfoContainer = new JPanel(new BorderLayout(10, 10));
            else
                pnlInfoContainer.removeAll();

            var btnUpgrade: JLabelButton = new JLabelButton("Upgrade");
            var btnDonate: JLabelButton = new JLabelButton("Contribute");
            var btnUpdateRank: JLabelButton = new JLabelButton("Ranks");
            var btnInvite: JLabelButton = new JLabelButton("Invite");
            var btnSetDescription: JLabelButton = new JLabelButton("Set Announcement");
            var btnDismantle: JLabelButton = new JLabelButton("Dismantle");
            var btnLeave: JLabelButton = new JLabelButton("Leave");
            var btnTransfer: JLabelButton = new JLabelButton("Transfer Tribe");

            var pnlActions: JPanel = new JPanel(new FlowWrapLayout(200, AsWingConstants.LEFT, 10, 0, false));
            pnlActions.setConstraints("North");
            // Show correct buttons depending on rank
            if (Constants.session.tribe.hasRight(Tribe.ALL))  /// Tribe Chief
            {
                pnlActions.appendAll(btnSetDescription, btnInvite, btnUpgrade, btnDonate, btnDismantle, btnTransfer, btnUpdateRank);
            }
            else
            {
                if (Constants.session.tribe.hasRight(Tribe.ANNOUNCEMENT)) pnlActions.appendAll(btnSetDescription);
                if (Constants.session.tribe.hasRight(Tribe.INVITE)) pnlActions.appendAll(btnInvite);
                pnlActions.appendAll(btnUpgrade, btnDonate, btnLeave);
            }

            // upgrade btn (Always show it and disable if person doesnt have right so they can see resources amt)
            var upgradeTooltip: TribeUpgradeTooltip = new TribeUpgradeTooltip(profileData.tribeLevel, profileData.resources);
            upgradeTooltip.bind(btnUpgrade);
            btnUpgrade.addActionListener(function(e: Event): void {
                Global.mapComm.Tribe.upgrade();
            });
            btnUpgrade.setEnabled(Constants.session.tribe.hasRight(Tribe.UPGRADE));
            btnUpgrade.mouseEnabled = true;

            // description
            var pnlDescriptionHolder: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            var description: String = profileData.description == "" ? "The tribe chief hasn't set an announcement yet" : profileData.description;
            var lblDescription: MultilineLabel = new MultilineLabel(description);
            GameLookAndFeel.changeClass(lblDescription, "Message");
            lblDescription.setBorder(new EmptyBorder(null, new Insets(0, 0, 15)));

            pnlDescriptionHolder.append(lblDescription);
            if (profileData.publicDescription) {
                var lblPublicDescription: MultilineLabel = new MultilineLabel(profileData.publicDescription);
                GameLookAndFeel.changeClass(lblPublicDescription, "Message");

                var lblPublicTitle: JLabel = new JLabel("Public Announcement", null, AsWingConstants.LEFT);
                GameLookAndFeel.changeClass(lblPublicTitle, "darkHeader");
                pnlDescriptionHolder.appendAll(lblPublicTitle, lblPublicDescription);
            }

            var scrollDescription: JScrollPane = Util.createTopAlignedScrollPane(pnlDescriptionHolder);

            // Side tab browser
            var lastActiveInfoTab: int = 0;
            if (pnlInfoTabs)
                lastActiveInfoTab = pnlInfoTabs.getSelectedIndex();

            pnlInfoTabs = new JTabbedPane();
            pnlInfoTabs.setPreferredWidth(450);

            // Put it all together
            var pnlEast: JPanel = new JPanel(new BorderLayout(0, 5));
            pnlInfoTabs.setConstraints("Center");
            pnlEast.appendAll(pnlInfoTabs);

            pnlInfoTabs.appendTab(createMembersTab(), "Members (" + profileData.members.length + ")");
            pnlInfoTabs.appendTab(createIncomingAttackTab(), "Invasions (" + profileData.incomingAttacks.length + ")");
            pnlInfoTabs.appendTab(createAssignmentTab(), "Assignments (" + profileData.assignments.length + ")");
            
            tribeLogTab = createLogTab();
            pnlInfoTabs.appendTab(tribeLogTab, "Log");

            pnlInfoTabs.setSelectedIndex(lastActiveInfoTab);

            var pnlWest: JPanel = new JPanel(new BorderLayout(0, 5));
            scrollDescription.setConstraints("Center");

            pnlWest.appendAll(pnlActions, scrollDescription);

            pnlWest.setConstraints("Center");
            pnlEast.setConstraints("East");

            pnlInfoContainer.appendAll(pnlWest, pnlEast);

            // Button handlers
            btnSetDescription.addActionListener(function(e: Event = null): void {
                var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

                var txtDescription: JTextArea = new JTextArea(profileData.description, 10, 10);
                txtDescription.setMaxChars(3000);
                GameLookAndFeel.changeClass(txtDescription, "Message");
                var scrollDescription: JScrollPane = new JScrollPane(txtDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);

                var txtPublicDescription: JTextArea = new JTextArea(profileData.publicDescription, 10, 10);
                txtPublicDescription.setMaxChars(3000);
                GameLookAndFeel.changeClass(txtPublicDescription, "Message");
                var scrollPublicDescription: JScrollPane = new JScrollPane(txtPublicDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);

                pnl.appendAll(
                        new JLabel("Private Announcement. This will only be visible to your tribe members.", null, AsWingConstants.LEFT),
                        scrollDescription,
                        new JLabel("Public Announcement. This will be visible when others view your tribe profile.", null, AsWingConstants.LEFT),
                        scrollPublicDescription);
                InfoDialog.showMessageDialog("Say something to your tribe", pnl, function(result: * ): void {
                    if (result == JOptionPane.CANCEL || result == JOptionPane.CLOSE) {
                        return;
                    }

                    Global.mapComm.Tribe.setTribeDescription(txtDescription.getText(), txtPublicDescription.getText());
                });
            });

            btnInvite.addActionListener(function(e: Event): void {
                var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
                var txtPlayerName: JTextField = new AutoCompleteTextField(Global.mapComm.General.autoCompletePlayer);
                pnl.appendAll(new JLabel("Type in the name of the player you want to invite", null, AsWingConstants.LEFT), txtPlayerName);

                InfoDialog.showMessageDialog("Invite a new tribesman", pnl, function(response: * ) : void {
                    if (response != JOptionPane.OK) { return; }
                    if (txtPlayerName.getLength() > 0)
                        Global.mapComm.Tribe.invitePlayer(txtPlayerName.getText());
                });
            });

            btnDismantle.addActionListener(function(e: Event): void {
                InfoDialog.showInputDialog("Dismantle tribe", "If you really want to dismantle your tribe then type 'delete' below and click ok. You will not be able to create a new tribe for 24 hours.", function(input: *) : void {
                    if (input == "'delete'" || input == "delete")
                        Global.mapComm.Tribe.dismantle();
                });
            });

            btnTransfer.addActionListener(function(e: Event): void {
                InfoDialog.showInputDialog("Transfer tribe", "Tribes can only be transferred to players that are already part of the tribe. Type in the player name you wish to give this tribe to. You will be demoted to Elder and the player promoted to Owner.", function(input: * ) : void {

                    if (input == null) {
                        return;
                    }

                    InfoDialog.showMessageDialog("Transfer Tribe", StringUtil.substitute("Transfer control to {0}? You cannot regain control of the tribe unless it is transferred back to you.", input), function(result: int): void {
                        if (result == JOptionPane.YES)
                            Global.mapComm.Tribe.transfer(input);
                    }, null, true, true, JOptionPane.YES | JOptionPane.NO);
                });
            });

            btnLeave.addActionListener(function(e: Event): void {
                InfoDialog.showMessageDialog("Leave tribe", StringHelper.localize("TRIBE_LEAVE_WARNING"), function(result: *) : void {
                    if (result == JOptionPane.YES)
                        Global.mapComm.Tribe.leave();
                }, null, true, true, JOptionPane.YES | JOptionPane.NO);
            });

            btnDonate.addActionListener(function(e: Event): void {
                InfoDialog.showMessageDialog("Contribute to tribe", "Use a Trading Post to contribute resources.");
            });

            btnUpdateRank.addActionListener(function(e: Event): void {
                var dialog : TribeUpdateRankDialog = new TribeUpdateRankDialog();
                dialog.show(null, true, function():void {
                    update();
                });
            });

            // First row of header panel which contains player name + ranking
            var pnlHeaderFirstRow: JPanel = new JPanel(new BorderLayout(5));

            var lblTribeName: JLabel = new JLabel(profileData.tribeName + " (Level " + profileData.tribeLevel + ")", null, AsWingConstants.LEFT);
            GameLookAndFeel.changeClass(lblTribeName, "darkHeader");

            var lblEstablished: JLabel = new JLabel(StringHelper.localize("STR_ESTABLISHED_WITH_TIME", DateUtil.niceDays(Global.map.getServerTime() - profileData.created)));

            var pnlHeaderTitle: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));
            pnlHeaderTitle.setConstraints("Center");
            pnlHeaderTitle.appendAll(lblTribeName, lblEstablished);

            var pnlResources: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 10, 0, false));
            var lblVictoryPoint: JLabel = new JLabel(profileData.victoryPoint.toFixed(1),  new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR")));
            new SimpleTooltip(lblVictoryPoint, StringHelper.localize("STR_VICTORY_POINT"));
            lblVictoryPoint.setIconTextGap(0);

            pnlResources.setConstraints("East");
            pnlResources.append(lblVictoryPoint);
            pnlResources.append(new SimpleResourcesPanel(profileData.resources, false));
            pnlHeaderFirstRow.appendAll(pnlHeaderTitle, pnlResources);

            pnlHeader.removeAll();
            pnlHeader.append(pnlHeaderFirstRow);

            AsWingManager.callLater(function(): void {
                pnlHeader.repaintAndRevalidate();
                pnlInfoContainer.repaintAndRevalidate();
                scrollDescription.repaintAndRevalidate();
            });

            pnlInfoTabs.addStateListener(function (e: InteractiveEvent): void {
                if (pnlInfoTabs.getSelectedComponent() == tribeLogTab) {
                    tribeLogPagingBar.loadInitially();
                }
            }, 0, true);

            return pnlInfoContainer;
        }

        public function ReceiveNewMessage(): void{
            messageBoard.showRefreshButton();
        }
    }

}