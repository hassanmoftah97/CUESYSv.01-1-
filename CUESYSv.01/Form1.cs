using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Word = Microsoft.Office.Interop.Word;
using System.Data;

namespace CUESYSv._01
{
    public partial class Form1 : Form
    {
        ///// NOTES START //////////////////////////////////////////////////////////
        // Should include log items stored on database
        // Bookings only in single hour "slots", would be better to custom set
        // Cannot search for booking (by room, date or customer)
        // Only view and edit upcoming x days
        // User+Pass check, insecure - later versions should use a database lookup
        // formatting odd on maximize/resize/different screen resolutions
        // menu is shown when software ran, this allows modification of customer entries before login - not secure
        // devlog out of sync with actions
        // autoscroll devlogs
        // need to hide menu bar on start (good for debug though)
        ///// NOTES END ////////////////////////////////////////////////////////////


        ///// VARIABLES START //////////////////////////////////////////////////////
        dbConn mysqlConn = new dbConn();
        private string varFloor;
        private string varRoom;
        ///// VARIABLES END ////////////////////////////////////////////////////////

        ///// METHODS START ////////////////////////////////////////////////////////
        public void devLogs(string logItem)
        {//Write development log to DevLog
            using (StreamWriter devlog = new StreamWriter("DevLog.txt", append: true))
            { devlog.WriteLine(DateTime.Now + " --- " + logItem); }//Concat current time and logItem and write to DevLog file
        }
        public bool dbConfig()
        {
            try
            {
                mysqlConn.varConfigServer = "ac8453.cucstudents.org";
                mysqlConn.varConfigDatabase = "ac8453_CUEsys";
                mysqlConn.varConfigUser = "ac8453_CUEDadmin";
                mysqlConn.varConfigPass = "Password123!";
                return true;
            }
            catch { return false; }
        }

        public void resetControls(string newFocus)
        {//Hide all controls and only show those needed
            devLogs("resetControls triggered");
            foreach (Control control in this.Controls) { control.Visible = false; }//Hide all controls
            lbCueSys.Visible = true;//Show logo
            panClock.Visible = true;//Show clock panel
            mainMenu.Visible = true;//Show menu
            foreach (var clockLbl in panClock.Controls.OfType<Label>()){ clockLbl.Visible = true; };//Show clock in panel
            switch (newFocus)//Use control statement to selectively show controls based on newFocus argument
            {
                case "Program started":
                    lbUserName.Visible = lbUserPass.Visible = tbUserName.Visible = tbUserPass.Visible = btLogin.Visible = true;//make login controls visible
                    devLogs("Login controls visible");
                    break;
                case "landing":
                    dgRoomBookingsSummary.Visible = true;
                    dbReturn("SELECT * FROM `tblBookings` WHERE `bookingDateTime` >= CURDATE()");
                    break;
                case "Book Room":
                    panFloorLayout.Visible = true;
                    cbBuilding.Visible = true;
                    cbFloor.Visible = true;
                    foreach (var x in panFloorLayout.Controls.OfType<Button>())
                    {//Make each button transparent
                        x.Parent = panFloorLayout;
                        x.Visible = true;
                        x.BackColor = Color.Transparent;
                        x.FlatAppearance.MouseDownBackColor = Color.Transparent;
                        x.FlatAppearance.MouseOverBackColor = Color.Transparent;
                        x.FlatStyle = FlatStyle.Flat;
                        x.ForeColor = BackColor;
                        x.UseVisualStyleBackColor = true;
                        x.FlatAppearance.BorderSize = 0;
                    };
                    break;
                case "create customer":
                    lbCustAdd1.Visible = true;
                    lbCustAdd2.Visible = true;
                    lbCustContact.Visible = true;
                    lbCustEmail.Visible = true;
                    lbCustPostcode.Visible = true;
                    lbCustTel.Visible = true;
                    lbCustTitle.Visible = true;
                    lbCustTownCity.Visible = true;
                    tbCustAdd1.Visible = true;
                    tbCustAdd2.Visible = true;
                    tbCustContact.Visible = true;
                    tbCustEmail.Visible = true;
                    tbCustPostcode.Visible = true;
                    tbCustTel.Visible = true;
                    tbCustTownCity.Visible = true;
                    btCustSave.Visible = true;
                    lbCustTitle.Text = "Create Customer";
                    break;
                case "view customers":
                    //show all customers
                    dgRoomBookingsSummary.Visible = true;
                    dbReturn("SELECT * FROM `tblCustomer`");
                    break;
                case "Exit":
                    Application.Exit();
                    break;
                default:
                    devLogs("resetControls default case triggered, no controls visible");
                    break;
            }
            devLogs("Focus changed to " + newFocus);
        }
        public void dbReturn(string returnWhat)
        {
            devLogs(returnWhat + " sql run");
            if (mysqlConn.connOpen() == true)
            {
                dgRoomBookingsSummary.DataSource = mysqlConn.qry(returnWhat).Tables[0];
            }
        }
        public void createBooking(string room)
        {
            resetControls("");
            switch (cbFloor.Text)
            {
                case "Ground":
                    varFloor = "G";
                    break;
                case "First":
                    varFloor = "1";
                    break;
                case "Second":
                    varFloor = "2";
                    break;
                case "Third":
                    varFloor = "3";
                    break;
                case "Fourth":
                    varFloor = "4";
                    break;
                default:
                    break;
            }
            lbBookingInfo.Visible = true;
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            tbcont.Visible = true;
            tbCustomer.Visible = true;
            tbnation.Visible = true;
            mcDate.Visible = true;
            tbTime.Visible = true;
            tbCost2.Visible = true;
            invoicebtn.Visible = true;
            cbPaid.Visible = true;
            btBook.Visible = true;
            mcDate.MaxSelectionCount = 1;
            lbBookingInfo.Text = cbBuilding.Text + " - " + varFloor + room;
            varRoom = room;
        }
        ///// METHODS END //////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            File.WriteAllText("DevLog.txt", String.Empty);//Clear contents of DevLog
            lbCueSys.Font = new Font("Comic Sans MS", 40, FontStyle.Bold);
            this.ActiveControl = tbUserName;
            dbConfig();
            mysqlConn.connect();
            resetControls("Program started");
            devLogs("Program started");
        }

