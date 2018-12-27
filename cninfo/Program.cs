using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace cninfo
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("pdf");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DownloadPDF("0");
            DownloadPDF("6");

            Console.WriteLine("Done");
        }

        private static void DownloadPDF(string prefix)
        {
            int stockIndex = 1;

            do
            {
                var pageName = GetPageName(prefix, stockIndex);

                Console.WriteLine(pageName);

                string uri = $"http://www.cninfo.com.cn/disclosure/annualreport/stocks/ar1y/cninfo/{pageName}.js";

                HttpClient client = new HttpClient();
                if (!client.GetAsync(uri).Result.IsSuccessStatusCode)
                {
                    if (stockIndex > 100000)
                    {
                        break;
                    }
                    else
                    {
                        stockIndex++;
                        continue;
                    }
                }

                var response = client.GetStreamAsync(uri).Result;

                var responseString = "";
                using (StreamReader reader = new StreamReader(response, Encoding.GetEncoding("GB2312")))
                {
                    responseString = reader.ReadToEnd();
                }

                var splitted = responseString.Split(',');

                for (int i = 0; i < splitted.Length; i++)
                {
                    if (splitted[i].Contains("2017年年度报告\""))
                    {
                        string pdf = splitted[i - 1].Replace("\"", string.Empty);

                        var pdfstream = client.GetStreamAsync($"http://www.cninfo.com.cn/{pdf}").Result;

                        var fileStream = File.Create(Path.Combine("pdf", $"{pageName}.pdf"));
                        pdfstream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                }

                stockIndex++;
            }
            while (true);
        }

        private static string GetPageName(string prefix, int stockIndex)
        {
            if (stockIndex < 10)
            {
                return prefix + "0000" + stockIndex;
            }
            else if (stockIndex < 100)
            {
                return prefix + "000" + stockIndex;
            }
            else if (stockIndex < 1000)
            {
                return prefix + "00" + stockIndex;
            }
            else if (stockIndex < 10000)
            {
                return prefix + "0" + stockIndex;
            }
            else if (stockIndex < 100000)
            {
                return prefix + stockIndex;
            }
            else
            {
                return stockIndex.ToString();
            }
        }
    }
}
