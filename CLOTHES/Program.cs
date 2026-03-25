using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QLBH.DTO;

namespace CLOTHES
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles and set default font for Vietnamese support
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Set default font to Segoe UI for proper Vietnamese character rendering
            Application.SetDefaultFont(new Font("Segoe UI", 9F, FontStyle.Regular));
            
            // Register encoding provider for proper Vietnamese text handling
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

           Application.Run(new FrmLogin());
        }
    }
}