        ///// EVENTS START /////////////////////////////////////////////////////////
        private void timeClock_Tick(object sender, EventArgs e)
        {//Timer to control clock
            lbClockTime.Text = DateTime.Now.ToString("HH:mm");
            lbClockSeconds.Text = DateTime.Now.ToString("ss");
            lbClockDate.Text = DateTime.Now.ToString("ddd")+"  "+DateTime.Now.ToString("dd/MM/yyyy");
        }


        private void btLogin_Click(object sender, EventArgs e)
        {
            devLogs("Login button clicked");
            //User+Pass check, not secure and only allows one login
            if (tbUserName.Text == "admin" && tbUserPass.Text == "admin")
            { resetControls("landing"); devLogs("Login success for user " + tbUserName.Text); }//Login success
            else
            { MessageBox.Show("Sorry, wrong password/user combo!"); devLogs("Login failure for user " + tbUserName.Text); }//Login failure
            tbUserName.Text = ""; tbUserPass.Text = ""; //Clear logon credentials
        }
        private void tbUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {//Change focus to password box on enter key
                this.ActiveControl = tbUserPass;
                devLogs("enter key detected in tbUserName");
            }
        }
        private void tbUserPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {//Trigger login button on enter key
                btLogin_Click(this, new EventArgs());
                devLogs("enter key detected in tbUserPass");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {//Generic keyboard shortcuts
            if (keyData == (Keys.Alt | Keys.L))
            {
                devLogs("alt-l shortcut intercepted");
                resetControls("Program started");
                return true;
            }
            if (keyData == (Keys.Alt | Keys.X))
            {
                devLogs("alt-x shortcut intercepted");
                resetControls("Exit");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void viewDevLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form devForm = new Form();
            devForm.Text = "DevLogs";
            RichTextBox rtbDevLogs = new RichTextBox();
            Timer timerRefreshDevLogs = new Timer();
            timerRefreshDevLogs.Interval = 2500;
            timerRefreshDevLogs.Tick += new EventHandler(devRefreshTimer_Tick);
            timerRefreshDevLogs.Start();
            rtbDevLogs.Location = new Point(0, 0);
            rtbDevLogs.Size = new Size(300, 380);
            rtbDevLogs.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            devForm.Size = new Size(300, 400);
            devForm.Controls.Add(rtbDevLogs);
            devLogs("devlogs viewed");
            void devRefreshTimer_Tick(object timer, EventArgs args)
            {
                rtbDevLogs.Text = "";
                string line;
                try
                {
                    StreamReader sr = new StreamReader("DevLog.txt");
                    line = sr.ReadLine();
                    while (line != null)
                    {
                        rtbDevLogs.Text += line + "\r\n";
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
                catch (Exception) { devLogs("error reading devlogs"); }
            }
            devForm.Show();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("Program started");devLogs("user logged out");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("Exit"); devLogs("application exit request");
        }

        private void bookRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("Book Room");devLogs("book room request");
        }

        private void btRoomA_Click(object sender, EventArgs e)
        {
            createBooking("1");
            tbCost2.Text = "70"; // This sets a fixed price for every room A
            devLogs("room a clicked");
        }

        private void btRoomB_Click(object sender, EventArgs e)
        {
            createBooking("2");
            tbCost2.Text = "90";
            devLogs("room b clicked");
        }

        private void btRoomC_Click(object sender, EventArgs e)
        {
            createBooking("3");
            tbCost2.Text = "75";
            devLogs("room c clicked");
        }

        private void btRoomD_Click(object sender, EventArgs e)
        {
            createBooking("4");
            tbCost2.Text = "75";
            devLogs("room d clicked");
        }

        private void btRoomE_Click(object sender, EventArgs e)
        {
            createBooking("5");
            tbCost2.Text = "75";
            devLogs("room e clicked");
        }

        private void btRoomF_Click(object sender, EventArgs e)
        {
            createBooking("6");
            tbCost2.Text = "70";
            devLogs("room f clicked");
        }

        private void btRoomG_Click(object sender, EventArgs e)
        {
            createBooking("7");
            tbCost2.Text = "30";
            devLogs("room g clicked");
        }

        private void btRoomH_Click(object sender, EventArgs e)
        {
            createBooking("8");
            tbCost2.Text = "60";
            devLogs("room h clicked");
        }

        private void createCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("create customer"); devLogs("create customer request");
        }
        
        private void viewBookingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("landing"); devLogs("show bookings");
        }

        private void viewCustomersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetControls("view customers"); devLogs("show customers");
        }

        private void btCustSave_Click(object sender, EventArgs e)
        {
            devLogs("insert new customer");
            if (mysqlConn.connOpen() == true)
            {
                mysqlConn.insertCustomer(tbCustContact.Text, tbCustEmail.Text, tbCustTel.Text, tbCustAdd1.Text, tbCustAdd2.Text, tbCustTownCity.Text, tbCustPostcode.Text);
            }
            tbCustContact.Text = tbCustEmail.Text = tbCustTel.Text = tbCustAdd1.Text = tbCustAdd2.Text = tbCustTownCity.Text = tbCustPostcode.Text = "";
            resetControls("view customers");
        }

        private void dgRoomBBookingsSummary_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgRoomBookingsSummary.Columns[0].Name == "bookingID") {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this booking?", "Delete booking", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    mysqlConn.deleteBooking(Convert.ToString(dgRoomBookingsSummary.CurrentRow.Cells[0].Value));
                }
                resetControls("landing");
            }
            if (dgRoomBookingsSummary.Columns[0].Name == "custID")
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this customer?", "Delete customer", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    mysqlConn.deleteCustomer(Convert.ToString(dgRoomBookingsSummary.CurrentRow.Cells[0].Value));
                }
                resetControls("view customers");
            }
        }

        private void btBook_Click(object sender, EventArgs e)
        {
            string varDateTime = mcDate.SelectionRange.Start.ToString("yyyy-MM-dd") + " " + tbTime.Text + ":00"; ;
            string varPaid;
            if (cbPaid.Checked == true) { varPaid = "Y"; }
            else { varPaid = "N"; }
            if (mysqlConn.connOpen() == true)
            {
                mysqlConn.insertBooking(tbCustomer.Text, cbBuilding.Text, varFloor, varRoom, varDateTime, tbCost2.Text, varPaid, tbcont.Text, tbnation.Text);
            }
            resetControls("landing");
        }

        private void panClock_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tbCost2_Click(object sender, EventArgs e)
        {

        }

        private void panFloorLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        //The code below is used for the Print Invoice button
        //Find and Replace Method
        private void FindAndReplace(Word.Application wordApp, object ToFindText, object replaceWithText)
        {
            object matchCase = true;
            object matchWholeWord = true;
            object matchWildCards = false;
            object matchSoundLike = false;
            object nmatchAllforms = false;
            object forward = true;
            object format = false;
            object matchKashida = false;
            object matchDiactitics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = 2;
            object wrap = 1;

            wordApp.Selection.Find.Execute(ref ToFindText,
                ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundLike,
                ref nmatchAllforms, ref forward,
                ref wrap, ref format, ref replaceWithText,
                ref replace, ref matchKashida,
                ref matchDiactitics, ref matchAlefHamza,
                ref matchControl);
        }

        //Create the Invoice Method
        private void CreateWordDocument(object filename, object SaveAs)
        {
            Word.Application wordApp = new Word.Application();
            object missing = Missing.Value;
            Word.Document myWordDoc = null;

            if (File.Exists((string)filename))
            {
                object readOnly = false;
                object isVisible = false;
                wordApp.Visible = false;

                myWordDoc = wordApp.Documents.Open(ref filename, ref missing, ref readOnly,
                                        ref missing, ref missing, ref missing,
                                        ref missing, ref missing, ref missing,
                                        ref missing, ref missing, ref missing,
                                        ref missing, ref missing, ref missing, ref missing);
                myWordDoc.Activate();

                //find and replace for invoice file
                this.FindAndReplace(wordApp, "<name>", tbCustomer.Text);
                this.FindAndReplace(wordApp, "<Room name>", lbBookingInfo.Text);
                this.FindAndReplace(wordApp, "<Price>", tbCost2.Text);
            }
            else
            {
                MessageBox.Show("File not Found!");
            }

            //Save as for invoice file
            myWordDoc.SaveAs2(ref SaveAs, ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing,
                            ref missing, ref missing, ref missing);

            myWordDoc.Close();
            wordApp.Quit();
            MessageBox.Show("File Created! Saved to Desktop.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateWordDocument(@"C:\Users\H_2\Desktop\CUESYSv.01 (1)\CUESYSv.01\Resources\Invoice.docx", @"C:\Users\H_2\Desktop\Invoice.docx"); // This line of code retrieves the template file from the 1st pathway and then uses the 2nd pathway to place the new file
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }


        private void mcDate_DateChanged(object sender, DateRangeEventArgs e) //This method is used to ensure that users cannot select a date that has already been booked by another guest
        {

           
            // Change the SQL to use where room and floor 
            var sql = $"SELECT bookingDateTime FROM tblBookings WHERE(bookingDateTime >= CURDATE()) AND(bookingFloor = {varFloor}) AND (bookingRoom = {varRoom}) AND (bookingBuilding = '{cbBuilding.SelectedItem}')";
            var dt = mysqlConn.qry(sql).Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                // Find the selected date in the Calendar (Datecompare C#)
                // Check if that date matches any of the dates in the row?
                // If there is a match then that date is taken.. tell the user

               
               //int result = DateTime.Compare(mcDate.SelectionRange.Start, (DateTime) row[0]);

                //Less than zero ---------- t1 is earlier than t2.
                //Zero---------- t1 is the same as t2.
                //Greater than zero ----------t1 is later than t2.

                if (mcDate.SelectionRange.Start.Date == ((DateTime)row[0]).Date)
                {
                    MessageBox.Show("This room is already booked on this date. Please select an alternative date.");
                    mcDate.SelectionStart = e.Start.AddDays(1);
                    break;
                }
            }
        }

        private void lbCustContact_Click(object sender, EventArgs e)
        {

        }


        ///// EVENTS END ///////////////////////////////////////////////////////////
    }
}
