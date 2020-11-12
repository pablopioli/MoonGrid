using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MoonGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample
{
    public static class Program
    {
        public static ICollection<Product> FakeData;
        public static ICollection<Product> DatosFalsos;

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSweetAlert2();

            // You can localize the grid
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture;
            MoonGridLocalization.Default = new MoonGrid.Localization.English();

            // Configure default values for grid
            // Since it uses FontAwesomePro by default, in this sample 
            // we need to change the icons to something available in the free version
            MoonGridLocalization.Default.FilterEnabledIcon = "fas fa-filter";
            MoonGridLocalization.Default.FilterDisabledIcon = "fas fa-filter";

            // Generate fake data
            var faker = new Bogus.Faker("en");

            var data = new List<Product>();
            FakeData = data;
            for (int i = 0; i < 200; i++)
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

            faker = new Bogus.Faker("es");
            data = new List<Product>();
            DatosFalsos = data;
            for (int i = 0; i < 100; i++)
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
