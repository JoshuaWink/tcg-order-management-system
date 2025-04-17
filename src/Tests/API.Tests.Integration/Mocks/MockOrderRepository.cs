using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGOrderManagement.OrderService.Models;
using TCGOrderManagement.OrderService.Repositories;

namespace TCGOrderManagement.API.Tests.Integration.Mocks
{
    public class MockOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders;

        public MockOrderRepository()
        {
            _orders = new List<Order>
            {
                new Order
                {
                    Id = "order1",
                    CustomerId = "customer1",
                    Status = OrderStatus.Created,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = "orderItem1",
                            ItemId = "item1",
                            Name = "Black Lotus",
                            Price = 10000.00m,
                            Quantity = 1
                        }
                    },
                    ShippingAddress = new Address
                    {
                        Line1 = "123 Test St",
                        City = "Test City",
                        State = "TS",
                        ZipCode = "12345",
                        Country = "Test Country"
                    },
                    PaymentInfo = new PaymentInfo
                    {
                        Method = PaymentMethod.CreditCard,
                        TransactionId = "transaction1"
                    },
                    ShippingInfo = new ShippingInfo
                    {
                        Method = ShippingMethod.Standard,
                        Cost = 5.99m,
                        TrackingNumber = "tracking1",
                        EstimatedDeliveryDate = DateTime.Now.AddDays(5)
                    },
                    SubTotal = 10000.00m,
                    Tax = 800.00m,
                    Total = 10805.99m,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                },
                new Order
                {
                    Id = "order2",
                    CustomerId = "customer2",
                    Status = OrderStatus.PaymentPending,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = "orderItem2",
                            ItemId = "item2",
                            Name = "Charizard 1st Edition",
                            Price = 5000.00m,
                            Quantity = 1
                        }
                    },
                    ShippingAddress = new Address
                    {
                        Line1 = "456 Test Ave",
                        City = "Test Town",
                        State = "TS",
                        ZipCode = "67890",
                        Country = "Test Country"
                    },
                    PaymentInfo = new PaymentInfo
                    {
                        Method = PaymentMethod.PayPal
                    },
                    ShippingInfo = new ShippingInfo
                    {
                        Method = ShippingMethod.Express,
                        Cost = 15.99m,
                        EstimatedDeliveryDate = DateTime.Now.AddDays(2)
                    },
                    SubTotal = 5000.00m,
                    Tax = 400.00m,
                    Total = 5415.99m,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                }
            };
        }

        public Task<Order> CreateOrderAsync(Order order)
        {
            order.Id = Guid.NewGuid().ToString();
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            
            foreach (var item in order.Items)
            {
                item.Id = Guid.NewGuid().ToString();
            }
            
            _orders.Add(order);
            return Task.FromResult(order);
        }

        public Task<Order> GetOrderAsync(string id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(order);
        }

        public Task<IEnumerable<Order>> GetOrdersAsync(
            string customerId = null, 
            OrderStatus? status = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int page = 1, 
            int pageSize = 20)
        {
            var query = _orders.AsQueryable();
            
            if (!string.IsNullOrEmpty(customerId))
            {
                query = query.Where(o => o.CustomerId == customerId);
            }
            
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }
            
            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value);
            }
            
            return Task.FromResult(query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable());
        }

        public Task<Order> UpdateOrderAsync(Order order)
        {
            var existingOrder = _orders.FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder != null)
            {
                existingOrder.Status = order.Status;
                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.PaymentInfo = order.PaymentInfo;
                existingOrder.ShippingInfo = order.ShippingInfo;
                existingOrder.SubTotal = order.SubTotal;
                existingOrder.Tax = order.Tax;
                existingOrder.Total = order.Total;
                existingOrder.UpdatedAt = DateTime.Now;
                
                return Task.FromResult(existingOrder);
            }
            
            return Task.FromResult<Order>(null);
        }

        public Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.Now;
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
    }
} 