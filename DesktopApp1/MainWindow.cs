using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using System.IO;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System.Threading;

namespace SeekerSearch
{
    public partial class MainWindow : Form
    {
        private string sPassword;
        private string sUsername;
        private int iNumMessages;
        public FirefoxDriver driver;
        

        public MainWindow()
        {
            InitializeComponent();
            //Read Text file and place the text in the Conversation starts text box
            ChatUpLines.Text = File.ReadAllText("ChatUpLines.txt");
            // Get the previously used username and place it in the username textbox
            UserName_Box.Text = File.ReadAllText("Inputs.txt");
            MessagesSent_Label.Text = "Total Sent: " + CountMessagesSent().ToString();
            TextBox_NumMessages.MaxLength = 2;
        }

        public void button1_Click(object sender, EventArgs e)
        {

            //Check if username and password are blank
            if (CheckForPasswordInput(sPassword) == false)
            {
                return;
            }
            
            ProgressBar.ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.ProgressBar.MarqueeAnimationSpeed = 30;

            if (backgroundWorker1.IsBusy != true)
            {
                Console.WriteLine("Starting BackGround Worker...");
                backgroundWorker1.RunWorkerAsync();
            }
           
        }

        private string RandomChatUpLine()
        {
            //Add Chatuplines to array
            string[] ChatUpArray = File.ReadAllLines("ChatUpLines.txt");
                        
            //remove blank lines
            List<string> list = new List<string>(ChatUpArray);
            for (int i = 0; i < list.Count; i++)
            {
                bool nullOrEmpty = string.IsNullOrEmpty(list[i]);
                if (nullOrEmpty)
                {
                    list.RemoveAt(i);
                    --i;
                }
            }

            Random rand = new Random();
            // Generate a random index less than the size of the array.  
            int index = rand.Next(list.Count);

            Console.WriteLine(list[index]);
            return list[index];

        }

        private bool CheckForPasswordInput(string sPassword)
        {
            //Check if username and password are blank
            if (String.IsNullOrEmpty(sPassword))
            {
                Password_Box.BackColor = System.Drawing.Color.Red;
                ToolStrip.Text = "Please enter a password...";
                return false;
            }
            else
            {
                Password_Box.BackColor = System.Drawing.Color.White;
                ToolStrip.Text = "Ready...";
                return true;
            }
        }

        private void Password_Box_TextChanged(object sender, EventArgs e)
        {
            sPassword = Password_Box.Text;            
        }

        private void UserName_Box_TextChanged(object sender, EventArgs e)
        {
            sUsername = UserName_Box.Text;
            // Write the text in the username box to a text file
            File.WriteAllText("Inputs.txt", UserName_Box.Text);            
        }

        private bool BrowserCheck()
        {
            RegistryKey browserKeys;
            //on 64bit the browsers are in a different location
            browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
            if (browserKeys == null)
                browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");

            string[] browserNames = browserKeys.GetSubKeyNames();

            foreach (string eachkey in browserNames)
            {
                Console.WriteLine(eachkey);
                if (eachkey.Contains("FIREFOX"))
                {
                    return true;
                }
                
            }

            MessageBox.Show("FireFox Must Be Installed.", "Fatal Error");
            return false;
            
        }

        private void ChatUpLines_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText("ChatUpLines.txt", ChatUpLines.Text);
        }

        private void FishLogin(FirefoxDriver driver)
        {
            string DionysusAngelis = "Name";

            //Nav to websight
            ToolStrip.Text = "Going to website...";
            driver.Url = "https://www.pof.com";

            //Login Username
            IWebElement UserName = driver.FindElementById("logincontrol_username");
            ToolStrip.Text = "Logging into POF";
            UserName.Click();
            UserName.Clear();
            UserName.SendKeys(sUsername);

            //Login Password
            IWebElement Password = driver.FindElementById("logincontrol_password");
            Password.Click();
            Password.Clear();
            Password.SendKeys(sPassword);

            //Submit Login Button
            IWebElement LoginButton = driver.FindElementById("logincontrol_submitbutton");
            ToolStrip.Text = "Clicking login button...";
            LoginButton.Click();
            
        }
        
