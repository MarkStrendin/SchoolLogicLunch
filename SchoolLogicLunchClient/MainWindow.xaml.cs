using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SLData;

namespace SchoolLogicLunchClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Timer timer = new Timer();

        private static bool UILoaded = false;
        private static Dictionary<string, Student> allStudents = new Dictionary<string, Student>();
        private static Dictionary<int, MealType> mealTypes = new Dictionary<int, MealType>();
        private ObservableCollection<PurchasedMeal> mealLog = new ObservableCollection<PurchasedMeal>();

        private static bool undoMode = false;
        private static bool currentlyHandlingPurchase = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Dispatcher.Invoke((Action)(RefreshUI));
        }
        
        private void RefreshUI()
        {
            if ((allStudents.Count > 0) && (mealTypes.Count > 0))
            {
                txtStudentNumberEntry.IsEnabled = true;
            }
            else
            {
                txtStudentNumberEntry.IsEnabled = false;
            }

            if (undoMode)
            {
                txtStudentNumberEntry.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                btnUndo.Content = "Cancel";
            }
            else
            {
                txtStudentNumberEntry.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                btnUndo.Content = "Reverse a transaction";
            }

            lblSchoolID.Content = "SchoolID: " + Settings.SchoolDatabaseID;
            lblMealTypeID.Content = "MealType: " + Settings.MealType;
            if (mealTypes.ContainsKey(Settings.MealType))
            {
                lblMealTypeID.Content = lblMealTypeID.Content + " (" + mealTypes[Settings.MealType].Name + ")";
            }
            else
            {
                lblMealTypeID.Content = lblMealTypeID.Content + " (INVALID)";
            }

            lblStatus.Content = "Students: " + allStudents.Count + ", MealTypes: " + mealTypes.Count;

            if (txtStudentNumberEntry.IsEnabled)
            {
                txtStudentNumberEntry.Focus();
                timer.Stop();
            }

        }

        async void LoadMealTypes()
        {
            // Load students
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Settings.ServerURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/MealType");
                if (response.IsSuccessStatusCode)
                {
                    List<MealType> loadedMealTypes = await response.Content.ReadAsAsync<List<MealType>>();

                    foreach (MealType mealType in loadedMealTypes)
                    {
                        if (!mealTypes.ContainsKey(mealType.ID))
                        {
                            mealTypes.Add(mealType.ID, mealType);
                        }
                    }
                }
            }

            Dispatcher.Invoke((Action)(RefreshUI));
        }

        async void LoadStudents()
        {
            // Load students
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Settings.ServerURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/Student");
                if (response.IsSuccessStatusCode)
                {
                    List<Student> students = await response.Content.ReadAsAsync<List<Student>>();

                    foreach (Student student in students)
                    {
                        if (string.IsNullOrEmpty(student.StudentNumber.Trim())) continue;
                        if (!allStudents.ContainsKey(student.StudentNumber))
                        {
                            allStudents.Add(student.StudentNumber, student);
                        }
                    }
                }
            }

            Dispatcher.Invoke((Action)(RefreshUI));
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStudents();
            LoadMealTypes();
            
            // Set up a timer for the clock
            txtStudentNumberEntry.Focus();
            timer.Interval = 100;
            timer.Elapsed += TimerOnElapsed;
            timer.Start();

            listMealLog.DataContext = mealLog;

            UILoaded = true;


            resetStudentTextField();
            RefreshUI();
        }

        private void resetStudentTextField()
        {
            if (txtStudentNumberEntry.IsEnabled)
            {
                txtStudentNumberEntry.Focus();
                txtStudentNumberEntry.Clear();
                currentlyHandlingPurchase = false;    
            }
        }

        private void TxtStudentNumberEntry_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (UILoaded)
            {
                if (!currentlyHandlingPurchase)
                {
                    if ((e.Key == Key.Enter) || (e.Key == Key.Return))
                    {
                        currentlyHandlingPurchase = true;

                        string parsedStudentIDNumber = txtStudentNumberEntry.Text.Trim();
                        if (!string.IsNullOrEmpty(parsedStudentIDNumber))
                        {
                            if (allStudents.ContainsKey(parsedStudentIDNumber))
                            {
                                if (mealTypes.ContainsKey(Settings.MealType))
                                {
                                    MealType selectedMealType = mealTypes[Settings.MealType];
                                    Student selectedStudent = allStudents[parsedStudentIDNumber];

                                    if (selectedStudent != null)
                                    {
                                        HandleMealPurchase(selectedStudent, selectedMealType);
                                    }
                                    resetStudentTextField();
                                }
                                else
                                {
                                    MessageBox.Show("Selected MealType was not found!", "MealType not found",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Student not found!", "Student not found", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                resetStudentTextField();
                            }
                        }
                    }
                }
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void HandleMealPurchase(Student student, MealType mealtype)
        {
            int mealAmount = 1;

            if (undoMode)
            {
                mealAmount = -1;
                undoMode = false;
                RefreshUI();
            }

            // Try to create a valid meal purchase object
            PurchasedMeal newMeal = new PurchasedMeal()
            {
                Amount = mealAmount,
                MealType = mealtype.ID,
                SchoolID = Settings.SchoolDatabaseID,
                StudentID = student.ID,
                DateAndTime = DateTime.Now,
                Student = student,
                MealInfo = mealtype
            };

            if (newMeal.IsValid())
            {
                // Try to push to the web API
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Settings.ServerURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.PostAsJsonAsync("api/PurchasedMeal", newMeal);
                        if (response.IsSuccessStatusCode)
                        {
                            mealLog.Add(newMeal);
                        }
                        else
                        {
                            MessageBox.Show(response.StatusCode.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Invalid meal constructed - unable to post\n" + newMeal, "Invalid PurchasedMeal object", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (undoMode)
            {
                undoMode = false;
            }
            else
            {
                undoMode = true;
            }
            
            RefreshUI();
        }
        
    }
}
