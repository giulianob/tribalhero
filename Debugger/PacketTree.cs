using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Game.Comm;
namespace Debugger {
    class PacketTree {
        Packet packet;
        public PacketTree(Packet packet) {
            this.packet = packet;
        }
        public TreeNode[] getTreeNodes() {
            packet.reset();
			TreeNode[] parent = new TreeNode[2];

			TreeNode header = new TreeNode("Header");
			header.Tag = "pnode";
			header.Nodes.Add(new TreeNode("Command: " + packet.Cmd));

			//options can have children
			TreeNode optionsNode = new TreeNode("Options: " + packet.Option);
            if ((packet.Option & (ushort)Packet.Options.COMPRESSED) == (int)Packet.Options.COMPRESSED)
				optionsNode.Nodes.Add(new TreeNode("Compressed"));
            if ((packet.Option & (ushort)Packet.Options.REPLY) == (int)Packet.Options.REPLY)
				optionsNode.Nodes.Add(new TreeNode("Reply"));
            if ((packet.Option & (ushort)Packet.Options.FAILED) == (int)Packet.Options.FAILED)
				optionsNode.Nodes.Add(new TreeNode("Failed"));

			header.Nodes.Add(optionsNode);

			header.Nodes.Add(new TreeNode("Seq: " + packet.Seq));
			header.Nodes.Add(new TreeNode("Length: " + packet.Length));

            //parameters are listed under a Parameter node
	/*		TreeNode asciiParamsNode = new TreeNode("ASCII Parameters");
			IEnumerator ite = parameters.GetEnumerator();
			int i = 0;
			const int strMaxSize = 100;
			while (ite.MoveNext()) {
				DwParameter param = (DwParameter)ite.Current;

				TreeNode paramNode = new TreeNode("Param[" + i + "] [" + DwDataTypeHelper.ToString(param.Type)+"] ["+param.Length.ToString()+" bytes]");
				paramNode.Tag = "pnode"; //lets packetviewer class know this is a parent and children should be copied to clipboard

				string[] asciidump = param.ToString().Split('\n');
				for (int j = 0; j < asciidump.Length; j++) {
					int partsCount = (int)Math.Ceiling((double)asciidump[j].Length / strMaxSize);

					for (int x = 0; x < partsCount; x++) {
						int end = Math.Min(strMaxSize, asciidump[j].Length - x * strMaxSize);
						string str = asciidump[j].Substring(x * strMaxSize, end);
						TreeNode h = new TreeNode(str);
						h.Tag = "cnode";
						paramNode.Nodes.Add(h);
					}
				}
				asciiParamsNode.Nodes.Add(paramNode);
				i++;
			}
*/
			TreeNode hexParamsNode = new TreeNode("HEX Parameters");
            string[] hexdump = Game.Util.HexDump.GetString(packet.getBytes(packet.Length), 0).Split(System.Environment.NewLine.ToCharArray());
            foreach (string hd in hexdump)
            {
                if (hd.Length == 0) continue;
                hexParamsNode.Nodes.Add(new TreeNode(hd));
            }


            //hexParamsNode.Nodes.Add(Game.Util.HexDump.GetString(packet.getBytes(packet.Length), 0));
	/*		ite = parameters.GetEnumerator();
			i = 0;
			while (ite.MoveNext()) {
				DwParameter param = (DwParameter)ite.Current;

				TreeNode paramNode = new TreeNode("Param[" + i + "] [" + DwDataTypeHelper.ToString(param.Type) + "] [" + param.Length.ToString() + " bytes]");
				paramNode.Tag = "pnode"; //lets packetviewer class know this is a parent and children should be copied to clipboard

				string[] hexdump = param.ToHex().Split(System.Environment.NewLine.ToCharArray());
				foreach(string hd in hexdump) {
					if(hd.Length==0) continue;
					TreeNode h = new TreeNode(hd);
					h.Tag = "cnode";
					paramNode.Nodes.Add(h);
				}

				hexParamsNode.Nodes.Add(paramNode);
				i++;
			}*/

			header.Expand();
			parent[0] = header;
		//	parent[1] = asciiParamsNode;
			parent[1] = hexParamsNode;

            return parent;
        }
    }
}
