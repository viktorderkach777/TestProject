using FluxDayAutomation.Drivers;
using FluxDayAutomation.PageObjects;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace FluxDayAutomation.UITests
{
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V1)]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V2)]
    //[TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V1)]
    //[TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V2)]
    [Category("AddNewTask")]
    public class AddNewTaskUiTest
    {
        private const int IMPLICIT_WAIT_SECONDS = 15;
        private const int EXPLICIT_WAIT_SECONDS = 10;        
        private const int RETRYING_ITERATION_COUNT = 5;
        private const string APPLICATION_URL = "https://app.fluxday.io/users/sign_in";
        private const string MANAGER_EMAIL = "admin@fluxday.io";
        private const string MANAGER_PASSWORD = "password";
        private const string TASK_DESCRIPTION = "New task";
        private const string TASK_TEAM = "Uzity Development";
        private const string TASK_STARTDATE = "2016-07-01 16:33:00 UTC";
        private const string TASK_ENDDATE = "2018-11-05 16:33:00 UTC";
        private const string TASK_PRIORITY = "Low";
        private const string KEY_RESULT_TAGNAME = "input";
        private const string TASK_CLASSNAME = "name";
        private const string ASSIGNED_PERSON_1 = "Employee 1";
        private const string ASSIGNED_PERSON_1_OKR_NAME = "Q3 ";
        private const string ASSIGNED_PERSON_1_OBJECTIVE = "Enhance Uzity";
        private const string ASSIGNED_PERSON_1_KEY_RESULT_1 = "Plan, Prioritize, Design, Develop, Test and Release Features/issues";
        private const string ASSIGNED_PERSON_1_KEY_RESULT_2 = "key 1";
        private const string ASSIGNED_PERSON_2 = "Team Lead";
        private const string ASSIGNED_PERSON_2_OKR_NAME = "Q4 ";
        private const string ASSIGNED_PERSON_2_OBJECTIVE = "Enhance Uzity";
        private const string ASSIGNED_PERSON_2_KEY_RESULT_1 = "Conduct various R&Ds for product enhancement/improvement";
        private const string ASSIGNED_PERSON_2_KEY_RESULT_2 = "key 2";
        private const string OKR_1_DELETING_EXCEPTION = "OKR for assigned person 1 was not deleted!";
        private const string OKR_2_DELETING_EXCEPTION = "OKR for assigned person 2 was not deleted!";
        private const string TASK_DELETING_EXCEPTION = "Task was not deleted!";
        private const int FIREFOX_SLEEP_WAITER = 500;

        private LoginPageObject login;
        private SideBarMenuPageObject sideBarMenuPageObject;
        private MyTasksPageObject myTasksPageObject;
        private AddTaskPageObject addTaskPageObject;
        private TopPanelPageObject topPanelPageObject;
        private DeleteTaskPageObject deleteTaskPageObject;
        private OKRListPageObject okrListForAssignedPerson1;
        private OKRListPageObject okrListForAssignedPerson2;
        private SetOKRPageObject setOKRPageObject;
        private ApproveOKRPageObject approveOKRPageObject;
        private List<UserKeyResultsPageObject> assigneesList;
        private List<TaskPageObject> taskList;
        private IWebDriver driver;
        private WebDriverWait wait;
        private string taskTitle;
        private string currentBrowser;
        private string identificationNumber;
        private string okrName1;
        private string okrName2;
        private bool isNotStaleElement;

        public AddNewTaskUiTest(string browser, string version)
        {
            this.driver = SelenoidDrivers.CreateDriver(browser, version);
            currentBrowser = browser;
        }


        [SetUp, Description("CI browser configurations, pre-conditions")]
        public void Initialization()
        {
            // General driver configuration
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(IMPLICIT_WAIT_SECONDS);
            driver.Navigate().GoToUrl(APPLICATION_URL);

            // Assigning of Page Objects
            login = new LoginPageObject(driver);
            sideBarMenuPageObject = new SideBarMenuPageObject(driver);
            myTasksPageObject = new MyTasksPageObject(driver);
            topPanelPageObject = new TopPanelPageObject(driver);
            addTaskPageObject = new AddTaskPageObject(driver);
            deleteTaskPageObject = new DeleteTaskPageObject(driver);
            okrListForAssignedPerson1 = new OKRListPageObject(driver);
            okrListForAssignedPerson2 = new OKRListPageObject(driver);
            approveOKRPageObject = new ApproveOKRPageObject(driver);
            setOKRPageObject = new SetOKRPageObject(driver);

            identificationNumber = DateTime.Now.ToString("dd:MM:yyyy/HH:mm");
            assigneesList = new List<UserKeyResultsPageObject>();
            taskList = new List<TaskPageObject>();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(EXPLICIT_WAIT_SECONDS));

            // Login into system
            login.UserEmailTextBox.Click();
            login.UserEmailTextBox.Clear();
            login.UserEmailTextBox.SendKeys(MANAGER_EMAIL);
            login.UserPasswordTextBox.Click();
            login.UserPasswordTextBox.Clear();
            login.UserPasswordTextBox.SendKeys(MANAGER_PASSWORD);
            login.LoginButton.Click();

            // Assigning unique names to OKRs
            okrName1 = ASSIGNED_PERSON_1_OKR_NAME + identificationNumber;
            okrName2 = ASSIGNED_PERSON_2_OKR_NAME + identificationNumber;

            // Creating OKRs for the assignees
            CreateOkr(ASSIGNED_PERSON_1, okrName1, ASSIGNED_PERSON_1_OBJECTIVE,
                      ASSIGNED_PERSON_1_KEY_RESULT_1, ASSIGNED_PERSON_1_KEY_RESULT_2, okrListForAssignedPerson1);

            CreateOkr(ASSIGNED_PERSON_2, okrName2, ASSIGNED_PERSON_2_OBJECTIVE,
                      ASSIGNED_PERSON_2_KEY_RESULT_1, ASSIGNED_PERSON_2_KEY_RESULT_2, okrListForAssignedPerson2);
        }


        [Test, Description("This test case verifies the possibility to create task by manager")]
        public void AddNewTaskTestCase()
        {
            // Navigating to TopPanel for creating new task           
            topPanelPageObject.PlusTaskButton.Click();

            // Assigning random name to task           
            taskTitle = "Task_" + identificationNumber;

            // Filling all fields of the task
            addTaskPageObject.TaskTitle.SendKeys(taskTitle);
            addTaskPageObject.TaskDescription.SendKeys(TASK_DESCRIPTION);
            addTaskPageObject.TeamField.Click();
            addTaskPageObject.TeamInput.SendKeys(TASK_TEAM + Keys.Enter);
            addTaskPageObject.TaskStartDate.Clear();
            addTaskPageObject.TaskStartDate.SendKeys(TASK_STARTDATE);
            addTaskPageObject.TaskEndDate.Clear();
            addTaskPageObject.TaskEndDate.SendKeys(TASK_ENDDATE);
            addTaskPageObject.TaskPriorityListComboBox.SelectByText(TASK_PRIORITY);

            // List of assignees
            assigneesList = addTaskPageObject.AssigneesList;

            foreach (var user in assigneesList)
            {
                var userName = user.UserName;
                var name = userName.Text;
                var keyresultList = user.KeyResults;

                foreach (var keyresult in keyresultList)
                {
                    var keyresultInstance = keyresult.Text;
                    var inputSet = keyresult.FindElement(By.TagName(KEY_RESULT_TAGNAME));

                    // Checking checkboxes with corresponding keyresults
                    if (!inputSet.Selected
                        && (name == ASSIGNED_PERSON_1 && keyresultInstance == ASSIGNED_PERSON_1_KEY_RESULT_1)
                        || (name == ASSIGNED_PERSON_2 && keyresultInstance == ASSIGNED_PERSON_2_KEY_RESULT_1))
                    {
                        inputSet.SendKeys(Keys.Space);
                    }
                }
            }

            addTaskPageObject.CreateTaskButton.SendKeys(Keys.Enter);

            // Clicking button 'MyTasks' 
            for (int i = 0; i < RETRYING_ITERATION_COUNT; i++)
            {
                try
                {
                    sideBarMenuPageObject.MyTasks.Click();

                    if (myTasksPageObject.MyTasksTitleLabel.Displayed)
                    {
                        isNotStaleElement = true;
                    }
                }
                catch (Exception)
                {
                    isNotStaleElement = false;
                }

                if (isNotStaleElement && myTasksPageObject.MyTasksTitleLabel.Text == "My tasks")
                {
                    break;
                }
            }

            // Assigning the value to the variable taskList
            for (int i = 0; i < RETRYING_ITERATION_COUNT; i++)
            {
                try
                {
                    wait.Until(driver => myTasksPageObject.TaskList.Count > 0);
                    wait.Until(ExpectedConditions.ElementToBeClickable(myTasksPageObject.TaskList[0].TaskName));

                    // List of all tasks for this user
                    taskList = myTasksPageObject.TaskList;
                                 
                }
                catch (Exception)
                {                    
                }

                if (taskList.Count > 0)
                {
                    break;
                }
            }

            // Checking result of additing new task
            Assert.AreEqual(taskList.First().TaskName.Text, taskTitle);
        }


        [TearDown, Description("Cleaning, logout and close browser")]
        public void TearDown()
        {
            // Deleting of created task
            taskList.First().TaskName.Click();

            string temp = taskList.First().TaskName.Text;
            deleteTaskPageObject.SettingsIcon.Click();
            deleteTaskPageObject.DropdownList.DeleteItem.Click();

            // Cheking of deleting of task
            if (myTasksPageObject.TaskList.Count > 0 && myTasksPageObject.TaskList.First().TaskName.Text == taskTitle)
            {
                throw new Exception(TASK_DELETING_EXCEPTION);
            }

            // Deleting of created OKR1
            DeleteOkr(ASSIGNED_PERSON_1, okrName1, okrListForAssignedPerson1);
            SelectUser(ASSIGNED_PERSON_1, okrListForAssignedPerson1);

            // Cheking of deleting of OKR1
            if (okrListForAssignedPerson1.OKRsList.Count > 0 && okrListForAssignedPerson1.OKRsList.Last().TitleLabel.Text.Equals(okrName1))
            {
                throw new Exception(OKR_1_DELETING_EXCEPTION);
            }

            DeleteOkr(ASSIGNED_PERSON_2, okrName2, okrListForAssignedPerson2);
            SelectUser(ASSIGNED_PERSON_2, okrListForAssignedPerson2);

            // Cheking of deleting of OKR1
            if (okrListForAssignedPerson2.OKRsList.Count > 0 && okrListForAssignedPerson2.OKRsList.Last().TitleLabel.Text == okrName2)
            {
                throw new Exception(OKR_2_DELETING_EXCEPTION);
            }

            sideBarMenuPageObject.LogoutItem.Click();
            driver.Quit();
        }


        // Creating OKR
        private void CreateOkr(string assignedPerson, string okrName, string objectiveName, string keyResultFirst, string keyResultSecond, OKRListPageObject okrList)
        {
            SelectUser(assignedPerson, okrList);

            // Click button "New OKR"
            okrList.NewOKRButton.Click();

            // Adding new OKR 
            setOKRPageObject.OKRNameField.Click();
            setOKRPageObject.OKRNameField.Clear();
            setOKRPageObject.OKRNameField.SendKeys(okrName);

            setOKRPageObject.ObjectivesList[0].ObjectiveNameField.Click();
            setOKRPageObject.ObjectivesList[0].ObjectiveNameField.Clear();
            setOKRPageObject.ObjectivesList[0].ObjectiveNameField.SendKeys(objectiveName);

            setOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.Click();
            setOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.Clear();
            setOKRPageObject.ObjectivesList[0].KeyResultsList[0].KeyResultField.SendKeys(keyResultFirst);

            setOKRPageObject.ObjectivesList[0].KeyResultsList[1].KeyResultField.Click();
            setOKRPageObject.ObjectivesList[0].KeyResultsList[1].KeyResultField.Clear();
            setOKRPageObject.ObjectivesList[0].KeyResultsList[1].KeyResultField.SendKeys(keyResultSecond);

            setOKRPageObject.SaveButton.Click();
        }


        // Deleting OKR
        private void DeleteOkr(string assignedPerson, string okrName, OKRListPageObject okrList)
        {
            // Selecting an assigned person
            SelectUser(assignedPerson, okrList);

            // Сleaning (removal of the created OKR) 
            if (okrList.OKRsList.Last().TitleLabel.Text == okrName)
            {
                // Clicking on 'okrList.OKRsList.Last().TitleLabel' sometimes causes an incorrect result
                var okrLink = okrList.OKRsList.Last().OKRCardLink.GetAttribute("href");
                driver.Navigate().GoToUrl(okrLink);

                approveOKRPageObject.SettingsButton.Click();
                approveOKRPageObject.SettingsMenu.DeleteMenuItem.Click();

                wait.Until(ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert().Accept();
            }
        }


        // Selecting an assigned person
        private void SelectUser(string assignedPerson, OKRListPageObject okrList)
        {
            // Clicking button 'OKR' 
            for (int i = 0; i < RETRYING_ITERATION_COUNT; i++)
            {
                try
                {                    
                    sideBarMenuPageObject.OKRItem.Click();

                    if (myTasksPageObject.MyTasksTitleLabel.Displayed)
                    {
                        isNotStaleElement = true;
                    }
                }
                catch (Exception)
                {
                    isNotStaleElement = false;
                }

                if (isNotStaleElement && myTasksPageObject.MyTasksTitleLabel.Text == "OKR")
                {
                    break;
                }
            }           

            // To avoid choosing a false person 
            for (int i = 0; i < RETRYING_ITERATION_COUNT; i++)
            {
                try
                {                   
                    okrList.ActualUserName.Click();
                    okrList.UserComboBoxInput.SendKeys(assignedPerson + Keys.Enter);

                    if (okrList.ActualUserName.Displayed)
                    {
                        isNotStaleElement = true;
                    }
                }
                catch (Exception)
                {
                    isNotStaleElement = false;
                }                

                if (isNotStaleElement && okrList.ActualUserName.Text == assignedPerson)
                {
                    break;
                }
            }
            
            // To avoid choosing false value (Firefox changes sended value to default value after short time) 
            if (currentBrowser == SelenoidDrivers.FIREFOX)
            {
                Thread.Sleep(FIREFOX_SLEEP_WAITER);
            }
        }       
    }
}
