using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace BT2ver2
{
    public partial class Form1 : Form
    {
        private XmlDocument doc;
        private TreeNode currentNode;
        private string filePath;
        public Form1()
        {
            InitializeComponent();
        }

        //Get path
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text= o.FileName;
                filePath = o.FileName;
            }
        }
        //Load XML file to TreeView
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doc=new XmlDocument();
            try {
                doc.Load(filePath);
                RefreshTreeView();
            } catch (Exception ex)
            {
                MessageBox.Show($"Error loading XML:{ex.Message}");
            }
        }
        //Clear TreeView and Upload latest Data
        private void RefreshTreeView()
        {
            treeView1.Nodes.Clear();// Cho cay rong
            if(doc.DocumentElement != null )// Xml node != null
            {
                treeView1.Nodes.Add(CreateTreeNode(doc.DocumentElement));
            }
        }
        private TreeNode CreateTreeNode(XmlNode xmlNode)// Root
        {
            TreeNode treeNode = new TreeNode(xmlNode.Name);
            foreach(XmlAttribute attr in xmlNode.Attributes) // Childs
            {
                treeNode.Nodes.Add($"{attr.Name}:{attr.Value}");
            }

            foreach(XmlNode node in xmlNode.ChildNodes)
            {
                if(node.NodeType == XmlNodeType.Element)
                {
                    
                    treeNode.Nodes.Add(CreateTreeNode(node));//treeNode.Text = $"{node.ParentNode.Name}:{node.Value}"; // vd price:13
                } else if (node.NodeType == XmlNodeType.Text) // leaf node
                {
                    treeNode.Text=$"{node.ParentNode.Name}:{node.Value}";
                }
            }
            return treeNode;
        }
        private TextBox dynamicTextBox = null;
        private TextBox dynamicTextBox1 = null;
        private string nodeName = ""; // Temporary storage for the node name

        private void addNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Initialize the first dynamic text box for the node name
            dynamicTextBox = new TextBox
            {
                Location = new Point(226, 41),
                Size = new Size(235, 20),
                Text = ""
            };

            label2.Text = "Enter node name";
            dynamicTextBox.KeyPress += DynamicTextBox_KeyPress;
            this.Controls.Add(dynamicTextBox);
            dynamicTextBox.Focus();
        }

        private void DynamicTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && sender == dynamicTextBox)
            {
                nodeName = dynamicTextBox.Text.Trim(); // Capture the node name
                dynamicTextBox.Visible = false; // Optionally hide or remove the first text box

                // Initialize the second text box for the node value
                dynamicTextBox1 = new TextBox
                {
                    Location = new Point(226, 41), // Slightly below the first text box
                    Size = new Size(235, 20),
                    Text = "",
                    Visible=true
                };

                dynamicTextBox1.KeyPress += DynamicTextBox1_KeyPress;
                this.Controls.Add(dynamicTextBox1);
                dynamicTextBox1.Focus();

                label2.Text = "Enter node value"; // Update the label for the next input
            }
        }
        private void DynamicTextBoxv2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string newValue = dynamicTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newValue))
                {
                    currentNode.Text = newValue;
                   // UpdateXmlNodeFromTreeNode(selectedNode, newValue);
                    dynamicTextBox.Visible = false;
                }
            }
        }
        private void DynamicTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && sender == dynamicTextBox1)
            {
                string nodeValue = dynamicTextBox1.Text.Trim(); // Capture the node value

                // Create a new TreeNode with the name and value
                TreeNode newNode = new TreeNode($"{nodeName}:{nodeValue}");

                // Add the new node to the TreeView
                if (treeView1.SelectedNode != null)
                {
                    treeView1.SelectedNode.Nodes.Add(newNode);
                    treeView1.SelectedNode = newNode;
                }
                else
                {
                    treeView1.Nodes.Add(newNode);
                    treeView1.SelectedNode = newNode;
                }

                dynamicTextBox1.Visible = false; // Optionally hide or remove the second text box
            }
        }
      /*  private void UpdateXmlNodeFromTreeNode(TreeNode treeNode, string newName, string newValue)
        {
            // Split the TreeNode.Text to extract the name part
            string nodeName = treeNode.Text.Split(new[] { ':' }, 2)[0].Trim();

            XmlNode xmlNode = FindXmlNodeByName(nodeName, doc.DocumentElement); // Use the name to find the XML node
            if (xmlNode != null)
            {
                // Update the XML node's inner text with the new value
                xmlNode.InnerText = newValue;
            }
        }

        private XmlNode FindXmlNodeByName(string nodeName, XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name == nodeName)
                {
                    return node;
                }

                XmlNode foundNode = FindXmlNodeByName(nodeName, node); // Recursively search for the node
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }*/
        
        private void deleteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent != null)
                {
                    treeView1.SelectedNode.Parent.Nodes.Remove(treeView1.SelectedNode);
                }
                else
                {
                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                }
            }
            else
            {
                MessageBox.Show("No node selected to delete.");
            }
        }

        private void findNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dynamicTextBox = new TextBox
            {
                Location = new Point(226, 41),
                Size = new Size(235, 20),
                Text = currentNode.Text
            };
            this.Controls.Add(dynamicTextBox);
            dynamicTextBox.Focus();

            dynamicTextBox.KeyPress += (s, keyPressEventArgs) =>
            {
                if (keyPressEventArgs.KeyChar == (char)Keys.Enter)
                {
                    // Use the text from the TextBox for searching
                    string partialNameToFind = dynamicTextBox.Text.Trim();
                    TreeNode foundNode = FindNodeByPartialName(treeView1.Nodes, partialNameToFind);

                    if (foundNode != null)
                    {
                        treeView1.SelectedNode = foundNode;
                        treeView1.Focus();
                    }
                    else
                    {
                        MessageBox.Show("No node with the specified partial name found.");
                    }

                    dynamicTextBox.Clear();
                    dynamicTextBox.Visible = false;
                    keyPressEventArgs.Handled = true;
                }
            };
        }
        private TreeNode FindNodeByPartialName(TreeNodeCollection nodes, string partialName)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return node;
                }

                TreeNode foundNode = FindNodeByPartialName(node.Nodes, partialName);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode != null)
            {
                dynamicTextBox = new TextBox
                {
                    Location = new Point(226, 41),
                    Size = new Size(235, 20),
                    Text = currentNode.Text,
                    Visible = true // Ensure the text box is visible
                };
                this.Controls.Add(dynamicTextBox); // Make sure to add it to the form's controls
                dynamicTextBox.Focus(); // Focus on the text box

                dynamicTextBox.KeyPress += DynamicTextBoxEdit_KeyPress;
            }
            else
            {
                MessageBox.Show("Please select a node to edit.");
            }
        }

        private void DynamicTextBoxEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                currentNode.Text = dynamicTextBox.Text.Trim();
                dynamicTextBox.Visible = false; // Hide after editing
                this.Controls.Remove(dynamicTextBox); // Optionally, remove the control
                dynamicTextBox.Dispose(); // Clean up the control
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (doc != null)
            {
                try
                {
                    ExportToXML(treeView1, filePath);
                    MessageBox.Show("Data saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving XML: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No data loaded to save.");
            }
        }
        public void ExportToXML(TreeView tree, string filename)
        {
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteStartDocument();
                // Assume the first node is a single root for the XML document.
                if (tree.Nodes.Count > 0)
                {
                    SaveNodeToXML(tree.Nodes[0], xmlTextWriter); // Start with the root node
                }
                xmlTextWriter.WriteEndDocument();
            }
        }

        private void SaveNodeToXML(TreeNode node, XmlTextWriter writer)
        {
            // Split the node's text into name and value, if applicable.
            string[] parts = node.Text.Split(':');
            if (parts.Length > 1)
            {
                string nodeName = parts[0].Trim();
                string nodeValue = parts[1].Trim();

                writer.WriteStartElement(nodeName);
                writer.WriteString(nodeValue);
            }
            else
            {
                // If no colon is present, use the whole text as the element name.
                writer.WriteStartElement(node.Text);
            }

            // Recursively save child nodes
            foreach (TreeNode childNode in node.Nodes)
            {
                SaveNodeToXML(childNode, writer);
            }

            writer.WriteEndElement(); // Close the current node's element
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNode = e.Node;
        }

        
    }
}