        private int CountMessagesSent()
        {
            var lineCount = 0;
            using (var reader = File.OpenText("Contacts.txt"))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            return lineCount;
        }

        private bool UserNameExistsInFile(string ProfileUserName)
        {
            // Check if profile name already exists in file
            foreach (var line in File.ReadAllLines("Contacts.txt"))
            {
                if (line.Contains(ProfileUserName))
                {
                    //Username exists in file
                    return true;
                }

            }

            // Add Profile name to text file
            File.AppendAllText("Contacts.txt", ProfileUserName + Environment.NewLine);
            
            return false;

        }

        private bool SendingMessage(FirefoxDriver driver, string ProfileUserName)
        {
            ToolStrip.Text = "Sending Message...";
            IWebElement SendMessage = driver.FindElementById("send-message-textarea");

            //Use Javascript to scroll the message window into view.
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", SendMessage);

            SendMessage.Click();
            SendMessage.Clear();
            SendMessage.SendKeys(RandomChatUpLine());

            IWebElement SendMessageButton = driver.FindElementById("send-quick-message-submit");
            SendMessageButton.Click();

            //accepts messages only from people who have certain profile attributes (for example, users who don't smoke).

            IWebElement GetTextOnPage = driver.FindElementByTagName("body");
            if (GetTextOnPage.Text.Contains("You must have a picture"))
            {
                //Nav to websight
                ToolStrip.Text = "Profile requires a photo.";
                Console.WriteLine("Profile requires a photo.");
                driver.Url = "https://www.pof.com";
                return false;

            }
            else if (GetTextOnPage.Text.Contains("has chosen to only accept messages from upgraded users"))
            {
                //MessageBox.Show("Code Missing to handle this section...");
                //Nav to websight
                ToolStrip.Text = "Profile only accepts upgraded users.";
                Console.WriteLine("Profile only accepts upgraded users.");
                driver.Url = "https://www.pof.com";
                return false;
            }
            else if (GetTextOnPage.Text.Contains("prefers to receive longer messages"))
            {

                string sTheLongMessage = "I am an average guy with an average life – I am down to earth, but there was a sadness in my heart. I was missing something. Today, I realized exactly what I was missing. It was you.  Your beauty transcends the limiting instrumentality of language itself.  I surrender to your judgements sweet kiss";

                
                // go back to the previous page 
                driver.Navigate().Back();
                // get the sendmessage box and type longer message
                ToolStrip.Text = "Sending Longer Message...";
                IWebElement SendLongMessage = driver.FindElementById("send-message-textarea");
                SendLongMessage.Click();
                SendLongMessage.Clear();
                SendLongMessage.SendKeys(sTheLongMessage);
                ToolStrip.Text = "Submiting Message...";
                IWebElement SendLongMessageButton = driver.FindElementById("send-quick-message-submit");
                SendLongMessageButton.Click();

                //driver.Url = "https://www.pof.com";
            }
            else if (GetTextOnPage.Text.Contains("POF blocks cut and paste messages and you may only contact 55 new people in a 24 hour period."))
            {
                MessageBox.Show("You may only contact 55 new people in a 24 hour period.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                // remove username from list becuase message was not sent.
                File.WriteAllLines("Contacts.txt", File.ReadLines("Contacts.txt").Where(l => l != ProfileUserName).ToList());
                //close the program.
                Environment.Exit(1);
                
            }
            else if (GetTextOnPage.Text.Contains("accepts messages only from people who have certain profile attributes"))
            {
                ToolStrip.Text = "Profile only requires certain profile attributes.";
                Console.WriteLine("Profile only requires certain profile attributes.");
                driver.Url = "https://www.pof.com";
                return false;
            }

            ToolStrip.Text = "Message sent...";
            return true;

        }

        private bool Fishing(FirefoxDriver driver)
        {
                     
            //Click on My Matches
            ToolStrip.Text = "Finding your matches...";
            string MyMatchXpath = "//*[@id=\"sub-menu\"]/li[1]/a";
            IWebElement MyMatches = driver.FindElementByXPath(MyMatchXpath);
            // need error handling for mymatches
            //failed to notice the picture was missing from the mans profile
            MyMatches.Click();

            ToolStrip.Text = "Refining your search...";
            // Select Contact drop down
            IWebElement contacted = driver.FindElementByName("contacted");
            
            // Refine search to show only users I have not contacted.
            SelectElement SelectContacted = new SelectElement(contacted);
            SelectContacted.SelectByText("Users I've Not Contacted");
            
            // Select Miles Drop down and set it to 10.
            IWebElement miles = driver.FindElementByName("miles");
            SelectElement SelectDistance = new SelectElement(miles);
            SelectDistance.SelectByText("50 miles");
            
            //Click Refine Matches
            string RefineSearchXpath = "//*[@id=\"form1\"]/center/table/tbody/tr/td[8]/input";
            IWebElement RefineSearch = driver.FindElementByXPath(RefineSearchXpath);
            RefineSearch.Click();
            
            //need to get the a herf links on the page so we can get the profile id
            IWebElement Profile = driver.FindElementByClassName("mi");
            ToolStrip.Text = "Clicking on profile...";

            //try looping through all the profiles and geting the href links to get the profile id before we click
            //unless i start tracking contacts using the profile ID this runs into the stale element problem
            var ProfileList = driver.FindElementsByClassName("mi");

            Profile.Click();

            //Get username from page
            string sUserNameXPath = "//*[@id=\"username\"]";
            string ProfileUserName = driver.FindElementByXPath(sUserNameXPath).Text;
            Console.WriteLine("Profile Username: " + ProfileUserName);

            if (UserNameExistsInFile(ProfileUserName))
            {
                //Username already in file
                Console.WriteLine("Username already exists in file...");
                return false;
            }
            
            SendingMessage(driver, ProfileUserName);

            return true;
            
            
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                ProgressBar.Style = ProgressBarStyle.Continuous;
                ProgressBar.MarqueeAnimationSpeed = 0;
                // Update the Total sent label
                MessagesSent_Label.Text = "Total Sent: " + CountMessagesSent().ToString();
                Console.WriteLine("Background worker cancelled...");
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                //MessageBox.Show(e.Result.ToString());
                ProgressBar.Style = ProgressBarStyle.Continuous;
                ProgressBar.MarqueeAnimationSpeed = 0;
                // Update the Total sent label
                MessagesSent_Label.Text = "Total Sent: " + CountMessagesSent().ToString();
                Console.WriteLine("Background worker stopped...");
            }
           
        }

