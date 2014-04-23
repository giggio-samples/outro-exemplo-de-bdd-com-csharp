using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace ExemploBdd.Testes.Funcionais.Features.Support
{
    public static class ExtensoesWebDriver
    {
        public static void TireScreenshoot(this IWebDriver driver, string nomeArquivo = "")
        {
            if (!string.IsNullOrWhiteSpace(nomeArquivo)) nomeArquivo = "_" + nomeArquivo;
            nomeArquivo = DateTime.Now.ToString("yy-MM-dd-HH-mm-ss-fff") + nomeArquivo + ".png";
            nomeArquivo = Regex.Replace(nomeArquivo, @"[\<\>\:""\/\\\|\?\*]", string.Empty);
            nomeArquivo = Path.Combine(Path.GetTempPath(), nomeArquivo);
            Debug.WriteLine("Salvando screenshot em {0}.", (object)nomeArquivo);
            driver.TakeScreenshot().SaveAsFile(nomeArquivo, ImageFormat.Png);
        }
    }
}