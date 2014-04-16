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
    public partial class Form2 : Form
    {
        //create database connection string
        static string connString = "Server=mysql.aldpesiupui.dreamhosters.com;Port=3306;Database=capstone_brunner2;Uid=programaster;password=programaster123;";
        private bool editButton = false; //records if the edit button has been pressed

        //Creates the encryption key for password encryption
        //Hello World is encryption key string
        private const string ENCRYPTION_KEY = "ENmtH6kEeV1.I";

        //Creates a SALT, KEY, keyGenerator, and IV for password encryption
        private readonly static byte[] SALT = Encoding.ASCII.GetBytes(ENCRYPTION_KEY);
        private readonly byte[] key;
        private readonly byte[] iv;
        private readonly Rfc2898DeriveBytes keyGenerator;

        int userID = -1;
        int addedUserID = -1;

        //initialize the second form
        public Form2()
        {
            //Set up encryption seeds
            keyGenerator = new Rfc2898DeriveBytes(ENCRYPTION_KEY, SALT);
            key = keyGenerator.GetBytes(32);
            iv = keyGenerator.GetBytes(16);
            InitializeComponent();
            //create the drop down for departments
            createDepartmentDropdown();
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

        //on close, cancel the close operation and hide the form instead
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        //Creates the department dropdown from the database
        private void createDepartmentDropdown()
        {
            //create two dictionaries to hold the department names and job titles for dropdown lists
            Dictionary<string, string> departmentList = new Dictionary<string, string>();
            Dictionary<string, string> jobList = new Dictionary<string, string>();
            
            //Create sql connection using connection string
            MySqlConnection conn = new MySqlConnection(connString);

            try
            {
                conn.Open(); //open the connection
                MySqlCommand cmd = conn.CreateCommand();

                //select the ids and names from the departments
                cmd.CommandText = "SELECT ID, dName FROM Department";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string deptID = reader["ID"].ToString();
                    string deptName = reader["dName"].ToString();
                    //store results from each read into the department list
                    departmentList.Add(deptName, deptID);
                }
            }
            catch (MySqlException ex) //check for problems
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
                conn.Close(); //close the connection after access
            }

            try
            {
                conn.Open(); //open the connections
                MySqlCommand cmd = conn.CreateCommand();

                //select the id and titles from the job
                cmd.CommandText = "SELECT ID, JobTitle FROM Job";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string jobID = reader["ID"].ToString();
                    string jobTitle = reader["JobTitle"].ToString();
                    //store information into the job list
                    jobList.Add(jobTitle, jobID);
                }
            }
            catch (MySqlException ex) //catch any issues
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

            //bind the dictionaries to the appropriate dropdown
            departmentBox.DataSource = new BindingSource(departmentList, null);
            departmentBox.DisplayMember = "Key";
            departmentBox.ValueMember = "Value";

            jobTitleBox.DataSource = new BindingSource(jobList, null);
            jobTitleBox.DisplayMember = "Key";
            jobTitleBox.ValueMember = "Value";
        }

        private void setEditBool(bool editPressed)
        {
            editButton = editPressed;
        }

        private void setAddedUserID(int addedUser)
        {
            addedUserID = addedUser;
        }

        //Checks to see if the job position is critical
        private void jobTitleBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = ((KeyValuePair<string, string>)jobTitleBox.SelectedItem).Value;
            MySqlConnection conn = new MySqlConnection(connString);
            bool checkCritical = true;
            

            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                
                //get the isCritical from the job to detect if it is critical
                cmd.CommandText = "SELECT isCritical FROM Job WHERE ID = @theID";
                cmd.Parameters.AddWithValue("@theID", value);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //critical boolean altered from reader
                    checkCritical = (bool)reader["isCritical"];
                }
            }
            catch (MySqlException ex) //check for issues
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
                conn.Close(); //close the database connections
            }

            //Checks the critical box if the employee is critical
            if (checkCritical)
            {
                criticalBox.Checked = true;
            }
            else
            {
                criticalBox.Checked = false;
            }
            
        }

        private void setUserID(int user)
        {
            userID = user;
        }

        //if the edit employee menu item was selected
        public void checkIfEditPressed(bool editStatus, int CurruserID, bool[] permList)
        {
            setUserID(CurruserID);
            setEditBool(editStatus);
            if (permList[2])
            {
                addEmployeeBox.Enabled = true;
                editEmp.Enabled = true;
                canChangePerm.Enabled = false;
            }
            else
            {
                addEmployeeBox.Enabled = false;
                editEmp.Enabled = false;
                canChangePerm.Enabled = false;
            }
            //create a dictionary that populates with the names of employees
            Dictionary<string, string> employeeList = new Dictionary<string, string>();

            //initialize the connection string
            MySqlConnection conn = new MySqlConnection(connString);
            try
            {
                conn.Open(); //open the database connection and create a command
                MySqlCommand cmd = conn.CreateCommand();

                //Get the ID, First name, and last name of the employees
                cmd.CommandText = "SELECT ID, fName, lName FROM Employee ORDER BY lName";

                //begin the reader
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //record the id and create a string of first name and last name
                    string empID = reader["ID"].ToString();
                    string empName = reader["lName"].ToString() + ", " + reader["fName"].ToString();

                    //add the variables to the employee list
                    employeeList.Add(empName, empID);
                }
            }
            catch (MySqlException ex) //catch any errors
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
                conn.Close(); //close the database connection
            }

            //bind the dictionary to populate the employee dropdown
            empChoiceBox.DataSource = new BindingSource(employeeList, null);
            empChoiceBox.DisplayMember = "Key";
            empChoiceBox.ValueMember = "Value";

            //If the application should show the edit screen, relocate the form2 controls
            //Mostly, move the controls down
            editButton = editStatus;
            passBox.Enabled = false;
            passBox.PasswordChar = '*';
            EmployeefName.Location = new System.Drawing.Point(25, 83);
            EmployeelName.Location = new System.Drawing.Point(25, 106);
            firstName.Location = new System.Drawing.Point(93, 80);
            lastName.Location = new System.Drawing.Point(93, 106);
            UserNameLabel.Location = new System.Drawing.Point(25, 135);
            usernameBox.Location = new System.Drawing.Point(93, 132);
            EmployeePasswordLabel.Location = new System.Drawing.Point(25, 162);
            passBox.Location = new System.Drawing.Point(93, 159);
            criticalBox.Location = new System.Drawing.Point(93, 240);
            departmentBox.Location = new System.Drawing.Point(93, 185);
            deparmentLabel.Location = new System.Drawing.Point(25, 188);
            jobTitleBox.Location = new System.Drawing.Point(93, 213);
            jobLabel.Location = new System.Drawing.Point(25, 216);
            addEmployLabel.Text = "Edit an Employee";
            addEmployeeButton.Text = "Edit Employee";
            chooseEmpLabel.Visible = true;
            empChoiceBox.Visible = true;
            passBox.Enabled = false;
            passBox.PasswordChar = '*';
        }

        public void checkIfAddPressed(bool editStatus, int CurruserID, bool[] permList)
        {
            setUserID(CurruserID);
            setEditBool(editStatus);
            //reset the text fields
            firstName.Text = "";
            lastName.Text = "";
            usernameBox.Text = "";
            passBox.Text = "";
            editButton = editStatus;

            //If the add employee menu item is clicked, relocate the controls
            passBox.PasswordChar = '*';
            EmployeefName.Location = new System.Drawing.Point(25, 53);
            EmployeelName.Location = new System.Drawing.Point(25, 76);
            firstName.Location = new System.Drawing.Point(93, 50);
            lastName.Location = new System.Drawing.Point(93, 76);
            UserNameLabel.Location = new System.Drawing.Point(25, 105);
            usernameBox.Location = new System.Drawing.Point(93, 102);
            EmployeePasswordLabel.Location = new System.Drawing.Point(25, 132);
            passBox.Location = new System.Drawing.Point(93, 129);
            criticalBox.Location = new System.Drawing.Point(93, 210);
            departmentBox.Location = new System.Drawing.Point(93, 155);
            deparmentLabel.Location = new System.Drawing.Point(25, 158);
            jobTitleBox.Location = new System.Drawing.Point(93, 183);
            jobLabel.Location = new System.Drawing.Point(25, 186);
            addEmployLabel.Text = "Add an Employee";
            addEmployeeButton.Text = "Add Employee";
            chooseEmpLabel.Visible = false; //hide the employee label
            empChoiceBox.Visible = false; //hide the employee dropdown
        }

        //If the employee dropdown list is selected and changed, then run this
        private void empChoiceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = ((KeyValuePair<string, string>)empChoiceBox.SelectedItem).Value;
            //reset the checkboxes and disable them
            addEmployeeBox.Checked = false;
            editEmp.Checked = false;
            canChangePerm.Checked = false;

            //create a new connection to the database
            MySqlConnection conn = new MySqlConnection(connString);
            try
            {
                conn.Open(); //open the connection
                MySqlCommand cmd = conn.CreateCommand();

                //get the first name, last name, job title, username, and password from the database
                cmd.CommandText = "SELECT fName, lName, jobTitle, uName, Username.password, deptID FROM Employee INNER JOIN Username ON Employee.uName = Username.username WHERE ID = @empID";
                cmd.Parameters.AddWithValue("@empID", value);
                MySqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    //set the textboxes to the proper field names
                    firstName.Text = reader["fName"].ToString();
                    lastName.Text = reader["lName"].ToString();
                    usernameBox.Text = reader["uName"].ToString();
                    passBox.Text = reader["password"].ToString();
                    jobTitleBox.SelectedValue = reader["jobTitle"].ToString();
                    departmentBox.SelectedValue = reader["deptID"].ToString();
                }
            }
            catch (MySqlException ex) //catch any errors
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

            try
            {
                conn.Open(); //try opening the connection and create the connection
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT permID FROM PermAssignments WHERE empID = @empID";
                cmd.Parameters.AddWithValue("@empID", value);
                MySqlDataReader reader = cmd.ExecuteReader();

                //check the permissions of the employee from the database
                while (reader.Read())
                {
                    switch(reader["permID"].ToString()){
                        case "1": //can add an employee
                            addEmployeeBox.Checked = true;
                            break;
                        case "3": //can edit an employee
                            editEmp.Checked = true;
                            break;
                        case "4": //can change an employee's permissions
                            canChangePerm.Checked = true;
                            break;
                    }
                }
            }
            catch (MySqlException ex) //catch an errors
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
        }

        private void addEmployeeButton_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(connString);

            if (editButton)
            {
                /*try
                {
                    conn.Open(); //try opening the connection and create the connection
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT uName FROM Employee";


                }
                catch (MySqlException ex) //catch an errors
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
                }*/
            }
            else
            {
                
                try
                {
                    conn.Open(); //try opening the connection and create the connection
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Username(username,password) VALUES(@uName,@pass)";
                    cmd.Parameters.AddWithValue("@uName", usernameBox.Text);
                    cmd.Parameters.AddWithValue("@pass", EncryptPassword(passBox.Text));
                    cmd.ExecuteNonQuery();
                    
                }
                catch (MySqlException ex) //catch an errors
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
                
                try
                {
                    string jobValue = ((KeyValuePair<string, string>)jobTitleBox.SelectedItem).Value;
                    string deptValue = ((KeyValuePair<string, string>)departmentBox.SelectedItem).Value;
                    conn.Open(); //try opening the connection and create the connection
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Employee(fName,lName,jobTitle,deptID,uName) VALUES(@first,@last,@title,@depart,@user)";
                    cmd.Parameters.AddWithValue("@first", firstName.Text);
                    cmd.Parameters.AddWithValue("@last",lastName.Text);
                    cmd.Parameters.AddWithValue("@title",jobValue);
                    cmd.Parameters.AddWithValue("@depart", deptValue);
                    cmd.Parameters.AddWithValue("@user", usernameBox.Text);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex) //catch an errors
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

                try
                {
                    conn.Open(); //try opening the connection and create the connection
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT ID FROM Employee Where uName = @user";
                    cmd.Parameters.AddWithValue("@user", usernameBox.Text);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        setAddedUserID((int)reader["ID"]);
                    }
                }
                catch (MySqlException ex) //catch an errors
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

                try
                {
                    bool commandChanged = false;
                    conn.Open(); //try opening the connection and create the connection
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO PermAssignments(empID,permID) VALUES";
                    if (addEmployeeBox.Checked)
                    {
                        cmd.CommandText += "(@emp,@perm0)";
                        cmd.Parameters.AddWithValue("@perm0", "1");
                        commandChanged = true;
                    }
                    if (editEmp.Checked)
                    {
                        cmd.CommandText += ",(@emp,@perm1)";
                        cmd.Parameters.AddWithValue("@perm1", "3");
                        commandChanged = true;
                    }
                    if (canChangePerm.Checked)
                    {
                        cmd.CommandText += ",(@emp,@perm2)";
                        cmd.Parameters.AddWithValue("@perm1", "4");
                        commandChanged = true;
                    }

                    cmd.Parameters.AddWithValue("@emp", addedUserID);
                    if (commandChanged)
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex) //catch an errors
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

                this.Hide();
                usernameBox.Text = "";
                passBox.Text = "";
                editEmp.Checked = false;
                canChangePerm.Checked = false;
                addEmployeeBox.Checked = false;
                firstName.Text = "";
                lastName.Text = "";
                usernameBox.Text = "";
            }
        }
    }
}
