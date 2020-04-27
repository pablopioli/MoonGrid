using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MoonGrid;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample
{
    public static class Program
    {
        public static ICollection<Product> FakeData;

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSweetAlert2();

            // You can localize the grid
            GridConfiguration.Default = new MoonGrid.Localization.English();

            // Configure default values for grid
            // Since it uses FontAwesomePro by default, in this sample 
            // we need to change the icons to something available in the free version
            GridConfiguration.Default.FilterEnabledIcon = "fas fa-filter";
            GridConfiguration.Default.FilterDisabledIcon = "fas fa-filter";

            // Generate fake data
            var faker = new Bogus.Faker("en");

            var data = new List<Product>();
            FakeData = data;
            for (int i = 0; i < 1000; i++)
            {
                var product = new Product()
                {
                    Sku = faker.Phone.PhoneNumberFormat(),
                    Name = faker.Commerce.ProductName(),
                    Price = decimal.Parse(faker.Commerce.Price()),
                    Stock = faker.Random.Number(1, 1000),
                    Category = faker.Commerce.Department()
                };

                data.Add(product);
            }
            await builder.Build().RunAsync();
        }
    }
}
