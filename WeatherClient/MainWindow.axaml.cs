using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherClient
{
    public partial class MainWindow : Window
    {
        // Reuse one HttpClient for performance:
        private readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void FetchButton_Click(object? sender, RoutedEventArgs e)
        {
            var zip = ZipTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(zip))
            {
                ResultText.Text = "Please enter a ZIP code.";
                return;
            }

            try
            {
                // Call your local API:
                var json = await _http.GetStringAsync($"/weather/{zip}");

                // Parse a field—e.g. temperature (in °F):
                using var doc = JsonDocument.Parse(json);
                var temp = doc.RootElement
                              .GetProperty("main")
                              .GetProperty("temp")
                              .GetDouble();

                ResultText.Text = $"Temperature in {zip}: {temp:F1} °F";
            }
            catch (HttpRequestException hrex)
            {
                ResultText.Text = $"Network error: {hrex.Message}";
            }
            catch (KeyNotFoundException)
            {
                ResultText.Text = "Unexpected response format.";
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error: {ex.Message}";
            }
        }
    }
}
