using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //Create Connection string with username and password
        static string connString = "Server=mysql.aldpesiupui.dreamhosters.com;Port=3306;Database=capstone_brunner2;Uid=programaster;password=programaster123;";
        
        //preserves the color of the grid labels
        private Color labelColor;

        //Checks if the user is Logged Into the System
        private bool loginBool = false;

        //Creates the encryption key for password encryption
        //Hello World is encryption key string
        private const string ENCRYPTION_KEY = "ENmtH6kEeV1.I";
        
        //Checks if the popUpPanel should be hidden with a different control's handler
        private bool hidePanel = false;

        //Creates a SALT, KEY, keyGenerator, and IV for password encryption
        private readonly static byte[] SALT = Encoding.ASCII.GetBytes(ENCRYPTION_KEY);
        private readonly byte[] key;
        private readonly byte[] iv;
        private readonly Rfc2898DeriveBytes keyGenerator;

        //variable to hold the id of the user for permission purposes
        int IdOfUser = -1;

        //boolean list for permissions
        bool[] permissionList = new bool[3];

        //Creates a new form for the add/edit employee window
        Form2 addEmployeeForm = new Form2();
        
        //Main Form Constructor
        public Form1()
        {
            //Set up encryption seeds
            keyGenerator = new Rfc2898DeriveBytes(ENCRYPTION_KEY, SALT);
            key = keyGenerator.GetBytes(32);
            iv = keyGenerator.GetBytes(16);

            //Create the main form
            InitializeComponent();

            //After creation, hide dayLabels (tableLayoutPanel with days of the week)
            //Also, hide the popUpPanel on creation
            dayLabels.Visible = false;
            popUpPanel.Visible = false;

            //Create pieces of the form dynamically
            createEmployeeList(); //Generate the Employee List on the right side of the screen
            createScheduleGrid(); //Create the list of 
            for (int counter = 0; counter < permissionList.Length; counter++)
            {
                permissionList[counter] = false;
            }

            labelColor = Color.White; //default the original label color to white on initialization

            //Set login defaults for ease of access
            loginUserName.Text = "rbrunner";
            loginPassword.Text = "science";

        }

        //Function to set the login boolean
        private void setLoginBool(bool loginStatus)
        {
            loginBool = loginStatus;
        }

        //Function to get the login boolean
        private bool getLoginBool()
        {
            return loginBool;
        }

        //Function to set the color of the affected label: mostly used for hover function
        private void setColor(Color currColor)
        {
            labelColor = currColor;
        }

        //Function to get the stored label color: mostly used for hover function
        private Color getColor()
        {
            return labelColor;
        }

        //Function to form the Employee List on the right side of the form
        //Also runs and shows the login functionality
        //Labels are created dynamically
        private void createEmployeeList()
        {
            //sets the position of the login elements
            loginUserName.Location = new System.Drawing.Point((this.Width / 2), 50);
            loginPassword.Location = new System.Drawing.Point((this.Width / 2), 75);
            userLabel.Location = new System.Drawing.Point((this.Width / 2) - (this.Width / 10), 53);
            passLabel.Location = new System.Drawing.Point((this.Width / 2) - (this.Width / 10), 78);

            //sets the initial location that the labels should be placed
            int labelXLocation = 0;
            int labelYLocation = 0;

            //sets the dimensions for the labels
            int labelHeight = 50;
            int labelWidth = 200;

            //Store some default color selections rgb style
            int[] redcolorList = new int[10];
            int[] bluecolorList = new int[10];
            int[] greencolorList = new int[10];

            //Critical Employee Color
            redcolorList[0] = 255;
            bluecolorList[0] = 128;
            greencolorList[0] = 128;

            //Standard Employee Color
            redcolorList[1] = 128;
            bluecolorList[1] = 128;
            greencolorList[1] = 255;

            //Create a new connection string    
            MySqlConnection conn = new MySqlConnection(connString);

            //Try to open the connection to prevent errors
            try
            {
                //Open the connection
                conn.Open();

                //Create a new command to get first and last names of employee and their ID
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ID, fName, lName, jobStatus FROM Employee";

                //Execute the command above and start the command reader
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //store elements of the sql command results one by one
                    int EmployeeID = (int)reader["ID"];
                    string labelFirstName = reader["fName"].ToString();
                    string labelLastName = reader["lName"].ToString();
                    string isCritical = reader["jobStatus"].ToString();

                    //Create a custom class that inherits label
                    labelWithValue testLabel = new labelWithValue();
                    testLabel.setEmpID(EmployeeID); //The Label's employee ID is set to the ID field from sql
                    
                    //If the employee is full time, set the background of the label to red
                    if (reader["jobStatus"].ToString() == "True")
                    {
                        testLabel.BackColor = Color.FromArgb(255, 128, 128); //light red
                    }
                    else //If the employee is part time, set the background of the label to green
                    {
                        testLabel.BackColor = Color.FromArgb(128, 255, 128); //light green
                    }
                    
                    //Assign Properties to the new label
                    testLabel.Name = labelFirstName + labelLastName; //The name of testlabel gets employee first name and last name
                    testLabel.Text = labelFirstName + " " + labelLastName; //Text gets first name and last name as well
                    testLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; //Give the label a border
                    testLabel.Cursor = System.Windows.Forms.Cursors.Hand; //Pointer should show the hand on hover over the label
                    testLabel.Location = new Point(labelXLocation, labelYLocation); //give the label a location: the y increments each loop
                    labelYLocation += labelHeight; //Increase the y location by the size of the label to 'stack' the labels in the panel
                    testLabel.Size = new Size(labelWidth, labelHeight); //give the label height and width
                    employeeList.Controls.Add(testLabel); //add the new label to the employeelist panel
                    
                    //add a mousedown event on the label to give drag and drop functionality
                    testLabel.MouseDown += new MouseEventHandler(labelClick); 
                }
            }
            catch (MySqlException ex)
            {
                //Check for an exception in case the database could not be accessed and report the error
                switch (ex.Number)
                {
                    case 1042: //cannot connect
                        MessageBox.Show("Unable to Connect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    case 0: //access denied
                        MessageBox.Show("Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                //Close the connection to database
                conn.Close();
            }
        }

        //Create the schedule grid
        private void createScheduleGrid()
        {
            //initialize needed variables
            bool thirty = false; //should time end in 00 or 30?
            bool morning = true; //should time end in AM or PM?
            bool DoThis = false; //should an action be taken, used to track noon times
            string timeStamp = ""; //initialize an empty string for future use
            
            //initialize counters for future use
            int SpaceCounter = 1; //tracks which cell to add time labels
            int listCounter = 0; //tracks cell label number

            for (int counter = 0; counter < 24; counter++)
            {
                //Create a new labelWithValue and initialize properties to create time list
                labelWithValue timeLabel = new labelWithValue();
                timeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left; //set the label to sit on the left
                timeLabel.AutoSize = true; //let the label autosize
                timeLabel.TabIndex = 0;

                if (counter == 0 && !DoThis) //if counter is 0 and first time
                {
                    //the first time is 12AM
                    timeStamp = "12:00 AM";
                    DoThis = true;
                    counter--;
                }
                else if(counter == 0 && DoThis){ //if counter is 0 and the second run
                    //the second time is 12:30AM
                    timeStamp = "12:30 AM";
                    DoThis = false;
                }
                else //if it is the third time or later, then run this
                {
                    //if the counter is a multiple of 12, then the remaining times are in the PM
                    if (counter % 12 == 0)
                    {
                        morning = false;
                    }

                    //set the time to end in :30
                    if (thirty)
                    {
                        if (counter % 12 == 0)
                        {
                            timeStamp = "12:30";
                        }
                        else
                        {
                            timeStamp = counter % 12 + ":30";
                        }
                        thirty = false;
                    }
                    else //otherwise, time ends in 00
                    {
                        if (counter % 12 == 0)
                        {
                            timeStamp = "12:00";
                        }
                        else
                        {
                            timeStamp = counter % 12 + ":00";
                        }
                        counter--;
                        thirty = true;
                    }

                    //if it is the morning, time ends in AM
                    if (morning)
                    {
                        timeStamp = timeStamp + " AM";
                    }
                    else //otherwise, time is PM
                    {
                        timeStamp = timeStamp + " PM";
                    }

                }

                timeLabel.Text = timeStamp; //the timeLabel gets the string set in the previous conditionals
                scheduleGrid.Controls.Add(timeLabel, 0, SpaceCounter); //add label to the schedulegrid
                SpaceCounter++;
                timeStamp = ""; //reset the timeStamp string
            }

            for (int rowCount = 1; rowCount <= scheduleGrid.RowCount; rowCount++)
            {
                for (int colCount = 1; colCount < scheduleGrid.ColumnCount; colCount++)
                {
                    //Create new labelwithvalue and give it some properties
                    labelWithValue cellLabel = new labelWithValue();
                    cellLabel.AutoSize = false; //cell should not autosize
                    cellLabel.BackColor = Color.White; //set background to white
                    cellLabel.AllowDrop = true; //cell takes drop information
                    cellLabel.Name = "cellLabel" + listCounter; //name the cell
                    cellLabel.Anchor = System.Windows.Forms.AnchorStyles.None; //center the label in the cell layout
                    cellLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; //give the label a border
                    scheduleGrid.Controls.Add(cellLabel, colCount, rowCount); //add the label to the schedule grid
                    
                    //Create several event handelers for the new cell label
                    cellLabel.DragEnter += new DragEventHandler(scheduleGrid_DragEnter); 
                    
                    //accept drag input
                    cellLabel.DragDrop += new DragEventHandler(scheduleGrid_DragDrop);
                    cellLabel.DragLeave += new EventHandler(scheduleGrid_DragLeave);

                    //events on mouse action
                    cellLabel.MouseDown += new MouseEventHandler(labelClick); //enable drag and drop
                    cellLabel.MouseHover += new EventHandler(label_OnHover); //shows list of assigned employees
                    cellLabel.MouseEnter += new EventHandler(label_Enter_HidePanel); //hide panel if its currently shown
                    
                    listCounter++;
                }
            }

            //get the columns widths of the schedule grid
            int[] colWidths = scheduleGrid.GetColumnWidths();
            dayLabels.Width = scheduleGrid.Width-colWidths[0]-20; //set the daylabel width based on the widths of scheduleGird
            dayLabels.Location = new System.Drawing.Point(scheduleGrid.Location.X+colWidths[0] , scheduleGrid.Location.Y-10);
            dayLabels.BringToFront(); //bring the days to the front of the form

            //Hide all non-login elements
            overallPanel.Visible = false;
            scheduleGrid.Visible = false;
            employeeList.Visible = false;
            menuStrip1.Visible = false;
        }

        //Eventhandler on cellLabels
        private void label_Enter_HidePanel(object sender, EventArgs e)
        {
            //if the panel is shown, hide it when the mouse enters a cell label
            if (hidePanel)
            {
                popUpPanel.Visible = false;
                hidePanel = false;
            }
            
        }

        //Eventhandler to populate the tooltip while hovering over a cell label
        private void label_OnHover(object sender, EventArgs e)
        {
            labelWithValue control = (labelWithValue)sender;
            popUpInfo.SetToolTip(control, control.getEmployeeFullList());
        }

        //When the form size is changed, alter size aspects of controls
        private void Form1_SizeChanged(object sender, System.EventArgs e)
        {
            //hide the popuppanel when the window gets resized
            popUpPanel.Visible = false;

            Control control = (Control)sender;

            //set the new location of the employeeList, it does not get any wider
            employeeList.Location = new Point(control.Width - 250, 50);
            //Resize the employeeList, it does get taller as the form does
            employeeList.Height = control.Height - 100;
            
            //set the schedule grid to a maximum height
            if (control.Height - 100 < 960)
            {
                scheduleGrid.Height = control.Height - 100;
            }
            else
            {
                scheduleGrid.Height = 960;
            }

            //The schedule grid width is the control width minus the width of the employeelist
            scheduleGrid.Width = control.Width - 270;

            //the day label width gets the width of the schedule grid minus 95 pixels
            dayLabels.Width = scheduleGrid.Width - 95;

            //if the width of the window is large enough, expand the full names
            if (control.Width < 925)
            {
                Monday.Text = "Mon.";
                Tuesday.Text = "Tues.";
                Wednesday.Text = "Wed.";
                Thursday.Text = "Thu.";
                Friday.Text = "Fri.";
                Saturday.Text = "Sat.";
                Sunday.Text = "Sun.";
            }
            else
            {
                Monday.Text = "Monday";
                Tuesday.Text = "Tuesday";
                Wednesday.Text = "Wednesday";
                Thursday.Text = "Thursday";
                Friday.Text = "Friday";
                Saturday.Text = "Saturday";
                Sunday.Text = "Sunday";
            }

        }


        //OnMouseDown event handler for employee list labels
        //enable drag and drop
        private void labelClick(object sender, MouseEventArgs e)
        {
            
            labelWithValue dragLabel = (labelWithValue)sender;
            dragLabel.DoDragDrop(dragLabel, DragDropEffects.Move); //create move effect with the mouse
        }

        //enable drop into schedule grid
        private void scheduleGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect; //create the generated proper effect
            labelWithValue currentLabel = (labelWithValue)sender;
            setColor(currentLabel.BackColor); //set the stored label color
            currentLabel.BackColor = Color.Cyan; //on hover while dragging, turn cellLabel cyan
        }

        //code that defines drag drop functionality
        private void scheduleGrid_DragDrop(object sender, DragEventArgs e)
        {
            //initialize some locations
            int labelXLocation = 0;
            int labelYLocation = 0;
            int labelHeight = 20;
            labelWithValue currentLabel = (labelWithValue)sender;
            Point panelLocation = new Point(currentLabel.Location.X, currentLabel.Location.Y);

            //gets the labelwithvalue representation of the drag and drop function
            labelWithValue myLabel = (labelWithValue)e.Data.GetData(typeof(labelWithValue));
            popUpPanel.Controls.Clear(); //delete all child controls of the popuppanel

            //if the background color of the label is cyan and the name is not in the label already
            if (myLabel.BackColor != Color.Cyan && !currentLabel.searchList(myLabel.Text))
            {
                //if the number of names in the label is greater than 1
                if (myLabel.getNumElementsInList() > 1)
                {
                    //set the point of the popup panel to right next to the hovered label
                    if (currentLabel.Location.Y + 50 < scheduleGrid.Height - 50)
                    {

                        panelLocation.X = panelLocation.X - 85;
                        panelLocation.Y = panelLocation.Y + 50;
                    }
                    else
                    {
                        panelLocation.X = panelLocation.X - 85;
                        panelLocation.Y = panelLocation.Y - 30;
                        
                    }
                    if (panelLocation.X < 0) //if the panel would be off the screen, move to the other side of the label
                    {
                        panelLocation.X = panelLocation.X + 100 + currentLabel.Width;
                    }

                    popUpPanel.Location = panelLocation;

                    for (int counter = 0; counter < myLabel.getNumElementsInList(); counter++)
                    {
                        //Create a new label and initialize some properties
                        labelWithValue tempLabel = new labelWithValue();
                        tempLabel.Size = new System.Drawing.Size(97, labelHeight); //set label size
                        tempLabel.Text = myLabel.getEmployeeList(counter); //set the text of the label
                        tempLabel.Location = new Point(labelXLocation, labelYLocation); //set the location within the popup panel
                        tempLabel.BackColor = myLabel.getEmployeeColor(counter); //backcolor equals the original color of the employee
                        labelYLocation += labelHeight; //stack the labels in the panel
                        popUpPanel.Controls.Add(tempLabel); //add the label to the panel
                        tempLabel.MouseLeave += new EventHandler(tempLabel_MouseLeave); //when the mouse leaves the label, test to hide the popup panel
                        tempLabel.MouseDoubleClick += new MouseEventHandler((senders, es) => tempLabel_MouseDoubleClick(senders, es, currentLabel)); //fire event on double click
                    }
                    popUpPanel.Visible = true; //reveal the popuppanel
                    popUpPanel.HorizontalScroll.Enabled = false; //try and stop the horizontal scroll from showing
                    popUpPanel.BringToFront(); //bring it to the front
                }
                else //if the number of names is 0 or 1
                {
                    popUpPanel.Visible = false; //close the popupmenu
                    currentLabel.addEmployeeToList(myLabel.Text, myLabel.BackColor); //add the employee to the label
                    currentLabel.BackColor = myLabel.BackColor; //change the background color of the cell label
                    if (currentLabel.getNumElementsInList() > 1) //if the label has more than 1 employee
                    {
                        currentLabel.Text = "..."; //set text to ...
                    }
                    else
                    {
                        currentLabel.Text = currentLabel.getEmployeeList(0); //set text to recently added employee
                    }
                    
                }
            }
            if (currentLabel.BackColor == Color.Cyan)
            {
                currentLabel.BackColor = getColor(); //return the background color to original
            }
        }

        //when the mouse leaves a label in the popup menu, check the mouse position
        void tempLabel_MouseLeave(object sender, EventArgs e)
        {
            hidePanel = true;
            Point mousePos = Control.MousePosition;
            Point ClientMousePos = popUpPanel.PointToClient(mousePos);
            if (ClientMousePos.X > 100 || ClientMousePos.X < 0 || ClientMousePos.Y < 0 || ClientMousePos.Y > 100)
            {
                //if the mouse position is outside the panel, hide the panel
                //only works on the top and left of the panel due to scroll bars
                popUpPanel.Visible = false;
            }
        }

        //if the mouse leaves the popuppanel, hide it
        void popUpPanel_MouseLeave(object sender, EventArgs e)
        {
            popUpPanel.Visible = false;
        }

        //eventhandler for the doubleclick on the labels in the popuppanel
        void tempLabel_MouseDoubleClick(object sender, MouseEventArgs e, labelWithValue currentLabel)
        {
            labelWithValue control = (labelWithValue)sender;
            //as long as the employee is not already in the label, add it
            if (!currentLabel.searchList(control.Text))
            {
                currentLabel.addEmployeeToList(control.Text, control.BackColor); //add employee and color to list
                currentLabel.BackColor = control.BackColor; //update color
                if (currentLabel.getNumElementsInList() > 1)
                {
                    currentLabel.Text = "...";
                }
                else
                {
                    currentLabel.Text = currentLabel.getEmployeeList(0);
                }
            }
            popUpPanel.Visible = false; //after doubleclick, hide the popuppanel
        }

        //on drag leaving
        private void scheduleGrid_DragLeave(object sender, EventArgs e)
        {
            labelWithValue currentLabel = (labelWithValue)sender;
            currentLabel.BackColor = getColor(); //restore the value of background
            
        }

        //encrypt the password entered
        private string EncryptPassword(string textBoxPassword)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged { Key = key, IV = iv };
            byte[] plainText = Encoding.Unicode.GetBytes(textBoxPassword);

            using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
        
        void loginButton_Click(object sender, System.EventArgs e)
        {
            //setLoginBool(true);
            int queryCount = 0;
            int userID = -1;

            //Permission Booleans
            bool canAddEmployees = false;
            bool canEditEmployees = false;
            bool canEditPermissions = false;

            //create a new connection
            MySqlConnection conn = new MySqlConnection(connString);
         
            try
            {
                conn.Open(); //open the connection to the database
                MySqlCommand cmd = conn.CreateCommand();

                //select the ID's of the employee that matches the username and password entered
                cmd.CommandText = "SELECT ID FROM Employee INNER JOIN Username ON Employee.uName = Username.username WHERE uName = @userName AND Username.password = @pass";

                //username and password parameters
                cmd.Parameters.AddWithValue("@userName", loginUserName.Text);
                cmd.Parameters.AddWithValue("@pass", EncryptPassword(loginPassword.Text));

                //create and start the command and reader
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //count the number of IDs found
                    queryCount++;
                    userID = (int)reader["ID"];
                }
                setUserID(userID);
            }
            catch (MySqlException ex) //catch any exceptions
            {

                switch (ex.Number)
                {
                    case 1042:
                        MessageBox.Show("Unable to Connect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    case 0:
                        MessageBox.Show("Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                conn.Close(); //close the connection
            }

           //if an ID is found, then...
           if (queryCount >= 1)
           {
                //hide all of the login controls
                loginButton.Visible = false;
                loginLabel.Visible = false;
                loginPassword.Visible = false;
                loginUserName.Visible = false;
                userLabel.Visible = false;
                passLabel.Visible = false;

                //show all of the scheduling controls
                employeeList.Visible = true;
                scheduleGrid.Visible = true;
                dayLabels.Visible = true;

                try
                {
                    conn.Open(); //open a database connection
                    MySqlCommand cmd = conn.CreateCommand(); //create a new command

                    //get Employee IDs, first names, and the permissions the employee has
                    cmd.CommandText = "SELECT Employee.ID, Employee.fName, PermAssignments.permID FROM Employee INNER JOIN PermAssignments ON Employee.ID=PermAssignments.empID WHERE Employee.ID = @useID";
                    
                    //parameters of sql command
                    cmd.Parameters.AddWithValue("@useID", userID);

                    //execute and run the reader
                     MySqlDataReader reader = cmd.ExecuteReader();
                     while (reader.Read())
                     {
                         switch ((int)reader["permID"])
                         {
                             case 1:
                                 //if they have permission 1, they can add employees
                                 canAddEmployees = true;
                                 setPermissions(true, 0);
                                 break;
                             case 3:
                                 canEditEmployees = true;
                                 setPermissions(true, 1);
                                 break;
                             case 4:
                                 canEditPermissions = true;
                                 setPermissions(true, 2);
                                 break;
                             default:
                                 break;
                         }
                         
                     }

                }
                catch (MySqlException ex) //catch any exceptions
                {
                    switch (ex.Number)
                    {
                        case 1042:
                            MessageBox.Show("Unable to Connect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case 0:
                            MessageBox.Show("Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    conn.Close();
                }

                if (canAddEmployees) //if they can add employees, show the menu
                {
                    menuStrip1.Visible = true;
                }
                
                dayLabels.BringToFront(); // bring the daylabels to the front
           }

        }

        //sets permission status
        private void setPermissions(bool permission, int permissionNum)
        {
            permissionList[permissionNum] = permission;
        }

        //sets the id of the current user
        private void setUserID(int user)
        {
            IdOfUser = user;
        }

        //When the add employee menu is click, show the proper menu
        private void addEmployeeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addEmployeeForm.Show();
            addEmployeeForm.checkIfAddPressed(false, IdOfUser, permissionList); //if add is pressed, show proper menu
        }

        //when the edit employee menu is clicked, show the proper menu
        private void editEmployeeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addEmployeeForm.Show();
            addEmployeeForm.checkIfEditPressed(true, IdOfUser, permissionList);
        }

        //when the mouse enters the schedule grid, hide the panel if it is visible
        private void scheduleGrid_MouseEnter(object sender, EventArgs e)
        {
            if (hidePanel)
            {
                popUpPanel.Visible = false;
                hidePanel = false;
            }
        }

    }
}