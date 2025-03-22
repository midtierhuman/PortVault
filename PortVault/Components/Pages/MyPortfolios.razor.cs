using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Components.Pages
{
    public partial class MyPortfolios : ComponentBase
    {
        protected List<MutualFund> funds = new();

        protected override async Task OnInitializedAsync()
        {
            await FetchAMFIData();
        }

        private async Task FetchAMFIData()
        {
            try
            {
                using HttpClient client = new();
                string url = "https://www.amfiindia.com/spages/NAVAll.txt"; // AMFI API endpoint
                string response = await client.GetStringAsync(url);

                funds = ParseAMFIResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching AMFI data: {ex.Message}");
            }
        }

        private List<MutualFund> ParseAMFIResponse(string data)
        {
            List<MutualFund> result = new();
            string[] lines = data.Split('\n');

            foreach (string line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 4 && decimal.TryParse(parts[1], out decimal nav))
                {
                    result.Add(new MutualFund
                    {
                        Name = parts[0].Trim(),
                        NAV = nav,
                        Date = parts[3].Trim()
                    });
                }
            }
            return result;
        }
    }

    // Model class
    public class MutualFund
    {
        public string Name { get; set; }
        public decimal NAV { get; set; }
        public string Date { get; set; }
    }
}
