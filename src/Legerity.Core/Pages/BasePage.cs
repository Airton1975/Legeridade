namespace Legerity.Pages;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Exceptions;
using Extensions;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;

/// <summary>
/// Defines a base page for creating tests following the page object pattern.
/// </summary>
public abstract class BasePage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BasePage"/> class using the <see cref="AppManager.App"/> instance that verifies the page has loaded within 2 seconds.
    /// </summary>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown in 2 seconds.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    protected BasePage()
        : this(AppManager.App, TimeSpan.FromSeconds(2))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePage"/> class using a <see cref="WebDriver"/> instance that verifies the page has loaded within 2 seconds.
    /// </summary>
    /// <param name="app">
    /// The instance of the started application driver that will be used to drive the page interaction.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown in 2 seconds.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    protected BasePage(WebDriver app)
        : this(app, TimeSpan.FromSeconds(2))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePage"/> class using the <see cref="AppManager.App"/> instance that verifies the page has loaded within the given timeout.
    /// </summary>
    /// <param name="traitTimeout">
    /// The amount of time the driver should wait when searching for the <see cref="Trait"/> if it is not immediately present.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown in the given timeout.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    protected BasePage(TimeSpan? traitTimeout)
        : this(AppManager.App, traitTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePage"/> class using a <see cref="WebDriver"/> instance that verifies the page has loaded within the given timeout.
    /// </summary>
    /// <param name="app">
    /// The instance of the started application driver that will be used to drive the page interaction.
    /// </param>
    /// <param name="traitTimeout">
    /// The amount of time the driver should wait when searching for the <see cref="Trait"/> if it is not immediately present.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown in the given timeout.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    protected BasePage(WebDriver app, TimeSpan? traitTimeout)
    {
        App = app;
        WaitTimeout = traitTimeout ?? TimeSpan.FromSeconds(2);
        VerifyPageShown(WaitTimeout);
    }

    /// <summary>
    /// Gets the instance of the started application.
    /// <para>
    /// The <see cref="App"/> instance serves as base for the drivers and can be referenced for basic Selenium functions.
    /// </para>
    /// <para>
    /// This could be a <see cref="WindowsDriver"/>, <see cref="AndroidDriver"/>, <see cref="IOSDriver"/>, or web driver.
    /// </para>
    /// </summary>
    public WebDriver App { get; }

    /// <summary>
    /// Gets or sets the amount of time the driver should wait when searching for elements if they are not immediately present.
    /// </summary>
    public TimeSpan WaitTimeout { get; set; }

    /// <summary>
    /// Gets the instance of the started Windows application.
    /// </summary>
    protected WindowsDriver WindowsApp => App as WindowsDriver;

    /// <summary>
    /// Gets the instance of the started Android application.
    /// </summary>
    protected AndroidDriver AndroidApp => App as AndroidDriver;

    /// <summary>
    /// Gets the instance of the started iOS application.
    /// </summary>
    protected IOSDriver IOSApp => App as IOSDriver;

    /// <summary>
    /// Gets the instance of the started web application.
    /// </summary>
    protected WebDriver WebApp => App;

    /// <summary>
    /// Gets a given trait of the page to verify that the page is in view.
    /// </summary>
    protected abstract By Trait { get; }

    /// <summary>
    /// Finds the first element in the page that matches the <see cref="By" /> locator.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>A <see cref="WebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public WebElement FindElement(By locator)
    {
        return App.FindWebElement(locator);
    }

    /// <summary>
    /// Finds all the elements in the page that matches the <see cref="By" /> locator.
    /// </summary>
    /// <param name="locator">The locator to find the elements.</param>
    /// <returns>A readonly collection of <see cref="WebElement"/>.</returns>
    public ReadOnlyCollection<WebElement> FindElements(By locator)
    {
        return App.FindWebElements(locator);
    }

    /// <summary>
    /// Finds the first element in the page that matches the specified XPath.
    /// </summary>
    /// <param name="xpath">The XPath to find the element.</param>
    /// <returns>A <see cref="WebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public WebElement FindElementByXPath(string xpath)
    {
        return App.FindElementByXPath(xpath);
    }

    /// <summary>
    /// Finds all the elements in the page that matches the specified XPath.
    /// </summary>
    /// <param name="xpath">The XPath to find the elements.</param>
    /// <returns>A readonly collection of <see cref="WebElement"/>.</returns>
    public ReadOnlyCollection<WebElement> FindElementsByXPath(string xpath)
    {
        return App.FindElementsByXPath(xpath);
    }

    /// <summary>
    /// Finds the first element in the page that matches the specified ID.
    /// </summary>
    /// <param name="id">The ID of the element.</param>
    /// <returns>A <see cref="WebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public WebElement FindElementById(string id)
    {
        return App.FindElementById(id);
    }

    /// <summary>
    /// Finds the first of element in the page that matches the specified name.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <returns>A <see cref="WebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public WebElement FindElementByName(string name)
    {
        return App.FindElementByName(name);
    }

    /// <summary>
    /// Determines whether the current page is shown immediately.
    /// </summary>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public void VerifyPageShown()
    {
        VerifyPageShown(null);
    }

    /// <summary>
    /// Determines whether the current page is shown with the specified timeout.
    /// </summary>
    /// <param name="timeout">
    /// The amount of time the driver should wait when searching for the <see cref="Trait"/> if it is not immediately present.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="PageNotShownException">Thrown when the page is not shown.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public void VerifyPageShown(TimeSpan? timeout)
    {
        if (App == null)
        {
            throw new DriverNotInitializedException(
                $"An app driver has not been initialized. Call 'AppManager.StartApp()' with an instance of an {nameof(AppManagerOptions)} to setup for testing.");
        }

        if (timeout == null)
        {
            if (App != null && App.FindElement(Trait) == null)
            {
                throw new PageNotShownException(GetType().Name);
            }
        }
        else
        {
            if (App != null)
            {
                AttemptWaitForDriverElement(Trait, timeout.Value, App);
            }
        }
    }

    /// <summary>
    /// Determines whether the given element is shown.
    /// </summary>
    /// <param name="locator">
    /// The locator for the element to find.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="ElementNotShownException">Thrown when the element is not shown.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public void VerifyElementShown(By locator)
    {
        VerifyElementShown(locator, null);
    }

    /// <summary>
    /// Determines whether the given element is shown with the specified timeout.
    /// </summary>
    /// <param name="locator">
    /// The locator for the element to find.
    /// </param>
    /// <param name="timeout">
    /// The amount of time the driver should wait when searching for the <paramref name="locator"/> element if it is not immediately present.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    /// <exception cref="ElementNotShownException">Thrown when the element is not shown.</exception>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public void VerifyElementShown(By locator, TimeSpan? timeout)
    {
        if (App == null)
        {
            throw new DriverNotInitializedException(
                $"An app driver has not been initialized. Call 'AppManager.StartApp()' with an instance of an {nameof(AppManagerOptions)} to setup for testing.");
        }

        if (timeout == null)
        {
            if (App != null && App.FindElement(locator) == null)
            {
                throw new ElementNotShownException(locator.ToString());
            }
        }
        else
        {
            if (App != null)
            {
                AttemptWaitForDriverElement(locator, timeout.Value, App);
            }
        }
    }

    /// <summary>
    /// Determines whether the given element is not shown.
    /// </summary>
    /// <param name="locator">
    /// The locator for the element to locate.
    /// </param>
    /// <exception cref="DriverNotInitializedException">Thrown when AppManager.StartApp() has not been called.</exception>
    public void VerifyElementNotShown(By locator)
    {
        if (App == null)
        {
            throw new DriverNotInitializedException(
                $"An app driver has not been initialized. Call 'AppManager.StartApp()' with an instance of an {nameof(AppManagerOptions)} to setup for testing.");
        }

        try
        {
            VerifyElementShown(locator);
        }
        catch (ElementNotShownException)
        {
        }
        catch (WebDriverException wde) when (wde.Message.Contains("element could not be located"))
        {
        }
    }

    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    private static void AttemptWaitForDriverElement(By locator, TimeSpan timeout, IWebDriver appDriver)
    {
        var wait = new WebDriverWait(appDriver, timeout);
        wait.Until(driver => driver.FindElement(locator) != null);
    }
}
