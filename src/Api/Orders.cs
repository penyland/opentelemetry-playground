using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace Api;

public class OrdersModule
{
    public List<Order> Orders { get; } = [];
    public List<Product> Products { get; } = [];

    private readonly string MeterName = "Api.OrderModule";

    private readonly Meter meter;
    private readonly Counter<int> totalOrdersCounter;
    private readonly Counter<int> totalProductsCount;
    private readonly ObservableGauge<int> totalOrdersGauge;
    private readonly Histogram<double> productsPerOrderHistogram;

    public OrdersModule(IMeterFactory meterFactory)
    {
        meter = meterFactory.Create(MeterName);

        totalOrdersCounter = meter.CreateCounter<int>("total-orders-count");
        totalProductsCount = meter.CreateCounter<int>("total-products-count");
        totalOrdersGauge = meter.CreateObservableGauge<int>("total-orders", () => Orders.Count);
        productsPerOrderHistogram = meter.CreateHistogram<double>("products-per-order", "Products", "Distribution of products per order.");
    }

    public Product CreateProduct(string name, decimal price)
    {
        var product = new Product
        {
            Id = Products.Count + 1,
            Name = name,
            Price = price
        };

        Products.Add(product);
        totalProductsCount.Add(1, KeyValuePair.Create<string, object?>("ProductPrice", price));

        return product;
    }

    public Order CreateOrder(List<int> productIds)
    {
        var order = new Order
        {
            Id = Orders.Count + 1,
            CreatedAt = DateTime.UtcNow,
            Products = Products.Where(x => productIds.Contains(x.Id)).ToList()
        };

        Orders.Add(order);
        totalOrdersCounter.Add(1);

        return order;
    }

    public Order? GetOrder(int id) => Orders.FirstOrDefault(x => x.Id == id);

    public Order? DeleteOrder(int id)
    {
        var existingOrder = Orders.FirstOrDefault(x => x.Id == id);

        if (existingOrder is null)
        {
            return null;
        }

        Orders.Remove(existingOrder);

        return existingOrder;
    }

    public void Checkout(int orderId)
    {
        var order = Orders.FirstOrDefault(x => x.Id == orderId);

        productsPerOrderHistogram.Record(order!.Products.Count);
    }
}

public record Order
{
    public int Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<Product> Products { get; init; } = new();

    public decimal TotalPrice => Products.Sum(x => x.Price);
}

public record Product
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public decimal Price { get; init; }
}

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/orders");

        group.MapPost("/", (OrdersModule orderModule, [FromBody] List<int> productIds) =>
        {
            var order = orderModule.CreateOrder(productIds);

            return TypedResults.Created(order.Id.ToString(), order);
        });

        group.MapDelete("/{id:int}", (OrdersModule orderModule, int id) =>
        {
            orderModule.DeleteOrder(id);

            return TypedResults.Ok();
        });

        group.MapPost("/{id:int}/checkout", (OrdersModule orderModule, int id) =>
        {
            orderModule.Checkout(id);

            return TypedResults.Ok();
        });

        return group;
    }

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/product", (OrdersModule orderModule, string name, decimal price) =>
        {
            var product = orderModule.CreateProduct(name, price);
            return TypedResults.Created(product.Id.ToString(), product);
        });

        return builder;
    }
}