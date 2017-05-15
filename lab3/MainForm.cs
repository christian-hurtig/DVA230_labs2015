using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace lab3 {

    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private void emptyLog() {
            tbxLog.Text = "";
        }

        private void log(string text) {
            tbxLog.Text = tbxLog.Text + text + Environment.NewLine;
        }

        private void openFile(string fileName) {
            tbxCode.Text = File.ReadAllText(fileName);
            emptyLog();
        }

        private void compile() {
            emptyLog();
            if (tbxCode.Text == "") { 
                log("Nothing to compile!");
                return;
            }

            // Create tokens (lab 1)
            LexicalAnalyzer c;
            try {
                c = new LexicalAnalyzer(tbxCode.Text);
            } catch (Exception e) {
                log("ANALYZER: " + e.Message);
                return;
            }

            // Parse and create abstract syntax tree (lab 1)
            Parser p;
            treeStructure.Nodes.Clear();

            try {
                p = new Parser(c);
            } catch (Exception e) {
                log("PARSER: " + e.Message);
                return;
            }

            treeStructure.Nodes.Add(p.createTree());

            // Check correct types (lab 3)
            TypeChecker t;

            try {
                t = new TypeChecker(p);
            } catch (Exception e) {
                log("TYPECHECK: " + e.Message);
                return;
            }

            // Generate code (lab 3)
            CodeGenerator g;

            try {
                g = new CodeGenerator(t);
            } catch (Exception e) {
                log("GENERATOR: " + e.Message);
                return;
            }

            log(g.ToString());

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            DialogResult result = ofdOpenFile.ShowDialog();

            if (result == DialogResult.OK)
                openFile(ofdOpenFile.FileName);
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e) {
            compile();
        }
    }
}
