﻿using Groundsman.Models;
using Xamarin.Forms;

namespace Groundsman.Interfaces;

public interface INavigationService<T>
{
    Task NavigateToEditPage(T feature);
    Task NavigateToNewEditPage(GeoJSONType type);
    Task NavigateToNewLoggerPage();
    Task NavigateToLoggerPage(T feature);
    Task PushAddFeaturePage();
    Task PushWelcomePage();
    Task PushAboutPage();
    Task NavigateBack(bool modal);
    Page GetCurrentPage();
    Task<bool> ShowAlert(string title, string body, bool question);
    Task ShowImportAlert(int successfulImports);
}
