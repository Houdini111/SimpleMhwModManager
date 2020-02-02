using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraperToCSV
{
    class Program
    {
        private static string baseURL = "https://mhw.poedb.tw/eng/";
        private static string weaponURL = "wp_weapon";
        private static string armorURL = "pl_equip";

        static void Main(string[] args)
        {
            scrape().GetAwaiter().GetResult();
        }

        private static async Task scrape()
        {
            Task armorTask = scrapeArmor();
            Task weaponTask = scrapeWeapons();

            await Task.WhenAll(armorTask, weaponTask);
            Console.WriteLine("Successfully scraped, interpreted, and wrote data");
            return;
        }

        private static async Task scrapeArmor()
        {
            string[] expectedNames = { "Type", "ArmorId", "Name", "LayeredId", "Male", "Female" };
            IHtmlTableElement table = await getTable(baseURL + armorURL);

            verifyHeaderNames(expectedNames.ToList(), table);

            List<Armor> allArmor = new List<Armor>();
            List<IHtmlTableRowElement> rows = getRows(table);
            foreach(IHtmlTableRowElement row in rows)
            {
                Armor armor = new Armor();
                armor.type = (Armor.ARMOR_SLOT)Enum.Parse(typeof(Armor.ARMOR_SLOT), row.Cells[0].InnerHtml, true);
                armor.ID = Int32.Parse(row.Cells[1].InnerHtml);
                armor.name = row.Cells[2].InnerHtml;
                armor.layered_ID = Int32.Parse(row.Cells[3].InnerHtml);
                armor.male_location = row.Cells[4].InnerHtml;
                armor.female_location = row.Cells[5].InnerHtml;
                allArmor.Add(armor);
            }

            using (StreamWriter sw = new StreamWriter("armor.json", false))
            {
                sw.Write(JsonConvert.SerializeObject(allArmor, Formatting.Indented));
            }
        }

        private static async Task scrapeWeapons()
        {
            string[] expectedNames = { "Type", "Id", "Name", "ModelType", "MainModel", "PartModel" };
            IHtmlTableElement table = await getTable(baseURL + weaponURL);

            verifyHeaderNames(expectedNames.ToList(), table);

            List<Weapon> allWeapon = new List<Weapon>();
            List<IHtmlTableRowElement> rows = getRows(table);
            foreach (IHtmlTableRowElement row in rows)
            {
                Weapon weapon = new Weapon();
                weapon.weapon_type = (Weapon.WEAPON_TYPE)Enum.Parse(typeof(Weapon.WEAPON_TYPE), row.Cells[0].InnerHtml, true);
                weapon.ID = Int32.Parse(row.Cells[1].InnerHtml);
                weapon.name = row.Cells[2].InnerHtml;
                weapon.model_type = (Weapon.MODEL_TYPE)Enum.Parse(typeof(Weapon.MODEL_TYPE), row.Cells[3].InnerHtml, true);
                weapon.main_model = row.Cells[4].InnerHtml;
                weapon.part_model = row.Cells[5].InnerHtml;
                allWeapon.Add(weapon);
            }

            using (StreamWriter sw = new StreamWriter("weapon.json", false))
            {
                sw.Write(JsonConvert.SerializeObject(allWeapon, Formatting.Indented));
            }
        }

        private static async Task<IHtmlTableElement> getTable(string url)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(url);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);


            //A sanity check to ensure that there are no unexpected changes
            List<AngleSharp.Dom.IElement> tables = document.All.Where(elem => "table".Equals(elem.TagName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (tables.Count != 1)
            {
                throw new Exception("Unexpect number of tables: " + tables.Count + ", expected 1");
            }
            IHtmlTableElement table = (IHtmlTableElement)tables[0];
            return table;
        }

        //Sanity check to ensure the page hasn't change format
        private static bool verifyHeaderNames(List<string> expected, IHtmlTableElement given)
        {
            List<string> headerNames = new List<string>();
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> headerCells = given.QuerySelectorAll("th");
            foreach (IHtmlTableHeaderCellElement headerCell in headerCells)
            {
                headerNames.Add(headerCell.TextContent);
            }

            if (!expected.ToList().SequenceEqual(headerNames))
            {
                throw new Exception("Headers do not match. Expected: |" + String.Join(",", expected) + "| but found |" + String.Join(",", headerNames));
            }
            return true;
        }

        private static List<IHtmlTableRowElement> getRows(IHtmlTableElement table)
        {
            IHtmlTableSectionElement body = (IHtmlTableSectionElement)table.QuerySelector("tbody");
            return body.Rows.ToList();
        }
    }
}
