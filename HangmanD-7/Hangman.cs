using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HangmanD_7
{
    public partial class Hangman : Form
    {
        enum HangState
        {
            None,
            Piller,
            Rope,
            Head,
            body,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg
        }

        // Holds currnent word characters
        List<Label> labels = new List<Label>();
        // Word under consideration
        public string currentWord { get; set; }
        // Default character for hidden word letters
        public string DefaultChar { get { return "__"; } }
        // Current hangstate, used specially to repaint panel grphics
        private HangState CurrentHangState = HangState.None;
        // HangState enum size
        public int HangStateSize { get { return (Enum.GetValues(typeof(HangState)).Length - 1); } }
        
        // Global graphics data
        Pen p;
        Pen pRope;
        int panelLocX = 0;
        int panelLocY = 0;
        int panelWidth = 0;
        int panelHeight = 0;

        // Piller endpoints (We are not storing in Point object since we need individual x, y-coordinate)
        int pillerVerBottomX;
        int pillerVerBootomY;
        int pillerVerTopX;
        int pillerVerTopY;
        int pillerHorRightX;
        int pillerHorRightY;
        int pillerHorLeftX;
        int pillerHorLeftY;

        // Rope data
        int ropeTopX;
        int ropeTopY;
        int ropeBottomX;
        int ropeBottomY;

        // Head Data
        int diameter = 40;
        int HeadBoundingRectX;

        // Body data
        int bodyBoundingRectY;
        int bodySize;

        public Hangman()
        {
            InitializeComponent();
            AddButtons();
        }

        /// <summary>
        /// Adds buttons
        /// </summary>
        private void AddButtons()
        {
            for (int i = (int)'A'; i <= (int)'Z'; i++)
            {
                Button b = new Button();
                b.Text = ((char)i).ToString();
                b.Parent = flowLayoutPanel1;
                b.Font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold);
                b.Size = new Size(40, 40);
                b.BackColor = Color.LawnGreen;                
                b.Click += b_Click; // Event hook-up
            }

            // Disabling buttons
            flowLayoutPanel1.Enabled = false;
        }

        void b_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            char charClicked = b.Text.ToCharArray()[0];
            b.Enabled = false;

            if ((currentWord = currentWord.ToUpper()).Contains(charClicked))
            {
                // char is there (right guess)
                lblInfo.Text = "Awesome!";
                lblInfo.ForeColor = Color.Green;
                char[] charArray = currentWord.ToCharArray();
                for (int i = 0; i < currentWord.Length; i++)
                {
                    if (charArray[i] == charClicked)
                        labels[i].Text = charClicked.ToString();
                }

                // Winning condition               
                if (labels.Where(x => x.Text.Equals(DefaultChar)).Any())
                    return;

                lblInfo.ForeColor = Color.Green;
                lblInfo.Text = "Hurray! You win.";
                flowLayoutPanel1.Enabled = false;
            }
            else
            {
                // Wrong guess
                lblInfo.Text = "Boo..";
                lblInfo.ForeColor = Color.Brown;
                if (CurrentHangState != HangState.RightLeg)
                    CurrentHangState++;
                txtGuessesLeft.Text = (HangStateSize - (int)CurrentHangState).ToString();
                txtWrongguesses.Text += string.IsNullOrWhiteSpace(txtWrongguesses.Text) ? charClicked.ToString() : "," + charClicked;

                panel1.Invalidate();

                if (CurrentHangState == HangState.RightLeg)
                {
                    lblInfo.Text = "You lose!";
                    lblInfo.ForeColor = Color.Red;
                    flowLayoutPanel1.Enabled = false;

                    // Reveal the word
                    for (int i = 0; i < currentWord.Length; i++)
                    {
                        if (labels[i].Text.Equals(DefaultChar))
                        {
                            labels[i].Text = currentWord[i].ToString();
                            labels[i].ForeColor = Color.Blue;
                        }
                    }
                }
            }
        }        

        private void InitializeVars()
        {
            // Global graphics data            
            p = new Pen(Color.Blue, 20);
            pRope = new Pen(Color.Blue, 5);
            panelLocX = panel1.Location.X;
            panelLocY = panel1.Location.Y;
            panelWidth = panel1.Width;
            panelHeight = panel1.Height;

            // Piller endpoints (We are not storing in Point member since we need individual x, y-coordinate)
            pillerVerBottomX = panelWidth - 30;
            pillerVerBootomY = panelHeight - 15;
            pillerVerTopX = pillerVerBottomX;
            pillerVerTopY = panelHeight - panelHeight + 30;
            pillerHorRightX = panelWidth - 30 + 10;
            pillerHorRightY = panelHeight - panelHeight + 50;
            pillerHorLeftX = panelWidth - panelWidth + 50;
            pillerHorLeftY = pillerHorRightY;

            // Rope data
            ropeTopX = (pillerHorRightX + pillerHorLeftX) / 3;
            ropeTopY = pillerHorLeftY;
            ropeBottomX = ropeTopX;
            ropeBottomY = ropeTopY + 30; // 30 is rope length

            // Head Data
            diameter = 40;
            HeadBoundingRectX = ropeBottomX - diameter / 2;

            // Body data
            bodyBoundingRectY = ropeBottomY + diameter;
            bodySize = (pillerVerBootomY - pillerVerTopY) / 2;
        }        

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Enabled)
                if (MessageBox.Show("Game in progress, wanna start again?", "Game in progress", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            ResetControls();
            SelectWord();
            AddLabels();
        }

        private void AddLabels()
        {
            // Adding word to labels and labels to group Box
            groupBox1.Controls.Clear();
            labels.Clear();
            char[] wordChars = currentWord.ToCharArray();
            int len = wordChars.Length;
            int refer = groupBox1.Width / len;

            for (int i = 0; i < len; i++)
            {
                Label l = new Label();
                l.Text = DefaultChar;
                l.Location = new Point(10 + i * refer, groupBox1.Height - 30);
                l.Parent = groupBox1;
                l.BringToFront();
                labels.Add(l);
            }

            // Writting text boxes 
            txtWordLen.Text = len.ToString();
            txtGuessesLeft.Text = HangStateSize.ToString();
        }

        private void ResetControls()
        {
            // Resetting things
            flowLayoutPanel1.Controls.Clear();
            AddButtons();
            CurrentHangState = HangState.None;
            panel1.Invalidate();
            txtWrongguesses.Clear();
            lblInfo.Text = "";
            flowLayoutPanel1.Enabled = true;
        }

        /// <summary>
        /// Randomizes a word reading text file (Words.txt) from current directory (exe location)
        /// </summary>
        private void SelectWord()
        {                      
            string filePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Words.txt");
            using (TextReader tr = new StreamReader(filePath, Encoding.ASCII))
            {
                Random r = new Random();
                var allWords = tr.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);                
                currentWord = allWords[r.Next(0, allWords.Length - 1)];
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            InitializeVars();
            var g = e.Graphics; //Graphic to draw on

            if (CurrentHangState >= HangState.Piller)
            {
                g.DrawLine(p, new Point(pillerVerBottomX, pillerVerBootomY), new Point(pillerVerTopX, pillerVerTopY));
                g.DrawLine(p, new Point(pillerHorRightX, pillerHorRightY), new Point(pillerHorLeftX, pillerHorLeftY));
            }

            if (CurrentHangState >= HangState.Rope)
            {
                g.DrawLine(pRope, new Point(ropeTopX, ropeTopY), new Point(ropeBottomX, ropeBottomY));
            }

            if (CurrentHangState >= HangState.Head)
            {
                g.DrawEllipse(pRope, new Rectangle(new Point(HeadBoundingRectX, ropeBottomY), new Size(diameter, diameter)));
                g.FillRectangles(new SolidBrush(Color.Crimson),
                    new[] {
                            new Rectangle( new Point(HeadBoundingRectX + 10, ropeBottomY + 10), new Size(6, 6)), // Left eye
                            new Rectangle( new Point(HeadBoundingRectX + diameter - 10 - 6, ropeBottomY + 10), new Size(6, 6)), // Right eye
                            new Rectangle(new Point(ropeBottomX - 5/2, ropeBottomY + diameter/2), new Size(5, 5)),  // Nose
                            new Rectangle(new Point(ropeBottomX - 10, ropeBottomY + diameter/2 + 10), new Size(20, 5))   // Mouth
                        });
            }

            if (CurrentHangState >= HangState.body)
            {
                g.DrawEllipse(pRope, new Rectangle(new Point(HeadBoundingRectX, bodyBoundingRectY), new Size(diameter, bodySize)));
            }

            if (CurrentHangState >= HangState.LeftHand)
            {
                g.DrawCurve(pRope,
                new[] { 
                            new Point(HeadBoundingRectX + 8, bodyBoundingRectY + 15), 
                            new Point(HeadBoundingRectX - 30, bodyBoundingRectY + 30),
                            new Point(HeadBoundingRectX - 30, bodyBoundingRectY + 20),
                            
                            new Point(HeadBoundingRectX + 5, bodyBoundingRectY + 25)
                        });
            }

            if (CurrentHangState >= HangState.RightHand)
            {
                g.DrawCurve(pRope,
                 new[] { 
                            new Point(HeadBoundingRectX + diameter - 8, bodyBoundingRectY + 15), 
                            new Point(HeadBoundingRectX + diameter + 30, bodyBoundingRectY + 30),
                            new Point(HeadBoundingRectX + diameter + 30, bodyBoundingRectY + 20),
                            
                            new Point(HeadBoundingRectX + diameter - 5, bodyBoundingRectY + 25)
                        });
            }

            if (CurrentHangState >= HangState.LeftLeg)
            {
                g.DrawCurve(pRope,
                new[] { 
                            new Point(HeadBoundingRectX + 8, bodyBoundingRectY + bodySize - 15), 
                            new Point(HeadBoundingRectX - 30, bodyBoundingRectY + bodySize - 5),
                            new Point(HeadBoundingRectX - 30, bodyBoundingRectY + bodySize),
                            new Point(HeadBoundingRectX + 5, bodyBoundingRectY + bodySize - 25)
                        });
            }

            if (CurrentHangState >= HangState.RightLeg)
            {
                g.DrawCurve(pRope,
                  new[] { 
                            new Point(HeadBoundingRectX + diameter - 8, bodyBoundingRectY + bodySize - 15), 
                            new Point(HeadBoundingRectX + diameter + 30, bodyBoundingRectY + bodySize - 5),
                            new Point(HeadBoundingRectX + diameter + 30, bodyBoundingRectY + bodySize),
                            new Point(HeadBoundingRectX + diameter - 5, bodyBoundingRectY + bodySize - 25)
                        });
            }
        }       
    }
}
