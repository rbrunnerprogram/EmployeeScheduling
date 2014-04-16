using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Custom class that extends the built-in label class
//
//Creates a label that has variable capability
//Added: 
//  empList - stores a list of employees in the label
//  colorList - stores a list of colors for maintaining employee colors
//  empID - stores the empID: used with the employeelist
namespace WindowsFormsApplication1
{
    public partial class labelWithValue : System.Windows.Forms.Label
    {
        private int empID;
        private List<string> empList = new List<string>();
        private List<Color> colorList = new List<Color>();
        public labelWithValue()
        {
            InitializeComponent();
        }

        //returns the last index of the employee list
        public int lastIndexOfEmployeeList()
        {
            return empList.Count - 1;
        }

        //returns the last index of the color list
        public int lastIndexOfColorList()
        {
            return colorList.Count - 1;
        }

        //sets the employee ID of the label
        public void setEmpID(int ID)
        {
            empID = ID;
        }

        //returns the employee ID of the label
        public int getEmpID()
        {
            return empID;
        }

        //Add the employee and its color to the appropriate list
        public void addEmployeeToList(string empName, Color labelColor)
        {
            empList.Add(empName);
            colorList.Add(labelColor);
        }

        //returns the index 'index' of the employee list
        public string getEmployeeList(int index)
        {
            return empList[index];
        }

        //returns the index 'index' of the color list
        public Color getEmployeeColor(int index)
        {
            return colorList[index];
        }
         //returns the full employee list
        public string getEmployeeFullList()
        {
            string fullList = ""; //initialize an empty string
            foreach (string item in empList)
            {
                //give each item to fullList with a newline character inbetween
                fullList += item + System.Environment.NewLine;
            }

            //return the list
            return fullList;
        }

        //searches the list for string 'empName'
        public bool searchList(string empName)
        {
            string result = empList.Find(item => item == empName);
           
            if (result != null) //if the result is found
            {
                return true; //return true
            }
            else
            {
                return false; //otherwise, return false
            }
        }

        //return the number of elements in the employee list
        public int getNumElementsInList()
        {
            return empList.Count;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
