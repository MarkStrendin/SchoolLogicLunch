using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
    enum PriceMode
    {
        FullPrice,
        ReducedPrice,
        Free
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
    public partial class MainWindow : Window
    {
        // Timer used to set focus to the student entry box, when the students are all loaded from calls to the web API
        private static readonly Timer timer = new Timer();
        
        // Cached list of all students in the system, to translate student numbers to student database ID numbers
        private static Dictionary<string, Student> allStudents = new Dictionary<string, Student>();

        // Cached list of meal types, so we know the price of the selected meal
        private static Dictionary<int, MealType> mealTypes = new Dictionary<int, MealType>();

        // The log of the meals purchased this session - this is directly tied to the list in the UI
        private ObservableCollection<PurchasedMeal> mealLog = new ObservableCollection<PurchasedMeal>();

        // Has the UI loaded? An exception happens if we try to disable or hide UI elements that haven't loaded yet.
        private static bool UILoaded = false;

        // Which price to use - full, reduced, or free
        private static PriceMode PriceMode = PriceMode.FullPrice;

        // Is the user currently voiding a transaction
        private static bool voidMode = false;

        // Is the system currently handling a purchase? Don't try to start a new one until the last one is taken care of
        private static bool currentlyHandlingPurchase = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Settings.IsConfigFileValid())
            {
                CriticalError("Config file not found or not complete.", "Invalid configuration file");
            }
            else
            {
                lblStatus.Content = "Customizing UI from configuration file";

                if (!Settings.AllowFreeMeals)
                {
                    txtPriceFree.Visibility = Visibility.Hidden;
                    btnPriceFree.Visibility = Visibility.Hidden;
                }

                if (!Settings.AllowReducedMeals)
                {
                    txtPriceReduced.Visibility = Visibility.Hidden;
                    btnPriceReduced.Visibility = Visibility.Hidden;
                }

                if (!Settings.AllowFreeMeals && !Settings.AllowReducedMeals)
                {
                    txtPriceFull.Visibility = Visibility.Hidden;
                    btnPriceFull.Visibility = Visibility.Hidden;
                    PriceMode = PriceMode.FullPrice;
                }

                ClearLastStudentInfo();

                lblStatus.Content = "Loading from " + Settings.ServerURL;

                lblStatus.Content = "Loading meal types from " + Settings.ServerURL;
                LoadMealTypes();

                lblStatus.Content = "Loading students from " + Settings.ServerURL;
                LoadStudents();

                lblStatus.Content = "Waiting for students to be returned";
                // Set up a timer that will help set focus to the student number input when the students have been loaded
                txtStudentNumberEntry.Focus();
                timer.Interval = 100;
                timer.Elapsed += TimerOnElapsed;
                timer.Start();

                listMealLog.DataContext = mealLog;

                UILoaded = true;

                resetStudentTextField();
                RefreshUI();

                lblStatus.Content = "Ready to start scanning student barcodes";
            }
        }

        
        #region UI Data display methods

        /// <summary>
        /// Clear the student entry box and set focus to it
        /// </summary>
        private void resetStudentTextField()
        {
            if (txtStudentNumberEntry.IsEnabled)
            {
                txtStudentNumberEntry.Focus();
                txtStudentNumberEntry.Clear();
                currentlyHandlingPurchase = false;
            }
        }

        /// <summary>
        /// Refresh any labels, text areas, etc when anything changes.
        /// </summary>
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

            if (voidMode)
            {
                txtStudentNumberEntry.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                btnUndo.Content = "Cancel";
            }
            else
            {
                txtStudentNumberEntry.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                btnUndo.Content = "Void";
            }

            lblStatus.Content = "[READY] Students: " + allStudents.Count + ", MealTypes: " + mealTypes.Count + ", SelectedSchoolID: " + Settings.SchoolDatabaseID + ", SelectedMealType: " + Settings.MealType;

            if (txtStudentNumberEntry.IsEnabled)
            {
                txtStudentNumberEntry.Focus();
                timer.Stop();
            }

            if (mealTypes.ContainsKey(Settings.MealType))
            {
                MealType selectedMealType = mealTypes[Settings.MealType];
                txtMealName.Text = selectedMealType.Name;
                txtPriceFree.Text = selectedMealType.FreeAmount.ToString("C");
                txtPriceFull.Text = selectedMealType.FullAmount.ToString("C");
                txtPriceReduced.Text = selectedMealType.ReducedAmount.ToString("C");

                switch (PriceMode)
                {
                    case PriceMode.FullPrice:
                        txtMealPrice.Text = selectedMealType.FullAmount.ToString("C");
                        break;
                    case PriceMode.ReducedPrice:
                        txtMealPrice.Text = selectedMealType.ReducedAmount.ToString("C");
                        break;
                    case PriceMode.Free:
                        txtMealPrice.Text = selectedMealType.FreeAmount.ToString("C");
                        break;
                }
            }

            txtPriceFull.FontWeight = FontWeights.Normal;
            txtPriceReduced.FontWeight = FontWeights.Normal;
            txtPriceFree.FontWeight = FontWeights.Normal;

            btnPriceFull.IsEnabled = true;
            btnPriceReduced.IsEnabled = true;
            btnPriceFree.IsEnabled = !Settings.AllowFreeMeals && false;

            switch (PriceMode)
            {
                case PriceMode.FullPrice:
                    btnPriceFull.IsEnabled = false;
                    txtPriceFull.FontWeight = FontWeights.Bold;
                    break;
                case PriceMode.ReducedPrice:
                    btnPriceReduced.IsEnabled = false;
                    txtPriceReduced.FontWeight = FontWeights.Bold;
                    break;
                case PriceMode.Free:
                    btnPriceFree.IsEnabled = false;
                    txtPriceFree.FontWeight = FontWeights.Bold;
                    break;
            }

        }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Dispatcher.Invoke((Action)(RefreshUI));
        }
        
        /// <summary>
        /// Clears the text areas that would display information on the last student entered
        /// </summary>
        private void ClearLastStudentInfo()
        {
            txtLastStudentName.Text = string.Empty;
            txtLastStudentID.Text = string.Empty;
            txtLastStudentMedical.Text = string.Empty;
        }

        /// <summary>
        /// Display information on the last student scanned
        /// </summary>
        /// <param name="student"></param>
        private void UpdateLastStudentInfo(Student student)
        {
            txtLastStudentName.Text = student.DisplayName;
            txtLastStudentID.Text = student.StudentNumber;

            if (!string.IsNullOrEmpty(student.MedicalNotes))
            {
                txtLastStudentMedical.Visibility = Visibility.Visible;
                txtLastStudentMedical.Text = "***" + student.MedicalNotes + "***";
            }
            else
            {
                txtLastStudentMedical.Text = string.Empty;
                txtLastStudentMedical.Visibility = Visibility.Hidden;
            }

        }

        /// <summary>
        /// Display information on the last student scanned, if the student number wasn't found.
        /// </summary>
        /// <param name="enteredStudentNumber"></param>
        private void UpdateLastStudentInfo_StudentNotFound(string enteredStudentNumber)
        {
            txtLastStudentName.Text = "Student not found";
            txtLastStudentID.Text = enteredStudentNumber;
            txtLastStudentMedical.Text = string.Empty;
        }

        #endregion

        /// <summary>
        /// Display an error message, and close the program
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        private static void CriticalError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                timer.Stop();
                Application.Current.Shutdown();
            } catch { }
        }

        #region Data Loading methods

        /*
         * These apparently need to be async, which means they can't return values, so they need to interact with static lists.
         * */

        async void LoadMealTypes()
        {
            try
            {
                // Load Meal Types
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Settings.ServerURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync("api/MealType").Result;

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
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            CriticalError("Access not allowed from this location", "Access denied");
                        }
                        else
                        {
                            CriticalError("Error loading MealTypes: " + response.StatusCode, "Error loading MealTypes");
                        }
                        
                    }
                }
                Dispatcher.Invoke((Action)(RefreshUI));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
                else
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        CriticalError("Access not allowed from this location", "Access denied");
                    }
                    else
                    {
                        CriticalError("Error loading Students: " + response.StatusCode, "Error loading Students");
                    }
                }
            }

            Dispatcher.Invoke((Action)(RefreshUI));
        }

        #endregion

        
        #region UI Button logic

        private void TxtStudentNumberEntry_OnKeyUp(object sender, KeyEventArgs e)
        {
            // Don't attempt to handle anything before the UI is fully loaded
            if (UILoaded)
            {
                // Don't handle a purchase until we're finished handling the last one
                if (!currentlyHandlingPurchase)
                {
                    // Barcode reader needs to press one of these buttons after it's scanned a barcode
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
                                        UpdateLastStudentInfo(selectedStudent);
                                    }
                                    resetStudentTextField();
                                }
                            }
                            else
                            {
                                UpdateLastStudentInfo_StudentNotFound(parsedStudentIDNumber);
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

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            voidMode = !voidMode;
            RefreshUI();
        }

        private void btnPriceFull_Click(object sender, RoutedEventArgs e)
        {
            PriceMode = PriceMode.FullPrice;
            RefreshUI();
            resetStudentTextField();
        }

        private void btnPriceReduced_Click(object sender, RoutedEventArgs e)
        {
            PriceMode = PriceMode.ReducedPrice;
            RefreshUI();
            resetStudentTextField();
        }

        private void btnPriceFree_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.AllowFreeMeals)
            {
                PriceMode = PriceMode.Free;
                RefreshUI();
            }
            resetStudentTextField();
        }
        
        #endregion
        

        private async void HandleMealPurchase(Student student, MealType mealtype)
        {
            // The only information we need is a student database ID and a mealtype id
            // If we don't have that, don't bother continuing.
            if ((student == null) || (mealtype == null)) return;

            // Figure out what this meal is going to cost
            decimal mealCost = mealtype.FullAmount;
            if (PriceMode == PriceMode.ReducedPrice)
            {
                mealCost = mealtype.ReducedAmount;
            }
            if (PriceMode == PriceMode.Free)
            {
                mealCost = mealtype.FreeAmount;
            }
            
            // Try to create a valid meal purchase object
            PurchasedMeal newMeal = new PurchasedMeal()
            {
                Amount = mealCost,
                MealType = mealtype.ID,
                SchoolID = Settings.SchoolDatabaseID,
                StudentID = student.ID,
                DateAndTime = DateTime.Now,
                Student = student,
                MealInfo = mealtype
            };

            if (newMeal.IsValid())
            {
                if (voidMode)
                {
                    // Find the last entry for the entered student and reverse it
                    bool foundPreviousEntry = false;
                    foreach (PurchasedMeal pmeal in mealLog.Where(p => p.Voided == false))
                    {
                        if (pmeal.StudentID == student.ID)
                        {
                            foundPreviousEntry = true;
                            pmeal.Voided = true;
                            newMeal.Amount = pmeal.Amount * -1;
                            newMeal.Voided = true;
                            break;
                        }
                    }

                    voidMode = false;
                    RefreshUI();

                    // If there is no previous entry to void, don't try to post anything, just ignore this one.
                    if (foundPreviousEntry == false)
                    {
                        return;
                    }
                }

                // Try to push to the web API
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Settings.ServerURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

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
                MessageBox.Show("Invalid meal constructed - unable to post\n" + newMeal,
                    "Invalid PurchasedMeal object", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Scroll to the last item scanned
            listMealLog.Items.MoveCurrentToLast();
            listMealLog.ScrollIntoView(listMealLog.Items.CurrentItem);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }

    }
}
