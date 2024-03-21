using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace BT2
{
    public partial class Form1 : Form
    {
        private XmlDocument doc;
        private TreeNode selectedNode;

        public Form1()
        {
            InitializeComponent();
        }
        private string filePath;
        private void button0_Click(object sender, EventArgs e) // Browse_Click
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog()==DialogResult.OK)
            {
                textBox2.Text= o.FileName;
                filePath= o.FileName;
            }
        }
        private void button1_Click(object sender, EventArgs e) // Load_click
        {
            doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
                RefreshTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading XML: {ex.Message}");
            }
        }

        private void RefreshTreeView()
        {
            treeView1.Nodes.Clear();
            if (doc.DocumentElement != null)
            {
                treeView1.Nodes.Add(CreateTreeNode(doc.DocumentElement));
            }
        }

        private TreeNode CreateTreeNode(XmlNode xmlNode)
        {
            TreeNode treeNode = new TreeNode(xmlNode.Name);
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                treeNode.Nodes.Add($"{attribute.Name}: {attribute.Value}");
            }

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    treeNode.Nodes.Add(CreateTreeNode(node));
                }
                else if (node.NodeType == XmlNodeType.Text)
                {
                    treeNode.Text = node.Value;
                }
            }
            return treeNode;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = e.Node;
        }

        private TextBox dynamicTextBox; 

        private void button4_Click(object sender, EventArgs e) // Edit_click
        {
            if (selectedNode != null)
            {
                dynamicTextBox = new TextBox
                {
                    Location = new Point(325, 130),
                    Size = new Size(193, 20),
                    Text = selectedNode.Text
                };
                dynamicTextBox.KeyPress += TextBox_KeyPress; 
                dynamicTextBox.Focus();

                this.Controls.Add(dynamicTextBox);
            }
            else
            {
                MessageBox.Show("Please select a node to edit.");
            }
        }
        private void button6_Click(object sender, EventArgs e) //Find_click
        {
            dynamicTextBox = new TextBox
            {
                Location = new Point(325, 130),
                Size = new Size(193, 20),
                Visible = true
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
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) 
            {
                string newValue = dynamicTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newValue))
                {
                    selectedNode.Text = newValue;
                    UpdateXmlNodeFromTreeNode(selectedNode, newValue);
                    dynamicTextBox.Visible = false;
                }
            }
        }

        private void UpdateXmlNodeFromTreeNode(TreeNode treeNode, string newValue)
        {
            XmlNode xmlNode = FindXmlNode(treeNode, doc.DocumentElement);
            if (xmlNode != null)
            {
                xmlNode.InnerText = newValue;
            }
        }

        private XmlNode FindXmlNode(TreeNode treeNode, XmlNode parent) 
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name == treeNode.Text)
                {
                    return node;
                }
                XmlNode foundNode = FindXmlNode(treeNode, node);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        public void ExportToXML(TreeView tree, string filename)
        {
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented; 
                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement(tree.Nodes[0].Text);
                foreach (TreeNode node in tree.Nodes)
                {
                    SaveToXML(node.Nodes, xmlTextWriter);
                }
                xmlTextWriter.WriteEndElement();
                xmlTextWriter.Close();
            }
        }


        private void SaveToXML(TreeNodeCollection nodes, XmlTextWriter writer)
        {
            foreach (TreeNode node in nodes)
            {
                writer.WriteStartElement("Node");
                writer.WriteString(node.Text);
                if (node.Nodes.Count > 0)
                {
                    SaveToXML(node.Nodes, writer);
                }
                writer.WriteEndElement();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dynamicTextBox = new TextBox
            {
                Location = new Point(325, 130),
                Size = new Size(193, 20),
                Text = ""
            };
            dynamicTextBox.KeyPress += TextBox_KeyPress;
            dynamicTextBox.Focus();
            this.Controls.Add(dynamicTextBox);

            TreeNode newNode = new TreeNode(dynamicTextBox.Text);
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
        }

        private void button3_Click(object sender, EventArgs e)
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

        private void button5_Click(object sender, EventArgs e)
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
    }
}