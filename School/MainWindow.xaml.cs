using School.Data;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace School
{
    public partial class MainWindow : Window
    {
        // Connection to the School database
        private SchoolDBEntities schoolContext = null;
        // Field for tracking the currently selected teacher
        private Teacher teacher = null;
        // List for tracking the students assigned to the teacher's class
        private IList studentsInfo = null;
        public MainWindow()
        {
            InitializeComponent();
        }
        // Connect to the database and display the list of teachers when the window appears
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.schoolContext = new SchoolDBEntities();
            teachersList.DataContext = this.schoolContext.Teachers;
        }
        // When the user selects a different teacher, fetch and display the students for that teacher
        private void teachersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Find the teacher that has been selected
            this.teacher = teachersList.SelectedItem as Teacher;
            this.schoolContext.LoadProperty<Teacher>(this.teacher, s => s.Students);

            // Find the students for this teacher
            this.studentsInfo = ((IListSource)teacher.Students).GetList();

            // Use databinding to display these students
            studentsList.DataContext = this.studentsInfo;
        }
        // When the user presses a key, determine whether to add a new student to a class, remove a student from a class, or modify the details of a student
        private void studentsList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // If the user pressed Enter, edit the details for the currently selected student
                case Key.Enter:
                    EditStudent();
                    break;
                case Key.Insert:
                    AddNewStudent();
                    break;
                case Key.Delete:
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure???", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                    if (MessageBoxResult.Yes == result)
                    {
                        Student studentDelete = this.studentsList.SelectedItem as Student;
                        RemoveStudent(studentDelete);
                    }
                    break;
            }
        }
        private void studentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditStudent();
        }
        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            schoolContext.SaveChanges();
            saveChanges.IsEnabled = false;
        }
        private void EditStudent() {
            Student student = this.studentsList.SelectedItem as Student;

            // Use the StudentsForm to display and edit the details of the student
            StudentForm sf = new StudentForm();

            // Set the title of the form and populate the fields on the form with the details of the student           
            sf.Title = "Edit Student Details";
            sf.firstName.Text = student.FirstName;
            sf.lastName.Text = student.LastName;
            sf.dateOfBirth.Text = student.DateOfBirth.ToString("d"); // Format the date to omit the time element

            // Display the form
            if (sf.ShowDialog().Value)
            {
                // When the user closes the form, copy the details back to the student
                student.FirstName = sf.firstName.Text;
                student.LastName = sf.lastName.Text;
                student.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text, CultureInfo.InvariantCulture);
                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }
        }
        private void AddNewStudent() {
            Student studentNew = new Student();
            StudentForm sfNew = new StudentForm();
            sfNew.Title = "Edit Student Details";
            sfNew.firstName.Text = studentNew.FirstName;
            sfNew.lastName.Text = studentNew.LastName;
            sfNew.dateOfBirth.Text = studentNew.DateOfBirth.ToString("d"); // Format the date to omit the time element
            if (sfNew.ShowDialog().Value)
            {
                if (sfNew.firstName.Text == "")
                {
                    MessageBox.Show("U moet een naam ingeven");
                    AddNewStudent();
                }
                studentNew.FirstName = sfNew.firstName.Text;
                studentNew.LastName = sfNew.lastName.Text;
                studentNew.DateOfBirth = DateTime.Parse(sfNew.dateOfBirth.Text, CultureInfo.InvariantCulture);
                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
                studentsInfo.Add(studentNew);
            }
        }
        private void RemoveStudent(Student student) {
            studentsInfo.Remove(student);
            saveChanges.IsEnabled = true;
        }
    }
    [ValueConversion(typeof(string), typeof(Decimal))]
    class AgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            DateTime birthDate = (DateTime)value;
            TimeSpan ageTS = DateTime.Now.Subtract(birthDate);
            int leeftijd = (int)(ageTS.Days / 365.25);
            return leeftijd;
        }

        #region Predefined code

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
