using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;


namespace ArticlesBot
{
    public class Sukharskiy_Week4_Task2
    {
        public static void Main(string[] args)
        {
            const string URL = "http://nz.ukma.edu.ua/index.php?option=com_content&task=category&sectionid=10&id=60&Itemid=47";
            var bot = new Eddie();
            bot.makeRequest(URL);
            Console.ReadKey();
        } 
    }
    class Eddie
    {
        public void makeRequest(string requestURL)
        {
            var result = GetResponseString(requestURL).Result;
            var converted = Encoding.ASCII.GetString(result);

            const string HF = "href\\s*=\\s*(?:[\"'](http)(?<hf>[^\"']*)(Itemid=47))";//лінки на потрібні нам статті
            const string HREFS = "href\\s*=\\s*(?:[\"'](?<hf>[^\"']*)(\\.pdf))";//лінки на пдф файли

            var linksMatches = Regex.Matches(converted, HF);
            foreach (Match linkMatch in linksMatches)
            {
                Match match = linkMatch;
                if (match.Groups[1].ToString() == "http")
                {
                    string curHref = match.Groups[1].ToString() + match.Groups["hf"].ToString() + "Itemid=47";
                    //програма знаходить посилання в яких замість & стоїть &amp; . Прибираю зайве, щоб отримати коректний лінк
                    curHref = curHref.Replace("amp;", "");
                    Console.WriteLine("FOUND_OUTSIDE - " + curHref);
                    
                    var pdfs = GetResponseString(curHref).Result;
                    var conv = Encoding.ASCII.GetString(pdfs);

                    var pdfLinksMatches = Regex.Matches(conv, HREFS);
                    foreach (Match link in pdfLinksMatches)
                    {
                        Match pdf = link;
                        var hrefPDF = pdf.Groups["hf"] + ".pdf";
                        Console.WriteLine("FOUND_DEEP - " + hrefPDF);
                        this.savePDF(hrefPDF);
                    }
                    
                }
            }
            Console.WriteLine("END");

        }
        public void savePDF(string href)
        {
            Uri uri = new Uri(href);
            System.IO.File.WriteAllBytes("D:\\PDFS\\"+ System.IO.Path.GetFileName(uri.LocalPath), GetResponseString(href).Result);
        }
        public static async Task<byte[]> GetResponseString(string requestURI)
        {
            //await Task.Delay(5000);
            var httpClient = new HttpClient();
            var response = await httpClient.GetByteArrayAsync(requestURI);
            return response;
        }
    }
}


