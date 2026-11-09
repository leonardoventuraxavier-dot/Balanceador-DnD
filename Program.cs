using Microsoft.Web.WebView2.WinForms;
using System.Reflection;

namespace BalanceadorDnD;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private readonly WebView2 browser = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "Balanceador de Encontros D&D 5e";
        Width = 1200;
        Height = 850;
        MinimumSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(browser);
        Shown += async (_, _) => await LoadAppAsync();
    }

    private async Task LoadAppAsync()
    {
        try
        {
            var appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BalanceadorDnD");
            Directory.CreateDirectory(appFolder);
            var htmlPath = Path.Combine(appFolder, "index.html");

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("BalanceadorDnD.index.html"))
            {
                if (stream is null) throw new FileNotFoundException("O HTML incorporado não foi encontrado.");
                using var output = File.Create(htmlPath);
                await stream.CopyToAsync(output);
            }

            var userData = Path.Combine(appFolder, "WebView2Data");
            var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment
                .CreateAsync(null, userData);
            await browser.EnsureCoreWebView2Async(environment);
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            browser.Source = new Uri(htmlPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível abrir o aplicativo. Verifique se o Microsoft Edge WebView2 Runtime está instalado.\n\n" + ex.Message,
                "Erro ao iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