        public FirefoxDriver OpenBrowser()
        {
            //Find the embeded firefox.exe 
            Console.WriteLine(Directory.GetCurrentDirectory());
            FirefoxDriver driver = new FirefoxDriver(Directory.GetCurrentDirectory());
            // Minimize Browser Window
            driver.Manage().Window.Minimize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            return driver;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            BrowserCheck();
            FirefoxDriver driver = OpenBrowser();
            FishLogin(driver);
            
            for (int i = 0; i < iNumMessages; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                    if (Fishing(driver))
                    {
                        //Returned true
                        Console.WriteLine("Message Sent...");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        // failed to send message
                        iNumMessages = iNumMessages + 1;
                    }
                    
                } 
              
            }

            ToolStrip.Text = "Closing Browser...";
            driver.Close();
            driver.Quit();
            ToolStrip.Text = "Ready";
            
        }

        private void Stop_Button_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
            }
        }

        private void TextBox_NumMessages_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(TextBox_NumMessages.Text, "[^0-9]"))
            {
                //MessageBox.Show("Please enter only numbers.");
                TextBox_NumMessages.Text = TextBox_NumMessages.Text.Remove(TextBox_NumMessages.Text.Length - 1);
                
            } else
            {
                if (String.IsNullOrEmpty(TextBox_NumMessages.Text))
                {
                    //string is empty
                    iNumMessages = 1;
                }
                else
                {
                    iNumMessages = int.Parse(TextBox_NumMessages.Text);
                    Console.WriteLine(iNumMessages);
                }
                
            }
        }
    }
}
