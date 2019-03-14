using System;
using OpenQA.Selenium;
using NUnit.Framework;
using FluxDayAutomation.Drivers;
using FluxDayAutomation.PageObjects;
using System.Linq;

namespace FluxDayAutomation.UITests
{
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V1)]
    [TestFixture(SelenoidDrivers.CHROME, SelenoidDrivers.CHROME_V2)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V1)]
    [TestFixture(SelenoidDrivers.FIREFOX, SelenoidDrivers.FIREFOX_V2)]
    [Author("Ihor Vavrysh", "ihor2009@gmail.com")]
    [Category("DeleteTask")]
    public class DeleteTaskUITest
    {
        private const string URL = "https://app.fluxday.io";
        private const string TITLE = "My task";
        private const string DESCRIPTION = "Interesting task";
        private const string TEAM = "DevOps";
        private const string PRIORITY = "Low";
        private const string EMAIL = "admin@fluxday.io";
        private const string PASSWORD = "password";
        private const int IMPLICIT_WAIT_SECONDS = 10;
        private const int ELEMENT_NUMBER = 0;

        private LoginPageObject loginPageObject;
        private DeleteTaskPageObject deleteTaskPageObject;
        private MyTasksPageObject myTasksPageObject;
        private SideBarMenuPageObject sidebarMenuPageObject;
        private TopPanelPageObject topPanelPageObject;
        private AddTaskPageObject addTaskPageObject;

        private IWebDriver driver;

        public DeleteTaskUITest(string browser, string version)
        {
            this.driver = SelenoidDrivers.CreateDriver(browser, version);
        }

        [SetUp, Description("Environment, login and adding new task")]
        public void Setup()
        {
            // Setup for remote running
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(IMPLICIT_WAIT_SECONDS);
            driver.Navigate().GoToUrl(URL);

            // Initialization Page Objects
            loginPageObject = new LoginPageObject(driver);
            deleteTaskPageObject = new DeleteTaskPageObject(driver);
            myTasksPageObject = new MyTasksPageObject(driver);
            sidebarMenuPageObject = new SideBarMenuPageObject(driver);
            topPanelPageObject = new TopPanelPageObject(driver);
            addTaskPageObject = new AddTaskPageObject(driver);

            // Log into an application
            Login(EMAIL, PASSWORD);

            NavigateMyTasks();

            // Create task 
            AddTask(TITLE, DESCRIPTION, TEAM, PRIORITY);
        }

        [Test, Description("Deleting the task that was created in the precondition")]
        public void DeleteTask()
        {
            var taskList = myTasksPageObject.TaskList;
            var taskAttribute = taskList[ELEMENT_NUMBER].GetHref;

            deleteTaskPageObject.SettingsIcon.Click();
            deleteTaskPageObject.DropdownList.DeleteItem.Click();
            taskList = myTasksPageObject.TaskList;

            // Assertion whether exist task with attribute "href" that equals taskAttribute
            Assert.That(taskList.All(t => !t.GetHref.Equals(taskAttribute)), "The task isn't deleted");
        }

        [TearDown, Description("Deleting tasks if the test failed, closing browser")]
        public void CleanUp()
        {
            CleanTask();
            driver.Quit();
        }

        /// <summary>
        /// Add new task
        /// </summary>
        public void AddTask(string title, string description, string team, string priority)
        {
            topPanelPageObject.PlusTaskButton.Click();
            addTaskPageObject.TaskTitle.SendKeys(title);
            addTaskPageObject.TaskDescription.SendKeys(description);
            addTaskPageObject.TeamListComboBox.SelectByText(team);
            addTaskPageObject.TaskPriorityListComboBox.SelectByText(priority);
            addTaskPageObject.CreateTaskButton.Click();
        }

        /// <summary>
        /// Log into the application 
        /// </summary>
        public void Login(string email, string password)
        {
            loginPageObject.UserEmailTextBox.Clear();
            loginPageObject.UserEmailTextBox.SendKeys(email);
            loginPageObject.UserPasswordTextBox.Clear();
            loginPageObject.UserPasswordTextBox.SendKeys(password);
            loginPageObject.LoginButton.Click();
        }

        public void NavigateMyTasks()
        {
            sidebarMenuPageObject.MyTasks.Click();
        }

        /// <summary>
        /// Deleting tasks if the test failed
        /// </summary>
        public void CleanTask()
        {
            var taskList = myTasksPageObject.TaskList;

            foreach (var taskElementList in taskList)
            {
                var taskName = taskElementList.TaskName.Text;

                if (taskName == TITLE)
                {
                    taskElementList.TaskName.Click();
                    deleteTaskPageObject.SettingsIcon.Click();
                    deleteTaskPageObject.DropdownList.DeleteItem.Click();
                }
            }            
        }
    }
}