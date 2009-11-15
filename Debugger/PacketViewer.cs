using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

public class PacketViewer : System.Windows.Forms.Form {
    public System.Windows.Forms.TreeView packetTree;
    private System.Windows.Forms.ContextMenu treeMenu;
    private System.Windows.Forms.MenuItem menuItem1;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    private MenuItem menuItem2;

    public string hexdump = "";

    public PacketViewer() {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (components != null) {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        this.packetTree = new System.Windows.Forms.TreeView();
        this.treeMenu = new System.Windows.Forms.ContextMenu();
        this.menuItem1 = new System.Windows.Forms.MenuItem();
        this.menuItem2 = new System.Windows.Forms.MenuItem();
        this.SuspendLayout();
        // 
        // packetTree
        // 
        this.packetTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.packetTree.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.packetTree.Location = new System.Drawing.Point(0, 0);
        this.packetTree.Name = "packetTree";
        this.packetTree.Size = new System.Drawing.Size(576, 360);
        this.packetTree.TabIndex = 1;
        this.packetTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.packetTree_AfterSelect);
        this.packetTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.packetTree_MouseDown);
        // 
        // treeMenu
        // 
        this.treeMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
        // 
        // menuItem1
        // 
        this.menuItem1.Index = 0;
        this.menuItem1.Text = "Copy";
        this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
        // 
        // menuItem2
        // 
        this.menuItem2.Index = 1;
        this.menuItem2.Text = "Copy Without Newlines";
        this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
        // 
        // PacketViewer
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(576, 358);
        this.Controls.Add(this.packetTree);
        this.Name = "PacketViewer";
        this.Text = "PacketViewer";
        this.Load += new System.EventHandler(this.PacketViewer_Load);
        this.ResumeLayout(false);

    }
    #endregion

    private void packetTree_DoubleClick(object sender, System.EventArgs e) {

    }

    private void CopyToClipboard(bool newLines) {
        string str = packetTree.SelectedNode.Text;

        if ((string)packetTree.SelectedNode.Tag == "pnode" && packetTree.SelectedNode.Nodes.Count > 0) {
            str = "";
            TreeNode hexNode = packetTree.SelectedNode;
            for (int i = 0; i < hexNode.Nodes.Count; i++) {
                str += hexNode.Nodes[i].Text + (newLines ? System.Environment.NewLine : "");
            }
        }
        Clipboard.SetDataObject(str, true);
    }

    private void menuItem1_Click(object sender, System.EventArgs e) {
        CopyToClipboard(true);
    }

    private void packetTree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            packetTree.SelectedNode = packetTree.GetNodeAt(e.X, e.Y);

            if (packetTree.SelectedNode != null) {
                treeMenu.Show(packetTree, new System.Drawing.Point(e.X, e.Y));
            }
        }
    }

    private void PacketViewer_Load(object sender, EventArgs e) {

    }

    private void packetTree_AfterSelect(object sender, TreeViewEventArgs e) {

    }

    private void menuItem2_Click(object sender, EventArgs e) {
        CopyToClipboard(false);
    }
}