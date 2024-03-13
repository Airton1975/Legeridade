namespace Legerity.Exceptions;

using System;

/// <summary>
/// Defines an exception for when the Appium sever could not be loaded.
/// </summary>
public class AppiumServerLoadFailedException : LegerityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppiumServerLoadFailedException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception thrown as a result of the Appium server load failure.</param>
    internal AppiumServerLoadFailedException(Exception innerException)
        : base("The Appium server could not be loaded", innerException)
    {
    }
}
