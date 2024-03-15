namespace Legerity.Extensions;

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

/// <summary>
/// Defines a collection of extensions for elements.
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Determines the bounding rectangle for the specified element.
    /// </summary>
    /// <param name="element">The element to determine the rect for.</param>
    /// <returns>The elements bounding rectangle.</returns>
    /// <exception cref="StaleElementReferenceException">Thrown when an element is no longer valid in the document DOM.</exception>
    public static Rectangle GetBoundingRect(this IWebElement element)
    {
        var location = element.Location;
        var size = element.Size;
        return new Rectangle(location.X, location.Y, size.Width, size.Height);
    }

    /// <summary>
    /// Finds the first element in the given element that matches the <see cref="By" /> locator.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>A <see cref="WebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public static WebElement FindWebElement(this IWebElement element, By locator)
    {
        return element.FindElement(locator) as WebElement;
    }

    /// <summary>
    /// Finds all the elements in the given element that matches the <see cref="By" /> locator.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="locator">The locator to find the elements.</param>
    /// <returns>A readonly collection of <see cref="WebElement"/>.</returns>
    public static ReadOnlyCollection<WebElement> FindWebElements(this IWebElement element, By locator)
    {
        return element.FindElements(locator).Cast<WebElement>().ToList().AsReadOnly();
    }

    /// <summary>
    /// Finds the first element in the given element that matches the specified text.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="text">The text to find.</param>
    /// <returns>A <see cref="IWebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public static IWebElement FindElementByText(this IWebElement element, string text)
    {
        return element.FindElement(ByExtras.Text(text));
    }

    /// <summary>
    /// Finds all the elements in the given element that matches the specified text.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="text">The text to find.</param>
    /// <returns>A readonly collection of <see cref="IWebElement"/>.</returns>
    public static ReadOnlyCollection<IWebElement> FindElementsByText(this IWebElement element, string text)
    {
        return element.FindElements(ByExtras.Text(text));
    }

    /// <summary>
    /// Finds the first element in the given element that matches the specified text partially.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="text">The partial text to find.</param>
    /// <returns>A <see cref="IWebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown when no element matches the expected locator.</exception>
    public static IWebElement FindElementByPartialText(this IWebElement element, string text)
    {
        return element.FindElement(ByExtras.PartialText(text));
    }

    /// <summary>
    /// Finds all the elements in the given element that matches the specified text partially.
    /// </summary>
    /// <param name="element">The remote web element.</param>
    /// <param name="text">The partial text to find.</param>
    /// <returns>A readonly collection of <see cref="IWebElement"/>.</returns>
    public static ReadOnlyCollection<IWebElement> FindElementsByPartialText(this IWebElement element, string text)
    {
        return element.FindElements(ByExtras.PartialText(text));
    }

    /// <summary>
    /// Retrieves all child elements that can be located by the driver for the given element.
    /// </summary>
    /// <param name="element">The remote web driver.</param>
    /// <returns>A readonly collection of <see cref="IWebElement"/>.</returns>
    public static ReadOnlyCollection<IWebElement> GetAllChildElements(this IWebElement element)
    {
        return element.FindElements(By.XPath(".//*"));
    }

    /// <summary>
    /// Attempts to wait until a specified element condition is met, with an optional timeout.
    /// </summary>
    /// <param name="element">The element to wait on.</param>
    /// <param name="condition">The condition of the element to wait on.</param>
    /// <param name="timeout">The optional timeout wait on the condition being true.</param>
    /// <param name="retries">An optional count of retries after a timeout before accepting the failure.</param>
    /// <param name="exceptionHandler">The optional exception handler thrown if an error occurs as a result of timeout.</param>
    /// <returns>Whether the wait was a success.</returns>
    /// <exception cref="Exception">Thrown when the <paramref name="exceptionHandler"/> callback throws an exception.</exception>
    public static bool TryWaitUntil(
        this WebElement element,
        Func<WebElement, bool> condition,
        TimeSpan? timeout = default,
        int retries = 0,
        Action<Exception> exceptionHandler = null)
    {
        try
        {
            WaitUntil(element, condition, timeout, retries);
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Waits until a specified element condition is met, with an optional timeout.
    /// </summary>
    /// <param name="element">The element to wait on.</param>
    /// <param name="condition">The condition of the element to wait on.</param>
    /// <param name="timeout">The optional timeout wait on the condition being true.</param>
    /// <param name="retries">An optional count of retries after a timeout before accepting the failure.</param>
    /// <typeparam name="TResult">The type of expected result from the wait condition.</typeparam>
    /// <returns>The <typeparamref name="TResult"/> of the wait until operation.</returns>
    /// <exception cref="WebDriverException">Thrown when the condition is not met in the allocated timeout period.</exception>
    /// <exception cref="Exception">Thrown when the <paramref name="condition"/> callback throws an exception.</exception>
    public static TResult WaitUntil<TResult>(
        this WebElement element,
        Func<WebElement, TResult> condition,
        TimeSpan? timeout = default,
        int retries = 0)
    {
        try
        {
            return new WebDriverWait(element.WrappedDriver, timeout ?? TimeSpan.Zero).Until(_ =>
            {
                try
                {
                    return condition(element);
                }
                catch (StaleElementReferenceException)
                {
                    return default;
                }
            });
        }
        catch (WebDriverException)
        {
            if (retries <= 0)
            {
                throw;
            }

            return WaitUntil(element, condition, timeout, retries - 1);
        }
    }
}
